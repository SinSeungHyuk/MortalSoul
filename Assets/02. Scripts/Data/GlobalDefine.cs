using System;

namespace MS.Data
{
    public enum EGrade
    {
        Normal,
        Rare,
        Unique,
        Legendary
    }

    public enum EWeaponType
    {
        GreatSword,
        OneHandSword,
        Dagger,
        Bow,
        Staff
    }

    public enum EZoneType
    {
        Battle,
        Shop,
        Event,
        Boss
    }

    public enum ESkillValueType
    {
        Default,
        Damage,
        Knockback,
        Move,
        Buff,
        Duration,
        Casting
    }
}
