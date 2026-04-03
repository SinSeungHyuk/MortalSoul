using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System.Threading;
using UnityEngine;


namespace MS.Skill
{
    public class RedExplosion : BaseSkill
    {
        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            owner.Animator.SetTrigger(Settings.AnimHashAttack);

            var skillObject = SkillObjectManager.Instance.SpawnSkillObject<AreaObject>("Area_RedExplosion", owner, Settings.MonsterLayer);
            skillObject.InitArea();
            skillObject.transform.position = MonsterManager.Instance.GetNearestMonster(owner.Position).Position;
            skillObject.SetDuration(2f);
            skillObject.SetDelay(1f);
            skillObject.SetMaxHitCount(1);
            skillObject.SetHitCountPerAttack(1);
            skillObject.SetHitCallback((_skillObject, _ssc) =>
            {
                float damage = BattleUtils.CalcSkillBaseDamage(attributeSet.GetStatValueByType(EStatType.AttackPower), skillData);
                bool isCritic = BattleUtils.CalcSkillCriticDamage(damage, attributeSet.GetStatValueByType(EStatType.CriticChance), attributeSet.GetStatValueByType(EStatType.CriticMultiple), out float finalDamage);

                DamageInfo damageInfo = new DamageInfo(
                    _attacker: owner,
                    _target: _ssc.Owner,
                    _attributeType: skillData.AttributeType,
                    _damage: finalDamage,
                    _isCritic: isCritic,
                    _knockbackForce: skillData.GetValue(ESkillValueType.Knockback),
                    _sourceSkill: this
                );
                _ssc.TakeDamage(damageInfo);

                float gainHealth = finalDamage * 0.03f;
                ownerSSC.AttributeSet.Health += (gainHealth);
                GameplayCueManager.Instance.PlayCue("GC_Acquire_GainHealth", owner);
            });


            await UniTask.CompletedTask;
        }
    }
}

