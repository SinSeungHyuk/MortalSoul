using System;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Data
{
    [Serializable]
    public class ArtifactSettingData
    {
        public string IconKey { get; set; }

        public ArtifactTriggerType TriggerType { get; set; }

        public List<ArtifactConditionData> ConditionList { get; set; }
        public List<ArtifactActionData> ActionList { get; set; }
    }

    [Serializable]
    public class ArtifactConditionData
    {
        public ArtifactConditionType ConditionType { get; set; }
        public string StringVal { get; set; }  // 스킬 이름, 아이템 키 등
        public float FloatVal { get; set; }    // 확률 등
    }

    [Serializable]
    public class ArtifactActionData
    {
        public ArtifactActionType ActionType { get; set; }
        public string StringVal { get; set; }     // 스킬 키, 스탯 종류 등
        public float FloatVal { get; set; }       // 버프 수치, 데미지 계수 등
    }

    public enum ArtifactTriggerType
    {
        OnAcquire,
        OnSkillUse,
        OnItemAcquire,
        OnHit,
    }

    public enum ArtifactConditionType
    {
        RandPercent,
        SkillNameEqual,
        AcquireItem,
    }

    public enum ArtifactActionType
    {
        SpawnStar,
        SpawnLightningAura,
        KnockbackMonster,
        StarAura,
    }
}