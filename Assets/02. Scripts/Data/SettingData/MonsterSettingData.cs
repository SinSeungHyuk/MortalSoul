using MS.Battle;
using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class MonsterSettingData
    {
        public MonsterAttributeSetSettingData AttributeSetSettingData { get; set; }
        public string DropItemKey { get; set; }
        public List<MonsterSkillSettingData> SkillList { get; set; }
    }

    [Serializable]
    public class MonsterAttributeSetSettingData
    {
        public float MaxHealth { get; set; }
        public float AttackPower { get; set; }
        public float Defense { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackRange { get; set; }
        public EDamageAttributeType WeaknessAttributeType { get; set; }
    }

    [Serializable]
    public class MonsterSkillSettingData
    {
        public string SkillKey { get; set; }
        public int SkillActivateRate { get; set; }
        public string AnimTriggerKey { get; set; }
        public float SkillDuration { get; set; }
    }
}
