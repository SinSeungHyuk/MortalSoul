using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core
{
    public class MSToggleButton : Selectable, IPointerClickHandler
    {
        public event Action<bool> onToggleChanged;

        public bool IsOn => isOn;

        [SerializeField] private GameObject activeObject;
        [SerializeField] private GameObject inactiveObject;

        private bool isOn;
        private MSToggleGroup group;

        public void SetIsOn(bool _value, bool _invokeCallback = true)
        {
            if (isOn == _value) return;

            isOn = _value;
            UpdateVisual();

            if (_invokeCallback)
                onToggleChanged?.Invoke(isOn);
        }

        public void SetGroup(MSToggleGroup _group)
        {
            group = _group;
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if (group != null)
            {
                group.NotifyToggleClicked(this);
                return;
            }

            SetIsOn(!isOn);
        }

        private void UpdateVisual()
        {
            if (activeObject != null)
                activeObject.SetActive(isOn);
            if (inactiveObject != null)
                inactiveObject.SetActive(!isOn);
        }
    }
}
