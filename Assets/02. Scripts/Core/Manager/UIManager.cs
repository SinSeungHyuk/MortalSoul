using UnityEngine;
using MS.Utils;

namespace Core
{
    public class UIManager
    {
        public Transform ViewCanvas { get; private set; }
        public Transform PopupCanvas { get; private set; }
        public Transform SystemCanvas { get; private set; }

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
    }
}
