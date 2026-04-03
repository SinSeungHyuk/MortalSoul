using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MS.UI
{
    public class SkillRewardPopup : BasePopup
    {
        private PlayerCharacter player;

        private RectTransform skillRewardContainer;
        private SkillRewardInfoRow skillRewardInfoRowTemplate;


        public void InitSkillRewardPopup(List<string> _skillRewardList, PlayerCharacter _player)
        {
            FindComponents();

            Time.timeScale = 0f;
            player = _player;

            foreach (Transform child in skillRewardContainer)
            {
                if (child.gameObject == skillRewardInfoRowTemplate.gameObject) continue; // ÅÛÇÃ¸´Àº ÆÄ±«ÇÏÁö ¸»°í ³ÀµÖ¾ßÇÔ
                Destroy(child.gameObject);
            }

            for (int i = 0; i < _skillRewardList.Count; i++)
            {
                SkillRewardInfoRow rewardRow = Instantiate(skillRewardInfoRowTemplate, skillRewardContainer);
                rewardRow.gameObject.SetActive(true);
                rewardRow.InitSkillRewardInfoRow(_skillRewardList[i], OnBtnSkillRewardClicked);
            }
        }

        private void FindComponents()
        {
            if (skillRewardContainer != null) return;

            skillRewardContainer = transform.FindChildComponentDeep<RectTransform>("SkillRewardContainer");
            skillRewardInfoRowTemplate = transform.GetOrAddComponent<SkillRewardInfoRow>("SkillRewardInfoRowTemplate");
            skillRewardInfoRowTemplate.gameObject.SetActive(false);
        }

        private void OnBtnSkillRewardClicked(string _skillKey)
        {
            player.SSC.GiveSkill(_skillKey);

            Close();
        }

        public override void Close()
        {
            base.Close();

            Time.timeScale = 1f;
        }
    }


    public class SkillRewardInfoRow : MonoBehaviour
    {
        public event Action<string> OnBtnSkillRewardClicked;

        private Image imgSkillIcon;
        private TextMeshProUGUI txtSkillName;
        private TextMeshProUGUI txtSkillDesc;
        private RectTransform categoryContainer;
        private CategoryRow categoryTemplate;
        private Button btnStatReward;
        private string skillKey;


        private void Awake()
        {
            txtSkillName = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtSkillName");
            txtSkillDesc = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtSkillDesc");
            categoryContainer = transform.FindChildComponentDeep<RectTransform>("CategoryContainer");
            categoryTemplate = transform.GetOrAddComponent<CategoryRow>("CategoryTemplate");
            categoryTemplate.gameObject.SetActive(false);
            imgSkillIcon = transform.FindChildComponentDeep<Image>("ImgSkillIcon");
            btnStatReward = GetComponent<Button>();

            if (btnStatReward != null)
            {
                btnStatReward.onClick.RemoveListener(OnBtnSkillRewardClickedCallback);
                btnStatReward.onClick.AddListener(OnBtnSkillRewardClickedCallback);
            }
        }

        public void InitSkillRewardInfoRow(string _skillKey, Action<string> _callback)
        {
            var skillData = DataManager.Instance.SkillSettingDataDict[_skillKey];

            txtSkillName.text = StringTable.Instance.Get("SkillName", _skillKey);
            txtSkillDesc.text = DataUtils.GetSkillDesc(_skillKey);

            for (int i = 0; i < skillData.CategoryKeyList.Count; i++)
            {
                CategoryRow rewardRow = Instantiate(categoryTemplate, categoryContainer);
                rewardRow.gameObject.SetActive(true);
                string category = StringTable.Instance.Get("SkillCategory", skillData.CategoryKeyList[i]);
                rewardRow.InitCategoryRow(category);
            }
            CategoryRow attributeRow = Instantiate(categoryTemplate, categoryContainer);
            attributeRow.gameObject.SetActive(true);
            string attribute = StringTable.Instance.Get("AttributeType", skillData.AttributeType.ToString());
            attributeRow.InitCategoryRow(attribute);

            Sprite icon = AddressableManager.Instance.LoadResource<Sprite>(skillData.IconKey);
            imgSkillIcon.sprite = icon;

            skillKey = _skillKey;

            OnBtnSkillRewardClicked = null;
            OnBtnSkillRewardClicked += _callback;
        }

        private void OnBtnSkillRewardClickedCallback()
        {
            OnBtnSkillRewardClicked?.Invoke(skillKey);
        }
    }

    public class CategoryRow : MonoBehaviour
    {
        private TextMeshProUGUI txtCategory;

        private void Awake()
        {
            txtCategory = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtCategory");
        }

        public void InitCategoryRow(string _category)
        {
            txtCategory.text = _category;
        }
    }
}