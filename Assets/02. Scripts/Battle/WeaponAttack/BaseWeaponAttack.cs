using System;
using MS.Field;

namespace MS.Battle
{
    public abstract class BaseWeaponAttack
    {
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;

        public bool IsAttacking => isAttacking;

        protected FieldCharacter owner;
        protected BaseAttributeSet attributeSet;
        protected SpineController spine;
        protected bool isAttacking;

        public virtual void InitBaseWeaponAttack(FieldCharacter _owner, BaseAttributeSet _attrSet)
        {
            owner = _owner;
            attributeSet = _attrSet;
            spine = _owner.SpineController;
        }

        public abstract void ActivateAttack();

        protected void InvokeAttackStarted() => OnAttackStarted?.Invoke();
        protected void InvokeAttackEnded() => OnAttackEnded?.Invoke();
    }
}
