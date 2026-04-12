using System;
using System.Collections.Generic;
using UnityEngine;

public enum EBonusType
{
    Flat,       // 고정 수치 합연산 
    Percentage  // 퍼센트 합연산 
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
    // 스탯 변경 이벤트
    public event Action<float> OnValueChanged;

    // 보너스 스탯을 얻은 키로 관리하는 보너스 스탯 딕셔너리
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

    public void AddBonusStat(string _key, EBonusType _bonusType, float _value)
    {
        BonusStat newBonusStat = new BonusStat { Type = _bonusType, Value = _value };

        if (bonusStatDict.ContainsKey(_key))
        {
            bonusStatDict[_key] = newBonusStat; // 기존의 보너스 스탯 덮어쓰기
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

        // 모든 깡수치를 더하고 거기에 모든 % 수치를 합연산
        float calculatedValue = finalFlat * (1 + (totalPercent / 100f));
        return Mathf.Max(0f, calculatedValue);
    }
}