using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Core
{
    public class EffectManager
    {
        private List<MSEffect> effectList = new List<MSEffect>();


        public MSEffect PlayEffect(string _effectKey, Vector3 _position, Quaternion _rotation)
        {
            if (string.IsNullOrEmpty(_effectKey))
            {
                Debug.LogError("[EffectManager] Effect Key가 비어있습니다.");
                return null;
            }

            GameObject instance = Main.Instance.ObjectPoolManager.Get(_effectKey, _position, _rotation);
            if (instance == null)
            {
                Debug.LogError($"[EffectManager] 풀에서 '{_effectKey}'를 가져오는데 실패했습니다.");
                return null;
            }

            MSEffect effectComponent = instance.GetComponent<MSEffect>();
            if (effectComponent == null)
            {
                Debug.LogError($"[EffectManager] Prefab '{_effectKey}'에 MSEffect 컴포넌트가 없습니다.");
                Main.Instance.ObjectPoolManager.Return(_effectKey, instance);
                return null;
            }

            effectComponent.InitEffect(_effectKey);
            effectList.Add(effectComponent);

            return effectComponent;
        }

        public void OnUpdate(float _deltaTime)
        {
            for (int i = effectList.Count - 1; i >= 0; i--)
            {
                var effect = effectList[i];
                if (effect == null || !effect.gameObject.activeInHierarchy)
                {
                    effectList.RemoveAt(i);
                    continue;
                }
                effect.OnUpdate(_deltaTime);
            }
        }

        public void StopEffectsByKey(string _poolKey)
        {
            for (int i = effectList.Count - 1; i >= 0; i--)
            {
                var e = effectList[i];
                if (e == null) { effectList.RemoveAt(i); continue; }
                if (e.PoolKey == _poolKey) e.StopEffect();
            }
        }

        public void ClearEffect()
        {
            effectList.Clear();
        }

        public async UniTask LoadAllEffectAsync()
        {
            try
            {
                var tasks = new List<UniTask>
                {
                };

                await UniTask.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
