# ObjectPoolManager + Main 의존성 주입 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** ObjectPoolManager를 일반 클래스로 구현하고, Main이 코드로 컨테이너를 생성하여 Initialize()로 주입하는 패턴을 확립한다.

**Architecture:** Main(MonoSingleton)이 Awake()에서 ObjectPool 컨테이너 GameObject를 생성하고, ObjectPoolManager(일반 클래스)의 Initialize(Transform)에 주입한다. ObjectPoolManager의 풀링 로직은 레퍼런스와 동일하며, MonoBehaviour 메서드를 Object 정적 메서드로 대체한다.

**Tech Stack:** Unity 6, C#, Addressables, UniTask

---

## File Structure

| 파일 | 동작 | 책임 |
|------|------|------|
| `Assets/02. Scripts/Core/Manager/ObjectPoolManager.cs` | 전체 재작성 | 오브젝트 풀 생성/Get/Return/Clear |
| `Assets/02. Scripts/Core/Main.cs` | 수정 (ObjectPoolManager 초기화 부분) | 컨테이너 생성 + Initialize 호출 |

---

### Task 1: ObjectPoolManager 구현

**Files:**
- Rewrite: `Assets/02. Scripts/Core/Manager/ObjectPoolManager.cs`

- [ ] **Step 1: ObjectPoolManager 전체 구현**

레퍼런스(`MortalSoulDoc/04. Assets/02. Scripts/Manager/CoreManager/ObjectPoolManager.cs`) 로직을 일반 클래스로 전환하여 작성한다.

```csharp
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

        public void Initialize(Transform poolParent)
        {
            _poolParent = poolParent;
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
                Debug.LogWarning($"[PoolManager] Pool with key '{key}'가 이미 존재합니다.");
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
                GameObject instance = Object.Instantiate(prefabToSpawn, _poolParent);
                instance.SetActive(false);
                newPool.Push(instance);
            }

            Debug.Log($"[PoolManager] 풀 생성 완료: '{key}' (개수: {initialCount})");
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
                Debug.LogError($"[PoolManager] Grow: '{key}'에 대한 핸들을 찾을 수 없어 확장이 불가능합니다.");
                return null;
            }

            GameObject newInstance = Object.Instantiate(handle.Result, _poolParent);
            return newInstance;
        }

        public void Return(string key, GameObject instance)
        {
            if (instance == null) return;

            if (string.IsNullOrEmpty(key) || !_pool.TryGetValue(key, out Stack<GameObject> pool))
            {
                Debug.LogWarning($"[PoolManager] Return: '{key}' 풀을 찾을 수 없습니다. 오브젝트를 파괴합니다.");
                Object.Destroy(instance);
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
                Object.Destroy(_poolParent.gameObject);
                _poolParent = new GameObject("ObjectPool").transform;
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
```

- [ ] **Step 2: 컴파일 확인**

Unity 에디터에서 컴파일 에러가 없는지 확인한다.

---

### Task 2: Main.cs ObjectPoolManager 초기화 수정

**Files:**
- Modify: `Assets/02. Scripts/Core/Main.cs:15-26` (Awake 내부)

- [ ] **Step 1: Main.cs의 ObjectPoolManager 초기화를 Initialize 패턴으로 변경**

Awake() 내부에서 ObjectPoolManager 생성 부분만 수정한다. 다른 매니저는 그대로 둔다.

변경 전:
```csharp
ObjectPoolManager = new ObjectPoolManager();
```

변경 후:
```csharp
Transform poolContainer = new GameObject("ObjectPool").transform;
poolContainer.SetParent(transform);

ObjectPoolManager = new ObjectPoolManager();
ObjectPoolManager.Initialize(poolContainer);
```

- [ ] **Step 2: 컴파일 확인**

Unity 에디터에서 컴파일 에러가 없는지 확인한다.

---

### Task 3: 검증 및 커밋

- [ ] **Step 1: Unity 에디터에서 플레이모드 진입 후 확인**

Main 오브젝트 하이어라키에 "ObjectPool" 자식 오브젝트가 생성되는지 확인한다.

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Core/Manager/ObjectPoolManager.cs"
git add "Assets/02. Scripts/Core/Main.cs"
git commit -m "ObjectPoolManager 일반 클래스 전환 및 Main 의존성 주입 패턴 구현"
```
