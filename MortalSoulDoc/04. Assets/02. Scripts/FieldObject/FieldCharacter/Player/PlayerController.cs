using MS.Skill;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using MS.Utils;

namespace MS.Field
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerCharacter player;
        private CharacterController characterController;
        private Vector2 moveInput;
        private float verticalVelocity;

        public Vector3 MoveDir { get; private set; }
        public CharacterController CC => characterController;


        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            player = GetComponent<PlayerCharacter>();
        }

        private void Update()
        {
            if (player.SSC.AttributeSet == null || player.IsMovementLocked)
            {
                moveInput = Vector2.zero;
            }

            if (Time.timeScale > 0)
            {
                HandleGravity();
                HandleMovement();
            }
        }

        private void HandleGravity()
        {
            if (characterController.isGrounded)
                verticalVelocity = -2f;
            else
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        private void HandleMovement()
        {
            MoveDir = new Vector3(moveInput.x, 0f, moveInput.y);
            float inputMagnitude = MoveDir.sqrMagnitude;

            Vector3 frameMovement = Vector3.up * verticalVelocity * Time.deltaTime;

            if (inputMagnitude > 0.01f)
            {
                float moveSpeed = player.SSC.AttributeSet.GetStatValueByType(EStatType.MoveSpeed);
                Vector3 targetPosition = transform.position + (MoveDir * moveSpeed * Time.deltaTime);

                if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    Vector3 moveOffset = hit.position - transform.position;
                    moveOffset.y = 0f;
                    frameMovement += moveOffset;
                }

                transform.rotation = Quaternion.LookRotation(MoveDir);
            }

            characterController.Move(frameMovement);
            player.Animator.SetFloat(Settings.AnimHashSpeed, inputMagnitude);
        }

        public void OnMove(InputValue inputValue)
        {
            moveInput = inputValue.Get<Vector2>().normalized;
        }
    }
}