using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        InitTestSoulAsync().Forget();
    }

    private async UniTaskVoid InitTestSoulAsync()
    {
        await UniTask.WaitUntil(() => Main.Instance.IsBootCompleted);
        await UniTask.DelayFrame(1);

        var player = Main.Instance.PlayerManager.CurPlayer;
        if (player == null) return;

        player.AcquireSoul("test2");
        Debug.Log("[Test] 서브 소울 'test2' 지급 완료");
    }
}
