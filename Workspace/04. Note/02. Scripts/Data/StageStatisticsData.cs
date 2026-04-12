using UnityEngine;
using System.Collections.Generic;

namespace MS.Data
{
    public class StageStatisticsData
    {
        public int KillCount;
        public int Gold;
        public int PlayerLevel;
        public List<SkillStatisticsInfo> SkillStatList;
        public bool IsClear;
    }

    public struct SkillStatisticsInfo
    {
        public string SkillKey { get; set; }
        public string IconKey { get; set; }
        public float TotalDamage { get; set; }
        public float DPS { get; set; }
    }
}