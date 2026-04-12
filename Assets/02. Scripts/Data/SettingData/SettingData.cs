using Core;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace MS.Data
{
    public class SettingData
    {
        public GameCharacterSettingData CharacterSettingData { get; private set; }
        public Dictionary<string, MonsterSettingData> MonsterSettingDict { get; private set; }
        public Dictionary<string, SkillSettingData> SkillSettingDict { get; private set; }
        public Dictionary<string, SoundSettingData> SoundSettingDict { get; private set; }

        public async UniTask LoadAllSettingDataAsync()
        {
            try
            {
                TextAsset characterJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("CharacterSettingData");
                CharacterSettingData = JsonConvert.DeserializeObject<GameCharacterSettingData>(characterJson.text);

                TextAsset monsterJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("MonsterSettingData");
                MonsterSettingDict = JsonConvert.DeserializeObject<Dictionary<string, MonsterSettingData>>(monsterJson.text);

                TextAsset skillJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("SkillSettingData");
                SkillSettingDict = JsonConvert.DeserializeObject<Dictionary<string, SkillSettingData>>(skillJson.text);

                // TextAsset soundJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("SoundSettingData");
                // SoundSettingDict = JsonConvert.DeserializeObject<Dictionary<string, SoundSettingData>>(soundJson.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SettingData] 세팅데이터 로드 실패: {e.Message}");
            }
        }
    }
}
