using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Data
{
    [Serializable]
    public class StageSettingData
    {
        public string MapKey { get; set; }
        public List<WaveSpawnInfo> WaveSpawnInfoList { get; set; }
    }


    [Serializable]
    public class WaveSpawnInfo
    {
        public List<MonsterSpawnInfo> MonsterSpawnInfoList { get; set; }
        public string BossMonsterKey { get; set; }
        public float SpawnInterval { get; set; }
        public int CountPerSpawn { get; set; } // 동시에 스폰되는 몬스터 수
        public float FieldItemSpawnChance { get; set; }
    }

    [Serializable]
    public class MonsterSpawnInfo
    {
        public string MonsterKey { get; set; }
        public int MonsterSpawnRate { get; set; }
    }
}