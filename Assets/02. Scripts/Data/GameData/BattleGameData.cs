using System.Collections.Generic;

namespace MS.Data
{
    public class BattleGameData
    {
        public int KillCount { get; set; }
        public int GoldEarned { get; set; }
        public Dictionary<string, float> SkillDpsDict { get; set; }

        public BattleGameData()
        {
            KillCount = 0;
            GoldEarned = 0;
            SkillDpsDict = new Dictionary<string, float>();
        }
    }
}
