using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class DialogueSettingData
    {
        public List<DialogueLineSettingData> DialogueList { get; set; }
    }

    [Serializable]
    public class DialogueLineSettingData
    {
        public string SpeakerKey { get; set; }
        public string PortraitSpineKey { get; set; }
        public List<string> SkinKeyList { get; set; }
        public bool IsLeft { get; set; }
        public string Text { get; set; }
    }
}
