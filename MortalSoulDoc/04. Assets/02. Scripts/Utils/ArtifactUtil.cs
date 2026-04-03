using MS.Core;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Skill;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Utils
{
    public class ArtifactContext
    {
        public string StringVal;
        public float FloatVal;
    }

    public static class ArtifactUtil
    {
        private delegate bool ConditionDelegate(ArtifactConditionData data, ArtifactContext context);
        private static Dictionary<ArtifactConditionType, ConditionDelegate> conditionDict = new();

        private delegate void ActionDelegate(PlayerCharacter player, ArtifactActionData data, ArtifactContext _context);
        private static Dictionary<ArtifactActionType, ActionDelegate> actionDict = new();


        static ArtifactUtil()
        {
            conditionDict.Add(ArtifactConditionType.RandPercent, CheckRandPercent);
            conditionDict.Add(ArtifactConditionType.SkillNameEqual, CheckSkillNameEqual);
            conditionDict.Add(ArtifactConditionType.AcquireItem, CheckAcquireItem);

            actionDict.Add(ArtifactActionType.SpawnStar, ActionSpawnStar);
            actionDict.Add(ArtifactActionType.SpawnLightningAura, ActionSpawnLightningAura);
            actionDict.Add(ArtifactActionType.KnockbackMonster, ActionKnockbackMonster);
            actionDict.Add(ArtifactActionType.StarAura, ActionStarAura);
        }


        #region Condition
        // 모든 조건을 체크해서 모두 만족하면 true 반환
        public static bool CheckAllConditions(List<ArtifactConditionData> _conditions, ArtifactContext _context)
        {
            if (_conditions == null) return true;

            for (int i = 0; i < _conditions.Count; i++)
            {
                if (conditionDict.TryGetValue(_conditions[i].ConditionType, out ConditionDelegate checker))
                {
                    if (!checker(_conditions[i], _context)) return false;
                }
            }
            return true;
        }

        private static bool CheckRandPercent(ArtifactConditionData data, ArtifactContext context)
        {
            return UnityEngine.Random.Range(0f, 100f) <= data.FloatVal;
        }

        private static bool CheckSkillNameEqual(ArtifactConditionData data, ArtifactContext context)
        {
            return context.StringVal == data.StringVal;
        }

        private static bool CheckAcquireItem(ArtifactConditionData data, ArtifactContext context)
        {
            return context.StringVal == data.StringVal;
        }
        #endregion


        #region Action
        public static void ExecuteAllActions(this PlayerCharacter _player, List<ArtifactActionData> _actions, ArtifactContext _context)
        {
            if (_actions == null) return;

            for (int i = 0; i < _actions.Count; i++)
            {
                if (actionDict.TryGetValue(_actions[i].ActionType, out ActionDelegate executor))
                {
                    executor(_player, _actions[i], _context);
                }
            }
        }

        private static void ActionSpawnStar(PlayerCharacter _player,ArtifactActionData data, ArtifactContext _context)
        {
            int projecCnt = 1 + (int)_player.SSC.AttributeSet.GetStatValueByType(EStatType.ProjectileCount);

            for (int i = 0; i < projecCnt; i++)
            {
                Vector3 fireDir;

                MonsterCharacter target = MonsterManager.Instance.GetRandMonster();
                if (target)
                {
                    fireDir = (target.Position - _player.Position).normalized;
                }
                else
                {
                    Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized;
                    fireDir = new Vector3(randomCircle.x, 0, randomCircle.y);
                }

                ProjectileObject projectile = SkillObjectManager.Instance.SpawnSkillObject<ProjectileObject>(
                "Projec_Star", _player, Settings.MonsterLayer);
                projectile.InitProjectile(fireDir, 17f);
                projectile.SetDuration(2f);
                projectile.SetHitCallback((_obj, _ssc) =>
                {
                    float damage = _player.SSC.AttributeSet.GetStatValueByType(EStatType.AttackPower) * data.FloatVal;
                    bool isCritic = BattleUtils.CalcSkillCriticDamage(damage, _player.SSC.AttributeSet.GetStatValueByType(EStatType.CriticChance), _player.SSC.AttributeSet.GetStatValueByType(EStatType.CriticMultiple), out float finalDamage);

                    DamageInfo damageInfo = new DamageInfo(
                        _attacker: _player,
                        _target: _ssc.Owner,
                        _attributeType: EDamageAttributeType.None,
                        _damage: finalDamage,
                        _isCritic: isCritic,
                        _knockbackForce: 0
                    );
                    _ssc.TakeDamage(damageInfo);
                });
            }
        }

        private static void ActionSpawnLightningAura(PlayerCharacter _player, ArtifactActionData data, ArtifactContext _context)
        {
            var skillObject = SkillObjectManager.Instance.SpawnSkillObject<AreaObject>("Area_LightningAura", _player, Settings.MonsterLayer);
            skillObject.InitArea();
            skillObject.SetAttackInterval(0.2f);
            skillObject.transform.position = _player.Position;
            skillObject.SetDuration(2f);
            skillObject.SetHitCallback((_skillObject, _ssc) =>
            {
                DamageInfo damageInfo = new DamageInfo(
                    _attacker: _player,
                    _target: _ssc.Owner,
                    _attributeType: EDamageAttributeType.Electric,
                    _damage: 1,
                    _isCritic: false,
                    _knockbackForce: 0
                );
                _ssc.TakeDamage(damageInfo);

                _ssc.Owner.ApplyStunEffect("LightningAura", data.FloatVal);
            });
        }

        private static void ActionKnockbackMonster(PlayerCharacter _player, ArtifactActionData data, ArtifactContext _context)
        {
            Collider[] targets = Physics.OverlapSphere(_player.transform.position, 2f, Settings.MonsterLayer);
            foreach (var target in targets)
            {
                if (target.TryGetComponent<MonsterCharacter>(out var _monster))
                {
                    Vector3 dir = (_monster.Position - _player.Position).normalized;
                    _monster.ApplyKnockback(dir, data.FloatVal);
                }
            }
        }

        private static void ActionStarAura(PlayerCharacter _player, ArtifactActionData data, ArtifactContext _context)
        {
            MSEffect effect = EffectManager.Instance.PlayEffect("Eff_StarAura", _player.Position, Quaternion.identity);
            effect.SetTraceTarget(_player);

            _player.ApplyStatEffect("StarAuraAP", EStatType.AttackPower, data.FloatVal, EBonusType.Percentage);
            _player.ApplyStatEffect("StarAuraCC", EStatType.CriticChance, data.FloatVal, EBonusType.Flat);
            _player.ApplyStatEffect("StarAuraCM", EStatType.CriticMultiple, (data.FloatVal * 0.01f), EBonusType.Flat);
        }
        #endregion
    }
}