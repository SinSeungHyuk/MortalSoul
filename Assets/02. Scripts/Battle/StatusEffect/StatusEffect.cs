using System;

namespace MS.Battle
{
    public class StatusEffect
    {
        public float Duration { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsFinished => Duration > 0 && ElapsedTime >= Duration;

        public event Action OnStatusStartCallback;
        public event Action<float> OnStatusUpdateCallback;
        public event Action OnStatusEndCallback;

        public StatusEffect(float _duration)
        {
            Duration = _duration;
            ElapsedTime = 0f;
        }

        public void Start()
            => OnStatusStartCallback?.Invoke();

        public void Update(float _deltaTime)
        {
            ElapsedTime += _deltaTime;
            OnStatusUpdateCallback?.Invoke(_deltaTime);
        }

        public void End()
            => OnStatusEndCallback?.Invoke();
    }
}
