using MS.Manager;
using MS.Mode;
using MS.Utils;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MS.UI
{
    public class MainPanel : BaseUI
    {
        private Canvas canvas;

        private Button btnSurvivalMode;
        private Button btnHelp;
        private Button btnOption;
        private Button btnExit;


        public void InitMainPanel()
        {
            FindUIComponents();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }

        private void FindUIComponents()
        {
            if (canvas != null) return; 
            canvas = GetComponent<Canvas>();
            
            btnSurvivalMode = transform.FindChildComponentDeep<Button>("BtnSurvivalMode");
            btnHelp = transform.FindChildComponentDeep<Button>("BtnHelp");
            btnOption = transform.FindChildComponentDeep<Button>("BtnOption");
            btnExit = transform.FindChildComponentDeep<Button>("BtnExit");

            btnSurvivalMode.onClick.AddListener(OnBtnSurvivalModeClicked);
            btnHelp.onClick.AddListener(OnBtnHelpClicked);
            btnOption.onClick.AddListener(OnBtnOptionClicked);
            btnExit.onClick.AddListener(OnBtnExitClicked);
        }


        private void OnBtnSurvivalModeClicked()
        {
            GameManager.Instance.ChangeMode(new SurvivalMode(DataManager.Instance.StageSettingDataDict["Stage1"]));
        }

        private void OnBtnHelpClicked()
        {
            
        }

        private void OnBtnOptionClicked()
        {
            
        }

        private void OnBtnExitClicked()
        {
            Application.Quit();
        }
    }
}