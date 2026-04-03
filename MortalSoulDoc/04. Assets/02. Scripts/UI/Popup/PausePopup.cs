using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Mode;
using MS.UI;
using MS.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MS.UI
{
    public class PausePopup : BasePopup
    {
        private PlayerStatInfo playerStatInfo;
        private PlayerCharacter player;

        private Slider BGMSlider;
        private Slider SFXSlider;

        private Button btnResume;
        private Button btnExit;

        // SkillStatisticsContainer
        private SkillInfoRow skillInfoRowTemplate;
        private RectTransform skillContainer;


        public void InitPausePopup()
        {
            FindComponents();

            Time.timeScale = 0f;

            player = PlayerManager.Instance.Player;
            playerStatInfo.InitPlayerStatInfo(player.SSC.AttributeSet);

            foreach (Transform child in skillContainer)
            {
                if (child.gameObject == skillInfoRowTemplate.gameObject) continue; // ÅÛÇÃ¸´Àº ÆÄ±«ÇÏÁö ¸»°í ³ÀµÖ¾ßÇÔ
                Destroy(child.gameObject);
            }
            List<SkillStatisticsInfo> skillList = player.SSC.GetSkillStatistics();
            for (int i = 0; i < skillList.Count; i++)
            {
                SkillInfoRow skillRow = Instantiate(skillInfoRowTemplate, skillContainer);
                skillRow.gameObject.SetActive(true);
                skillRow.InitSkillInfoRow(skillList[i]);
            }
        }

        private void FindComponents()
        {
            if (playerStatInfo != null) return;

            playerStatInfo = transform.FindChildComponentDeep<PlayerStatInfo>("PlayerStatInfo");
            BGMSlider = transform.FindChildComponentDeep<Slider>("BGMSlider");
            SFXSlider = transform.FindChildComponentDeep<Slider>("SFXSlider");
            skillContainer = transform.FindChildComponentDeep<RectTransform>("SkillContainer");
            skillInfoRowTemplate = transform.GetOrAddComponent<SkillInfoRow>("SkillInfoRowTemplate");
            skillInfoRowTemplate.gameObject.SetActive(false);

            BGMSlider.onValueChanged.AddListener((value) =>
            {
                SoundManager.Instance.SetBGMVolume(value);
            });
            SFXSlider.onValueChanged.AddListener((value) =>
            {
                SoundManager.Instance.SetSFXVolume(value);
            });

            btnResume = transform.FindChildComponentDeep<Button>("BtnResume");
            btnResume.onClick.AddListener(() =>
            {
                Close();
            });

            btnExit = transform.FindChildComponentDeep<Button>("BtnExit"); 
            btnExit.onClick.AddListener(() =>
            {
                Close();
                GameManager.Instance.ChangeMode(new LobbyMode());
            });
        }



        public override void Close()
        {
            base.Close();

            Time.timeScale = 1f;
        }
    }
}