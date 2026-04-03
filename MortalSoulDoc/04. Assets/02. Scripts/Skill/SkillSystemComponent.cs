using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace MS.Skill
{
    public class SkillSystemComponent : MonoBehaviour
    {
        public event Action<string, BaseSkill> OnSkillAdded;
        public event Action<string> OnSkillUsed;
        public event Action<float, float> OnHealthChanged; // 어트리뷰트셋 래핑용 이벤트
        public event Action<int, bool> OnHitCallback;
        public event Action OnDeadCallback;

        private Dictionary<string, BaseSkill> ownedSkillDict = new Dictionary<string, BaseSkill>();
        private Dictionary<string, CancellationTokenSource> runningSkillDict = new Dictionary<string, CancellationTokenSource>();

        private Dictionary<string, StatusEffect> statusEffectDict = new Dictionary<string, StatusEffect>();
        private List<string> removeStatusEffectList = new List<string>();

        private BaseAttributeSet attributeSet;
        private FieldCharacter owner;

        public BaseAttributeSet AttributeSet => attributeSet;
        public FieldCharacter Owner => owner;



        void Update()
        {
            foreach (var skill in ownedSkillDict.Values)
            {
                skill.OnUpdate(Time.deltaTime);
            }

            foreach (var effectPair in statusEffectDict)
            {
                effectPair.Value.OnUpdate(Time.deltaTime);
                if (effectPair.Value.IsFinished)
                {
                    effectPair.Value.OnEnd();
                    removeStatusEffectList.Add(effectPair.Key);
                }
            }
            foreach (var key in removeStatusEffectList)
            {
                statusEffectDict.Remove(key);
            }
            removeStatusEffectList.Clear();
        }

        public void InitSSC(FieldCharacter _owner, BaseAttributeSet _attributeSet)
        {
            owner = _owner;
            attributeSet = _attributeSet;
            attributeSet.OnHealthChanged += OnHealthChangedCallback;
            Stat maxHealthStat = attributeSet.GetStatByType(EStatType.MaxHealth);
            maxHealthStat.OnValueChanged += OnMaxHealthChangedCallback;
            OnMaxHealthChangedCallback(maxHealthStat.Value);
        }

        public void GiveSkill(string _skillKey)
        {
            if (DataManager.Instance.SkillSettingDataDict.TryGetValue(_skillKey, out SkillSettingData _skillData))
            {
                // namespace 규칙이 반드시 보장되어야함
                var skillType = Type.GetType("MS.Skill." +  _skillKey);
                try
                {
                    BaseSkill skillInstance = (BaseSkill)Activator.CreateInstance(skillType);
                    skillInstance.InitSkill(this, _skillData);
                    ownedSkillDict.Add(_skillKey, skillInstance);
                    OnSkillAdded?.Invoke(_skillKey, skillInstance);
                }
                catch (Exception ex)
                {
                    Debug.LogError("SkillSystemComponent::GiveSkill : " + ex.Message);
                }
            }
        }

        public async UniTask UseSkill(string _skillKey)
        {
            if (!ownedSkillDict.TryGetValue(_skillKey, out BaseSkill skillToUse)) return;
            if (skillToUse.IsCooltime) return;
            if (!skillToUse.CanActivateSkill()) return;
            if (runningSkillDict.ContainsKey(_skillKey)) return;

            CancellationTokenSource cts = new CancellationTokenSource();
            runningSkillDict[_skillKey] = cts;

            try
            {
                if (!skillToUse.IsPostUseCooltime) skillToUse.SetCooltime();
                await skillToUse.ActivateSkill(cts.Token);
                OnSkillUsed?.Invoke(_skillKey);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{_skillKey} 스킬 캔슬");
            }
            catch (Exception e)
            {
                Debug.LogError($"{_skillKey} 스킬 실행 중 에러: {e.Message}");
            }
            finally
            {
                if (skillToUse.IsPostUseCooltime) skillToUse.SetCooltime();

                // 스킬이 완료되었든, 캔슬되었든, 오류가 났든 실행 목록에서 제거
                if (runningSkillDict.ContainsKey(_skillKey))
                {
                    runningSkillDict.Remove(_skillKey);
                    cts.Dispose();
                }
            }
        }

        public float TakeDamage(DamageInfo _damageInfo)
        {
            if (attributeSet.Health <= 0) return 0;
            if (attributeSet.GetStatValueByType(EStatType.Evasion) > 0)
            {
                if (BattleUtils.CalcEvasionStat(attributeSet.GetStatValueByType(EStatType.Evasion)))
                {
                    UIManager.Instance.ShowEvasionText(_damageInfo.Target.Position);
                    return 0;
                }
            }

            float finalDamage = _damageInfo.Damage;

            // 방어력, 약점속성 계산으로 최종 데미지 결정
            finalDamage = BattleUtils.CalcWeaknessAttribute(finalDamage, _damageInfo.AttributeType, attributeSet.WeaknessAttributeType);
            finalDamage = BattleUtils.CalcDefenseStat(finalDamage, attributeSet.GetStatValueByType(EStatType.Defense));

            attributeSet.Health -= finalDamage;
            ApplyKnockback(_damageInfo);

            // 공격자 생명흡수 적용
            float lifeSteal = _damageInfo.Attacker.SSC.attributeSet.GetStatValueByType(EStatType.LifeSteal);
            if (lifeSteal > 0 && _damageInfo.Attacker.SSC.attributeSet.HealthRatio != 1)
            {
                if (MathUtils.IsSuccess(lifeSteal))
                {
                    _damageInfo.Attacker.SSC.attributeSet.Health += Settings.LifeStealValue;
                    GameplayCueManager.Instance.PlayCue("GC_Acquire_GainHealth", _damageInfo.Attacker);
                }
            }

            OnHitCallback?.Invoke((int)finalDamage, _damageInfo.IsCritic);

            if (attributeSet.Health <= 0)
            {
                OnDeadCallback?.Invoke();
            }
            if (_damageInfo.sourceSkill != null)
            {
                _damageInfo.sourceSkill.AddTotalDamageDealt(finalDamage);
            }

            return finalDamage;
        }

        private void ApplyKnockback(DamageInfo _damageInfo)
        {
            if (_damageInfo.KnockbackForce <= 0 || _damageInfo.Attacker == null) return;

            // onwer(피격자) <-> Attacker(공격자) 넉백의 방향 구하기
            Vector3 knockbackDir = (owner.Position - _damageInfo.Attacker.Position).normalized;
            knockbackDir.y = 0;

            // 공격자의 넉백파워 스탯반영
            float knockbackForce = _damageInfo.KnockbackForce;
            float AttackerKnockbackStat = _damageInfo.Attacker.SSC.attributeSet.GetStatValueByType(EStatType.KnockbackMultiple);
            if (AttackerKnockbackStat > 0)
            {
                knockbackForce *= AttackerKnockbackStat;
            }

            owner.ApplyKnockback(knockbackDir, knockbackForce);
        }

        public void ApplyStatusEffect(string _key, StatusEffect _statusEffect)
        {
            if (_statusEffect == null) return;

            if (statusEffectDict.TryGetValue( _key, out StatusEffect _existEffect))
            {
                _existEffect.OnEnd(); // 기존에 적용되어 있던 효과의 OnEnd 호출
                statusEffectDict.Remove(_key);
            }

            statusEffectDict[_key] = _statusEffect;
            _statusEffect.OnApply();
        }

        #region Skill Util
        public bool IsCooltime(string _skillKey)
        {
            if (ownedSkillDict.TryGetValue(_skillKey, out BaseSkill skill))
            {
                return skill.IsCooltime;
            }

            return true;
        }

        public bool HasSkill(string _skillKey)
        {
            return ownedSkillDict.ContainsKey(_skillKey);
        }

        public List<SkillStatisticsInfo> GetSkillStatistics()
        {
            List<SkillStatisticsInfo> list = new List<SkillStatisticsInfo>();

            foreach (var pair in ownedSkillDict)
            {
                BaseSkill skill = pair.Value;
                list.Add(new SkillStatisticsInfo
                {
                    SkillKey = pair.Key,
                    IconKey = skill.SkillData.IconKey,
                    TotalDamage = skill.TotalDamageDealt,
                    DPS = skill.DPS,
                });
            }

            // 데미지 높은 순 정렬
            list.Sort((a, b) => b.TotalDamage.CompareTo(a.TotalDamage));
            return list;
        }
        #endregion


        #region Skill Cancel
        public void CancelSkill(string _skillKey)
        {
            if (runningSkillDict.TryGetValue(_skillKey, out CancellationTokenSource cts))
            {
                cts.Cancel();
                cts.Dispose();
                runningSkillDict.Remove(_skillKey);
            }
        }
        public void CancelAllSkills()
        {
            foreach (var cts in runningSkillDict.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            runningSkillDict.Clear();
        }
        #endregion

        // 어트리뷰트셋의 OnHealthChanged 래핑용 이벤트 콜백
        private void OnHealthChangedCallback(float _curHP, float _maxHP)
        {
            OnHealthChanged?.Invoke(_curHP, _maxHP);
        }
        private void OnMaxHealthChangedCallback(float _maxHP)
        {
            OnHealthChanged?.Invoke(attributeSet.Health, _maxHP);
        }


        public void ClearSSC()
        {
            foreach (var effect in statusEffectDict.Values)
            {
                effect.OnEnd();
            }
            CancelAllSkills();
            statusEffectDict.Clear();
            ownedSkillDict.Clear();
            OnDeadCallback = null;
            OnHitCallback = null;
        }
    }
}