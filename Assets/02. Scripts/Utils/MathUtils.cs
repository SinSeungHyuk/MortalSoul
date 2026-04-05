using UnityEngine;

namespace MS.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// 전투 스케일링 공식. value가 클수록 효과가 커지지만 체감 증가량은 감소.
        /// 100 → 50%, 200 → 66.7%, 300 → 75%
        /// </summary>
        public static float BattleScaling(float value)
        {
            return value / (value + Settings.BattleScalingConstant) * 100f;
        }

        /// <summary>
        /// 값을 퍼센트만큼 감소. DecreaseByPercent(100, 30) → 70
        /// </summary>
        public static float DecreaseByPercent(float value, float percent)
        {
            return value * (1f - (percent * 0.01f));
        }

        /// <summary>
        /// 확률 판정. percent가 0~100 범위.
        /// </summary>
        public static bool IsSuccess(float percent)
        {
            float chance = Random.Range(0f, 100f);
            return percent >= chance;
        }
    }
}
