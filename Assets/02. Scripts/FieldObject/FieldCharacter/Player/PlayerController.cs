using MS.Battle;
using MS.Core.StateMachine;
using MS.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MS.Field
{
    public class PlayerController : MonoBehaviour
    {
        public enum EMoveState { Idle, Move, Jump, Dash, Attack }

        private Rigidbody2D rb;
        private BoxCollider2D col;
        private SpineController spineController;
        private PlayerCharacter playerCharacter;
        private MSStateMachine<PlayerController> stateMachine;
        private WeaponSystemComponent wsc;

        private Vector2 moveInput;
        private bool isGrounded;
        private bool wasGrounded;
        private bool facingRight = true;

        private bool jumpRequested;
        private bool dashRequested;

        private float dashTimer;
        private float dashCooldownTimer;
        private float dashFreezeTimer;

        // 현재 속도
        private float curVelocityX;


        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            spineController = GetComponent<SpineController>();
            playerCharacter = GetComponent<PlayerCharacter>();
        }

        private void Start()
        {
            rb.gravityScale = Settings.GravityScale;

            InitStateMachine();
        }

        public void InitController(WeaponSystemComponent _wsc)
        {
            wsc = _wsc;
            wsc.OnAttackStarted += OnAttackStartedCallback;
            wsc.OnAttackEnded += OnAttackEndedCallback;
        }

        private void OnDestroy()
        {
            if (wsc != null)
            {
                wsc.OnAttackStarted -= OnAttackStartedCallback;
                wsc.OnAttackEnded -= OnAttackEndedCallback;
            }
        }

        private void OnAttackStartedCallback()
        {
            stateMachine.TransitState((int)EMoveState.Attack);
        }

        private void OnAttackEndedCallback()
        {
            if (!isGrounded)
                stateMachine.TransitState((int)EMoveState.Jump);
            else if (Mathf.Abs(moveInput.x) > 0.1f)
                stateMachine.TransitState((int)EMoveState.Move);
            else
                stateMachine.TransitState((int)EMoveState.Idle);
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
            stateMachine.RegisterState((int)EMoveState.Attack, OnAttackEnter, null, null);

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
                spineController.SetScaleX(true);
            }
            else if (moveInput.x < -0.01f && facingRight)
            {
                facingRight = false;
                spineController.SetScaleX(false);
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

        private void OnIdleEnter(int _prevState, object[] _param)
        {
            curVelocityX = 0f;
            spineController.PlayIdle();
        }

        private void OnIdleUpdate(float _dt)
        {
            if (CheckCurInput()) return;

            if (Mathf.Abs(moveInput.x) > 0.1f)
                stateMachine.TransitState((int)EMoveState.Move);
        }

        #endregion

        #region Move State

        private void OnMoveEnter(int _prevState, object[] _param)
        {
            spineController.PlayMove();
        }

        private void OnMoveUpdate(float _dt)
        {
            UpdateFacing();
            curVelocityX = moveInput.x * Settings.MoveSpeed;

            if (CheckCurInput()) return;

            if (Mathf.Abs(moveInput.x) <= 0.1f)
                stateMachine.TransitState((int)EMoveState.Idle);
        }

        #endregion

        #region Jump State

        private void OnJumpEnter(int _prevState, object[] _param)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Settings.JumpForce);
            spineController.PlayJump();
        }

        private void OnJumpUpdate(float _dt)
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

        private void OnDashEnter(int _prevState, object[] _param)
        {
            dashTimer = Settings.DashDuration;
            dashFreezeTimer = 0f;
            dashCooldownTimer = Settings.DashCooldown;

            float direction = facingRight ? 1f : -1f;
            curVelocityX = direction * Settings.DashSpeed;

            // 대시 중 중력 무시
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);

            spineController.PlayDash();
        }

        private void OnDashUpdate(float _dt)
        {
            // 대시 중 모든 입력 소비 (무시)
            jumpRequested = false;
            dashRequested = false;

            // 1) 이동 단계
            if (dashTimer > 0f)
            {
                dashTimer -= _dt;
                if (dashTimer <= 0f)
                {
                    // 이동 끝 → 프리즈 진입. 수평/수직 모두 정지, 중력은 계속 0 유지(공중도 멈춤)
                    curVelocityX = 0f;
                    rb.linearVelocity = Vector2.zero;
                    dashFreezeTimer = Settings.DashEndFreezeDuration;
                }
                return;
            }

            // 2) 프리즈 단계
            if (dashFreezeTimer > 0f)
            {
                dashFreezeTimer -= _dt;
                curVelocityX = 0f;
                rb.linearVelocity = Vector2.zero;
                if (dashFreezeTimer > 0f) return;
            }

            // 3) 종료 → 다음 상태
            if (!isGrounded)
                stateMachine.TransitState((int)EMoveState.Jump);
            else if (Mathf.Abs(moveInput.x) > 0.1f)
                stateMachine.TransitState((int)EMoveState.Move);
            else
                stateMachine.TransitState((int)EMoveState.Idle);
        }

        private void OnDashExit(int _nextState)
        {
            // 대시 종료 시 중력 복원
            rb.gravityScale = Settings.GravityScale;
            dashFreezeTimer = 0f;
        }

        #endregion

        #region Attack State

        private void OnAttackEnter(int _prevState, object[] _param)
        {
            curVelocityX = 0f;
            // 애니메이션은 WSC가 재생함
        }

        #endregion

        public void OnMove(InputValue _value)
        {
            moveInput = _value.Get<Vector2>();
            moveInput.y = 0f;
            moveInput.x = Mathf.Clamp(moveInput.x, -1f, 1f);
        }

        public void OnAttack(InputValue _value)
        {
            if (!_value.isPressed) return;
            if (wsc == null) return;
            if (!stateMachine.IsCurState((int)EMoveState.Idle) &&
                !stateMachine.IsCurState((int)EMoveState.Move) &&
                !stateMachine.IsCurState((int)EMoveState.Attack)) return;

            wsc.ActivateAttack();
        }
    }
}
