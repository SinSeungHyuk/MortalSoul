using System;
using Cysharp.Threading.Tasks;
using MS.Field;
using MS.Utils;
using UnityEngine;

namespace MS.Battle
{
    public class OneHandSwordAttack : BaseWeaponAttack
    {
        private int comboIndex;
        private bool isReserveNext;

        public override void OnAttackInput()
        {
            if (isAttacking)
            {
                isReserveNext = true;
                return;
            }
            ActivateAsync().Forget();
        }

        private async UniTaskVoid ActivateAsync()
        {
            isAttacking = true;
            comboIndex = 0;
            InvokeAttackStarted();

            try
            {
                while (true)
                {
                    float timeScale = attrSet.AttackSpeed.Value / 100f;

                    if (comboIndex == 0)
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

                    int finishIdx = await UniTask.WhenAny(
                        spine.WaitForAnimEventAsync(Settings.SpineEventComboReady),
                        spine.WaitForAnimCompleteAsync()
                    );
                    bool comboReady = (finishIdx == 0);

                    if (isReserveNext)
                    {
                        isReserveNext = false;
                        comboIndex = (comboIndex + 1) % 2;
                        continue;
                    }

                    if (!comboReady) break;

                    await spine.WaitForAnimCompleteAsync();
                    break;
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                comboIndex = 0;
                isReserveNext = false;
                isAttacking = false;
                InvokeAttackEnded();
            }
        }

        private void DoHit(float _range, float _offset, float _damageMul)
        {
            float facingSign = spine.IsFacingRight ? 1f : -1f;
            Vector2 center = (Vector2)owner.transform.position + new Vector2(_offset * facingSign, 0f);
            var colliders = DebugDraw.OverlapCircleAll(center, _range, Settings.MonsterLayer);

            foreach (var col in colliders)
            {
                var target = col.GetComponent<FieldCharacter>();
                if (target == null) continue;

                bool isCritic = UnityEngine.Random.value * 100f < attrSet.CriticChance.Value;
                float damage = _damageMul * attrSet.BaseAttackPower.Value;
                if (isCritic) damage *= attrSet.CriticMultiple.Value / 100f;

                var info = new DamageInfo
                {
                    Attacker = owner,
                    Target = target,
                    AttributeType = EDamageAttributeType.None,
                    Damage = damage,
                    IsCritic = isCritic,
                    KnockbackForce = Settings.BasicAttackKnockback,
                    SourceSkill = null
                };

                target.BSC.TakeDamage(info);
            }
        }
    }
}
