using System;
using MS.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace MS.Field
{
    /// <summary>
    /// Player/Monster 공용 Spine 애니메이션 컨트롤러.
    /// 이동 상태 루프 재생, 단발 액션 재생(+완료 콜백), Spine 이벤트의 게임 이벤트 변환을 담당한다.
    /// </summary>
    [RequireComponent(typeof(SkeletonAnimation))]
    public class SpineController : MonoBehaviour
    {
        public event Action OnAttackEvent;
        public event Action OnComboReadyEvent;
        public event Action OnActionCompleted;

        private const int MainTrack = 0;
        private const string EventNameAttack = "attack";
        private const string EventNameComboReady = "combo_ready";

        private SkeletonAnimation skeletonAnimation;
        private bool isActioning;
        private Action pendingOnceCallback;

        private void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        private void Start()
        {
            SetCombinedSkin();

            var state = skeletonAnimation.AnimationState;
            state.Data.DefaultMix = 0.2f;
            state.Event += OnSpineEvent;
            state.Complete += OnSpineComplete;

            PlayIdle();
        }

        private void OnDestroy()
        {
            if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
            {
                skeletonAnimation.AnimationState.Event -= OnSpineEvent;
                skeletonAnimation.AnimationState.Complete -= OnSpineComplete;
            }
        }

        // ===== 루프 재생 =====
        public void PlayIdle() => PlayLoop(Settings.AnimIdle);
        public void PlayMove() => PlayLoop(Settings.AnimRun);
        public void PlayJump() => PlayLoop(Settings.AnimJump);
        public void PlayDash() => PlayLoop(Settings.AnimDash);

        private void PlayLoop(string animationName)
        {
            isActioning = false;
            pendingOnceCallback = null;
            skeletonAnimation.AnimationState.SetAnimation(MainTrack, animationName, true);
        }

        // ===== 단발 재생 =====
        public void PlayOnce(string animationName, Action onComplete = null)
        {
            isActioning = true;
            pendingOnceCallback = onComplete;
            skeletonAnimation.AnimationState.SetAnimation(MainTrack, animationName, false);
        }

        // ===== 방향 =====
        public void SetFacing(bool right)
        {
            skeletonAnimation.Skeleton.ScaleX = right ? -1f : 1f;
        }

        // ===== 스킨 (몬스터에서는 호출하지 않음) =====
        public void SetCombinedSkin(params string[] skinKeys)
        {
            var skeleton = skeletonAnimation.Skeleton;
            var combinedSkin = new Skin("combined");

            if (skinKeys == null || skinKeys.Length == 0)
            {
                // 폴백: 테스트용 기본 스킨 조합
                combinedSkin.AddSkin(skeleton.Data.FindSkin("BODY/base"));
                combinedSkin.AddSkin(skeleton.Data.FindSkin("HEAD/headA"));
                combinedSkin.AddSkin(skeleton.Data.FindSkin("HAIR/hairA_a"));
                combinedSkin.AddSkin(skeleton.Data.FindSkin("RIGHTHAND/Sword_TwoHand_Common1"));
            }
            else
            {
                for (int i = 0; i < skinKeys.Length; i++)
                {
                    var skin = skeleton.Data.FindSkin(skinKeys[i]);
                    if (skin != null) combinedSkin.AddSkin(skin);
                }
            }

            skeleton.SetSkin(combinedSkin);
            skeleton.SetSlotsToSetupPose();
        }

        // ===== Spine 이벤트 → 게임 이벤트 =====
        private void OnSpineEvent(TrackEntry entry, Spine.Event e)
        {
            if (entry.TrackIndex != MainTrack) return;

            var name = e.Data.Name;
            if (name == EventNameAttack)
                OnAttackEvent?.Invoke();
            else if (name == EventNameComboReady)
                OnComboReadyEvent?.Invoke();
        }

        private void OnSpineComplete(TrackEntry entry)
        {
            if (entry.TrackIndex != MainTrack) return;
            if (!isActioning) return;

            isActioning = false;
            var cb = pendingOnceCallback;
            pendingOnceCallback = null;

            OnActionCompleted?.Invoke();
            cb?.Invoke();
        }
    }
}
