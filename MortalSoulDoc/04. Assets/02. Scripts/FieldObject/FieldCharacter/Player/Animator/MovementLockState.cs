using MS.Field;
using UnityEngine;

public class MovementLockState : StateMachineBehaviour
{
    PlayerCharacter player;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = animator.GetComponent<PlayerCharacter>();

        if (player != null)
        {
            player.SetMovementLock(true);
        }
    }
}
