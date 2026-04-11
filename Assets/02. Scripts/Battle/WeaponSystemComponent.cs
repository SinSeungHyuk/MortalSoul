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

        private FieldCharacter owner;
        private BaseAttributeSet attrSet;
        private EWeaponType curWeaponType;
        private BaseWeaponAttack curAttack;


        public void InitWSC(FieldCharacter _owner, BaseAttributeSet _attrSet, EWeaponType _weaponType)
        {
            owner = _owner;
            attrSet = _attrSet;
            ChangeWeaponType(_weaponType);
        }

        public void ChangeWeaponType(EWeaponType _weaponType)
        {
            if (curAttack != null)
            {
                curAttack.OnAttackStarted -= OnAttackStartedCallback;
                curAttack.OnAttackEnded -= OnAttackEndedCallback;
            }

            curWeaponType = _weaponType;
            Type attackType = Type.GetType($"MS.Battle.{_weaponType}Attack");
            if (attackType == null)
            {
                Debug.LogError($"[WSC] 무기 공격 클래스를 찾을 수 없음: {_weaponType}");
                curAttack = null;
                return;
            }

            curAttack = Activator.CreateInstance(attackType) as BaseWeaponAttack;
            if (curAttack == null)
            {
                Debug.LogError($"[WSC] 무기 공격 인스턴스 생성 실패: {_weaponType}");
                return;
            }

            curAttack.InitBaseWeaponAttack(owner, attrSet);
            curAttack.OnAttackStarted += OnAttackStartedCallback;
            curAttack.OnAttackEnded += OnAttackEndedCallback;
        }

        public void ActivateAttack() => curAttack?.ActivateAttack();

        private void OnAttackStartedCallback() => OnAttackStarted?.Invoke();
        private void OnAttackEndedCallback() => OnAttackEnded?.Invoke();
    }
}
