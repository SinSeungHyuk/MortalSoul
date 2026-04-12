using MS.Battle;
using MS.Data;

namespace MS.Utils
{
    public static class BattleUtils
    {
        public static float CalcDefenseStat(float _damage, float _defense)
        {
            float defensePercent = MathUtils.BattleScaling(_defense);
            return MathUtils.DecreaseByPercent(_damage, defensePercent);
        }

        public static bool CalcEvasionStat(float _evasion)
        {
            float evasionPercent = MathUtils.BattleScaling(_evasion);
            return MathUtils.IsSuccess(evasionPercent);
        }

        public static float CalcWeaknessAttribute(float _damage, EDamageAttributeType _damageType, EDamageAttributeType _weaknessType)
        {
            if ((_damageType & _weaknessType) != 0)
                return _damage * Settings.WeaknessAttributeMultiple;
            return _damage;
        }

        // _criticChance: 0~100 직접 % 값. _criticMultiple: 100 = 원본, 150 = 1.5배, 200 = 2배 (기본 150)
        public static bool CalcCriticDamage(float _baseDamage, float _criticChance, float _criticMultiple, out float _resultDamage)
        {
            if (_criticChance <= 0f || !MathUtils.IsSuccess(_criticChance))
            {
                _resultDamage = _baseDamage;
                return false;
            }

            _resultDamage = _baseDamage * (_criticMultiple * 0.01f);
            return true;
        }
    }
}
