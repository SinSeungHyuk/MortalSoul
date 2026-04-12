using UnityEngine;

namespace MS.Utils
{
    public static class MathUtils
    {
        // 전투 스케일링 공식. 100 → 50%, 200 → 66.7%, 300 → 75% (체감 증가량 감소)
        public static float BattleScaling(float _value)
        {
            return _value / (_value + Settings.BattleScalingConstant) * 100f;
        }

        public static float DecreaseByPercent(float _value, float _percent)
        {
            return _value * (1f - (_percent * 0.01f));
        }

        // _percent는 0~100 범위
        public static bool IsSuccess(float _percent)
        {
            if (_percent <= 0f) return false;
            if (_percent >= 100f) return true;
            float chance = Random.Range(0f, 100f);
            return _percent >= chance;
        }
    }
}
