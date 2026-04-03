using Cysharp.Threading.Tasks;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System;
using UnityEngine;


namespace MS.Field
{
    public class IndicatorObject : FieldObject
    {
        private event Action<Vector3> onIndicatorEnd;
        private Transform border;
        private Transform fill;
        private float range;
        private float duration;


        private void Awake()
        {
            border = transform.FindChildDeep("Border");
            fill = transform.FindChildDeep("Fill");
        }

        public void InitIndicator(float _range,float _duration, Action<Vector3> _callback)
        {
            ObjectLifeState = FieldObjectLifeState.Live;
            ObjectType = FieldObjectType.SkillObject;
            range = _range;
            duration = _duration;
            onIndicatorEnd = _callback;

            if (border != null) border.localScale = new Vector3(range , border.localScale.y, range);
            if (fill != null) fill.localScale = new Vector3(0 , fill.localScale.y, 0);

            PlayIndicatorAsync().Forget();
        }

        private async UniTask PlayIndicatorAsync()
        {
            float elapsed = 0f;
            var token = this.GetCancellationTokenOnDestroy();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float currentScale = progress * range;

                if (fill != null)
                {
                    fill.localScale = new Vector3(currentScale, fill.localScale.y, currentScale);
                }

                await UniTask.Yield(token);
            }

            if (fill != null)
            {
                fill.localScale = new Vector3(range, fill.localScale.y, range);
            }

            onIndicatorEnd?.Invoke(Position);
            ObjectLifeState = FieldObjectLifeState.Death;
            ObjectPoolManager.Instance.Return("Indicator", gameObject);
        }
    }
}