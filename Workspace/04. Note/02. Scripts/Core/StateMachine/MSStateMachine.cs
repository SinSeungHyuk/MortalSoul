using System;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Core.StateMachine
{
    public class MSStateMachine<OwnerType>
    {
        private Dictionary<int, MSState<OwnerType>> StateDict = new Dictionary<int, MSState<OwnerType>>();
        private MSState<OwnerType> curState;

        private MSState<OwnerType> reservedNextState;
        private object[] reservedParams;

        public OwnerType Owner { get; private set; }


        public MSStateMachine(OwnerType _owner)
        {
            Owner = _owner;
        }

        public void RegisterState(int _stateId,
                                  Action<int, object[]> _onEnter,
                                  Action<float> _onUpdate,
                                  Action<int> _onExit)
        {
            MSState<OwnerType> newState = new MSState<OwnerType>();
            newState.InitState(_stateId, this, _onEnter, _onUpdate, _onExit);

            if (StateDict.ContainsKey(_stateId))
            {
                StateDict[_stateId] = newState;
            }
            else
            {
                StateDict.Add(_stateId, newState);
            }
        }

        public void TransitState(int _toStateId, params object[] _params)
        {
            if (StateDict.TryGetValue(_toStateId, out MSState<OwnerType> nextState))
            {
                reservedNextState = nextState;
                reservedParams = _params;
            }
        }

        public void OnUpdate(float _deltaTime)
        {
            if (reservedNextState != null)
            {
                MSState<OwnerType> nextState = reservedNextState;
                object[] connectionParams = reservedParams;

                reservedNextState = null;
                reservedParams = null;

                int curStateId = -1;

                // 1. 현재 상태 종료
                if (curState != null)
                {
                    curStateId = curState.StateId;
                    curState.OnStateExit(nextState.StateId);
                }

                // 2. 상태 교체
                curState = nextState;

                // 3. 새 상태 진입
                curState.OnStateEnter(curStateId, connectionParams);
            }

            // 현재 상태 업데이트
            curState?.OnStateUpdate(_deltaTime);
        }

        public int GetCurrentStateId() 
            => curState != null ? curState.StateId : -1;
    }
}
