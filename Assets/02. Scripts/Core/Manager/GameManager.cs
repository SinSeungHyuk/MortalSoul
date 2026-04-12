using Cysharp.Threading.Tasks;
using MS.Core.StateMachine;
using MS.Data;
using UnityEngine;

namespace Core
{
    public class GameManager
    {
        private MSStateMachine<GameManager> stateMachine;

        public void InitGameManager()
        {
            stateMachine = new MSStateMachine<GameManager>(this);

            stateMachine.RegisterState((int)EGameState.Title,
                OnTitleEnter, null, null);

            stateMachine.RegisterState((int)EGameState.Village,
                OnVillageEnter, OnVillageUpdate, null);

            stateMachine.RegisterState((int)EGameState.Dungeon,
                OnDungeonEnter, OnDungeonUpdate, OnDungeonExit);
        }

        public void StartGame()
        {
            stateMachine.TransitState((int)EGameState.Title);
        }

        public void OnUpdate(float _deltaTime)
        {
            stateMachine.OnUpdate(_deltaTime);
        }

        public void TransitState(EGameState _state)
        {
            stateMachine.TransitState((int)_state);
        }

        public EGameState GetCurState()
        {
            return (EGameState)stateMachine.GetCurrentStateId();
        }

        private void OnTitleEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Title 진입");
            BootAsync().Forget();
        }

        private async UniTaskVoid BootAsync()
        {
            await Main.Instance.DataManager.SettingData.LoadAllSettingDataAsync();

            Debug.Log("[GameManager] SettingData 로드 완료 → Village 전환");
            TransitState(EGameState.Village);
        }

        private void OnVillageEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Village 진입");
        }

        private void OnVillageUpdate(float _deltaTime)
        {
            Main.Instance.EffectManager.OnUpdate(_deltaTime);
            Main.Instance.BattleObjectManager.OnUpdate(_deltaTime);
        }

        private void OnDungeonEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Dungeon 진입");
            Main.Instance.DataManager.InitGameData();
        }

        private void OnDungeonUpdate(float _deltaTime)
        {
            Main.Instance.EffectManager.OnUpdate(_deltaTime);
            Main.Instance.BattleObjectManager.OnUpdate(_deltaTime);
        }

        private void OnDungeonExit(int _nextStateId)
        {
            Debug.Log("[GameManager] Dungeon 퇴장");
            Main.Instance.BattleObjectManager.ClearBattleObject();
            Main.Instance.EffectManager.ClearEffect();
            Main.Instance.DataManager.ReleaseGameData();
        }
    }
}
