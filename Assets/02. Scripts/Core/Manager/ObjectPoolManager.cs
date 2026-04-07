using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core
{
    public class ObjectPoolManager
    {
        private Dictionary<string, Stack<GameObject>> _pool = new Dictionary<string, Stack<GameObject>>();
        private Dictionary<string, AsyncOperationHandle<GameObject>> _loadedAssetHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
        private Transform _poolParent;

        public void InitObjectPoolManager(Transform _parent)
        {
            _poolParent = _parent;
        }

        public async UniTask CreatePoolAsync(string _key, int _initialCount)
        {
            if (string.IsNullOrEmpty(_key))
            {
                Debug.LogError("[PoolManager] Pool Key가 null이거나 비어있습니다.");
                return;
            }

            if (_pool.ContainsKey(_key))
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{_key}'가 이미 존재합니다.");
                return;
            }

            AsyncOperationHandle<GameObject> handle;
            try
            {
                handle = Addressables.LoadAssetAsync<GameObject>(_key);
                await handle.ToUniTask();

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    throw new System.Exception($"Failed to load asset: {_key}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PoolManager] 어드레서블 로드 실패 '{_key}': {e.Message}");
                return;
            }

            _loadedAssetHandles[_key] = handle;
            GameObject prefabToSpawn = handle.Result;

            Stack<GameObject> newPool = new Stack<GameObject>(_initialCount);
            _pool[_key] = newPool;

            for (int i = 0; i < _initialCount; i++)
            {
                GameObject instance = Object.Instantiate(prefabToSpawn, _poolParent);
                instance.SetActive(false);
                newPool.Push(instance);
            }

            Debug.Log($"[PoolManager] 풀 생성 완료: '{_key}' (개수: {_initialCount})");
        }

        public GameObject Get(string _key, Vector3 _pos = default, Quaternion _rot = default)
        {
            GameObject instance = Get(_key);
            if (instance)
            {
                instance.transform.position = _pos;
                instance.transform.rotation = _rot;
                instance.transform.localScale = Vector3.one;
                instance.SetActive(true);
            }
            return instance;
        }

        public GameObject Get(string _key, Transform _transform)
        {
            GameObject instance = Get(_key);
            if (instance)
            {
                instance.transform.position = _transform.position;
                instance.transform.rotation = _transform.rotation;
                instance.transform.localScale = Vector3.one;
                instance.SetActive(true);
            }
            return instance;
        }

        private GameObject Get(string _key)
        {
            if (!_pool.TryGetValue(_key, out Stack<GameObject> pool))
            {
                Debug.LogError($"[PoolManager] Get: '{_key}'에 해당하는 풀이 없습니다. CreatePoolAsync를 먼저 호출해야 합니다.");
                return null;
            }

            if (pool.Count > 0)
            {
                GameObject instance = pool.Pop();
                return instance;
            }

            if (!_loadedAssetHandles.TryGetValue(_key, out var handle))
            {
                Debug.LogError($"[PoolManager] Grow: '{_key}'에 대한 핸들을 찾을 수 없어 확장이 불가능합니다.");
                return null;
            }

            GameObject newInstance = Object.Instantiate(handle.Result, _poolParent);
            return newInstance;
        }

        public void Return(string _key, GameObject _instance)
        {
            if (_instance == null) return;

            if (string.IsNullOrEmpty(_key) || !_pool.TryGetValue(_key, out Stack<GameObject> pool))
            {
                Debug.LogWarning($"[PoolManager] Return: '{_key}' 풀을 찾을 수 없습니다. 오브젝트를 파괴합니다.");
                Object.Destroy(_instance);
                return;
            }

            _instance.SetActive(false);
            _instance.transform.SetParent(_poolParent);
            pool.Push(_instance);
        }

        public void ClearAllPools()
        {
            if (_poolParent != null)
            {
                Transform parent = _poolParent.parent;
                Object.Destroy(_poolParent.gameObject);
                _poolParent = new GameObject("ObjectPool").transform;
                _poolParent.SetParent(parent);
            }
            _pool.Clear();

            foreach (var handle in _loadedAssetHandles.Values)
            {
                Addressables.Release(handle);
            }
            _loadedAssetHandles.Clear();

            Debug.Log("[PoolManager] 모든 풀이 정리되었고, 어드레서블 로드 리소스가 해제되었습니다.");
        }
    }
}
