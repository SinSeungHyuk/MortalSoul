using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System.Threading;
using UnityEngine;


namespace MS.Skill
{
    public class SlashGreen : BaseSkill
    {
        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            float speedBuff = skillData.GetValue(ESkillValueType.Buff);
            ownerSSC.AttributeSet.MoveSpeed.AddBonusStat("SlashGreen", EBonusType.Percentage, speedBuff);

            var skillObject = SkillObjectManager.Instance.SpawnSkillObject<AreaObject>("Area_SlashGreen", owner, Settings.MonsterLayer);
            skillObject.InitArea();
            skillObject.SetDuration(2.5f);
            skillObject.SetDelay(1f);
            skillObject.SetTraceTarget(owner, new Vector3(0f, 0.5f, 0f));
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
            });

            await UniTask.WaitForSeconds(1);

            ownerSSC.AttributeSet.MoveSpeed.RemoveBonusStat("SlashGreen");
        }
    }
}

