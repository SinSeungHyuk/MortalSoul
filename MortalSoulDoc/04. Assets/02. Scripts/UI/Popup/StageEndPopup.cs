using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Mode;
using MS.Skill;
using MS.UI;
using MS.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace MS.UI
{
    public class StageEndPopup : BasePopup
    {
        private PlayerStatInfo playerStatInfo;
        private PlayerCharacter player;

        private TextMeshProUGUI txtTitle;
        private Button btnExit;

        // PlayerStatisticsContainer
        private TextMeshProUGUI txtKillCount;
        private TextMeshProUGUI txtPlayerLevel;
        private TextMeshProUGUI txtGold;

        // SkillStatisticsContainer
        private SkillInfoRow skillInfoRowTemplate;
        private RectTransform skillContainer;


        public void InitStageEndPopup(StageStatisticsData _stageData,PlayerCharacter _player)
        {
            FindComponents();

            Time.timeScale = 0f;
            playerStatInfo.InitPlayerStatInfo(_player.SSC.AttributeSet);
            player = _player;

            string title = _stageData.IsClear ? "스테이지 성공!" : "스테이지 실패";
            txtTitle.SetText(title);
            txtKillCount.SetText(_stageData.KillCount.ToString("F0"));
            txtPlayerLevel.SetText(_stageData.PlayerLevel.ToString("F0"));
            txtGold.SetText(_stageData.Gold.ToString("F0"));

            foreach (Transform child in skillContainer)
            {
                if (child.gameObject == skillInfoRowTemplate.gameObject) continue; // 템플릿은 파괴하지 말고 냅둬야함
                Destroy(child.gameObject);
            }
            var skillList = _stageData.SkillStatList;
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
            txtTitle = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtTitle");
            btnExit = transform.FindChildComponentDeep<Button>("BtnExit");
            txtKillCount = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtKillCount");
            txtPlayerLevel = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtPlayerLevel");
            txtGold = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtGold");
            skillContainer = transform.FindChildComponentDeep<RectTransform>("SkillContainer");
            skillInfoRowTemplate = transform.GetOrAddComponent<SkillInfoRow>("SkillInfoRowTemplate");
            skillInfoRowTemplate.gameObject.SetActive(false);

            btnExit.onClick.AddListener(OnBtnExitClicked);
        }

        private void OnBtnExitClicked()
        {
            Time.timeScale = 1f;
            EffectManager.Instance.StopEffectsByKey("Eff_Firework");
            GameManager.Instance.ChangeMode(new LobbyMode());

            Close();
        }
    }


    public class SkillInfoRow : MonoBehaviour
    {
        private Image imgIcon;
        private TextMeshProUGUI txtSkillName;
        private TextMeshProUGUI txtTotalDamage;
        private TextMeshProUGUI txtDPS;


        public void InitSkillInfoRow(SkillStatisticsInfo _skillData)
        {
            imgIcon = transform.FindChildComponentDeep<Image>("ImgSkillIcon");
            txtSkillName = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtSkillName");
            txtTotalDamage = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtTotalDamage");
            txtDPS = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtDPS");

            string skillName = StringTable.Instance.Get("SkillName", _skillData.SkillKey);
            txtSkillName.SetText(skillName);

            Sprite icon = AddressableManager.Instance.LoadResource<Sprite>(_skillData.IconKey);
            imgIcon.sprite = icon;

            txtTotalDamage.SetText(_skillData.TotalDamage.ToString("F0"));
            txtDPS.SetText(_skillData.DPS.ToString("F0"));
        }
    }
}
