using Core;
using Cysharp.Threading.Tasks;
using MS.Core;
using MS.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


namespace MS.Manager
{
    public class SoundManager : Singleton<SoundManager>
    {
        private const string BGM_VOLUME_KEY = "BGM_Volume";
        private const string SFX_VOLUME_KEY = "SFX_Volume";

        private AudioMixer masterMixer;

        private AudioSource curBGM;
        private List<AudioSource> sfxList = new List<AudioSource>();



        public void PlayBGM(string _key)
        {
            StopBGM();

            GameObject instance = ObjectPoolManager.Instance.Get("BGMEmitter");
            AudioSource audioSource = instance.GetComponent<AudioSource>();
            if (audioSource != null) 
            {
                AudioClip clip = AddressableManager.Instance.LoadResource<AudioClip>(_key);
                audioSource.clip = clip;
                audioSource.Play();
                curBGM = audioSource;
            }
        }

        public void PlaySFX(string _key)
        {
            if (DataManager.Instance.SoundSettingDataDict.TryGetValue(_key, out SoundSettingData _soundData))
            {
                GameObject instance = ObjectPoolManager.Instance.Get("SFXEmitter");
                AudioSource audioSource = instance.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    AudioClip clip = AddressableManager.Instance.LoadResource<AudioClip>(_key);
                    audioSource.clip = clip;

                    audioSource.loop = _soundData.Loop;
                    float randVolume = UnityEngine.Random.Range(_soundData.MinVolume, _soundData.MaxVolume);
                    audioSource.volume = randVolume;

                    audioSource.Play();
                    sfxList.Add(audioSource);
                }
            }
        }

        public void SetBGMVolume(float _volume)
        {
            if (masterMixer != null)
            {
                float volume = _volume <= 0.0001f ? -80f : Mathf.Log10(_volume) * 20;
                masterMixer.SetFloat("BGMVolume", volume);

                PlayerPrefs.SetFloat(BGM_VOLUME_KEY, _volume);
                PlayerPrefs.Save();
            }
        }

        public void SetSFXVolume(float _volume)
        {
            if (masterMixer != null)
            {
                float volume = _volume <= 0.0001f ? -80f : Mathf.Log10(_volume) * 20;
                masterMixer.SetFloat("SFXVolume", volume);

                PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _volume);
                PlayerPrefs.Save();
            }
        }

        public void StopBGM()
        {
            if (curBGM != null)
            {
                curBGM.Stop();
                ObjectPoolManager.Instance.Return("BGMEmitter", curBGM.gameObject);
                curBGM = null;
            }
        }

        public void ClearAllSounds()
        {
            StopBGM();
            foreach (var sfx in sfxList)
            {
                if (sfx != null)
                {
                    sfx.Stop();
                    ObjectPoolManager.Instance.Return("SFXEmitter", sfx.gameObject);
                }
            }
            sfxList.Clear();
        }

        public async UniTask InitSoundAsync()
        {
            masterMixer = await AddressableManager.Instance.LoadResourceAsync<AudioMixer>("MasterMixer");
            await AddressableManager.Instance.LoadResourceAsync<AudioClip>("BGM_Title");
            await ObjectPoolManager.Instance.CreatePoolAsync("BGMEmitter", 1);

            SetBGMVolume(PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f));
            SetSFXVolume(PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f));
        }

        public async UniTask LoadAllSoundAsync()
        {
            try
            {
                var tasks = new List<UniTask>
                {
                    ObjectPoolManager.Instance.CreatePoolAsync("SFXEmitter", 30),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("FX_GainCoin"),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("FX_GainHp"),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("FX_Gold"),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("FX_Hit"),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("FX_LevelUp"),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("FX_StageClear"),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("FX_StatReward"),
                    AddressableManager.Instance.LoadResourceAsync<AudioClip>("BGM_Lobby"),
                    // ... 
                };

                await UniTask.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}