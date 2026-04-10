using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core
{
    public class MSButton : Button
    {
        [SerializeField] private string clickSfxKey;

        public override void OnPointerClick(PointerEventData _eventData)
        {
            if (!string.IsNullOrEmpty(clickSfxKey))
                Main.Instance.SoundManager.PlaySFX(clickSfxKey);

            base.OnPointerClick(_eventData);
        }

        public void SetSprite(Sprite _sprite)
        {
            image.sprite = _sprite;
        }
    }
}
