using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class AddressableManager
    {
        // TODO: Addressables 연동 시 실제 구현으로 교체
        public async UniTask<T> LoadResourceAsync<T>(string _key) where T : Object
        {
            Debug.LogWarning($"[AddressableManager] LoadResourceAsync 미구현 - key: {_key}");
            await UniTask.Yield();
            return null;
        }
    }
}
