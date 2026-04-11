using System;
using MS.Data;
using MS.Field;
using UnityEngine;

namespace MS.Battle
{
    public class WeaponSystemComponent
    {
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;

        public EWeaponType CurWeaponType => curWeaponType;
        public bool IsAttacking => curAttack?.IsAttacking ?? false;

        private FieldCharacter owner;
        private PlayerAttributeSet attrSet;
        private EWeaponType curWeaponType;
        private BaseWeaponAttack curAttack;


        public void InitWSC(FieldCharacter _owner, PlayerAttributeSet _attrSet)
        {
            owner = _owner;
            attrSet = _attrSet;
        }

        public void ChangeWeaponType(EWeaponType _weaponType)
        {
            if (curAttack != null)
            {
                curAttack.OnAttackStarted -= OnAttackStartedCallback;
                curAttack.OnAttackEnded -= OnAttackEndedCallback;
            }

            curWeaponType = _weaponType;

            string typeName = $"MS.Battle.{_weaponType}Attack";
            Type attackType = Type.GetType(typeName);
            if (attackType == null)
            {
                Debug.LogError($"[WSC] 무기 공격 클래스를 찾을 수 없음: {typeName}");
                curAttack = null;
                return;
            }

            curAttack = Activator.CreateInstance(attackType) as BaseWeaponAttack;
            if (curAttack == null)
            {
                Debug.LogError($"[WSC] 무기 공격 인스턴스 생성 실패: {typeName}");
                return;
            }

            curAttack.InitWeaponAttack(owner, attrSet);
            curAttack.OnAttackStarted += OnAttackStartedCallback;
            curAttack.OnAttackEnded += OnAttackEndedCallback;
        }

        public void ActivateAttack() => curAttack?.OnAttackInput();

        private void OnAttackStartedCallback() => OnAttackStarted?.Invoke();
        private void OnAttackEndedCallback() => OnAttackEnded?.Invoke();
    }
}
