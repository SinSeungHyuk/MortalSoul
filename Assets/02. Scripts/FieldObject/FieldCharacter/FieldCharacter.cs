using MS.Battle;
using UnityEngine;

namespace MS.Field
{
    public abstract class FieldCharacter : FieldObject
    {
        public BattleSystemComponent BSC { get; protected set; }
        public SpineController SpineController { get; protected set; }

        virtual protected void Awake()
        {
            SpineController = GetComponent<SpineController>();
        }

        virtual protected void Update()
        {
            BSC?.OnUpdate(Time.deltaTime);
        }
    }
}