using Cysharp.Threading.Tasks;
using MS.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace MS.Field
{
    public class SpineController : MonoBehaviour
    {
        private SkeletonAnimation skeletonAnimation;

        private string curWaitEventKey;
        private UniTaskCompletionSource curWaitEventTcs;
        private UniTaskCompletionSource curWaitCompleteTcs;


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

            PlayAnimation(Settings.AnimIdle, true);
        }

        private void OnDestroy()
        {
            if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
            {
                skeletonAnimation.AnimationState.Event -= OnSpineEvent;
                skeletonAnimation.AnimationState.Complete -= OnSpineComplete;
            }

            CancelAllWaitTcs();
        }

        public void PlayAnimation(string _animationName, bool _loop)
        {
            CancelAllWaitTcs();
            skeletonAnimation.AnimationState.SetAnimation(Settings.SpineMainTrack, _animationName, _loop);
        }

        // ===== 비동기 대기 헬퍼 =====
        public UniTask WaitForAnimEventAsync(string _eventKey)
        {
            curWaitEventTcs?.TrySetCanceled();

            curWaitEventKey = _eventKey;
            curWaitEventTcs = new UniTaskCompletionSource();
            return curWaitEventTcs.Task;
        }

        public UniTask WaitForAnimCompleteAsync()
        {
            curWaitCompleteTcs?.TrySetCanceled();
            curWaitCompleteTcs = new UniTaskCompletionSource();
            return curWaitCompleteTcs.Task;
        }

        private void CancelAllWaitTcs()
        {
            curWaitEventTcs?.TrySetCanceled();
            curWaitEventTcs = null;
            curWaitEventKey = null;

            curWaitCompleteTcs?.TrySetCanceled();
            curWaitCompleteTcs = null;
        }

        // ===== 방향 =====
        public void SetScaleX(bool _right)
        {
            skeletonAnimation.Skeleton.ScaleX = _right ? -1f : 1f;
        }

        // ===== 스킨 =====
        public void SetCombinedSkin(params string[] _skinKeys)
        {
            var skeleton = skeletonAnimation.Skeleton;
            var combinedSkin = new Skin("combined");

            if (_skinKeys == null || _skinKeys.Length == 0)
            {
                combinedSkin.AddSkin(skeleton.Data.FindSkin("BODY/base"));
                combinedSkin.AddSkin(skeleton.Data.FindSkin("HEAD/headA"));
                combinedSkin.AddSkin(skeleton.Data.FindSkin("HAIR/hairA_a"));
                combinedSkin.AddSkin(skeleton.Data.FindSkin("RIGHTHAND/Sword_OneHand_Common1"));
            }
            else
            {
                for (int i = 0; i < _skinKeys.Length; i++)
                {
                    var skin = skeleton.Data.FindSkin(_skinKeys[i]);
                    if (skin != null) combinedSkin.AddSkin(skin);
                }
            }

            skeleton.SetSkin(combinedSkin);
            skeleton.SetSlotsToSetupPose();
        }

        // ===== Spine 이벤트 =====
        private void OnSpineEvent(TrackEntry _entry, Spine.Event _e)
        {
            if (_entry.TrackIndex != Settings.SpineMainTrack) return;
            if (curWaitEventTcs == null) return;
            if (_e.Data.Name != curWaitEventKey) return;

            var tcs = curWaitEventTcs;
            curWaitEventTcs = null;
            curWaitEventKey = null;
            tcs.TrySetResult();
        }

        private void OnSpineComplete(TrackEntry _entry)
        {
            if (_entry.TrackIndex != Settings.SpineMainTrack) return;
            if (_entry.Loop) return;
            if (curWaitCompleteTcs == null) return;

            var tcs = curWaitCompleteTcs;
            curWaitCompleteTcs = null;
            tcs.TrySetResult();
        }
    }
}
