using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System;
using System.Threading;
using UnityEngine;


namespace MS.Skill
{
    public class Plexus : BaseSkill
    {
        private const float totalDuration = 2f;
        private const float spawnInterval = 0.15f;
        private const float spawnRadius = 10f;


        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            owner.Animator.SetTrigger(Settings.AnimHashAttack);

            float elapsed = 0f;
            try
            {
                while (elapsed < totalDuration)
                {
                    // 랜덤 위치 계산 (플레이어 주변 반경 spawnRadius 내)
                    Vector3 spawnPos = BattleUtils.GetRandomPoint(owner.Position, spawnRadius);

                    // 기존 설정 방식 그대로 스폰 후 세팅
                    var skillObject = SkillObjectManager.Instance.SpawnSkillObject<AreaObject>("Area_Plexus", owner, Settings.MonsterLayer);
                    skillObject.InitArea();
                    skillObject.transform.position = spawnPos;
                    skillObject.SetMaxHitCount(1);
                    skillObject.SetDelay(0.1f);
                    skillObject.SetDuration(1.5f);
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
                        _ssc.Owner.ApplyStunEffect("Plexus", skillData.GetValue(ESkillValueType.Duration));
                    });

                    // 다음 스폰 대기 (취소 토큰 적용)
                    await UniTask.WaitForSeconds(spawnInterval, cancellationToken: token);
                    elapsed += spawnInterval;
                }
            }
            catch (OperationCanceledException)
            {
                // 스킬 캔슬 처리
                return;
            }

            await UniTask.CompletedTask;
        }
    }
}

