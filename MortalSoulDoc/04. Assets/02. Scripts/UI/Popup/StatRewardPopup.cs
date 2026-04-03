using MS.Data;
using MS.Field;
using MS.Manager;
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
    public class StatRewardPopup : BasePopup
    {
        private PlayerStatInfo playerStatInfo;
        private PlayerCharacter player;

        private RectTransform statRewardContainer;
        private StatRewardInfoRow statRewardInfoRowTemplate;


        public void InitStatRewardPopup(List<StatRewardSettingData> _statRewardList, PlayerCharacter _player)
        {
            FindComponents();

            Time.timeScale = 0f;
            playerStatInfo.InitPlayerStatInfo(_player.SSC.AttributeSet);
            player = _player;

            foreach (Transform child in statRewardContainer)
            {
                if (child.gameObject == statRewardInfoRowTemplate.gameObject) continue; // ÅÛÇÃ¸´Àº ÆÄ±«ÇÏÁö ¸»°í ³ÀµÖ¾ßÇÔ
                Destroy(child.gameObject);
            }

            for (int i=0;i< _statRewardList.Count;i++)
            {
                StatRewardInfoRow rewardRow = Instantiate(statRewardInfoRowTemplate, statRewardContainer);
                rewardRow.gameObject.SetActive(true);
                rewardRow.InitStatRewardInfoRow(_statRewardList[i], OnBtnStatRewardClicked);
            }
        }

        private void FindComponents()
        {
            if (playerStatInfo != null) return;

            playerStatInfo = transform.FindChildComponentDeep<PlayerStatInfo>("PlayerStatInfo");

            statRewardContainer = transform.FindChildComponentDeep<RectTransform>("StatRewardContainer");
            statRewardInfoRowTemplate = transform.GetOrAddComponent<StatRewardInfoRow>("StatRewardInfoRowTemplate");
            statRewardInfoRowTemplate.gameObject.SetActive(false);
        }

        private void OnBtnStatRewardClicked(StatRewardSettingData _rewardData)
        {
            SoundManager.Instance.PlaySFX("FX_StatReward"); 
            Stat targetStat = player.SSC.AttributeSet.GetStatByType(_rewardData.StatType);
            if (targetStat != null)
            {
                targetStat.AddBaseValue(_rewardData.RewardValue);
            }

            Close();
        }

        public override void Close()
        {
            base.Close();

            EffectManager.Instance.StopEffectsByKey("Eff_Firework");
            Time.timeScale = 1f;
        }
    }


    public class StatRewardInfoRow : MonoBehaviour
    {
        public event Action<StatRewardSettingData> OnBtnStatRewardClicked;

        private Image ImgBg;
        private TextMeshProUGUI txtRewardType;
        private TextMeshProUGUI txtRewardValue;
        private Button btnStatReward;
        private StatRewardSettingData statRewardData;


        private void Awake()
        {
            txtRewardType = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtRewardType");
            txtRewardValue = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtRewardValue");
            ImgBg = GetComponent<Image>();
            btnStatReward = GetComponent<Button>();
            btnStatReward.onClick.AddListener(OnBtnStatRewardClickedCallback);
        }

        public void InitStatRewardInfoRow(StatRewardSettingData _rewardData, Action<StatRewardSettingData> _callback)
        {
            string statName = StringTable.Instance.Get("StatType", _rewardData.StatType.ToString());

            txtRewardType.text = statName;
            txtRewardValue.text = "+ " + _rewardData.RewardValue.ToString("0.#");

            txtRewardType.color = GlobalDefine.GradeColorDict[_rewardData.Grade];
            ImgBg.color = GlobalDefine.GradeColorDict[_rewardData.Grade];

            statRewardData = _rewardData;
            OnBtnStatRewardClicked += _callback;
        }

        private void OnBtnStatRewardClickedCallback()
        {
            OnBtnStatRewardClicked?.Invoke(statRewardData);
        }
    }
}