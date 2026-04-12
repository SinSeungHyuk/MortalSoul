using System;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Battle
{
    public class BattleSystemComponent
    {
        // (finalDamage, isCritic)
        public event Action<int, bool> OnHit;
        public event Action OnDodged;
        public event Action OnDead;

        public SkillSystemComponent SSC { get; private set; }
        public WeaponSystemComponent WSC { get; private set; }
        public AttributeSet AttributeSet { get; private set; }

        private Dictionary<string, StatusEffect> statusEffectDict;
        private List<string> removeStatusEffectList = new List<string>();


        public void InitBSC(FieldCharacter _owner, AttributeSet _attributeSet, EWeaponType? _weaponType = null)
        {
            statusEffectDict = new Dictionary<string, StatusEffect>();
            AttributeSet = _attributeSet;

            SSC = new SkillSystemComponent();
            SSC.InitSSC(_owner, _attributeSet);

            if (_weaponType.HasValue)
            {
                WSC = new WeaponSystemComponent();
                WSC.InitWSC(_owner, _attributeSet, _weaponType.Value);
            }
        }

        public void ClearBSC()
        {
            SSC?.ClearSSC();
            WSC?.ClearWSC();

            if (statusEffectDict != null)
            {
                foreach (var effect in statusEffectDict.Values)
                    effect.End();
                statusEffectDict.Clear();
            }

            OnHit = null;
            OnDodged = null;
            OnDead = null;
        }

        public float TakeDamage(DamageInfo _info)
        {
            if (AttributeSet == null || AttributeSet.Health <= 0f) return 0f;

            // 0. 공격자 어트리뷰트셋 세팅
            var attacker = _info.Attacker.BSC.AttributeSet;

            // 1. 회피 
            if (BattleUtils.CalcEvasionStat(AttributeSet.Evasion.Value))
            {
                OnDodged?.Invoke();
                return 0f;
            }

            float finalDamage = _info.Damage;
            // 2. 치명타 (공격자 스탯 기반)
            if (_info.Attacker != null && _info.Attacker.BSC != null && _info.Attacker.BSC.AttributeSet != null)
            {
                
                if (BattleUtils.CalcCriticDamage(finalDamage, attacker.GetStatValueByType(EStatType.CriticChance), attacker.GetStatValueByType(EStatType.CriticMultiple), out float criticDamage))
                {
                    finalDamage = criticDamage;
                    _info.IsCritic = true;
                }
            }
            // 3. 속성 약점
            finalDamage = BattleUtils.CalcWeaknessAttribute(finalDamage, _info.AttributeType, AttributeSet.WeaknessAttributeType);
            // 4. 방어력
            finalDamage = BattleUtils.CalcDefenseStat(finalDamage, AttributeSet.Defense.Value);
            // 5. 체력 감소
            AttributeSet.Health -= finalDamage;

            // 6. 공격자 LifeSteal
            if (_info.Attacker != null && _info.Attacker.BSC != null && _info.Attacker.BSC.AttributeSet != null)
            {
                float lifeSteal = attacker.GetStatValueByType(EStatType.LifeSteal);
                if (lifeSteal > 0f && MathUtils.IsSuccess(lifeSteal))
                {
                    attacker.Health += Settings.LifeStealValue;
                }
            }

            OnHit?.Invoke(Mathf.RoundToInt(finalDamage), _info.IsCritic);

            if (AttributeSet.Health <= 0f)
                OnDead?.Invoke();

            return finalDamage;
        }

        public async UniTask UseSkill(string _key)
        {
            // 상태이상 체크 (기절 등으로 스킬 사용 불가 시 차단)
            // 현재는 항상 통과 — 추후 구현
            await SSC.UseSkill(_key);
        }

        public void ApplyStatusEffect(string _key, StatusEffect _effect)
        {
            // 기존 키가 있으면 End 후 교체
            if (statusEffectDict.TryGetValue(_key, out StatusEffect existing))
            {
                existing.End();
                statusEffectDict.Remove(_key);
            }

            statusEffectDict.Add(_key, _effect);
            _effect.Start();
        }

        public void OnUpdate(float _deltaTime)
        {
            SSC.OnUpdate(_deltaTime);
            UpdateStatusEffects(_deltaTime);
        }

        private void UpdateStatusEffects(float _deltaTime)
        {
            removeStatusEffectList.Clear();

            foreach (var pair in statusEffectDict)
            {
                pair.Value.Update(_deltaTime);

                if (pair.Value.IsFinished)
                {
                    pair.Value.End();
                    removeStatusEffectList.Add(pair.Key);
                }
            }

            foreach (var key in removeStatusEffectList)
                statusEffectDict.Remove(key);
        }
    }
}
