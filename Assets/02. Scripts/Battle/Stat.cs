using System;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Battle
{
    public enum EStatType
    {
        MaxHealth,
        BaseAttackPower,
        SkillAttackPower,
        Defense,
        MoveSpeed,
        CriticChance,
        CriticMultiple,
        Evasion,
        LifeSteal,
        CooltimeAccel,
        AttackSpeed,
        AttackRange
    }

    public enum EBonusType
    {
        Flat,
        Percentage
    }

    [System.Serializable]
    public struct BonusStat
    {
        public EBonusType Type;
        public float Value;
    }

    [System.Serializable]
    public class Stat
    {
        public event Action<float> OnValueChanged;

        private Dictionary<string, BonusStat> bonusStatDict = new Dictionary<string, BonusStat>();

        private float baseValue;
        public float BaseValue => baseValue;

        public float Value => CalcValue();


        public Stat(float _baseValue)
        {
            baseValue = _baseValue;
            bonusStatDict = new Dictionary<string, BonusStat>();
        }

        public void AddBaseValue(float _amount)
        {
            baseValue += _amount;
            OnValueChanged?.Invoke(Value);
        }

        public void SetBaseValue(float _value)
        {
            baseValue = _value;
            OnValueChanged?.Invoke(Value);
        }

        public void AddBonusStat(string _key, EBonusType _bonusType, float _value)
        {
            BonusStat newBonusStat = new BonusStat { Type = _bonusType, Value = _value };

            if (bonusStatDict.ContainsKey(_key))
            {
                bonusStatDict[_key] = newBonusStat;
            }
            else
            {
                bonusStatDict.Add(_key, newBonusStat);
            }

            OnValueChanged?.Invoke(Value);
        }

        public void RemoveBonusStat(string _key)
        {
            if (bonusStatDict.ContainsKey(_key))
            {
                bonusStatDict.Remove(_key);
                OnValueChanged?.Invoke(Value);
            }
        }

        public void ClearBonusStat()
        {
            if (bonusStatDict.Count > 0)
            {
                bonusStatDict.Clear();
                OnValueChanged?.Invoke(Value);
            }
        }

        private float CalcValue()
        {
            float finalFlat = baseValue;
            float totalPercent = 0f;

            foreach (var modifier in bonusStatDict.Values)
            {
                if (modifier.Type == EBonusType.Flat)
                {
                    finalFlat += modifier.Value;
                }
                else if (modifier.Type == EBonusType.Percentage)
                {
                    totalPercent += modifier.Value;
                }
            }

            float calculatedValue = finalFlat * (1 + (totalPercent / 100f));
            return Mathf.Max(0f, calculatedValue);
        }
    }
}
