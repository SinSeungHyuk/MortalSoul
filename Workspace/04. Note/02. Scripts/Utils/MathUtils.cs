using MS.Data;
using UnityEngine;


namespace MS.Utils
{
    public static class MathUtils
    {
        // 게임화면 밖에 있는 오브젝트인지 검사 =====================================================
        public static bool IsOutScreen(GameObject target)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(target.transform.position);
            bool isOutScreen = screenPoint.x <= 0 || screenPoint.x >= Screen.width || screenPoint.y <= 0 || screenPoint.y >= Screen.height;

            return isOutScreen;
        }

        // 확률 계산하기  ===========================================================
        public static bool IsSuccess(int percent)
        {
            int chance = Random.Range(1, 101);
            return percent >= chance;
        }
        public static bool IsSuccess(float percent)
        {
            float chance = Random.Range(0f, 100f);
            return percent >= chance;
        }

        // 퍼센트 % 계산하기  ===========================================================
        public static int IncreaseByPercent(int value, float percent)
        {
            float finalValue = value * (1f + (percent * 0.01f));
            return Mathf.RoundToInt(finalValue);
        }
        public static float IncreaseByPercent(float value, float percent)
        {
            return value * (1f + (percent * 0.01f));
        }
        public static int DecreaseByPercent(int value, float percent)
        {
            float finalValue = value * (1f - (percent * 0.01f));
            return Mathf.RoundToInt(finalValue);
        }
        public static float DecreaseByPercent(float value, float percent)
        {
            return value * (1f - (percent * 0.01f));
        }

        // 스케일링 계산하기  ===========================================================
        public static float BattleScaling(float value)
        {
            // 100 -> 50% (이후 효율이 감소하는 커브)
            float f_value = value;
            float scale = (f_value / (f_value + Settings.BattleScalingConstant) * 100f);

            return scale;
        }

        // 등급 구하기 =================================================================
        public static EGrade GetRandomGrade()
        {
            int randomPoint = Random.Range(0, 100);
            int currentSum = 0;

            foreach (var pair in GlobalDefine.GradeProbDict)
            {
                currentSum += pair.Value;
                if (randomPoint < currentSum)
                {
                    return pair.Key;
                }
            }

            return EGrade.Normal;
        }
    }
}