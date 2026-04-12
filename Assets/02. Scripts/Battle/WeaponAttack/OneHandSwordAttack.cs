using System;
using Cysharp.Threading.Tasks;
using MS.Field;
using MS.Utils;
using UnityEngine;

namespace MS.Battle
{
    public class OneHandSwordAttack : BaseWeaponAttack
    {
        private int curComboCnt;
        private bool isNextCombo;

        public override void ActivateAttack()
        {
            if (isAttacking)
            {
                isNextCombo = true;
                return;
            }
            ActivateAsync().Forget();
        }

        private async UniTaskVoid ActivateAsync()
        {
            isAttacking = true;
            curComboCnt = 0;
            InvokeAttackStarted();

            try
            {
                while (true)
                {
                    float timeScale = attributeSet.AttackSpeed.Value / 100f;

                    if (curComboCnt == 0)
                    {
                        spine.PlayAnimation("Attack_OneHand1", false, timeScale);
                        await spine.WaitForAnimEventAsync("attack1");
                        DoHit(_range: 1.5f, _offset: 1.2f, _damageMul: 1.0f);
                    }
                    else
                    {
                        spine.PlayAnimation("Attack_OneHand2", false, timeScale);
                        await spine.WaitForAnimEventAsync("attack1");
                        DoHit(_range: 1.8f, _offset: 1.4f, _damageMul: 1.5f);
                    }

                    // 버그방지: combo_ready로 빠져나온 후에도 같은 completeTask를 재사용
                    var readyTask = spine.WaitForAnimEventAsync(Settings.AnimComboRdyEvent);
                    var completeTask = spine.WaitForAnimCompleteAsync();

                    int finishIdx = await UniTask.WhenAny(readyTask, completeTask);
                    bool isComboReady = (finishIdx == 0);

                    if (isNextCombo)
                    {
                        isNextCombo = false;
                        curComboCnt = (curComboCnt + 1) % 2;
                        continue;
                    }
                    if (!isComboReady) break;

                    await completeTask;
                    break;
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                curComboCnt = 0;
                isNextCombo = false;
                isAttacking = false;
                InvokeAttackEnded();
            }
        }

        private void DoHit(float _range, float _offset, float _damageMul)
        {
            float dir = spine.IsScaleXRight ? 1f : -1f;
            Vector2 center = (Vector2)owner.Position + new Vector2(_offset * dir, 0f);
            var hits = DebugDraw.OverlapCircleAll(center, _range, Settings.MonsterLayer);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<FieldCharacter>(out var target) == false)
                    continue;

                var info = new DamageInfo
                {
                    Attacker = owner,
                    Target = target,
                    AttributeType = EDamageAttributeType.None,
                    Damage = attributeSet.BaseAttackPower.Value * _damageMul,
                    KnockbackForce = Settings.BasicAttackKnockback,
                    SourceSkill = null
                };

                target.BSC.TakeDamage(info);
            }
        }
    }
}
