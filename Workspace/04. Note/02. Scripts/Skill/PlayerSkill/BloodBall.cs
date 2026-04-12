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
    public class BloodBall : BaseSkill
    {
        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            owner.Animator.SetTrigger(Settings.AnimHashAttack);
            bool isReturn = false;
            float totalDamage = 0f;
            List<ProjectileObject> projecList = new List<ProjectileObject>();

            int projecCnt = 1 + (int)ownerSSC.AttributeSet.GetStatValueByType(EStatType.ProjectileCount);
            List<MonsterCharacter> targetList = MonsterManager.Instance.GetNearestMonsters(owner.Position, projecCnt);

            for (int i = 0; i < projecCnt; i++)
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

                LayerMask layer = Settings.MonsterLayer | Settings.PlayerLayer;
                ProjectileObject bloodBall = SkillObjectManager.Instance.SpawnSkillObject<ProjectileObject>(
                "Projec_BloodBall", owner, layer);
                bloodBall.InitProjectile(fireDir, 13f);
                projecList.Add(bloodBall);
                bloodBall.SetHitCallback((_obj, _ssc) =>
                {
                    if (_ssc.Owner.ObjectType == FieldObject.FieldObjectType.Monster)
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
                        totalDamage += finalDamage;
                    }
                    else if (_ssc.Owner.ObjectType == FieldObject.FieldObjectType.Player && isReturn)
                    {
                        ownerSSC.AttributeSet.Health += (totalDamage / projecCnt) * 0.05f;
                        bloodBall.SetDuration(0f);
                    }
                });
            }

            await UniTask.WaitForSeconds(0.75f, cancellationToken: token);

            isReturn = true;
            foreach (var projec in projecList)
            {
                projec.SetMoveSpeed(8f);
                projec.SetTraceTarget(owner);
            }

            await UniTask.CompletedTask;
        }
    }
}