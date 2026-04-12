using MS.Manager;
using MS.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace MS.UI
{
    public class PlayerStatInfo : MonoBehaviour
    {
        private RectTransform statRowContainer;
        private PlayerStatInfoRow statInfoRowTemplate;


        public void InitPlayerStatInfo(BaseAttributeSet _attributeSet)
        {
            FindComponents();

            foreach (Transform child in statRowContainer)
            {
                if (child.gameObject == statInfoRowTemplate.gameObject) continue; // 템플릿은 파괴하지 말고 냅둬야함
                Destroy(child.gameObject);
            }

            foreach (EStatType statType in Enum.GetValues(typeof(EStatType)))
            {
                Stat targetStat = _attributeSet.GetStatByType(statType);
                if (targetStat == null) continue;

                // 몬스터 전용 스탯 등 표시하지 않을 예외 처리
                if (statType == EStatType.AttackRange) continue;

                PlayerStatInfoRow newRowObj = Instantiate(statInfoRowTemplate, statRowContainer);
                newRowObj.gameObject.SetActive(true);
                float statValue = targetStat.Value;
                newRowObj.InitPlayerStatInfoRow(statType, statValue);
            }
        }

        private void FindComponents()
        {
            if (statRowContainer != null) return;

            statRowContainer = transform.FindChildComponentDeep<RectTransform>("StatRowContainer");
            statInfoRowTemplate = transform.GetOrAddComponent<PlayerStatInfoRow>("PlayerStatInfoRowTemplate");
            statInfoRowTemplate.gameObject.SetActive(false);
        }
    }

    public class PlayerStatInfoRow : MonoBehaviour, IPointerClickHandler
    {
        private TextMeshProUGUI txtStatType;
        private TextMeshProUGUI txtStatValue;

        private string curStatType;
        private float scalingStatValue;
        

        private void Awake()
        {
            txtStatType = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtStatType");
            txtStatValue = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtStatValue");
        }

        public void InitPlayerStatInfoRow(EStatType _statType, float _statValue)
        {
            curStatType = _statType.ToString();
            scalingStatValue = MathUtils.BattleScaling(_statValue);

            string statName = StringTable.Instance.Get("StatType", curStatType);
            if (string.IsNullOrEmpty(statName)) statName = curStatType;

            txtStatType.text = statName;
            txtStatValue.text = _statValue.ToString("0.##");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Tooltip tooltip = UIManager.Instance.ShowSystemUI<Tooltip>("Tooltip");
            tooltip.InitTooltip(curStatType, eventData.position, scalingStatValue);
        }
    }
}