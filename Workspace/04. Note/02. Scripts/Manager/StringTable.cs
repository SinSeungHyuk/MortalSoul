using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


namespace MS.Manager
{
    public class StringTable : Singleton<StringTable>
    {
        private Dictionary<string, Dictionary<string, string>> stringTableDict = new Dictionary<string, Dictionary<string, string>>();

        public async UniTask LoadStringTable()
        {
            try
            {
                TextAsset stringJson = await AddressableManager.Instance.LoadResourceAsync<TextAsset>("StringTable");
                stringTableDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(stringJson.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StringTable] 데이터 로드 실패: {e.Message}");
            }
        }

        public string Get(string _category, string _key, params object[] _args)
        {
            if (stringTableDict.TryGetValue(_category, out var val))
            {
                if (val.TryGetValue(_key, out var rawString))
                {
                    if (_args != null && _args.Length > 0 && rawString.Contains("{")) 
                        return string.Format(rawString, _args); 
                    else 
                        return rawString;
                }
            }
            return "";
        }
    }
}