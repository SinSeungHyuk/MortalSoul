using MS.Field;
using UnityEngine;


namespace MS.Skill
{
    public enum EDamageAttributeType
    {
        None = 0,
        Fire = 1 << 0,
        Ice = 1 << 1,
        Electric = 1 << 2,
        Wind = 1 << 3,
        Saint = 1 << 4,
        Dark = 1 << 5
    }


    public struct DamageInfo
    {
        public FieldCharacter Attacker; // 공격자
        public FieldCharacter Target; // 피격자
        public EDamageAttributeType AttributeType; // 공격 속성
        public float Damage; // 데미지 (치명타,스킬계수,공격력 등 모두 계산한 값)
        public bool IsCritic; // 크리티컬 여부
        public float KnockbackForce; // 넉백
        public BaseSkill sourceSkill; // 원본 스킬

        public DamageInfo(FieldCharacter _attacker, FieldCharacter _target, EDamageAttributeType _attributeType, float _damage, bool _isCritic, float _knockbackForce,BaseSkill _sourceSkill = null)
        {
            Attacker = _attacker;
            Target = _target;
            AttributeType = _attributeType;
            Damage = _damage;
            IsCritic = _isCritic;
            KnockbackForce = _knockbackForce;
            sourceSkill = _sourceSkill;
        }
    }
}