using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class GameCharacterSettingData
    {
        public LevelSettingData LevelSettingData { get; set; }
        public Dictionary<string, CharacterSettingData> CharacterSettingDataDict { get; set; }


        public CharacterSettingData GetSoulSettingData(string _soulKey)
        {
            CharacterSettingDataDict.TryGetValue(_soulKey, out CharacterSettingData data);
            return data;
        }
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
        public EGrade Grade { get; set; }
        public PlayerAttributeSetSettingData AttributeSetSettingData { get; set; }
        public List<string> SkinKeys { get; set; }
        public EWeaponType WeaponType { get; set; }
        public List<string> SkillKeys { get; set; }
        public string SwitchingEffectKey { get; set; }
        public string SubPassiveKey { get; set; }
    }

    [Serializable]
    public class PlayerAttributeSetSettingData
    {
        public float MaxHealth { get; set; }
        public float BaseAttackPower { get; set; }
        public float SkillAttackPower { get; set; }
        public float Defense { get; set; }
        public float MoveSpeed { get; set; }
        public float CriticChance { get; set; }
        public float CriticMultiple { get; set; }
        public float Evasion { get; set; }
        public float LifeSteal { get; set; }
        public float CooltimeAccel { get; set; }
        public float AttackSpeed { get; set; }
    }
}
