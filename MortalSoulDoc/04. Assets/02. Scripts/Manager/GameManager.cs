using Core;
using Cysharp.Threading.Tasks;
using MS.Field;
using MS.Mode;
using MS.UI;
using System;
using UnityEngine;

namespace MS.Manager
{
    public partial class GameManager : MonoSingleton<GameManager>
    {
        private GameModeBase curGameMode;

        public GameModeBase CurGameMode => curGameMode;


        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = 60;
            InitializeGooglePlayAutoLogin();
        }

        private void Update()
        {
            if (curGameMode != null)
                curGameMode.OnUpdate(Time.deltaTime);
        }

        public async UniTask StartGameAsync()
        {
            try
            {
                UIManager.Instance.ShowSystemUI<BaseUI>("LoadingPanel");

                await DataManager.Instance.LoadAllGameSettingDataAsync();
                await StringTable.Instance.LoadStringTable();
                await GameplayCueManager.Instance.LoadAllGameplayCueAsync();
                await UIManager.Instance.LoadAllUIPrefabAsync();
                await SoundManager.Instance.LoadAllSoundAsync();

                ChangeMode(new LobbyMode());

                UIManager.Instance.CloseUI("LoadingPanel");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void ChangeMode(GameModeBase _mode)
        {
            if (curGameMode != null) curGameMode.EndMode();
            curGameMode = _mode;
            curGameMode.StartMode();
        }
    }
}
