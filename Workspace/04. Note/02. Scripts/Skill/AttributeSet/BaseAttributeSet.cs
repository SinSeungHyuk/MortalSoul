using MS.Data;
using MS.Skill;
using System;
using System.Collections.Generic;
using UnityEngine;


public enum EStatType
{
    MaxHealth,
    AttackPower,
    Defense,
    Evasion,
    CriticChance,
    CriticMultiple,
    LifeSteal,
    CooltimeAccel,
    ProjectileCount,
    AreaRangeMultiple,
    KnockbackMultiple,
    CoinMultiple,
    MoveSpeed,


    AttackRange,    // 몬스터 전용 스탯
}

public class BaseAttributeSet
{
    // 현재체력, 최대체력
    public event Action<float, float> OnHealthChanged;

    private float health;

    protected Dictionary<EStatType, Stat> statDict = new Dictionary<EStatType, Stat>();

    public Stat MaxHealth { get; protected set; }
    public Stat AttackPower { get; protected set; } 
    public Stat Defense { get; protected set; }
    public Stat MoveSpeed { get; protected set; }
    public EDamageAttributeType WeaknessAttributeType { get; protected set; } // 약점 속성


    public float Health
    {
        get => health;
        set
        {
            float clampedValue = Mathf.Clamp(value, 0, MaxHealth.Value);
            if (health != clampedValue)
            {
                health = clampedValue;
                OnHealthChanged?.Invoke(health, MaxHealth.Value);
            }
        }
    }

    public float HealthRatio => Health / MaxHealth.Value;


    #region Util
    public Stat GetStatByType(EStatType _type)
    {
        if (statDict.TryGetValue(_type, out Stat _stat))
            return _stat;
        return null;
    }
    public float GetStatValueByType(EStatType _type)
    {
        if (statDict.TryGetValue(_type, out Stat _stat))
            return _stat.Value;
        return -1f;
    }
    #endregion
}
