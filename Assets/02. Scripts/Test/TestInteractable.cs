using Core;
using Cysharp.Threading.Tasks;
using MS.Field;
using MS.Interaction;
using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string iconKey;
    [SerializeField] private string dialogueKey = "test_dialogue_01";

    public string InteractIconKey => iconKey;

    public async UniTask InteractAsync(PlayerCharacter _player)
    {
        Debug.Log($"[TestInteractable] 대화 시작 / target={name} / key={dialogueKey}");
        await Main.Instance.UIManager.ShowDialogueAsync(dialogueKey);
    }
}
