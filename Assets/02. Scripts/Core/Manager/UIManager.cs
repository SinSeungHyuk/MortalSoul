using UnityEngine;
using MS.Utils;

namespace Core
{
    public class UIManager
    {
        public Transform ViewCanvas { get; private set; }
        public Transform PopupCanvas { get; private set; }
        public Transform SystemCanvas { get; private set; }

        public void InitUIManager(Transform root)
        {
            if (root == null)
            {
                Debug.LogError("[UIManager] 초기화 실패: root Transform이 null입니다.");
                return;
            }

            ViewCanvas = root.FindChildDeep("ViewCanvas");
            PopupCanvas = root.FindChildDeep("PopupCanvas");
            SystemCanvas = root.FindChildDeep("SystemCanvas");
        }
    }
}
