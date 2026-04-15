using MS.Field;
using MS.Interaction;
using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string iconKey;
    [SerializeField] private string logMessage = "대화 시작";

    public string InteractIconKey => iconKey;

    public void Interact(PlayerCharacter _player)
    {
        Debug.Log($"[TestInteractable] {logMessage} / target={name}");
    }
}
