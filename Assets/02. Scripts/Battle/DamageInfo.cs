using MS.Field;

namespace MS.Battle
{
    [System.Flags]
    public enum EDamageAttributeType
    {
        None     = 0,
        Fire     = 1 << 0,
        Ice      = 1 << 1,
        Electric = 1 << 2,
        Wind     = 1 << 3,
        Saint    = 1 << 4,
        Dark     = 1 << 5
    }

    public struct DamageInfo
    {
        public FieldCharacter Attacker;
        public FieldCharacter Target;
        public EDamageAttributeType AttributeType;
        public float Damage;
        public bool IsCritic;
        public float KnockbackForce;
        public BaseSkill SourceSkill;
    }
}
