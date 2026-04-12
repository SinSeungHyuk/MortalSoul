using MS.Skill;
using UnityEngine;

namespace MS.Field
{
    public class ProjectileObject : SkillObject
    {
        private Vector3 moveDir;
        private float moveSpeed;


        public void InitProjectile(Vector3 _moveDir, float _moveSpeed)
        {
            moveDir = _moveDir;
            moveSpeed = _moveSpeed;
        }

        public void SetMoveSpeed(float _speed)
        {
            moveSpeed = _speed;
        }

        public void SetMoveDir(Vector3 _dir)
        {
            moveDir = _dir;
        }


        public void OnTriggerEnter(Collider _other)
        {
            if (IsValidTarget(_other, out SkillSystemComponent _ssc))
            {
                for (int i = 0; i < hitCountPerAttack; i++)
                {
                    onHitCallback?.Invoke(this, _ssc);
                }
                maxAttackCount--;
            }

            if (maxAttackCount <= 0)
                ObjectLifeState = FieldObjectLifeState.Death;
        }

        public override void OnUpdate(float _deltaTime)
        {
            base.OnUpdate(_deltaTime);

            if (traceTarget != null && traceTarget.ObjectLifeState == FieldObjectLifeState.Live)
            {
                Vector3 targetPos = traceTarget.Position + targetOffset;
                transform.position = Vector3.MoveTowards(Position, targetPos, moveSpeed * _deltaTime);
                transform.LookAt(targetPos);
            }
            else
            {
                transform.position += (moveDir * moveSpeed * _deltaTime);
            }
        }
    }
}