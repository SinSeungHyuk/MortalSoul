using System;
using System.Collections.Generic;

namespace MS.Data
{
    public enum EWeaponType
    {
        GreatSword,
        OneHandSword,
        Dagger,
        Bow,
        Staff
    }

    [Serializable]
    public class WeaponSettingData
    {
        public List<AttackComboData> ComboList { get; set; }
    }

    [Serializable]
    public class AttackComboData
    {
        public string AnimKey { get; set; }
        public float DamageMultiplier { get; set; }
        public float HitRange { get; set; }
        public float HitOffset { get; set; }
        public float Knockback { get; set; }
    }
}
