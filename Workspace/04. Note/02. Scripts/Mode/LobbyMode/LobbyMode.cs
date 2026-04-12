using MS.Manager;
using MS.UI;
using System;
using UnityEngine;


namespace MS.Mode
{
    public class LobbyMode : GameModeBase
    {

        public enum LobbyModeState
        {
            MainMenu,
        }
        protected override void OnRegisterStates()
        {
            modeStateMachine.RegisterState((int)LobbyModeState.MainMenu, OnMainMenuEnter, OnMainMenuUpdate, OnMainMenuExit);
            modeStateMachine.TransitState((int)LobbyModeState.MainMenu);
        }

        public override void EndMode()
        {
            SoundManager.Instance.StopBGM();
        }


        #region MainMenu State
        private void OnMainMenuEnter(int arg1, object[] arg2)
        {
            MainPanel MainPanel = UIManager.Instance.ShowView<MainPanel>("MainPanel");
            MainPanel.InitMainPanel();

            SoundManager.Instance.PlayBGM("BGM_Lobby");
        }
        private void OnMainMenuUpdate(float obj)
        {
            
        }
        private void OnMainMenuExit(int obj)
        {
            
        }
        #endregion
    }
}