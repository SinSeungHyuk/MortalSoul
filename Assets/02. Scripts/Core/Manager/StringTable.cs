using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core
{
    public class StringTable
    {
        private static readonly Regex placeholderRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

        private Dictionary<string, Dictionary<string, string>> stringTableDict = new();

        public async UniTask LoadStringTable()
        {
            try
            {
                TextAsset stringJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("StringTable");
                stringTableDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(stringJson.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StringTable] 데이터 로드 실패: {e.Message}");
            }
        }

        public string Get(string _category, string _key, params (string key, object value)[] _args)
        {
            if (!stringTableDict.TryGetValue(_category, out var val)) return "";
            if (!val.TryGetValue(_key, out var rawString)) return "";
            if (_args == null || _args.Length == 0) return rawString;

            return placeholderRegex.Replace(rawString, m =>
            {
                foreach (var (k, v) in _args)
                    if (k == m.Groups[1].Value) return v?.ToString() ?? "";
                return m.Value;
            });
        }
    }
}
