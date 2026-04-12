using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Manager
{
    public class DataManager : Singleton<DataManager>
    {
        public Dictionary<string, SkillSettingData> SkillSettingDataDict { get; private set; }
        public GameCharacterSettingData CharacterSettingData { get; private set; }
        public Dictionary<string, MonsterSettingData> MonsterSettingDataDict { get; private set; }
        public Dictionary<string, StageSettingData> StageSettingDataDict { get; private set; }
        public Dictionary<string, ItemSettingData> ItemSettingDataDict { get; private set; }
        public Dictionary<string, StatRewardSettingData> StatRewardSettingDataDict { get; private set; }
        public Dictionary<string, SoundSettingData> SoundSettingDataDict { get; private set; }
        public Dictionary<string, ArtifactSettingData> ArtifactSettingDataDict { get; private set; }


        public async UniTask LoadAllGameSettingDataAsync()
        {
            try
            {
                TextAsset skillJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("SkillSettingData");
                SkillSettingDataDict = JsonConvert.DeserializeObject<Dictionary<string, SkillSettingData>>(skillJson.text);

                TextAsset characterJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("CharacterSettingData");
                CharacterSettingData = JsonConvert.DeserializeObject<GameCharacterSettingData>(characterJson.text);

                TextAsset monsterJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("MonsterSettingData");
                MonsterSettingDataDict = JsonConvert.DeserializeObject<Dictionary<string, MonsterSettingData>>(monsterJson.text);

                TextAsset stageJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("StageSettingData");
                StageSettingDataDict = JsonConvert.DeserializeObject<Dictionary<string, StageSettingData>>(stageJson.text);

                TextAsset itemJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("ItemSettingData");
                ItemSettingDataDict = JsonConvert.DeserializeObject<Dictionary<string, ItemSettingData>>(itemJson.text);

                TextAsset statRewardJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("StatRewardSettingData");
                StatRewardSettingDataDict = JsonConvert.DeserializeObject<Dictionary<string, StatRewardSettingData>>(statRewardJson.text);

                TextAsset soundJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("SoundSettingData");
                SoundSettingDataDict = JsonConvert.DeserializeObject<Dictionary<string, SoundSettingData>>(soundJson.text);

                TextAsset artifactJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("ArtifactSettingData");
                ArtifactSettingDataDict = JsonConvert.DeserializeObject<Dictionary<string, ArtifactSettingData>>(artifactJson.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] 데이터 로드 실패: {e.Message}");
            }
        }
    }
}