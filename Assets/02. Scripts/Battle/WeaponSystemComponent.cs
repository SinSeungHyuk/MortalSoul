using System;
using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Utils;
using UnityEngine;

namespace MS.Battle
{
    public class WeaponSystemComponent
    {
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;

        public EWeaponType CurWeaponType => curWeaponType;

        private FieldCharacter owner;
        private EWeaponType curWeaponType;
        private WeaponSettingData curWeaponData;

        private int comboIndex;
        private bool isReserveNextCombo;
        private bool isAttacking;


        public void InitWSC(FieldCharacter _owner)
        {
            owner = _owner;
        }

        public void ChangeWeaponType(EWeaponType _weaponType)
        {
            if (!Main.Instance.DataManager.SettingData.WeaponSettingDict.TryGetValue(_weaponType, out var data))
            {
                Debug.LogError($"[WSC] WeaponSettingData를 찾을 수 없음: {_weaponType}");
                return;
            }

            curWeaponType = _weaponType;
            curWeaponData = data;
            comboIndex = 0;
        }

        public void ActivateAttack()
        {
            if (curWeaponData == null || curWeaponData.ComboList == null || curWeaponData.ComboList.Count == 0)
            {
                Debug.LogError($"[WSC] 장착 무기 데이터가 없거나 콤보 리스트가 비어있음");
                return;
            }

            if (isAttacking) // 공격중일때 공격을 시도하면 콤보공격 예약
            {
                isReserveNextCombo = true;
                return;
            }

            ActivateAttackAsync().Forget();
        }

        private async UniTaskVoid ActivateAttackAsync()
        {
            isAttacking = true;
            comboIndex = 0;
            OnAttackStarted?.Invoke();
            var spine = owner.SpineController;

            try
            {
                while (true)
                {
                    var comboData = curWeaponData.ComboList[comboIndex];
                    spine.PlayAnimation(comboData.AnimKey, false);

                    // 1) 히트 타이밍 대기 → 공격판정
                    await spine.WaitForAnimEventAsync(Settings.SpineEventAttack);
                    DoAttack(comboData);

                    // 2) 애니메이션 완료or콤보준비 중 먼저 들어오는 쪽
                    int finishIdx = await UniTask.WhenAny(
                        spine.WaitForAnimEventAsync(Settings.SpineEventComboReady),
                        spine.WaitForAnimCompleteAsync()
                    );
                    bool isComboRdy = (finishIdx == 0);

                    if (isReserveNextCombo) // 다음 콤보 예약이 들어왔다면
                    {
                        isReserveNextCombo = false;
                        comboIndex = (comboIndex + 1) % curWeaponData.ComboList.Count;
                        continue;
                    }

                    if (!isComboRdy)
                        break;

                    await spine.WaitForAnimCompleteAsync();
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                // 외부 Play 호출로 SpineController가 캔슬한 경우 (안전망) — 정상 종료
                Debug.Log("WSC::ActivateAttackAsync - 외부에서 공격 취소");
            }
            finally
            {
                comboIndex = 0;
                isReserveNextCombo = false;
                isAttacking = false;
                OnAttackEnded?.Invoke();
            }
        }

        private void DoAttack(AttackComboData _combo)
        {
            // TODO: OverlapBox 히트 판정 + BSC.TakeDamage 호출
            Debug.Log("Attack!!");
        }
    }
}
