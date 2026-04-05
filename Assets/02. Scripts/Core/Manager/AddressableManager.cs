using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Core
{
    public class AddressableManager
    {
        private readonly Dictionary<string, AsyncOperationHandle> _loadedHandles = new();

        // 리소스 하나를 key로 비동기 로드
        public async UniTask<T> LoadResourceAsync<T>(string _key) where T : Object
        {
            if (_loadedHandles.TryGetValue(_key, out var cached))
                return cached.Result as T;

            try
            {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(_key);

                await handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedHandles.Add(_key, handle);
                    return handle.Result;
                }
                else
                {
                    Addressables.Release(handle);
                    throw new Exception($"Failed to load asset: {_key}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableManager] LoadResourceAsync Failed: {_key} / {e.Message}");
                return null;
            }
        }

        // 어드레서블 그룹 label 단위로 리소스 일괄 로드
        public async UniTask<IList<T>> LoadResourcesLabelAsync<T>(string _label) where T : Object
        {
            if (_loadedHandles.TryGetValue(_label, out var cached))
                return (IList<T>)cached.Result;

            try
            {
                AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(_label, null);

                await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedHandles.Add(_label, handle);
                    return handle.Result;
                }
                else
                {
                    throw new Exception($"Failed to load assets with label: {_label}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableManager] LoadResourcesLabelAsync Failed: {_label} / {e.Message}");
                return null;
            }
        }

        // 리소스 하나를 key로 동기 로드
        public T LoadResource<T>(string _key) where T : Object
        {
            if (_loadedHandles.TryGetValue(_key, out var cached))
                return cached.Result as T;

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(_key);
            handle.WaitForCompletion();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedHandles.Add(_key, handle);
                return handle.Result;
            }

            Addressables.Release(handle);
            return null;
        }

        // 특정 key 리소스 해제
        public void Release(string _key)
        {
            if (_loadedHandles.TryGetValue(_key, out var handle))
            {
                Addressables.Release(handle);
                _loadedHandles.Remove(_key);
            }
        }

        // 전체 리소스 해제 (Main 정리 시 호출)
        public void ReleaseAll()
        {
            foreach (var handle in _loadedHandles.Values)
                Addressables.Release(handle);

            _loadedHandles.Clear();
        }
    }
}
