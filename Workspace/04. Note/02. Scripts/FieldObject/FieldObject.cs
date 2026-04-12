using UnityEngine;

namespace MS.Field
{
    public abstract class FieldObject : MonoBehaviour
    {
        public enum FieldObjectType
        {
            Player,
            Monster,
            SkillObject,
            FieldItem,
        }
        public enum FieldObjectLifeState
        {
            Live,
            Dying,
            Death
        }

        public FieldObjectType ObjectType { get; protected set; }
        public FieldObjectLifeState ObjectLifeState { get; protected set; }

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

    }
}
