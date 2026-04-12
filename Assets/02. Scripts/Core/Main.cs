using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class Main : MonoSingleton<Main>
    {
        public DataManager DataManager { get; private set; }
        public AddressableManager AddressableManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public SoundManager SoundManager { get; private set; }
        public ObjectPoolManager ObjectPoolManager { get; private set; }
        public MonsterManager MonsterManager { get; private set; }
        public BattleObjectManager BattleObjectManager { get; private set; }


        // TODO :: TEST
        public bool IsBootCompleted { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            DataManager = new DataManager();
            AddressableManager = new AddressableManager();
            UIManager = new UIManager();
            UIManager.InitUIManager(transform);
            SoundManager = new SoundManager();

            Transform poolContainer = new GameObject("ObjectPool").transform;
            poolContainer.SetParent(transform);

            ObjectPoolManager = new ObjectPoolManager();
            ObjectPoolManager.InitObjectPoolManager(poolContainer);
            MonsterManager = new MonsterManager();
            BattleObjectManager = new BattleObjectManager();
        }

        private void Update()
        {
            BattleObjectManager.OnUpdate(Time.deltaTime);
        }

        private void Start()
        {
            TESTBootAsync().Forget();
        }

        protected override void OnDestroy()
        {
            BattleObjectManager?.ClearBattleObject();
            ObjectPoolManager?.ClearAllPools();
            SoundManager?.ClearAllSounds();
            AddressableManager?.ReleaseAll();
            DataManager?.ReleaseGameData();

            base.OnDestroy();
        }

        private async UniTaskVoid TESTBootAsync()
        {
            await DataManager.SettingData.LoadAllSettingDataAsync();

            // TODO: 던전 입장 타이밍에 InitGameData(), 종료/복귀 시 ReleaseGameData()로 이동. 지금은 부팅 직후 임시 생성.
            DataManager.InitGameData();

            IsBootCompleted = true;
            Debug.Log("[Main] Boot completed");
        }
    }
}
