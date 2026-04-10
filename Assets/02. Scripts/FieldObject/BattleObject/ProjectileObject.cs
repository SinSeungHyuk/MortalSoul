using UnityEngine;

namespace MS.Field
{
    public class ProjectileObject : BattleObject
    {
        private Vector2 moveDir;
        private float moveSpeed;


        public void InitProjectile(Vector2 _moveDir, float _moveSpeed)
        {
            moveDir = _moveDir.normalized;
            moveSpeed = _moveSpeed;
        }

        public void SetMoveSpeed(float _speed)
        {
            moveSpeed = _speed;
        }

        public void SetMoveDir(Vector2 _dir)
        {
            moveDir = _dir.normalized;
        }


        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (IsValidTarget(_other, out FieldCharacter _fieldChar))
            {
                for (int i = 0; i < hitCountPerAttack; i++)
                {
                    onHitCallback?.Invoke(this, _fieldChar.BSC);
                }
                maxAttackCount--;

                if (maxAttackCount <= 0)
                    ObjectLifeState = FieldObjectLifeState.Death;
            }
        }

        public override void OnUpdate(float _deltaTime)
        {
            base.OnUpdate(_deltaTime);

            if (traceTarget != null && traceTarget.ObjectLifeState == FieldObjectLifeState.Live)
            {
                Vector2 targetPos = (Vector2)traceTarget.Position + (Vector2)targetOffset;
                Vector2 curPos = transform.position;
                transform.position = Vector2.MoveTowards(curPos, targetPos, moveSpeed * _deltaTime);

                Vector2 dir = targetPos - curPos;
                if (dir.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
            else
            {
                transform.position += (Vector3)(moveDir * moveSpeed * _deltaTime);
            }
        }
    }
}
