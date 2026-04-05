using System.Collections.Generic;

namespace MS.Data
{
    public class SoulGameData
    {
        public string MainSoulKey { get; set; }
        public string SubSoulKey { get; set; }
        public Dictionary<string, float> SoulHealthDict { get; set; }

        public SoulGameData()
        {
            SoulHealthDict = new Dictionary<string, float>();
        }
    }
}
