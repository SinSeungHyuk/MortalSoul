using Core;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;


namespace MS.Manager
{
    public class CameraManager : MonoSingleton<CameraManager>
    {
        [SerializeField] private CinemachineCamera mainCamera;
        private CinemachineBasicMultiChannelPerlin noiseComponent;
        private CancellationTokenSource shakeCts;

        public CinemachineCamera MainCamera => mainCamera;


        protected override void Awake()
        {
            base.Awake();
            noiseComponent = mainCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        public void InitMainCamera(Transform _target)
        {
            mainCamera.Follow = _target;
        }

        public void ShakeCamera(float _intensity, float _duration)
        {
            // 이전 쉐이크가 있으면 중단하고 새로 시작
            StopShake();

            shakeCts = new CancellationTokenSource();
            CameraShakeAsync(_intensity, _duration, shakeCts.Token).Forget();
        }

        public void StopShake()
        {
            // 안전하게 취소하고 즉시 진동을 0으로 설정
            if (shakeCts != null)
            {
                try
                {
                    if (!shakeCts.IsCancellationRequested) shakeCts.Cancel();
                }
                catch { }
                shakeCts.Dispose();
                shakeCts = null;
            }

            if (noiseComponent != null)
            {
                noiseComponent.AmplitudeGain = 0f;
            }
        }

        private async UniTaskVoid CameraShakeAsync(float _intensity, float _duration, CancellationToken token)
        {
            if (noiseComponent == null) return;

            try
            {
                // 1. 진동 시작
                noiseComponent.AmplitudeGain = _intensity;

                // 2. 시간 대기 (취소 토큰 적용)
                await UniTask.WaitForSeconds(_duration, cancellationToken: token);

                // 3. 진동 서서히 감소 (Linear하게 0으로)
                float elapsed = 0f;
                float fadeOutTime = 0.2f; // 부드럽게 멈추기 위한 페이드 아웃 시간

                while (elapsed < fadeOutTime)
                {
                    if (token.IsCancellationRequested) break;

                    elapsed += Time.deltaTime;
                    noiseComponent.AmplitudeGain = Mathf.Lerp(_intensity, 0f, elapsed / fadeOutTime);
                    await UniTask.Yield(token);
                }

                if (!token.IsCancellationRequested)
                {
                    noiseComponent.AmplitudeGain = 0f;
                }
            }
            catch (OperationCanceledException)
            {
                // 취소 시 즉시 정리
                if (noiseComponent != null) noiseComponent.AmplitudeGain = 0f;
            }
            finally
            {
                // 종료 시 CTS 정리
                if (shakeCts != null && token.IsCancellationRequested)
                {
                    try
                    {
                        shakeCts.Dispose();
                    }
                    catch { }
                    shakeCts = null;
                }
            }
        }
    }
}