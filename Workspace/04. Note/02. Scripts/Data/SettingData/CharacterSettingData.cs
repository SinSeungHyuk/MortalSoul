using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class GameCharacterSettingData
    {
        public LevelSettingData LevelSettingData { get; set; }
        public Dictionary<string, CharacterSettingData> CharacterSettingDataDict { get; set; }
    }

    [Serializable]
    public class LevelSettingData
    {
        public float BaseExp { get; set; }
        public float IncreaseExpPerLevel { get; set; }
    }

    [Serializable]
    public class CharacterSettingData
    {
        public AttributeSetSettingData AttributeSetSettingData { get; set; }
        public string DefaultSkillKey { get; set; }
        public string DefaultWeaponKey { get; set; }
    }

    [Serializable]
    public class AttributeSetSettingData
    {
        public float MaxHealth { get; set; }
        public float AttackPower { get; set; }
        public float Defense { get; set; }
        public float Evasion { get; set; }
        public float MoveSpeed { get; set; }
        public float CriticChance { get; set; }
        public float CriticMultiple { get; set; }
        public float LifeSteal { get; set; }
        public float CooltimeAccel { get; set; }
        public float ProjectileCount { get; set; }
        public float AreaRangeMultiple { get; set; }
        public float KnockbackMultiple { get; set; }
        public float CoinMultiple { get; set; }
    }
}