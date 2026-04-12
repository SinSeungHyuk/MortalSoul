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
    public class Charm : BaseSkill
    {
        private float angleStep = 25f; // 투사체 간의 각도 간격


        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            owner.Animator.SetTrigger(Settings.AnimHashAttack);

            int projecCnt = 1 + (int)ownerSSC.AttributeSet.GetStatValueByType(EStatType.ProjectileCount);

            float startAngle = -((projecCnt - 1) * angleStep) * 0.5f;

            for (int i = 0; i < projecCnt; i++)
            {
                float currentAngle = startAngle + (i * angleStep);
                Vector3 fireDir = GetRotatedDirection(owner.transform.forward, currentAngle);

                ProjectileObject charm = SkillObjectManager.Instance.SpawnSkillObject<ProjectileObject>(
                "Projec_Charm", owner, Settings.MonsterLayer);
                charm.InitProjectile(fireDir, 7f);
                charm.SetDuration(1.5f);
                charm.SetHitCallback((_obj, _ssc) =>
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
                    float duration = skillData.GetValue(ESkillValueType.Duration);
                    _ssc.Owner.ApplyCharmEffect("Charm", duration, owner);
                });
            }

            await UniTask.CompletedTask;
        }

        private Vector3 GetRotatedDirection(Vector3 _baseDir, float _angle)
        {
            return Quaternion.AngleAxis(_angle, Vector3.up) * _baseDir;
        }
    }
}