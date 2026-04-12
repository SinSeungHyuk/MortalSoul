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

    public enum EWeaponType
    {
        TwoHandSword,
        OneHandSword,
        Dagger,
        Bow,
        Staff
    }

    public enum EGameState
    {
        Title,
        Village,
        Dungeon
    }
}
