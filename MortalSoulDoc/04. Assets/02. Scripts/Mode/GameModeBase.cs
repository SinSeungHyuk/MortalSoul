using MS.Core.StateMachine;
using MS.Field;
using MS.Manager;
using UnityEngine;

namespace MS.Mode
{
    public abstract class GameModeBase
    {
        protected MSStateMachine<GameModeBase> modeStateMachine;
        protected FieldMap curFieldMap;

        public FieldMap CurFieldMap => curFieldMap;



        public GameModeBase() 
        {
            modeStateMachine = new MSStateMachine<GameModeBase>(this);
        }

        public virtual void StartMode()
        {
            OnRegisterStates();
        }

        public virtual void OnUpdate(float _deltaTime)
        {
            modeStateMachine.OnUpdate(_deltaTime);
        }

        public virtual void EndMode() { }

        protected abstract void OnRegisterStates();
    }
}