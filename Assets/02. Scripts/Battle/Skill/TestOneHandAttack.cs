using Cysharp.Threading.Tasks;
using MS.Field;
using System.Threading;
using UnityEngine;

namespace MS.Battle
{
    public class TestOneHandAttack : BaseSkill
    {
        public override async UniTask ActivateSkillAsync(CancellationToken _token)
        {
            Debug.Log($"[TestOneHandAttack] 스킬 사용! ATK: {attributeSet.BaseAttackPower.Value}");

            // if (owner is PlayerCharacter player && player.SpineComponent != null)
            // {
            //     player.SpineComponent.OnAttackOneHand();
            // }

            await UniTask.CompletedTask;
        }
    }
}
