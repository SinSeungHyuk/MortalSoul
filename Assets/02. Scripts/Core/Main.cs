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

        protected override void Awake()
        {
            base.Awake();

            DataManager = new DataManager();
            AddressableManager = new AddressableManager();
            UIManager = new UIManager();
            SoundManager = new SoundManager();
            ObjectPoolManager = new ObjectPoolManager();
            PlayerManager = new PlayerManager();
            MonsterManager = new MonsterManager();
        }
    }
}
