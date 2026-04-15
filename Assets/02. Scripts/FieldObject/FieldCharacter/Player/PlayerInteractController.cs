using System;
using Core;
using MS.Interaction;
using UnityEngine;

namespace MS.Field
{
    public class PlayerInteractController : MonoBehaviour
    {
        public IInteractable CurTarget { get; private set; }

        public event Action<IInteractable> OnTargetChanged;

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (!_other.TryGetComponent<IInteractable>(out var interactable)) return;

            CurTarget = interactable;
            OnTargetChanged?.Invoke(CurTarget);
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            CurTarget = null;
            OnTargetChanged?.Invoke(null);
        }

        public void TryInteract()
        {
            CurTarget?.Interact(Main.Instance.Player);
        }
    }
}
