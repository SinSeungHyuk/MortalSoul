using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System.Threading;
using UnityEngine;

namespace MS.Skill
{
    public class BowAttackA : BaseSkill
    {
        private PlayerCharacter player;

        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);
            player = PlayerManager.Instance.Player;
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            Vector3 fireDir = GetDirection();
            owner.transform.rotation = Quaternion.LookRotation(fireDir);

            await UniTask.WaitForSeconds(0.4f);

            fireDir = GetDirection();

            ProjectileObject projecArrow = SkillObjectManager.Instance.SpawnSkillObject<ProjectileObject>(
                "Projec_MonsterArrow", owner, Settings.PlayerLayer);
            projecArrow.InitProjectile(fireDir, 1.5f);
            projecArrow.SetDuration(15f);
            projecArrow.transform.rotation = Quaternion.LookRotation(fireDir);
            projecArrow.SetHitCallback((_obj, _ssc) =>
            {
                float damage = attributeSet.GetStatValueByType(EStatType.AttackPower);

                DamageInfo damageInfo = new DamageInfo(
                    _attacker: owner,
                    _target: _ssc.Owner,
                    _attributeType: EDamageAttributeType.None,
                    _damage: damage,
                    _isCritic: false,
                    _knockbackForce: 10f
                );

                _ssc.TakeDamage(damageInfo);
            });
        }

        private Vector3 GetDirection()
        {
            Vector3 dir = player.Position - owner.Position;
            dir.y = 0;

            return dir;
        }
    }
}