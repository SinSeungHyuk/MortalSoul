using System;
using UnityEngine;


namespace MS.Skill
{
    public class StatusEffect
    {
        public event Action OnStatusStartCallback;
        public event Action<float> OnStatusUpdateCallback;
        public event Action OnStatusEndCallback;

        private float duration;
        private float elapsedTime;

        public bool IsFinished => duration > 0 && elapsedTime >= duration;


        public void InitStatusEffect(float _duration)
        {
            duration = _duration;
        }

        public void OnApply() 
            => OnStatusStartCallback?.Invoke();

        public void OnUpdate(float _deltaTime)
        {
            elapsedTime += _deltaTime;
            OnStatusUpdateCallback?.Invoke(_deltaTime);
        }

        public void OnEnd()
            => OnStatusEndCallback?.Invoke();
    }
}