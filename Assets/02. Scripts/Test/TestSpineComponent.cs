using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using System;

/// <summary>
/// SpineController 프로토타입.
/// UI 버튼과 연결하여 Spine 애니메이션 재생을 테스트한다.
/// 추후 FieldCharacter용 SpineController로 발전시킬 예정.
/// </summary>
public class TestSpineComponent : MonoBehaviour
{
    [Header("Spine")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [Header("Animation Settings")]
    [SerializeField] private float defaultMix = 0.2f; // 애니메이션 전환 블렌딩 시간

    // 외부 접근용 프로퍼티/이벤트
    public bool IsActioning => isActioning;
    public bool IsDead => isDead;
    public event Action OnActionCompleted;

    // 현재 상태
    private bool isMoving;
    private bool isActioning; // 공격/스킬 등 액션 재생 중
    private bool isDead;
    private bool isCasting; // 캐스팅 중
    private bool facingRight = true;

    // 트랙 인덱스
    private const int MainTrack = 0;

    private void Awake()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    private void Start()
    {
        // 기본 스킨 조합 적용
        ApplyDefaultSkin();

        // 기본 믹스 타임 설정
        skeletonAnimation.AnimationState.Data.DefaultMix = defaultMix;

        // 액션 애니메이션 완료 콜백
        skeletonAnimation.AnimationState.Complete += OnAnimationComplete;

        // 시작 시 Idle 재생
        PlayIdle();
    }

    private void ApplyDefaultSkin()
    {
        var skeleton = skeletonAnimation.Skeleton;
        var combinedSkin = new Skin("default");

        combinedSkin.AddSkin(skeleton.Data.FindSkin("BODY/base"));
        combinedSkin.AddSkin(skeleton.Data.FindSkin("HEAD/headA"));
        combinedSkin.AddSkin(skeleton.Data.FindSkin("HAIR/hairA_a"));
        combinedSkin.AddSkin(skeleton.Data.FindSkin("RIGHTHAND/Sword_TwoHand_Common1"));

        skeleton.SetSkin(combinedSkin);
        skeleton.SetSlotsToSetupPose();
    }

    private void OnDestroy()
    {
        if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
            skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
    }

    // ===== 액션 완료 콜백 =====

    private void OnAnimationComplete(TrackEntry _trackEntry)
    {
        if (_trackEntry.TrackIndex != MainTrack) return;
        if (!isActioning) return;

        // 액션 애니메이션이 끝나면 상태 복귀
        isActioning = false;
        OnActionCompleted?.Invoke();

        if (isDead) return;

        if (isMoving)
            PlayAnimation("Run1", true);
        else
            PlayIdle();
    }

    // ===== UI 버튼에서 호출할 Public 메서드 =====

    /// <summary> Idle 상태로 전환 (이동 멈춤) </summary>
    public void OnStopMove()
    {
        if (isDead) return;

        isMoving = false;
        if (!isActioning)
            PlayIdle();
    }

    /// <summary> 이동 시작 (Run 애니메이션) </summary>
    public void OnStartMove()
    {
        if (isDead) return;

        isMoving = true;
        if (!isActioning)
            PlayAnimation("Run1", true);
    }

    /// <summary> 좌우 방향 전환 </summary>
    public void OnFlip()
    {
        if (isDead) return;

        facingRight = !facingRight;
        skeletonAnimation.Skeleton.ScaleX = facingRight ? 1f : -1f;
    }

    /// <summary> 왼쪽 방향 </summary>
    public void OnMoveLeft()
    {
        if (isDead) return;

        facingRight = false;
        skeletonAnimation.Skeleton.ScaleX = -1f;
        OnStartMove();
    }

    /// <summary> 오른쪽 방향 </summary>
    public void OnMoveRight()
    {
        if (isDead) return;

        facingRight = true;
        skeletonAnimation.Skeleton.ScaleX = 1f;
        OnStartMove();
    }

    /// <summary> 한손검 기본공격 </summary>
    public void OnAttackOneHand()
    {
        PlayAction("Attack_OneHand1");
    }

    /// <summary> 대검 기본공격 </summary>
    public void OnAttackTwoHand()
    {
        PlayAction("Attack_TwoHand1");
    }

    /// <summary> 이도류(단도) 기본공격 </summary>
    public void OnAttackDualHand()
    {
        PlayAction("Attack_DualHand1");
    }

    /// <summary> 활 공격 </summary>
    public void OnShoot()
    {
        PlayAction("Shoot1");
    }

    /// <summary> 스킬1 (Cast) </summary>
    public void OnCast()
    {
        PlayAction("Cast1");
    }

    /// <summary> 스킬2 (Spell) </summary>
    public void OnSpell()
    {
        PlayAction("Spell1");
    }

    /// <summary> 가드 </summary>
    public void OnGuard()
    {
        PlayAction("Guard1");
    }

    /// <summary> 피격 </summary>
    public void OnDamage()
    {
        PlayAction("Damage1");
    }

    /// <summary> 사망 </summary>
    public void OnDie()
    {
        if (isDead) return;

        isDead = true;
        isActioning = false;
        isMoving = false;
        PlayAnimation("Die", false);
    }

    /// <summary> 부활 (테스트용 리셋) </summary>
    public void OnRevive()
    {
        isDead = false;
        isActioning = false;
        isMoving = false;
        PlayIdle();
    }

    /// <summary> 점프 (Wait4 재생) </summary>
    public void OnJump()
    {
        if (isDead) return;

        isActioning = false;
        PlayAnimation("Wait4", true);
    }

    /// <summary> 대시 (Run3 재생) </summary>
    public void OnDash()
    {
        if (isDead) return;

        isActioning = false;
        PlayAnimation("Run3", true);
    }

    // ===== MoveComponent 연동 메서드 =====

    /// <summary> 방향 전환만 수행 (이동 애니메이션 변경 없이) </summary>
    public void SetFacing(bool _right)
    {
        if (isDead) return;

        facingRight = _right;
        skeletonAnimation.Skeleton.ScaleX = _right ? -1f : 1f;
    }

    /// <summary> 캐스팅 시작 (Cast1 루프 재생) </summary>
    public void OnStartCast()
    {
        if (isDead) return;

        isCasting = true;
        isActioning = false;
        PlayAnimation("Cast1", true);
    }

    /// <summary> 캐스팅 취소 → 현재 이동 상태에 따라 복귀 </summary>
    public void OnCancelCast()
    {
        if (!isCasting) return;

        isCasting = false;

        if (isMoving)
            PlayAnimation("Run1", true);
        else
            PlayIdle();
    }

    // ===== 내부 =====

    private void PlayAction(string _animationName)
    {
        if (isDead) return;

        isActioning = true;
        PlayAnimation(_animationName, false);
    }

    private void PlayIdle()
    {
        PlayAnimation("Wait1", true);
    }

    private void PlayAnimation(string _animationName, bool _loop)
    {
        skeletonAnimation.AnimationState.SetAnimation(MainTrack, _animationName, _loop);
    }
}
