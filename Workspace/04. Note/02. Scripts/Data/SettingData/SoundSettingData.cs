using System;
using UnityEngine;


namespace MS.Data
{
    [Serializable]
    public class SoundSettingData
    {
        public float MinVolume { get; set; }
        public float MaxVolume { get; set; }
        public bool Loop { get; set; }
    }
}