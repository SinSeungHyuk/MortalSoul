using MS.Battle;
using System;
using UnityEngine;

namespace MS.Field
{
    public abstract class BattleObject : FieldObject
    {
        protected Action<BattleObject, BattleSystemComponent> onHitCallback;

        protected FieldCharacter owner;
        protected LayerMask targetLayer;
        protected int hitCountPerAttack;
        protected int maxAttackCount;
        protected FieldObject traceTarget;
        protected Vector3 targetOffset;

        private string battleObjectKey;
        private float duration;
        private float elapsedTime;

        public string BattleObjectKey => battleObjectKey;


        public virtual void OnUpdate(float _deltaTime)
        {
            elapsedTime += _deltaTime;
            if (elapsedTime >= duration)
            {
                ObjectLifeState = FieldObjectLifeState.Death;
            }
        }

        public void InitBattleObject(string _battleObjectKey, FieldCharacter _owner, LayerMask _targetLayer)
        {
            ObjectLifeState = FieldObjectLifeState.Live;
            ObjectType = FieldObjectType.BattleObject;

            battleObjectKey = _battleObjectKey;
            owner = _owner;
            targetLayer = _targetLayer;

            traceTarget = null;
            elapsedTime = 0;
            duration = float.MaxValue;
            maxAttackCount = int.MaxValue;
            hitCountPerAttack = 1;
        }

        public void SetHitCountPerAttack(int _hitCountPerAttack)
            => hitCountPerAttack = _hitCountPerAttack;
        public void SetMaxHitCount(int _maxHitCount)
            => maxAttackCount = _maxHitCount;
        public void SetDuration(float _duration)
            => duration = _duration;
        public void SetHitCallback(Action<BattleObject, BattleSystemComponent> _onHitCallback)
            => onHitCallback = _onHitCallback;

        public void SetTraceTarget(FieldObject _target, Vector3 _offset = default)
        {
            traceTarget = _target;
            targetOffset = _offset;
        }
        public void ClearTraceTarget()
            => traceTarget = null;

        protected bool IsValidTarget(Collider2D _other, out FieldCharacter _fieldChar)
        {
            _fieldChar = null;
            if (((1 << _other.gameObject.layer) & targetLayer) == 0)
                return false;
            if (_other.TryGetComponent(out _fieldChar))
                return _fieldChar.BSC != null;
            return false;
        }
    }
}
