using MS.Data;
using MS.Manager;
using System.Text;
using UnityEngine;

namespace MS.Utils
{
    public static class DataUtils
    {
        public static string GetSkillDesc(string _skillKey)
        {
            string rawDesc = StringTable.Instance.Get("SkillDesc", _skillKey);

            if (string.IsNullOrEmpty(rawDesc))
                return "";

            SkillSettingData skillData = DataManager.Instance.SkillSettingDataDict[_skillKey];
            if (skillData == null)
                return rawDesc;

            StringBuilder sb = new StringBuilder(rawDesc);
            if (skillData.SkillValueDict != null)
            {
                foreach (var kvp in skillData.SkillValueDict)
                {
                    string placeholder = "{" + kvp.Key.ToString() + "}";
                    string coloredValue = $"<color=#FFD700>{kvp.Value}</color>";
                    sb.Replace(placeholder, coloredValue);
                }
            }

            return sb.ToString();
        }
    }
}