using Cysharp.Threading.Tasks;
using MS.Core.StateMachine;
using MS.Data;
using MS.Field;
using UnityEngine;
using UnityEngine.Video;

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

        private void OnTitleEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Title 진입");
            BootAsync().Forget();
        }

        private async UniTaskVoid BootAsync()
        {
            await Main.Instance.DataManager.SettingData.LoadAllSettingDataAsync();
            await Main.Instance.MonsterManager.LoadAllMonsterAsync();

            Debug.Log("[GameManager] SettingData 로드 완료 → Village 전환");
            stateMachine.TransitState((int)EGameState.Village);
        }

        private void OnVillageEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Village 진입");

            if (Main.Instance.Player == null)
                Main.Instance.Player = Object.FindAnyObjectByType<PlayerCharacter>();

            Main.Instance.Player.InitPlayer("test");
            Main.Instance.Player.GainSubSoul("test2");

            // TODO :: TEST
            Main.Instance.MonsterManager.SpawnMonster("MonsterDog", Vector3.zero);
            Main.Instance.test.InitTest();
        }

        private void OnVillageUpdate(float _deltaTime)
        {
            Main.Instance.EffectManager.OnUpdate(_deltaTime);
            Main.Instance.BattleObjectManager.OnUpdate(_deltaTime);
            Main.Instance.MonsterManager.OnUpdate(_deltaTime);
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
