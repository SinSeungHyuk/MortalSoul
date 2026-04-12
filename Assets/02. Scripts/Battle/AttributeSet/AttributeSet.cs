using System;
using System.Collections.Generic;
using MS.Data;
using UnityEngine;

namespace MS.Battle
{
    public class AttributeSet
    {
        // cur, max
        public event Action<float, float> OnHealthChanged;

        protected Dictionary<EStatType, Stat> statDict = new Dictionary<EStatType, Stat>();

        public Stat MaxHealth { get; protected set; }
        public Stat BaseAttackPower { get; protected set; }
        public Stat SkillAttackPower { get; protected set; }
        public Stat Defense { get; protected set; }
        public Stat MoveSpeed { get; protected set; }
        public Stat CriticChance { get; protected set; }
        public Stat CriticMultiple { get; protected set; }
        public Stat Evasion { get; protected set; }
        public Stat LifeSteal { get; protected set; }
        public Stat CooltimeAccel { get; protected set; }
        public Stat AttackSpeed { get; protected set; }
        public Stat AttackRange { get; protected set; }
        public EDamageAttributeType WeaknessAttributeType { get; set; }

        private float health;
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

        public float HealthRatio => MaxHealth.Value > 0 ? Health / MaxHealth.Value : 0f;

        public void InitAttributeSet(AttributeSetSettingData _data)
        {
            MaxHealth = new Stat(_data.MaxHealth);
            BaseAttackPower = new Stat(_data.BaseAttackPower);
            SkillAttackPower = new Stat(_data.SkillAttackPower);
            Defense = new Stat(_data.Defense);
            MoveSpeed = new Stat(_data.MoveSpeed);
            CriticChance = new Stat(_data.CriticChance);
            CriticMultiple = new Stat(_data.CriticMultiple);
            Evasion = new Stat(_data.Evasion);
            LifeSteal = new Stat(_data.LifeSteal);
            CooltimeAccel = new Stat(_data.CooltimeAccel);
            AttackSpeed = new Stat(_data.AttackSpeed);
            AttackRange = new Stat(_data.AttackRange);
            WeaknessAttributeType = _data.WeaknessAttributeType;

            statDict.Add(EStatType.MaxHealth, MaxHealth);
            statDict.Add(EStatType.BaseAttackPower, BaseAttackPower);
            statDict.Add(EStatType.SkillAttackPower, SkillAttackPower);
            statDict.Add(EStatType.Defense, Defense);
            statDict.Add(EStatType.MoveSpeed, MoveSpeed);
            statDict.Add(EStatType.CriticChance, CriticChance);
            statDict.Add(EStatType.CriticMultiple, CriticMultiple);
            statDict.Add(EStatType.Evasion, Evasion);
            statDict.Add(EStatType.LifeSteal, LifeSteal);
            statDict.Add(EStatType.CooltimeAccel, CooltimeAccel);
            statDict.Add(EStatType.AttackSpeed, AttackSpeed);
            statDict.Add(EStatType.AttackRange, AttackRange);

            Health = MaxHealth.Value;
        }

        public void SwapBaseValues(AttributeSetSettingData _data)
        {
            MaxHealth.SetBaseValue(_data.MaxHealth);
            BaseAttackPower.SetBaseValue(_data.BaseAttackPower);
            SkillAttackPower.SetBaseValue(_data.SkillAttackPower);
            Defense.SetBaseValue(_data.Defense);
            MoveSpeed.SetBaseValue(_data.MoveSpeed);
            CriticChance.SetBaseValue(_data.CriticChance);
            CriticMultiple.SetBaseValue(_data.CriticMultiple);
            Evasion.SetBaseValue(_data.Evasion);
            LifeSteal.SetBaseValue(_data.LifeSteal);
            CooltimeAccel.SetBaseValue(_data.CooltimeAccel);
            AttackSpeed.SetBaseValue(_data.AttackSpeed);
            AttackRange.SetBaseValue(_data.AttackRange);
        }

        public Stat GetStatByType(EStatType _type)
        {
            if (statDict.TryGetValue(_type, out Stat stat))
                return stat;
            return null;
        }

        public float GetStatValueByType(EStatType _type)
        {
            if (statDict.TryGetValue(_type, out Stat stat))
                return stat.Value;
            return -1f;
        }
    }
}
