using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class MonsterSettingData
    {
        public AttributeSetSettingData AttributeSetSettingData { get; set; }
        public string DropItemKey { get; set; }
        public List<MonsterSkillSettingData> SkillList { get; set; }
    }

    [Serializable]
    public class MonsterSkillSettingData
    {
        public string SkillKey { get; set; }
        public int SkillActivateRate { get; set; }
    }
}
