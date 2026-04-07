using Unity.VisualScripting;
using UnityEngine;


namespace MS.Utils
{
    public static class TransformExtensions
    {
        public static Transform FindChildDeep(this Transform _parent, string _childName)
        {
            foreach (Transform child in _parent)
            {
                if (child.name == _childName)
                {
                    return child;
                }

                Transform result = FindChildDeep(child, _childName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static T FindChildComponentDeep<T>(this Transform _parent, string _childName) where T : Component
        {
            var findTransform = FindChildDeep(_parent, _childName);
            if (findTransform != null)
            {
                return findTransform.GetComponent<T>();
            }
            return null;
        }

        public static T GetOrAddComponent<T>(this Transform _parent, string _childName) where T : Component
        {
            var findTransform = FindChildDeep(_parent, _childName);
            if (findTransform != null)
            {
                T result = findTransform.GetComponent<T>();
                if (result == null)
                {
                    result = findTransform.gameObject.AddComponent<T>();
                }
                return result;
            }
            return null;
        }
    }
}
