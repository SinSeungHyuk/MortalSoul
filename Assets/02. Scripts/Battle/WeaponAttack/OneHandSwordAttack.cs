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
        private PlayerAttributeSet playerAttributeSet;

        public override void InitBaseWeaponAttack(FieldCharacter _owner, BaseAttributeSet _attrSet)
        {
            base.InitBaseWeaponAttack(_owner, _attrSet);
            playerAttributeSet = (PlayerAttributeSet)_attrSet;
        }

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
                    float timeScale = playerAttributeSet.AttackSpeed.Value / 100f;

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

                    int finishIdx = await UniTask.WhenAny(
                        spine.WaitForAnimEventAsync(Settings.AnimComboRdyEvent),
                        spine.WaitForAnimCompleteAsync()
                    );
                    bool isComboReady = (finishIdx == 0);

                    if (isNextCombo)
                    {
                        isNextCombo = false;
                        curComboCnt = (curComboCnt + 1) % 2;
                        continue;
                    }
                    if (!isComboReady) break;

                    await spine.WaitForAnimCompleteAsync();
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

                // TODO :: BattleUtils를 통해 데미지 계산하여 넘겨주기
                // bool isCritic = UnityEngine.Random.value * 100f < attributeSet.CriticChance.Value;
                // float damage = _damageMul * attributeSet.BaseAttackPower.Value;
                // if (isCritic) damage *= attributeSet.CriticMultiple.Value / 100f;

                // var info = new DamageInfo
                // {
                //     Attacker = owner,
                //     Target = target,
                //     AttributeType = EDamageAttributeType.None,
                //     Damage = damage,
                //     IsCritic = isCritic,
                //     KnockbackForce = Settings.BasicAttackKnockback,
                //     SourceSkill = null
                // };

                // target.BSC.TakeDamage(info);
            }
        }
    }
}
