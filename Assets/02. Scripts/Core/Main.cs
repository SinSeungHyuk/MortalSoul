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
        public PlayerManager PlayerManager { get; private set; }
        public MonsterManager MonsterManager { get; private set; }
        public BattleObjectManager BattleObjectManager { get; private set; }

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
            PlayerManager = new PlayerManager();
            MonsterManager = new MonsterManager();
            BattleObjectManager = new BattleObjectManager();
        }

        private void Update()
        {
            BattleObjectManager.OnUpdate(Time.deltaTime);
        }

        private void Start()
        {
            BootAsync().Forget();
        }

        private async UniTaskVoid BootAsync()
        {
            await DataManager.SettingData.LoadAllSettingDataAsync();
            IsBootCompleted = true;
            Debug.Log("[Main] Boot completed");
        }
    }
}
