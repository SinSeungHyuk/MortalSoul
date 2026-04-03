using System;
using UnityEngine;

namespace MS.Data
{
    [Serializable]
    public class ItemSettingData
    {
        public string GameplayCueKey { get; set; } // 아이템 획득시 재생할 게임플레이큐
        public EItemType ItemType { get; set; }
        public float ItemValue { get; set; }
    }

    public enum EItemType
    {
        Coin,

        RedCrystal,
        GreenCrystal,
        BlueCrystal,

        BossChest,
        Artifact,
    }
}