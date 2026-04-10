using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MS.Data;
using UnityEngine;
using UnityEngine.Audio;

namespace Core
{
    public class SoundManager
    {
        private AudioMixer masterMixer;
        private AudioSource curBGM;
        private List<AudioSource> sfxList = new List<AudioSource>();
        private Dictionary<string, float> sfxCooldowns = new();


        public void PlayBGM(string _key)
        {
            StopBGM();

            GameObject instance = Main.Instance.ObjectPoolManager.Get("BGMEmitter");
            AudioSource audioSource = instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                AudioClip clip = Main.Instance.AddressableManager.LoadResource<AudioClip>(_key);
                audioSource.clip = clip;
                audioSource.Play();
                curBGM = audioSource;
            }
        }

        public void PlaySFX(string _key)
        {
            if (sfxCooldowns.TryGetValue(_key, out float lastTime) && Time.unscaledTime - lastTime < 0.05f) return;
            sfxCooldowns[_key] = Time.unscaledTime;

            if (Main.Instance.DataManager.SettingData.SoundSettingDict.TryGetValue(_key, out SoundSettingData _soundData))
            {
                GameObject instance = Main.Instance.ObjectPoolManager.Get("SFXEmitter");
                AudioSource audioSource = instance.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    AudioClip clip = Main.Instance.AddressableManager.LoadResource<AudioClip>(_key);
                    audioSource.clip = clip;
                    audioSource.loop = _soundData.Loop;
                    audioSource.volume = UnityEngine.Random.Range(_soundData.MinVolume, _soundData.MaxVolume);
                    audioSource.Play();
                    sfxList.Add(audioSource);

                    if (!_soundData.Loop)
                        ReturnSFXAfterPlay(audioSource, clip.length).Forget();
                }
            }
        }

        private async UniTaskVoid ReturnSFXAfterPlay(AudioSource _audioSource, float _duration)
        {
            await UniTask.WaitForSeconds(_duration, ignoreTimeScale: true);
            if (_audioSource == null) return;
            sfxList.Remove(_audioSource);
            Main.Instance.ObjectPoolManager.Return("SFXEmitter", _audioSource.gameObject);
        }

        public void SetBGMVolume(float _volume)
        {
            if (masterMixer != null)
            {
                float volume = _volume <= 0.0001f ? -80f : Mathf.Log10(_volume) * 20;
                masterMixer.SetFloat("BGMVolume", volume);

                PlayerPrefs.SetFloat("BGM_Volume", _volume);
                PlayerPrefs.Save();
            }
        }

        public void SetSFXVolume(float _volume)
        {
            if (masterMixer != null)
            {
                float volume = _volume <= 0.0001f ? -80f : Mathf.Log10(_volume) * 20;
                masterMixer.SetFloat("SFXVolume", volume);

                PlayerPrefs.SetFloat("SFX_Volume", _volume);
                PlayerPrefs.Save();
            }
        }

        public void StopBGM()
        {
            if (curBGM != null)
            {
                curBGM.Stop();
                Main.Instance.ObjectPoolManager.Return("BGMEmitter", curBGM.gameObject);
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
                    Main.Instance.ObjectPoolManager.Return("SFXEmitter", sfx.gameObject);
                }
            }
            sfxList.Clear();
        }

        public async UniTask LoadAllSoundAsync()
        {
            masterMixer = await Main.Instance.AddressableManager.LoadResourceAsync<AudioMixer>("MasterMixer");

            var tasks = new List<UniTask>
            {
                Main.Instance.ObjectPoolManager.CreatePoolAsync("BGMEmitter", 1),
                Main.Instance.ObjectPoolManager.CreatePoolAsync("SFXEmitter", 50),
            };

            await UniTask.WhenAll(tasks);

            SetBGMVolume(PlayerPrefs.GetFloat("BGM_Volume", 1.0f));
            SetSFXVolume(PlayerPrefs.GetFloat("SFX_Volume", 1.0f));
        }
    }
}
