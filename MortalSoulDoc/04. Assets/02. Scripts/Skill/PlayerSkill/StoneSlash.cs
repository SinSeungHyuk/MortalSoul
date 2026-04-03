using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System.Threading;
using UnityEngine;


namespace MS.Skill
{
    public class StoneSlash : BaseSkill
    {
        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            //owner.Animator.SetTrigger("Attack01");
            //owner.Animator.speed = 0.2f;

            //owner.Animator.SetBool("Attack02Casting", true);

            //var testProjectile = SkillObjectManager.Instance.SpawnSkillObject<ProjectileObject>(
            //    "Projectile_StoneSlash", owner, Settings.MonsterLayer);
            //testProjectile.InitProjectile(owner.transform.forward, 5f);
            //testProjectile.SetDuration(15f);
            //testProjectile.SetHitCallback((_skillObject, _ssc) =>
            //{
            //    DamageInfo damageInfo = new DamageInfo(
            //            _attacker: owner,
            //            _target: _ssc.Owner,
            //            _attributeType: EDamageAttributeType.Fire,
            //            _damage: 40f,
            //            _isCritic: false,
            //            _knockbackForce: 5f
            //        );
            //    _ssc.TakeDamage(damageInfo);
            //});

            //var testProjectile = SkillObjectManager.Instance.SpawnSkillObject<AreaObject>(
            //    "Area_StoneSlash", owner, Settings.PlayerLayer);
            //testProjectile.InitArea(2f);
            //testProjectile.transform.position = new Vector3(owner.Position.x + 10f, owner.Position.y, owner.Position.z + 10f);
            //testProjectile.SetDuration(15f);
            //testProjectile.SetMaxHitCount(5);
            //testProjectile.SetHitCountPerAttack(2);
            //testProjectile.SetHitCallback((_skillObject, _ssc) =>
            //{
            //    Debug.Log($"Player hit!");
            //});

            await UniTask.Yield();

            //await UniTask.Delay(3000);

            //owner.Animator.SetBool("Attack02Casting", false);
            //GameplayCueManager.Instance.PlayCue("GC_Test", owner);

            //CheckHit();
        }


        private float attackRadius = 5.0f;
        private float forwardOffset = 3.0f;
        private void CheckHit()
        {
            Vector3 center = owner.Position + (owner.transform.forward * forwardOffset);
            Collider[] hitColliders = Physics.OverlapSphere(center, attackRadius, Settings.MonsterLayer);
            foreach (var hit in hitColliders)
            {
                if (hit.gameObject == owner.gameObject) continue;

                if (hit.gameObject.TryGetComponent(out SkillSystemComponent targetSSC))
                {
                    // 데미지 정보 생성
                    DamageInfo damageInfo = new DamageInfo(
                        _attacker: owner,
                        _target: targetSSC.Owner,
                        _attributeType: EDamageAttributeType.Fire,
                        _damage: 30f,
                        _isCritic: false,
                        _knockbackForce: 5f
                    );

                    targetSSC.TakeDamage(damageInfo);
                }
            }
        }
    }
}

