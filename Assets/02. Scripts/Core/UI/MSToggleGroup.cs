using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class MSToggleGroup : MonoBehaviour
    {
        public event Action<MSToggleButton, MSToggleButton> onToggleChanged;

        public MSToggleButton CurSelected => curSelected;

        [SerializeField] private bool allowToggleOff;

        private List<MSToggleButton> toggleList = new List<MSToggleButton>();
        private MSToggleButton curSelected;

        private void Awake()
        {
            var children = GetComponentsInChildren<MSToggleButton>(true);
            foreach (var toggle in children)
                AddToggle(toggle);
        }

        public void SelectToggle(int _index, bool _invokeCallback = true)
        {
            if (_index < 0 || _index >= toggleList.Count) return;

            var target = toggleList[_index];
            ApplySelection(target, _invokeCallback);
        }

        public void AddToggle(MSToggleButton _toggle)
        {
            if (toggleList.Contains(_toggle)) return;

            toggleList.Add(_toggle);
            _toggle.SetGroup(this);
        }

        public void RemoveToggle(MSToggleButton _toggle)
        {
            if (!toggleList.Remove(_toggle)) return;

            _toggle.SetGroup(null);

            if (curSelected == _toggle)
                curSelected = null;
        }

        public void NotifyToggleClicked(MSToggleButton _toggle)
        {
            if (curSelected == _toggle)
            {
                if (allowToggleOff)
                    ApplySelection(null, true);
                return;
            }

            ApplySelection(_toggle, true);
        }

        private void ApplySelection(MSToggleButton _newToggle, bool _invokeCallback)
        {
            var prev = curSelected;

            if (prev != null)
                prev.SetIsOn(false, _invokeCallback);

            curSelected = _newToggle;

            if (curSelected != null)
                curSelected.SetIsOn(true, _invokeCallback);

            if (_invokeCallback)
                onToggleChanged?.Invoke(prev, curSelected);
        }
    }
}
