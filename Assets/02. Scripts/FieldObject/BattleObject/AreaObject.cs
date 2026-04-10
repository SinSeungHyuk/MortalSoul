using System.Collections.Generic;
using UnityEngine;

namespace MS.Field
{
    public class AreaObject : BattleObject
    {
        private List<FieldCharacter> attackTargetList = new List<FieldCharacter>();
        private float attackInterval;
        private float elapsedAttackTime;
        private float delayTime;
        private float elapsedDelayTime;


        public void InitArea()
        {
            attackTargetList.Clear();
            attackInterval = 0f;
            elapsedAttackTime = 0f;
            delayTime = 0f;
            elapsedDelayTime = 0f;
        }

        public void SetDelay(float _delay)
            => delayTime = _delay;
        public void SetAttackInterval(float _attackInterval)
        {
            attackInterval = _attackInterval;
            elapsedAttackTime = attackInterval;
        }


        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (IsValidTarget(_other, out FieldCharacter _fieldChar))
                attackTargetList.Add(_fieldChar);
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            if (IsValidTarget(_other, out FieldCharacter _fieldChar))
                attackTargetList.Remove(_fieldChar);
        }

        public override void OnUpdate(float _deltaTime)
        {
            base.OnUpdate(_deltaTime);

            if (traceTarget != null)
            {
                if (traceTarget.ObjectLifeState == FieldObjectLifeState.Live)
                    transform.position = traceTarget.Position + targetOffset;
                else
                    ClearTraceTarget();
            }

            if (delayTime > 0f && elapsedDelayTime < delayTime)
            {
                elapsedDelayTime += _deltaTime;
                return;
            }
            else if (elapsedDelayTime >= delayTime)
            {
                delayTime = 0f;
                elapsedDelayTime = 0f;
            }

            elapsedAttackTime += _deltaTime;
            if (elapsedAttackTime < attackInterval)
                return;

            if (maxAttackCount <= 0) return;
            maxAttackCount--;

            for (int i = attackTargetList.Count - 1; i >= 0; i--)
            {
                var attackTarget = attackTargetList[i];
                if (!attackTarget || attackTarget.ObjectLifeState != FieldObjectLifeState.Live)
                {
                    attackTargetList.RemoveAt(i);
                    continue;
                }

                for (int hitCount = 0; hitCount < hitCountPerAttack; hitCount++)
                {
                    onHitCallback?.Invoke(this, attackTarget.BSC);
                }
            }
            elapsedAttackTime = 0f;
        }
    }
}
