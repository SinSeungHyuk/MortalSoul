using MS.Battle;
using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class SkillSettingData
    {
        public string IconKey { get; set; }
        public List<string> CategoryKeyList { get; set; }
        public EDamageAttributeType AttributeType { get; set; }
        public float Cooltime { get; set; }
        public bool IsPostUseCooltime { get; set; }
        public Dictionary<ESkillValueType, float> SkillValueDict { get; set; }

        public float GetValue(ESkillValueType _valueType)
        {
            if (SkillValueDict != null && SkillValueDict.TryGetValue(_valueType, out float value))
                return value;
            return 0f;
        }
    }
}
