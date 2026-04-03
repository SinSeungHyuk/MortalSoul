using System.Collections.Generic;
using UnityEngine;

namespace MS.Data
{
    public enum EGrade
    {
        Normal,
        Rare,
        Unique,
        Legendary
    }

    public static class GlobalDefine
    {
        public static readonly Dictionary<EGrade, Color32> GradeColorDict = new Dictionary<EGrade, Color32>
        {
            { EGrade.Normal, new Color32(255, 255, 255, 255) },
            { EGrade.Rare, new Color32(11, 110, 204, 255) },
            { EGrade.Unique, new Color32(155, 61, 217, 255) },
            { EGrade.Legendary, new Color32(255, 112, 120, 255) }
        };

        public static readonly Dictionary<EGrade, int> GradeProbDict = new Dictionary<EGrade, int>
        {
            { EGrade.Normal, 50 },
            { EGrade.Rare, 35 },
            { EGrade.Unique, 10 },
            { EGrade.Legendary, 5 }
        };
    }
}