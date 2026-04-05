using UnityEngine;
using UnityEngine.InputSystem;
using MS.Core.StateMachine;
using MS.Utils;

/// <summary>
/// 2D 횡스크롤 캐릭터 이동 프로토타입.
/// Dynamic Rigidbody2D + MSStateMachine + TestSpineComponent 연동.
/// 추후 PlayerController + CharacterMotor2D로 분리 예정.
/// </summary>
public class TestMoveComponent : MonoBehaviour
{
    public enum EMoveState { Idle, Move, Jump, Dash, Attack, Cast }

    [Header("References")]
    [SerializeField] private TestSpineComponent spineComponent;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.4f, 0.05f);
    [SerializeField] private float groundCheckDistance = 0.1f;

    // 컴포넌트
    private Rigidbody2D rb;
    private BoxCollider2D col;

    // 상태 머신
    private MSStateMachine<TestMoveComponent> stateMachine;

    // 런타임 상태
    private Vector2 moveInput;
    private bool isGrounded;
    private bool wasGrounded;
    private bool facingRight = true;

    // 입력 요청 플래그 (버튼 입력은 프레임 단위로 소비)
    private bool jumpRequested;
    private bool attackRequested;
    private bool dashRequested;
    private bool castRequested;

    // 대시 타이머
    private float dashTimer;
    private float dashCooldownTimer;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        rb.gravityScale = Settings.GravityScale;

        InitStateMachine();

        if (spineComponent != null)
            spineComponent.OnActionCompleted += OnAttackComplete;
    }

    private void OnDestroy()
    {
        if (spineComponent != null)
            spineComponent.OnActionCompleted -= OnAttackComplete;
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

    // ===== 상태 머신 초기화 =====

    private void InitStateMachine()
    {
        stateMachine = new MSStateMachine<TestMoveComponent>(this);

        stateMachine.RegisterState((int)EMoveState.Idle, OnIdleEnter, OnIdleUpdate, null);
        stateMachine.RegisterState((int)EMoveState.Move, OnMoveEnter, OnMoveUpdate, null);
        stateMachine.RegisterState((int)EMoveState.Jump, OnJumpEnter, OnJumpUpdate, null);
        stateMachine.RegisterState((int)EMoveState.Dash, OnDashEnter, OnDashUpdate, OnDashExit);
        stateMachine.RegisterState((int)EMoveState.Attack, OnAttackEnter, OnAttackUpdate, null);
        stateMachine.RegisterState((int)EMoveState.Cast, OnCastEnter, OnCastUpdate, null);

        stateMachine.TransitState((int)EMoveState.Idle);
    }

    // ===== 물리 =====

    private void UpdateGroundCheck()
    {
        wasGrounded = isGrounded;

        if (col == null) return;

        Vector2 origin = (Vector2)transform.position + col.offset + Vector2.down * (col.size.y * 0.5f);
        RaycastHit2D hit = Physics2D.BoxCast(origin, groundCheckSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    /// <summary> 하강 시 추가 중력으로 스냅 있는 점프감 구현 </summary>
    private void UpdateFallGravity()
    {
        if (rb.linearVelocityY < 0f)
            rb.gravityScale = Settings.GravityScale * Settings.FallMultiplier;
        else
            rb.gravityScale = Settings.GravityScale;
    }

    /// <summary> 현재 수평 속도를 Rigidbody에 반영 (Y축은 물리 엔진이 처리) </summary>
    private void ApplyVelocity()
    {
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocityY);
    }

    // 상태별로 설정하는 목표 수평 속도
    private float targetVelocityX;

    private void UpdateTimers()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    // ===== 방향 전환 =====

    private void UpdateFacing()
    {
        if (moveInput.x > 0.01f && !facingRight)
        {
            facingRight = true;
            spineComponent.SetFacing(true);
        }
        else if (moveInput.x < -0.01f && facingRight)
        {
            facingRight = false;
            spineComponent.SetFacing(false);
        }
    }

    // ===== 공통 입력 체크 (Idle/Move/Cast에서 공유) =====

    private bool TryHandleCommonInput()
    {
        if (dashRequested && dashCooldownTimer <= 0f)
        {
            dashRequested = false;
            stateMachine.TransitState((int)EMoveState.Dash);
            return true;
        }

        if (attackRequested)
        {
            attackRequested = false;
            stateMachine.TransitState((int)EMoveState.Attack);
            return true;
        }

        if (jumpRequested && isGrounded)
        {
            jumpRequested = false;
            stateMachine.TransitState((int)EMoveState.Jump);
            return true;
        }

        if (castRequested)
        {
            castRequested = false;
            stateMachine.TransitState((int)EMoveState.Cast);
            return true;
        }

        return false;
    }

    // ===== Idle 상태 =====

    private void OnIdleEnter(int prevState, object[] param)
    {
        targetVelocityX = 0f;
        spineComponent.OnStopMove();
    }

    private void OnIdleUpdate(float dt)
    {
        if (TryHandleCommonInput()) return;

        if (Mathf.Abs(moveInput.x) > 0.1f)
            stateMachine.TransitState((int)EMoveState.Move);
    }

    // ===== Move 상태 =====

    private void OnMoveEnter(int prevState, object[] param)
    {
        spineComponent.OnStartMove();
    }

    private void OnMoveUpdate(float dt)
    {
        UpdateFacing();
        targetVelocityX = moveInput.x * Settings.MoveSpeed;

        if (TryHandleCommonInput()) return;

        if (Mathf.Abs(moveInput.x) <= 0.1f)
            stateMachine.TransitState((int)EMoveState.Idle);
    }

    // ===== Jump 상태 =====

    private void OnJumpEnter(int prevState, object[] param)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocityX, Settings.JumpForce);
        spineComponent.OnJump();
    }

    private void OnJumpUpdate(float dt)
    {
        UpdateFacing();
        targetVelocityX = moveInput.x * Settings.MoveSpeed * Settings.AirControlMultiplier;

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
        attackRequested = false;
        castRequested = false;
    }

    // ===== Dash 상태 =====

    private void OnDashEnter(int prevState, object[] param)
    {
        dashTimer = Settings.DashDuration;
        dashCooldownTimer = Settings.DashCooldown;

        float direction = facingRight ? 1f : -1f;
        targetVelocityX = direction * Settings.DashSpeed;

        // 대시 중 중력 무시
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);

        spineComponent.OnDash();
    }

    private void OnDashUpdate(float dt)
    {
        dashTimer -= dt;

        // 대시 중 모든 입력 소비 (무시)
        jumpRequested = false;
        attackRequested = false;
        dashRequested = false;
        castRequested = false;

        if (dashTimer <= 0f)
        {
            targetVelocityX = 0f;

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

    // ===== Attack 상태 =====

    private bool attackComplete;

    private void OnAttackEnter(int prevState, object[] param)
    {
        attackComplete = false;
        targetVelocityX = 0f;
        spineComponent.OnAttackOneHand(); // 테스트용: 한손검 공격
    }

    private void OnAttackUpdate(float dt)
    {
        // 공격 중 모든 입력 소비 (무시)
        jumpRequested = false;
        attackRequested = false;
        dashRequested = false;
        castRequested = false;

        if (attackComplete)
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
                stateMachine.TransitState((int)EMoveState.Move);
            else
                stateMachine.TransitState((int)EMoveState.Idle);
        }
    }

    private void OnAttackComplete()
    {
        if (stateMachine.GetCurrentStateId() == (int)EMoveState.Attack)
            attackComplete = true;
    }

    // ===== Cast 상태 =====

    private void OnCastEnter(int prevState, object[] param)
    {
        targetVelocityX = 0f;
        spineComponent.OnStartCast();
    }

    private void OnCastUpdate(float dt)
    {
        bool anyInput = dashRequested || attackRequested || jumpRequested
                        || Mathf.Abs(moveInput.x) > 0.1f;

        if (anyInput)
        {
            spineComponent.OnCancelCast();

            if (dashRequested && dashCooldownTimer <= 0f)
            {
                dashRequested = false;
                stateMachine.TransitState((int)EMoveState.Dash);
            }
            else if (attackRequested)
            {
                attackRequested = false;
                stateMachine.TransitState((int)EMoveState.Attack);
            }
            else if (jumpRequested && isGrounded)
            {
                jumpRequested = false;
                stateMachine.TransitState((int)EMoveState.Jump);
            }
            else if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                stateMachine.TransitState((int)EMoveState.Move);
            }
            else
            {
                stateMachine.TransitState((int)EMoveState.Idle);
            }
        }
    }

    // ===== Input System 콜백 (PlayerInput SendMessages) =====

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        moveInput.y = 0f;
        moveInput.x = Mathf.Clamp(moveInput.x, -1f, 1f);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpRequested = true;
    }

    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
            dashRequested = true;
    }
}
