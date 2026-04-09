using MS.Data;

namespace MS.Battle
{
    public class MonsterAttributeSet : BaseAttributeSet
    {
        public Stat AttackRange { get; private set; }

        public void InitAttributeSet(MonsterAttributeSetSettingData _data)
        {
            MaxHealth = new Stat(_data.MaxHealth);
            BaseAttackPower = new Stat(_data.AttackPower);
            Defense = new Stat(_data.Defense);
            MoveSpeed = new Stat(_data.MoveSpeed);
            AttackRange = new Stat(_data.AttackRange);
            WeaknessAttributeType = _data.WeaknessAttributeType;

            statDict.Add(EStatType.MaxHealth, MaxHealth);
            statDict.Add(EStatType.BaseAttackPower, BaseAttackPower);
            statDict.Add(EStatType.Defense, Defense);
            statDict.Add(EStatType.MoveSpeed, MoveSpeed);
            statDict.Add(EStatType.AttackRange, AttackRange);

            Health = MaxHealth.Value;
        }
    }
}
