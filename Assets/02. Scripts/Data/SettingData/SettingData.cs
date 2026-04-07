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
        public Dictionary<EWeaponType, WeaponSettingData> WeaponSettingDict { get; private set; }

        public async UniTask LoadAllSettingDataAsync()
        {
            CharacterSettingData = await LoadOneAsync<GameCharacterSettingData>("CharacterSettingData");
            MonsterSettingDict = await LoadOneAsync<Dictionary<string, MonsterSettingData>>("MonsterSettingData");
            SkillSettingDict = await LoadOneAsync<Dictionary<string, SkillSettingData>>("SkillSettingData");
            SoundSettingDict = await LoadOneAsync<Dictionary<string, SoundSettingData>>("SoundSettingData");
            WeaponSettingDict = await LoadOneAsync<Dictionary<EWeaponType, WeaponSettingData>>("WeaponSettingData");
        }

        private async UniTask<T> LoadOneAsync<T>(string _key) where T : class
        {
            try
            {
                TextAsset json = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>(_key);
                if (json == null)
                {
                    Debug.LogWarning($"[SettingData] 리소스 없음(스킵): {_key}");
                    return null;
                }
                return JsonConvert.DeserializeObject<T>(json.text);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SettingData] {_key} 로드 실패(스킵): {e.Message}");
                return null;
            }
        }
    }
}
