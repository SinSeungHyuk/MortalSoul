using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MS.Battle
{
    public class TestOneHandAttack : BaseSkill
    {
        public override UniTask ActivateSkillAsync(CancellationToken _token)
        {
            Debug.Log($"[TestOneHandAttack] 스킬 사용! ATK: {attributeSet.BaseAttackPower.Value}");
            return UniTask.CompletedTask;
        }
    }
}
