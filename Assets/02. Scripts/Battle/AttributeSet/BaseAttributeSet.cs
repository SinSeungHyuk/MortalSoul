using System;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Battle
{
    public abstract class BaseAttributeSet
    {
                            // cur, max
        public event Action<float, float> OnHealthChanged;

        protected Dictionary<EStatType, Stat> statDict = new Dictionary<EStatType, Stat>();

        public Stat MaxHealth { get; protected set; }
        public Stat BaseAttackPower { get; protected set; }
        public Stat SkillAttackPower { get; protected set; }
        public Stat Defense { get; protected set; }
        public Stat MoveSpeed { get; protected set; }
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
