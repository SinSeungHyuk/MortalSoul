namespace MS.Battle
{
    public class MonsterAttributeSet : BaseAttributeSet
    {
        public Stat AttackRange { get; private set; }

        protected override void RegisterAdditionalStats()
        {
            AttackRange = new Stat(0);

            statDict.Add(EStatType.AttackRange, AttackRange);
        }
    }
}
