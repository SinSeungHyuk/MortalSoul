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


        public void InitBaseAttributeSet(Dictionary<EStatType, float> _baseValues)
        {
            statDict.Clear();

            // 1. 공통 스탯 생성
            MaxHealth = new Stat(0);
            BaseAttackPower = new Stat(0);
            Defense = new Stat(0);
            MoveSpeed = new Stat(0);

            statDict.Add(EStatType.MaxHealth, MaxHealth);
            statDict.Add(EStatType.BaseAttackPower, BaseAttackPower);
            statDict.Add(EStatType.Defense, Defense);
            statDict.Add(EStatType.MoveSpeed, MoveSpeed);

            // 2. 서브클래스 추가 스탯 등록
            InitAttributeSet();

            // 3. baseValues로 초기값 설정
            foreach (var pair in _baseValues)
            {
                if (statDict.TryGetValue(pair.Key, out Stat stat))
                {
                    stat.AddBaseValue(pair.Value);
                }
            }

            // 4. 체력 초기화
            Health = MaxHealth.Value;
        }

        protected abstract void InitAttributeSet();

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
