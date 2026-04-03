using MS.Skill;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MS.Field.FieldObject;


namespace MS.Data
{
    [Serializable]
    public class SkillSettingData
    {
        public FieldObjectType OwnerType { get; set; }
        public string IconKey { get; set; }
        public List<string> CategoryKeyList { get; set; }
        public EDamageAttributeType AttributeType { get; set; }
        public float Cooltime { get; set; }
        public bool IsPostUseCooltime { get; set; } // 스킬을 사용한 후에 쿨타임이 시작되는지 여부
        public Dictionary<ESkillValueType, float> SkillValueDict { get; set; }
        
        public float GetValue(ESkillValueType _valueType)
        {
            SkillValueDict.TryGetValue(_valueType, out float value);
            return value;
        }
    }

    public enum ESkillValueType
    {
        Default,            // 기본 수치 (예: 기본데미지 n)
        Damage,             // 기본 공격력 대비 데미지 계수 (예: 1.5)
        Knockback,          // 넉백 수치
        Move,               // 이동 스킬의 이동 거리
        Buff,               // 버프 효과 배율 (예: 15% 증가)
        Duration,           // 지속 시간 (예: 버프 지속 시간)
        Casting,            // 캐스팅 시간
    }
}
