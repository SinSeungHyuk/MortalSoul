using MS.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Field
{
    public class AreaObject : SkillObject
    {
        private List<SkillSystemComponent> attackTargetList = new List<SkillSystemComponent>();
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

            float areaRange = owner.SSC.AttributeSet.GetStatValueByType(EStatType.AreaRangeMultiple);
            if (areaRange > 0)
            {
                transform.localScale *= areaRange;
            }
        }

        public void SetDelay(float _delay)
            => delayTime = _delay;
        public void SetAttackInterval(float _attackInterval)
        {
            attackInterval = _attackInterval;
            elapsedAttackTime = attackInterval;
        }


        public void OnTriggerEnter(Collider _other)
        {
            if (IsValidTarget(_other, out SkillSystemComponent _ssc))
            {
                attackTargetList.Add(_ssc);
            }
        }
        public void OnTriggerExit(Collider _other)
        {
            if (_other.TryGetComponent(out SkillSystemComponent _ssc))
                attackTargetList.Remove(_ssc);
        }

        public override void OnUpdate(float _deltaTime)
        {
            base.OnUpdate(_deltaTime);

            // 0. 대상 추적
            if (traceTarget != null)
            {
                if (traceTarget.ObjectLifeState == FieldObjectLifeState.Live)
                    transform.position = traceTarget.Position + targetOffset;
                else
                    ClearTraceTarget();
            }

            // 1. 스킬 최초 사용시 딜레이 적용
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

            // 2. 스킬효과 간격 적용
            elapsedAttackTime += _deltaTime;
            if (elapsedAttackTime < attackInterval)
                return;

            // 3. 남은 타격횟수 계산 (Area는 Duration으로 공격종료 판단)
            if (maxAttackCount <= 0) return;
            maxAttackCount--;

            // 4. 유효한 대상에게 히트콜백 적용
            for (int i = attackTargetList.Count - 1; i >= 0; i--)
            {
                var attackTarget = attackTargetList[i];
                if (!attackTarget || attackTarget.Owner.ObjectLifeState != FieldObjectLifeState.Live)
                {
                    attackTargetList.RemoveAt(i);
                    continue;
                }

                for (int hitCount = 0; hitCount < hitCountPerAttack; hitCount++)
                {
                    onHitCallback?.Invoke(this, attackTarget);
                }
            }
            elapsedAttackTime = 0f;
        }
    }
}