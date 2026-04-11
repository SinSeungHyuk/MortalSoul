using MS.Data;

namespace MS.Battle
{
    public class PlayerAttributeSet : BaseAttributeSet
    {
        public Stat CriticChance { get; private set; }
        public Stat CriticMultiple { get; private set; }
        public Stat Evasion { get; private set; }
        public Stat LifeSteal { get; private set; }
        public Stat CooltimeAccel { get; private set; }
        public Stat AttackSpeed { get; private set; }

        public void InitPlayerAttributeSet(PlayerAttributeSetSettingData _data)
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

            Health = MaxHealth.Value;
        }

        public void SwapBaseValues(PlayerAttributeSetSettingData _data)
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
        }
    }
}
