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
    public class StunBall : BaseSkill
    {
        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            owner.Animator.SetTrigger(Settings.AnimHashAttack);

            int projecCnt = 1 + (int)ownerSSC.AttributeSet.GetStatValueByType(EStatType.ProjectileCount);
            List<MonsterCharacter> targetList = MonsterManager.Instance.GetNearestMonsters(owner.Position, projecCnt);

            for (int i=0;i< projecCnt;i++)
            {
                Vector3 fireDir;

                // 리스트 범위 내에 있으면 해당 몬스터 타겟팅
                if (i < targetList.Count)
                {
                    fireDir = (targetList[i].Position - owner.Position).normalized;
                }
                // 타겟이 부족하면 랜덤 발사
                else
                {
                    Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized;
                    fireDir = new Vector3(randomCircle.x, 0, randomCircle.y);
                }

                ProjectileObject stunBall = SkillObjectManager.Instance.SpawnSkillObject<ProjectileObject>(
                "Projec_StunBall", owner, Settings.MonsterLayer);
                stunBall.InitProjectile(fireDir, 16f);
                stunBall.SetDuration(2f);
                stunBall.SetHitCallback((_obj, _ssc) =>
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
                    _ssc.Owner.ApplyStunEffect("StunBall", skillData.GetValue(ESkillValueType.Duration)); 
                });
            }

            await UniTask.CompletedTask;
        }
    }
}