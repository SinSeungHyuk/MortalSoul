using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MS.UI;
using MS.Utils;
using UnityEngine;

namespace Core
{
    public class UIManager
    {
        public Transform ViewCanvas { get; private set; }
        public Transform PopupCanvas { get; private set; }
        public Transform SystemCanvas { get; private set; }

        private readonly Dictionary<string, BasePopup> cachedPopupDict = new();
        private readonly Stack<BasePopup> popupStack = new();

        public void InitUIManager(Transform _root)
        {
            if (_root == null)
            {
                Debug.LogError("[UIManager] 초기화 실패: root Transform이 null입니다.");
                return;
            }

            ViewCanvas = _root.FindChildDeep("ViewCanvas");
            PopupCanvas = _root.FindChildDeep("PopupCanvas");
            SystemCanvas = _root.FindChildDeep("SystemCanvas");
        }

        public async UniTask<T> ShowPopupAsync<T>(string _key) where T : BasePopup
        {
            BasePopup popup;

            if (cachedPopupDict.TryGetValue(_key, out var cached))
            {
                popup = cached;
            }
            else
            {
                var prefab = await Main.Instance.AddressableManager.LoadResourceAsync<GameObject>(_key);
                if (prefab == null)
                {
                    Debug.LogError($"[UIManager] ShowPopupAsync :: 프리팹 로드 실패: {_key}");
                    return null;
                }

                popup = Object.Instantiate(prefab, PopupCanvas).GetComponent<BasePopup>();
                popup.name = _key;
                cachedPopupDict.Add(_key, popup);
            }

            popupStack.Push(popup);
            popup.Show();

            return popup as T;
        }

        public void ClosePopup(BasePopup _popup)
        {
            if (popupStack.Count == 0) return;
            if (popupStack.Peek() != _popup)
            {
                Debug.LogWarning("[UIManager] ClosePopup :: peek 불일치");
                return;
            }
            popupStack.Pop();
        }

        public async UniTask ShowDialogueAsync(string _dialogueKey)
        {
            var popup = await ShowPopupAsync<DialoguePopup>("DialoguePopup");
            if (popup == null) return;

            try { await popup.PlayDialogueAsync(_dialogueKey); }
            finally { popup.Close(); }
        }
    }
}
