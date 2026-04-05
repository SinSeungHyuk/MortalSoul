using MS.Field;
using UnityEngine;

namespace MS.Battle
{
    public class TestOneHandAttack : BaseSkill
    {
        public override void ActivateSkill()
        {
            Debug.Log($"[TestOneHandAttack] 스킬 사용! ATK: {attributeSet.AttackPower.Value}");

            if (owner is PlayerCharacter player && player.SpineComponent != null)
            {
                player.SpineComponent.OnAttackOneHand();
            }
        }
    }
}
