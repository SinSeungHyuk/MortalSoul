namespace MS.Battle
{
    public class PlayerAttributeSet : BaseAttributeSet
    {
        public Stat SkillAttackPower { get; private set; }
        public Stat CriticChance { get; private set; }
        public Stat CriticMultiple { get; private set; }
        public Stat Evasion { get; private set; }
        public Stat LifeSteal { get; private set; }
        public Stat CooltimeAccel { get; private set; }
        public Stat AttackSpeed { get; private set; }

        protected override void InitAttributeSet()
        {
            SkillAttackPower = new Stat(0);
            CriticChance = new Stat(0);
            CriticMultiple = new Stat(0);
            Evasion = new Stat(0);
            LifeSteal = new Stat(0);
            CooltimeAccel = new Stat(0);
            AttackSpeed = new Stat(0);

            statDict.Add(EStatType.SkillAttackPower, SkillAttackPower);
            statDict.Add(EStatType.CriticChance, CriticChance);
            statDict.Add(EStatType.CriticMultiple, CriticMultiple);
            statDict.Add(EStatType.Evasion, Evasion);
            statDict.Add(EStatType.LifeSteal, LifeSteal);
            statDict.Add(EStatType.CooltimeAccel, CooltimeAccel);
            statDict.Add(EStatType.AttackSpeed, AttackSpeed);
        }
    }
}
