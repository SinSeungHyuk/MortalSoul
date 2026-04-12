using UnityEngine;
using MS.Field;

namespace Core
{
    public class Main : MonoSingleton<Main>
    {
        public DataManager DataManager { get; private set; }
        public AddressableManager AddressableManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public SoundManager SoundManager { get; private set; }
        public ObjectPoolManager ObjectPoolManager { get; private set; }
        public EffectManager EffectManager { get; private set; }
        public MonsterManager MonsterManager { get; private set; }
        public BattleObjectManager BattleObjectManager { get; private set; }
        public GameManager GameManager { get; private set; }
        public PlayerCharacter Player { get; set; }

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
            EffectManager = new EffectManager();
            MonsterManager = new MonsterManager();
            BattleObjectManager = new BattleObjectManager();

            GameManager = new GameManager();
            GameManager.InitGameManager();
        }

        private void Start()
        {
            GameManager.StartGame();
        }

        private void Update()
        {
            GameManager.OnUpdate(Time.deltaTime);
        }

        protected override void OnDestroy()
        {
            BattleObjectManager?.ClearBattleObject();
            EffectManager?.ClearEffect();
            ObjectPoolManager?.ClearAllPools();
            SoundManager?.ClearAllSounds();
            AddressableManager?.ReleaseAll();
            DataManager?.ReleaseGameData();

            base.OnDestroy();
        }
    }
}
