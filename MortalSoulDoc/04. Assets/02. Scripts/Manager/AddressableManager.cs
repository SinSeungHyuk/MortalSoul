using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;


namespace MS.Manager
{
    public class AddressableManager : Singleton<AddressableManager>
    {
        // AsyncOperationHandle : 어드레서블의 로드할때 반환하는 인스턴스 (작업핸들)
        private Dictionary<string, AsyncOperationHandle> loadedHandles = new();


        // 리소스를 어드레서블 그룹의 label 단위로 로드
        public async UniTask<IList<T>> LoadResourcesLabelAsync<T>(string label) where T : Object
        {
            if (loadedHandles.TryGetValue(label, out var handle))
            {
                return (IList<T>)handle.Result;
            }

            try
            {
                AsyncOperationHandle<IList<T>> opHandle = Addressables.LoadAssetsAsync<T>(label, null);

                await opHandle.ToUniTask();

                if (opHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedHandles.Add(label, opHandle);
                    return opHandle.Result;
                }
                else
                {
                    throw new System.Exception($"Failed to load assets with label: {label}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LoadAssetsByLabelAsync Failed!!!! - {label}: {e.Message}");
                return null;
            }
        }

        // 리소스 하나를 로드
        public async UniTask<T> LoadResourceAsync<T>(string key) where T : Object
        {
            if (loadedHandles.ContainsKey(key))
            {
                return loadedHandles[key].Result as T;
            }

            try
            {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
                loadedHandles.Add(key, handle);

                await handle;

                return handle.Result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Load Asset Failed: {key} / {e.Message}");
                if (loadedHandles.ContainsKey(key)) loadedHandles.Remove(key);
                return null;
            }
        }

        // 리소스 '동기적' 로드
        public T LoadResource<T>(string key) where T : Object
        {
            if (loadedHandles.ContainsKey(key))
            {
                return loadedHandles[key].Result as T;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            handle.WaitForCompletion();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedHandles.Add(key, handle);
                return handle.Result;
            }

            return null;
        }

        public void ReleaseGroup(string groupLabel)
        {
            // handle을 해제하면 그 핸들로 로드한 모든 리소스 해제
            if (loadedHandles.TryGetValue(groupLabel, out var handle))
            {
                Addressables.Release(handle);
                loadedHandles.Remove(groupLabel); // 딕셔너리도 처리해주기
            }
        }
        public void ReleaseAll()
        {
            foreach (var handle in loadedHandles.Values)
            {
                Addressables.Release(handle);
            }

            loadedHandles.Clear();
        }
        private void OnDestroy()
            => ReleaseAll();
    }

}