using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MS.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace MS.Field
{
    public class SpineController : MonoBehaviour
    {
        public bool IsScaleXRight => skeletonAnimation.Skeleton.ScaleX > 0f;

        private SkeletonAnimation skeletonAnimation;
        private Skin curSkin;

        private string curWaitEventKey;
        private UniTaskCompletionSource curWaitEventTcs;
        private UniTaskCompletionSource curWaitCompleteTcs;


        private void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        private void Start()
        {
            var state = skeletonAnimation.AnimationState;
            state.Event += OnSpineEvent;
            state.Complete += OnSpineComplete;
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

        // ===== 스킨 =====
        public void SetSkin(List<string> _skinKeys)
        {
            var skeleton = skeletonAnimation.Skeleton;

            if (curSkin == null)
                curSkin = new Skin("combined");
            else
                curSkin.Clear();

            for (int i = 0; i < _skinKeys.Count; i++)
            {
                var skin = skeleton.Data.FindSkin(_skinKeys[i]);
                if (skin != null) curSkin.AddSkin(skin);
            }

            skeleton.SetSkin(curSkin);
            skeleton.SetSlotsToSetupPose();
        }

        public void PlayAnimation(string _animationName, bool _loop, float _timeScale = 1f)
        {
            CancelAllWaitTcs();
            var entry = skeletonAnimation.AnimationState.SetAnimation(Settings.SpineMainTrack, _animationName, _loop);
            if (entry != null) entry.TimeScale = _timeScale;
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

        public void SetScaleX(bool _right)
        {
            skeletonAnimation.Skeleton.ScaleX = _right ? 1f : -1f;
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
