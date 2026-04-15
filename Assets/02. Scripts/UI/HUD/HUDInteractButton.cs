using Core;
using Cysharp.Threading.Tasks;
using MS.Interaction;
using MS.Utils;
using UnityEngine;

namespace MS.UI.HUD
{
    public class HUDInteractButton : MonoBehaviour
    {
        private MSButton btn;
        private MSImage icon;

        private void Awake()
        {
            btn = GetComponent<MSButton>();
            icon = transform.FindChildComponentDeep<MSImage>("Icon");

            btn.onClick.AddListener(OnBtnInteractClicked);
        }

        public void InitTest()
        {
            Main.Instance.Player.PIC.OnTargetChanged += OnTargetChangedCallback;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Main.Instance != null && Main.Instance.Player != null && Main.Instance.Player.PIC != null)
                Main.Instance.Player.PIC.OnTargetChanged -= OnTargetChangedCallback;
        }

        private void OnTargetChangedCallback(IInteractable _target)
        {
            if (_target == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            LoadIconAsync(_target.InteractIconKey).Forget();
        }

        private async UniTaskVoid LoadIconAsync(string _iconKey)
        {
            if (string.IsNullOrEmpty(_iconKey)) return;

            Sprite sprite = await Main.Instance.AddressableManager.LoadResourceAsync<Sprite>(_iconKey);
            if (sprite != null && icon != null)
                icon.sprite = sprite;
        }

        private void OnBtnInteractClicked()
        {
            Main.Instance.Player.PIC.TryInteract();
        }
    }
}
