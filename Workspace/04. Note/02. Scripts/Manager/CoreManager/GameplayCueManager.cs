using Core;
using Cysharp.Threading.Tasks;
using MS.Core;
using MS.Data;
using MS.Field;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Manager
{
    public class GameplayCueManager : Singleton<GameplayCueManager>
    {
        private Dictionary<string, GameplayCue> gameplayCueDict = new Dictionary<string, GameplayCue>();


        public void PlayCue(string _cueKey, FieldObject _owner)
        {
            if (string.IsNullOrEmpty(_cueKey) || _owner == null) return;

            if (gameplayCueDict.TryGetValue(_cueKey, out GameplayCue cueToPlay))
            {
                cueToPlay.Play(_owner);
            }
            else
            {
                Debug.LogWarning($"[GameplayCueManager] '{_cueKey}' 키에 해당하는 큐를 찾을 수 없습니다.");
            }
        }

        public void PlayCue(string _cueKey, Vector3 _pos)
        {
            if (string.IsNullOrEmpty(_cueKey)) return;

            if (gameplayCueDict.TryGetValue(_cueKey, out GameplayCue cueToPlay))
            {
                cueToPlay.Play(_pos);
            }
            else
            {
                Debug.LogWarning($"[GameplayCueManager] '{_cueKey}' 키에 해당하는 큐를 찾을 수 없습니다.");
            }
        }


        public async UniTask LoadAllGameplayCueAsync()
        {
            try
            {
                IList<GameplayCue> loadedCues = await AddressableManager.Instance.LoadResourcesLabelAsync<GameplayCue>("GameplayCue");

                if (loadedCues == null)
                {
                    Debug.LogError("[GameplayCueManager] 큐 로드에 실패했습니다.");
                    return;
                }

                foreach (GameplayCue cue in loadedCues)
                {
                    if (cue != null && !gameplayCueDict.ContainsKey(cue.name))
                    {
                        gameplayCueDict.Add(cue.name, cue);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] 데이터 로드 실패: {e.Message}");
            }
        }
    }
}