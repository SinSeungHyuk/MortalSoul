using MS.Data;
using MS.Skill;
using UnityEngine;

public class MonsterAttributeSet : BaseAttributeSet
{
    public Stat AttackRange { get; protected set; }
    

    public void InitAttributeSet(MonsterAttributeSetSettingData _monsterData)
    {
        MaxHealth = new Stat(_monsterData.MaxHealth);
        Health = _monsterData.MaxHealth;
        AttackPower = new Stat(_monsterData.AttackPower);
        Defense = new Stat(_monsterData.Defense);
        MoveSpeed = new Stat(_monsterData.MoveSpeed);
        AttackRange = new Stat(_monsterData.AttackRange);
        WeaknessAttributeType = _monsterData.WeaknessAttributeType;

        statDict.Add(EStatType.MaxHealth, MaxHealth);
        statDict.Add(EStatType.AttackPower, AttackPower);
        statDict.Add(EStatType.Defense, Defense);
        statDict.Add(EStatType.MoveSpeed, MoveSpeed);
        statDict.Add(EStatType.AttackRange, AttackRange);
    }
}
