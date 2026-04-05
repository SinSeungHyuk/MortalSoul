using MS.Battle;
using MS.Core;
using System.Collections.Generic;

namespace MS.Data
{
    public class LevelGameData
    {
        public MSReactProp<int> CurrentLevel { get; private set; }
        public MSReactProp<float> CurrentExp { get; private set; }
        public int PendingLevelUpCount { get; set; }
        public Dictionary<EStatType, float> LevelUpGrowth { get; set; }

        public LevelGameData()
        {
            CurrentLevel = new MSReactProp<int>(1);
            CurrentExp = new MSReactProp<float>(0f);
            PendingLevelUpCount = 0;
            LevelUpGrowth = new Dictionary<EStatType, float>();
        }
    }
}
