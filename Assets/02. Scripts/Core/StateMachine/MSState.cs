using System;

namespace MS.Core.StateMachine
{
    public class MSState<OwnerType>
    {
        public int StateId { get; set; }
        public MSStateMachine<OwnerType> OwnerMachine { get; set; }

        private Action<int, object[]> onStateEnter; 
        private Action<float> onStateUpdate;
        private Action<int> onStateExit; 


        public void InitState(int _id, MSStateMachine<OwnerType> _machine,
                          Action<int, object[]> _onEnter,
                          Action<float> _onUpdate,
                          Action<int> _onExit)
        {
            StateId = _id;
            OwnerMachine = _machine;
            onStateEnter = _onEnter;
            onStateUpdate = _onUpdate;
            onStateExit = _onExit;
        }

        public void OnStateEnter(int _prevStateId, params object[] _params) => onStateEnter?.Invoke(_prevStateId, _params);
        public void OnStateUpdate(float _deltaTime) => onStateUpdate?.Invoke(_deltaTime);
        public void OnStateExit(int _nextStateId) => onStateExit?.Invoke(_nextStateId);
    }
}