using MS.Core.StateMachine;
using MS.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MS.Field
{
    /// <summary>
    /// 플레이어 이동/점프/대시 컨트롤러.
    /// Dynamic Rigidbody2D + MSStateMachine 기반. 중력은 물리 엔진이 처리하고
    /// gravityScale만 상황별로 조정(하강 가속, 대시 중 0).
    /// 공격/캐스트 상태는 BSC 통합 시 추가 예정.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public enum EMoveState { Idle, Move, Jump, Dash }

        private Rigidbody2D rb;
        private BoxCollider2D col;
        private SpineController spineController;
        private MSStateMachine<PlayerController> stateMachine;

        private Vector2 moveInput;
        private bool isGrounded;
        private bool wasGrounded;
        private bool facingRight = true;

        private bool jumpRequested;
        private bool dashRequested;

        private float dashTimer;
        private float dashCooldownTimer;

        // 현재 속도
        private float curVelocityX;


        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            spineController = GetComponent<SpineController>();
        }

        private void Start()
        {
            rb.gravityScale = Settings.GravityScale;

            InitStateMachine();
        }

        private void Update()
        {
            UpdateTimers();
            stateMachine.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            UpdateGroundCheck();
            UpdateFallGravity();
            ApplyVelocity();
        }

        private void InitStateMachine()
        {
            stateMachine = new MSStateMachine<PlayerController>(this);

            stateMachine.RegisterState((int)EMoveState.Idle, OnIdleEnter, OnIdleUpdate, null);
            stateMachine.RegisterState((int)EMoveState.Move, OnMoveEnter, OnMoveUpdate, null);
            stateMachine.RegisterState((int)EMoveState.Jump, OnJumpEnter, OnJumpUpdate, null);
            stateMachine.RegisterState((int)EMoveState.Dash, OnDashEnter, OnDashUpdate, OnDashExit);

            stateMachine.TransitState((int)EMoveState.Idle);
        }


        private void UpdateGroundCheck()
        {
            wasGrounded = isGrounded;

            if (col == null) return;

            Vector2 origin = (Vector2)transform.position + col.offset + Vector2.down * (col.size.y * 0.5f);
            RaycastHit2D hit = Physics2D.BoxCast(origin, Settings.GroundCheckSize, 0f, Vector2.down,
                Settings.GroundCheckDistance, Settings.GroundLayer);
            isGrounded = hit.collider != null;
        }

        private void UpdateFallGravity()
        {
            if (rb.linearVelocityY < 0f)
                rb.gravityScale = Settings.GravityScale * Settings.FallMultiplier;
            else
                rb.gravityScale = Settings.GravityScale;
        }

        private void ApplyVelocity()
        {
            rb.linearVelocity = new Vector2(curVelocityX, rb.linearVelocityY);
        }

        private void UpdateTimers()
        {
            if (dashCooldownTimer > 0f)
                dashCooldownTimer -= Time.deltaTime;
        }

        private void UpdateFacing()
        {
            if (moveInput.x > 0.01f && !facingRight)
            {
                facingRight = true;
                spineController.SetFacing(true);
            }
            else if (moveInput.x < -0.01f && facingRight)
            {
                facingRight = false;
                spineController.SetFacing(false);
            }
        }

        // 입력 체크
        private bool CheckCurInput()
        {
            if (dashRequested && dashCooldownTimer <= 0f)
            {
                dashRequested = false;
                stateMachine.TransitState((int)EMoveState.Dash);
                return true;
            }

            if (jumpRequested && isGrounded)
            {
                jumpRequested = false;
                stateMachine.TransitState((int)EMoveState.Jump);
                return true;
            }

            return false;
        }

        #region Idle State

        private void OnIdleEnter(int prevState, object[] param)
        {
            curVelocityX = 0f;
            spineController.PlayIdle();
        }

        private void OnIdleUpdate(float dt)
        {
            if (CheckCurInput()) return;

            if (Mathf.Abs(moveInput.x) > 0.1f)
                stateMachine.TransitState((int)EMoveState.Move);
        }

        #endregion

        #region Move State

        private void OnMoveEnter(int prevState, object[] param)
        {
            spineController.PlayMove();
        }

        private void OnMoveUpdate(float dt)
        {
            UpdateFacing();
            curVelocityX = moveInput.x * Settings.MoveSpeed;

            if (CheckCurInput()) return;

            if (Mathf.Abs(moveInput.x) <= 0.1f)
                stateMachine.TransitState((int)EMoveState.Idle);
        }

        #endregion

        #region Jump State

        private void OnJumpEnter(int prevState, object[] param)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Settings.JumpForce);
            spineController.PlayJump();
        }

        private void OnJumpUpdate(float dt)
        {
            UpdateFacing();
            curVelocityX = moveInput.x * Settings.MoveSpeed * Settings.AirControlMultiplier;

            // 점프 중 대시만 허용
            if (dashRequested && dashCooldownTimer <= 0f)
            {
                dashRequested = false;
                stateMachine.TransitState((int)EMoveState.Dash);
                return;
            }

            // 착지 판정
            if (isGrounded && !wasGrounded && rb.linearVelocityY <= 0f)
            {
                if (Mathf.Abs(moveInput.x) > 0.1f)
                    stateMachine.TransitState((int)EMoveState.Move);
                else
                    stateMachine.TransitState((int)EMoveState.Idle);
            }

            // 나머지 입력 소비 (무시)
            jumpRequested = false;
        }

        #endregion

        #region Dash State

        private void OnDashEnter(int prevState, object[] param)
        {
            dashTimer = Settings.DashDuration;
            dashCooldownTimer = Settings.DashCooldown;

            float direction = facingRight ? 1f : -1f;
            curVelocityX = direction * Settings.DashSpeed;

            // 대시 중 중력 무시
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);

            spineController.PlayDash();
        }

        private void OnDashUpdate(float dt)
        {
            dashTimer -= dt;

            // 대시 중 모든 입력 소비 (무시)
            jumpRequested = false;
            dashRequested = false;

            if (dashTimer <= 0f)
            {
                curVelocityX = 0f;

                // 공중 대시 종료 → 수직 낙하 (수평 속도 제거)
                if (!isGrounded)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocityY);
                    stateMachine.TransitState((int)EMoveState.Jump);
                }
                else if (Mathf.Abs(moveInput.x) > 0.1f)
                    stateMachine.TransitState((int)EMoveState.Move);
                else
                    stateMachine.TransitState((int)EMoveState.Idle);
            }
        }

        private void OnDashExit(int nextState)
        {
            // 대시 종료 시 중력 복원
            rb.gravityScale = Settings.GravityScale;
        }

        #endregion

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
            moveInput.y = 0f;
            moveInput.x = Mathf.Clamp(moveInput.x, -1f, 1f);
        }
    }
}
