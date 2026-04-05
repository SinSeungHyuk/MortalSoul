using MS.Battle;
using UnityEngine;

namespace MS.Field
{
    public abstract class FieldCharacter : FieldObject
    {
        public BattleSystemComponent BSC { get; private set; }

        protected void InitBSC(BaseAttributeSet _attributeSet)
        {
            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, _attributeSet);
        }

        virtual protected void Awake()
        {

        }

        virtual protected void Update()
        {
            BSC?.OnUpdate(Time.deltaTime);
        }
    }
}