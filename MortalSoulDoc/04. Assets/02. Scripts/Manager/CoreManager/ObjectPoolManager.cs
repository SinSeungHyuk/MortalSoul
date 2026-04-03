using Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MS.Manager
{
    public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
    {
        private Dictionary<string, Stack<GameObject>> _pool = new Dictionary<string, Stack<GameObject>>();
        private Dictionary<string, AsyncOperationHandle<GameObject>> _loadedAssetHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
        private Transform _poolParent;

        protected override void Awake()
        {
            base.Awake();
            if (_poolParent == null)
            {
                _poolParent = new GameObject("ObjectPool").transform;
            }
        }

        public async UniTask CreatePoolAsync(string key, int initialCount)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[PoolManager] Pool Key가 null이거나 비어있습니다.");
                return;
            }

            if (_pool.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{key}'는 이미 존재합니다.");
                return;
            }

            AsyncOperationHandle<GameObject> handle;
            try
            {
                handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle.ToUniTask();

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    throw new System.Exception($"Failed to load asset: {key}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PoolManager] 어드레서블 로드 실패 '{key}': {e.Message}");
                return;
            }

            _loadedAssetHandles[key] = handle;
            GameObject prefabToSpawn = handle.Result;

            Stack<GameObject> newPool = new Stack<GameObject>(initialCount);
            _pool[key] = newPool;

            for (int i = 0; i < initialCount; i++)
            {
                GameObject instance = Instantiate(prefabToSpawn, _poolParent);
                instance.SetActive(false);
                newPool.Push(instance);
            }

            Debug.Log($"[PoolManager] 풀 생성 완료: '{key}' (수량: {initialCount})");
        }

        public GameObject Get(string key, Vector3 pos = default, Quaternion rot = default)
        {
            GameObject instance = Get(key);
            if (instance)
            {
                instance.transform.position = pos;
                instance.transform.rotation = rot;
                instance.transform.localScale = Vector3.one;
                instance.SetActive(true);
            }
            return instance;
        }

        public GameObject Get(string key, Transform transform)
        {
            GameObject instance = Get(key);
            if (instance)
            {
                instance.transform.position = transform.position;
                instance.transform.rotation = transform.rotation;
                instance.transform.localScale = Vector3.one;
                instance.SetActive(true);
            }
            return instance;
        }

        private GameObject Get(string key)
        {
            if (!_pool.TryGetValue(key, out Stack<GameObject> pool))
            {
                Debug.LogError($"[PoolManager] Get: '{key}'에 해당하는 풀이 없습니다. CreatePoolAsync를 먼저 호출해야 합니다.");
                return null;
            }

            if (pool.Count > 0)
            {
                GameObject instance = pool.Pop();
                return instance;
            }

            if (!_loadedAssetHandles.TryGetValue(key, out var handle))
            {
                Debug.LogError($"[PoolManager] Grow: '{key}'의 원본 핸들을 찾을 수 없어 확장이 불가능합니다.");
                return null;
            }

            GameObject newInstance = Instantiate(handle.Result, _poolParent);

            return newInstance;
        }

        public void Return(string key, GameObject instance)
        {
            if (instance == null) return;

            if (string.IsNullOrEmpty(key) || !_pool.TryGetValue(key, out Stack<GameObject> pool))
            {
                Debug.LogWarning($"[PoolManager] Return: '{key}' 풀을 찾을 수 없습니다. 오브젝트를 파괴합니다.");
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(_poolParent);
            pool.Push(instance);
        }

        public void ClearAllPools()
        {
            if (_poolParent != null)
            {
                Destroy(_poolParent.gameObject);
                _poolParent = new GameObject("ObjectPool").transform;
            }
            _pool.Clear();

            foreach (var handle in _loadedAssetHandles.Values)
            {
                Addressables.Release(handle);
            }
            _loadedAssetHandles.Clear();

            Debug.Log("[PoolManager] 모든 풀이 제거되었고, 어드레서블 원본 리소스가 해제되었습니다.");
        }
    }
}