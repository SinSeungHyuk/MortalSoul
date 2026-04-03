using MS.Skill;
using System;
using UnityEngine;

namespace MS.Field
{
    public abstract class SkillObject : FieldObject
    {
        private const int INFINITE_ATTACK = int.MaxValue;
        private const float INFINITE_DURATION = float.MaxValue;

        protected Action<SkillObject, SkillSystemComponent> onHitCallback;

        protected FieldCharacter owner;
        protected LayerMask targetLayer;
        protected int hitCountPerAttack;
        protected int maxAttackCount;
        protected FieldObject traceTarget;
        protected Vector3 targetOffset;

        private string skillObjectKey;
        private float duration;
        private float elapsedTime;

        public string SkillObjectKey => skillObjectKey;


        public virtual void OnUpdate(float _deltaTime)
        {
            elapsedTime += _deltaTime;
            if (elapsedTime >= duration)
            {
                ObjectLifeState = FieldObjectLifeState.Death;
            }
        }

        public void InitSkillObject(string _skillObjectKey, FieldCharacter _owner, LayerMask _targetLayer)
        {
            ObjectLifeState = FieldObjectLifeState.Live;
            ObjectType = FieldObjectType.SkillObject;

            skillObjectKey = _skillObjectKey;
            owner = _owner;
            targetLayer = _targetLayer;

            traceTarget = null;
            elapsedTime = 0;
            duration = INFINITE_DURATION;
            maxAttackCount = INFINITE_ATTACK;
            hitCountPerAttack = 1;
        }

        public void SetHitCountPerAttack(int _hitCountPerAttack) 
            => hitCountPerAttack = _hitCountPerAttack;
        public void SetMaxHitCount(int _maxHitCount)
            => maxAttackCount = _maxHitCount;
        public void SetDuration(float _duration)
            => duration = _duration;
        public void SetHitCallback(Action<SkillObject, SkillSystemComponent> _onHitCallback)
            => onHitCallback = _onHitCallback;

        public void SetTraceTarget(FieldObject _target, Vector3 _offset = default)
        {
            traceTarget = _target;
            targetOffset = _offset;
        }
        public void ClearTraceTarget() 
            => traceTarget = null;

        protected bool IsValidTarget(Collider _other, out SkillSystemComponent _ssc)
        {
            _ssc = null;
            if (((1 << _other.gameObject.layer) & targetLayer) == 0)
                return false;
            return _other.TryGetComponent(out _ssc);
        }
    }
}