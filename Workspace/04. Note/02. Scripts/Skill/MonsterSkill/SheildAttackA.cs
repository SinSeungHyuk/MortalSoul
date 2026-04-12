using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Utils;
using System.Threading;
using UnityEngine;


namespace MS.Skill
{
    public class SheildAttackA : BaseSkill
    {
        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            await UniTask.WaitForSeconds(0.5f);
            CheckHit();
        }

        private float attackRadius = 1.0f;
        private float forwardOffset = 1.6f;
        private void CheckHit()
        {
            Vector3 center = owner.Position + (owner.transform.forward * forwardOffset);
            Collider[] hitColliders = Physics.OverlapSphere(center, attackRadius, Settings.PlayerLayer);
            foreach (var hit in hitColliders)
            {
                if (hit.gameObject == owner.gameObject) continue;

                float damage = attributeSet.GetStatValueByType(EStatType.AttackPower);

                if (hit.gameObject.TryGetComponent(out SkillSystemComponent targetSSC))
                {
                    // 데미지 정보 생성
                    DamageInfo damageInfo = new DamageInfo(
                        _attacker: owner,
                        _target: targetSSC.Owner,
                        _attributeType: EDamageAttributeType.None,
                        _damage: damage,
                        _isCritic: false,
                        _knockbackForce: 0f
                    );

                    targetSSC.TakeDamage(damageInfo);
                }
            }
        }
    }
}

