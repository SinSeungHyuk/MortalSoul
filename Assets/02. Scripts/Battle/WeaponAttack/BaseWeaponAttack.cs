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
        protected PlayerAttributeSet attrSet;
        protected SpineController spine;
        protected bool isAttacking;

        public virtual void InitWeaponAttack(FieldCharacter _owner, PlayerAttributeSet _attrSet)
        {
            owner = _owner;
            attrSet = _attrSet;
            spine = _owner.SpineController;
        }

        public abstract void OnAttackInput();

        protected void InvokeAttackStarted() => OnAttackStarted?.Invoke();
        protected void InvokeAttackEnded() => OnAttackEnded?.Invoke();
    }
}
