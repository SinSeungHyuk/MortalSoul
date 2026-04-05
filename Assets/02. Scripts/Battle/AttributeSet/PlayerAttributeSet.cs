namespace MS.Battle
{
    public class PlayerAttributeSet : BaseAttributeSet
    {
        public Stat CriticChance { get; private set; }
        public Stat CriticMultiple { get; private set; }
        public Stat Evasion { get; private set; }
        public Stat LifeSteal { get; private set; }
        public Stat CooltimeAccel { get; private set; }
        public Stat ProjectileCount { get; private set; }
        public Stat AreaRangeMultiple { get; private set; }
        public Stat KnockbackMultiple { get; private set; }
        public Stat CoinMultiple { get; private set; }

        protected override void RegisterAdditionalStats()
        {
            CriticChance = new Stat(0);
            CriticMultiple = new Stat(0);
            Evasion = new Stat(0);
            LifeSteal = new Stat(0);
            CooltimeAccel = new Stat(0);
            ProjectileCount = new Stat(0);
            AreaRangeMultiple = new Stat(0);
            KnockbackMultiple = new Stat(0);
            CoinMultiple = new Stat(0);

            statDict.Add(EStatType.CriticChance, CriticChance);
            statDict.Add(EStatType.CriticMultiple, CriticMultiple);
            statDict.Add(EStatType.Evasion, Evasion);
            statDict.Add(EStatType.LifeSteal, LifeSteal);
            statDict.Add(EStatType.CooltimeAccel, CooltimeAccel);
            statDict.Add(EStatType.ProjectileCount, ProjectileCount);
            statDict.Add(EStatType.AreaRangeMultiple, AreaRangeMultiple);
            statDict.Add(EStatType.KnockbackMultiple, KnockbackMultiple);
            statDict.Add(EStatType.CoinMultiple, CoinMultiple);
        }
    }
}
