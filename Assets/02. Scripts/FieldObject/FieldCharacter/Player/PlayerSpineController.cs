using MS.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace MS.Field
{
    /// <summary>
    /// 플레이어 Spine 애니메이션 컨트롤러.
    /// PlayerController의 이동 상태(Idle/Move/Jump/Dash)에 대응하는 루프 애니메이션
    /// 재생 + 좌우 방향 전환만 담당한다.
    /// 스킨 교체는 소울 시스템, 공격/스킬/피격은 BSC 통합 시 추가 예정.
    /// </summary>
    [RequireComponent(typeof(SkeletonAnimation))]
    public class PlayerSpineController : MonoBehaviour
    {
        private SkeletonAnimation skeletonAnimation;

        private void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        private void Start()
        {
            // TODO :: 테스트용 스킨 입히기
            var skeleton = skeletonAnimation.Skeleton;
            var combinedSkin = new Skin("default");
            combinedSkin.AddSkin(skeleton.Data.FindSkin("BODY/base"));
            combinedSkin.AddSkin(skeleton.Data.FindSkin("HEAD/headA"));
            combinedSkin.AddSkin(skeleton.Data.FindSkin("HAIR/hairA_a"));
            combinedSkin.AddSkin(skeleton.Data.FindSkin("RIGHTHAND/Sword_TwoHand_Common1"));
            skeleton.SetSkin(combinedSkin);
            skeleton.SetSlotsToSetupPose();

            skeletonAnimation.AnimationState.Data.DefaultMix = 0.2f;
            PlayIdle();
        }

        // ===== PlayerController 연동 =====
        public void PlayIdle() => PlayLoop(Settings.AnimIdle);
        public void PlayMove() => PlayLoop(Settings.AnimRun);
        public void PlayJump() => PlayLoop(Settings.AnimJump);
        public void PlayDash() => PlayLoop(Settings.AnimDash);

        public void SetFacing(bool right)
        {
            skeletonAnimation.Skeleton.ScaleX = right ? -1f : 1f;
        }

        private void PlayLoop(string animationName)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
        }
    }
}
