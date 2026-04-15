using System;
using Core;
using Cysharp.Threading.Tasks;
using MS.Interaction;
using UnityEngine;
using MS.Utils;

namespace MS.Field
{
    public class PlayerInteractController : MonoBehaviour
    {
        public IInteractable CurTarget { get; private set; }

        public event Action<IInteractable> OnTargetChanged;

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if ((Settings.InteractableLayer.value & (1 << _other.gameObject.layer)) == 0) return;
            if (!_other.TryGetComponent<IInteractable>(out var interactable)) return;

            CurTarget = interactable;
            OnTargetChanged?.Invoke(CurTarget);
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            if ((Settings.InteractableLayer.value & (1 << _other.gameObject.layer)) == 0) return;

            CurTarget = null;
            OnTargetChanged?.Invoke(null);
        }

        public void TryInteract()
        {
            CurTarget?.InteractAsync(Main.Instance.Player).Forget();
        }
    }
}
