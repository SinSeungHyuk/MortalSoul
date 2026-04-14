using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core
{
    public class VisualManager
    {
        private CinemachineCamera vcam;
        private CinemachineBasicMultiChannelPerlin noise;
        private Transform defaultFollow;
        private float defaultFOV;

        private Volume volume;
        private Vignette vignette;
        private ChromaticAberration chromaticAberration;

        private CancellationTokenSource zoomCts;
        private CancellationTokenSource moveCts;
        private CancellationTokenSource shakeCts;
        private CancellationTokenSource rotateCts;

        public void InitVisualManager()
        {
            vcam = Object.FindAnyObjectByType<CinemachineCamera>();
            if (vcam != null)
            {
                noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
                defaultFollow = vcam.Follow;
                defaultFOV = vcam.Lens.FieldOfView;
            }
            else
            {
                Debug.LogError("[VisualManager] CinemachineCamera를 찾지 못했습니다.");
            }

            volume = Object.FindAnyObjectByType<Volume>();
            if (volume != null && volume.profile != null)
            {
                volume.profile.TryGet(out vignette);
                volume.profile.TryGet(out chromaticAberration);
            }
            else
            {
                Debug.LogError("[VisualManager] Volume을 찾지 못했습니다.");
            }
        }

        #region Base Func

        public async UniTask ZoomFOV(float _target, float _duration)
        {
            CancelAction(ref zoomCts);
            zoomCts = new CancellationTokenSource();
            // TODO: FOV lerp
            await UniTask.CompletedTask;
        }

        public void SetFOV(float _target)
        {
            CancelAction(ref zoomCts);
            // TODO: 즉시 FOV 세팅
        }

        public async UniTask MoveTo(Vector3 _position, float _duration)
        {
            CancelAction(ref moveCts);
            moveCts = new CancellationTokenSource();
            // TODO: Follow 해제 후 위치 lerp
            await UniTask.CompletedTask;
        }

        public async UniTask MoveTo(Transform _target, float _duration)
        {
            CancelAction(ref moveCts);
            moveCts = new CancellationTokenSource();
            // TODO: Follow 해제 후 타겟 추적 lerp
            await UniTask.CompletedTask;
        }

        public void SetPosition(Vector3 _position)
        {
            CancelAction(ref moveCts);
            // TODO: 즉시 위치 세팅
        }

        public async UniTask Shake(float _intensity, float _duration)
        {
            CancelAction(ref shakeCts);
            shakeCts = new CancellationTokenSource();
            // TODO: Noise amplitude 세팅 → duration 대기 → 복귀
            await UniTask.CompletedTask;
        }

        public void StopShake()
        {
            CancelAction(ref shakeCts);
            // TODO: Noise amplitude 0
        }

        public async UniTask Rotate(float _angle, float _duration)
        {
            CancelAction(ref rotateCts);
            rotateCts = new CancellationTokenSource();
            // TODO: Z축 회전 lerp
            await UniTask.CompletedTask;
        }

        public void ResetToDefault()
        {
            CancelAction(ref zoomCts);
            CancelAction(ref moveCts);
            CancelAction(ref shakeCts);
            CancelAction(ref rotateCts);
            // TODO: 모든 카메라/PP 값 기본값으로 복귀
        }

        #endregion

        #region 실제 연출용 함수

        public void PlayHitImpact()
        {
            // TODO: Shake + Vignette pulse 조합
        }

        public void PlayDeathEffect()
        {
            // TODO: SetFOV(줌인) + Rotate(천천히) 조합
        }

        #endregion

        private void CancelAction(ref CancellationTokenSource _cts)
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
