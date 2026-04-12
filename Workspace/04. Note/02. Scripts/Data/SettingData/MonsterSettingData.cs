using MS.Skill;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Data
{
    [Serializable]
    public class MonsterSettingData
    {
        public MonsterAttributeSetSettingData AttributeSetSettingData { get; set; }
        public string DropItemKey { get; set; }
        public List<MonsterSkillSettingData> SkillList { get; set; } // 장착중인 기본스킬 리스트
    }

    [Serializable]
    public class MonsterAttributeSetSettingData
    {
        public float MaxHealth { get; set; }
        public float AttackPower { get; set; }
        public float Defense { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackRange { get; set; }
        public EDamageAttributeType WeaknessAttributeType { get; set; } // 약점 속성
    }

    [Serializable]
    public class MonsterSkillSettingData
    {
        public string SkillKey { get; set; }
        public int SkillActivateRate { get; set; } // 스킬의 발동 확률
        public string AnimTriggerKey { get; set; } // 스킬의 애니메이터 트리거 키
        public float SkillDuration { get; set; } // 스킬 사용 시간 (이 시간동안 공격상태에 머무름)
    }
}
