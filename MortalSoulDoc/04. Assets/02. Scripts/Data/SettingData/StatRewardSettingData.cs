using System;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Data
{
    [Serializable]
    public class StatRewardSettingData
    {
        public EStatType StatType { get; set; }
        public EGrade Grade { get; set; }
        public float RewardValue { get; set; }
    }
}