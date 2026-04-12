using Core;
using Cysharp.Threading.Tasks;
using MS.Core;
using MS.Field;
using MS.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Manager
{
    public class EffectManager : Singleton<EffectManager>
    {
        private List<MSEffect> effectList = new List<MSEffect>();


        public MSEffect PlayEffect(string effectKey, Vector3 position, Quaternion rotation)
        {
            if (string.IsNullOrEmpty(effectKey))
            {
                Debug.LogError("[EffectManager] Effect Key가 비어있습니다.");
                return null;
            }

            GameObject instance = ObjectPoolManager.Instance.Get(effectKey, position, rotation);
            if (instance == null)
            {
                Debug.LogError($"[EffectManager] 풀에서 '{effectKey}'를 가져오는데 실패했습니다.");
                return null;
            }

            MSEffect effectComponent = instance.GetComponent<MSEffect>();
            if (effectComponent == null)
            {
                Debug.LogError($"[EffectManager] Prefab '{effectKey}'에 MSEffect 컴포넌트가 없습니다.");
                ObjectPoolManager.Instance.Return(effectKey, instance);
                return null;
            }

            effectComponent.InitEffect(effectKey);
            effectList.Add(effectComponent);

            return effectComponent;
        }

        public void OnUpdate(float _deltaTime)
        {
            for (int i = effectList.Count - 1; i >= 0; i--)
            {
                var effect = effectList[i];
                effect.OnUpdate(_deltaTime);
            }
        }

        public void StopEffectsByKey(string poolKey)
        {
            for (int i = effectList.Count - 1; i >= 0; i--)
            {
                var e = effectList[i];
                if (e == null) { effectList.RemoveAt(i); continue; }
                if (e.PoolKey == poolKey) e.StopEffect();
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
                    ObjectPoolManager.Instance.CreatePoolAsync("DamageText", 100),

                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_Teleport", 1),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_GainBuff", 3),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_GainHp", 3),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_GainMoveSpeed", 3),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_MonsterHit", 200),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_BossPortal", 1),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_Firework", 1),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_StunLoop", 100),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_StunBegin", 100),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_Burn", 100),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_Charm", 100),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_Frost", 100),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_WaveMeteor", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_WaveMeteorHit", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Eff_StarAura", 1),
                    // ... 
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