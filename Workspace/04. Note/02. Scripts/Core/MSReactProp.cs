using System;
using UnityEngine;


namespace MS.Core
{
    public class MSReactProp<T>
    {
        private event Action<T, T> onValueChanged;

        private T value;

        public T Value
        {
            get => value;
            set
            {
                T oldValue = value;
                this.value = value;
                onValueChanged?.Invoke(oldValue, value);
            }
        }


        public MSReactProp(T _value = default) 
            => value = _value;

        public void ForceNotify()
        {
            onValueChanged?.Invoke(value, value);
        }

        public void Subscribe(Action<T, T> _callback)
        {
            onValueChanged += _callback;
        }
        public void Unsubscribe(Action<T, T> _callback)
        {
            onValueChanged -= _callback;
        }
        public void Clear()
        {
            onValueChanged = null;
        }
    }
}