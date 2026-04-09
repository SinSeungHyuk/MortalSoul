using Core;
using MS.Data;
using System;
using System.Collections.Generic;

namespace MS.Field
{
    public class PlayerSoulController
    {
        public string MainSoulKey { get; private set; }
        public string SubSoulKey { get; private set; }

        public event Action OnSoulSwapped;

        private PlayerCharacter owner;
        private float subSoulHealth;

        public void InitPSC(PlayerCharacter _owner, string _mainSoulKey)
        {
            owner = _owner;
            MainSoulKey = _mainSoulKey;
            SubSoulKey = null;
            subSoulHealth = 0f;
        }

        public bool CanSwap()
        {
            if (SubSoulKey == null) return false;
            if (owner.BSC.AttributeSet.Health <= 0f) return false;
            return true;
        }

        public float SwapSlots(float _curHealth)
        {
            (MainSoulKey, SubSoulKey) = (SubSoulKey, MainSoulKey);

            float restoredHealth = subSoulHealth;
            subSoulHealth = _curHealth;

            return restoredHealth;
        }

        public void SetSubSoul(string _soulKey)
        {
            SubSoulKey = _soulKey;
        }

        public void InitSubSoulHealth(float _maxHealth)
        {
            subSoulHealth = _maxHealth;
        }

        public CharacterSettingData GetMainSoulData()
        {
            var dict = Main.Instance.DataManager.SettingData.CharacterSettingData.CharacterSettingDataDict;
            dict.TryGetValue(MainSoulKey, out CharacterSettingData data);
            return data;
        }

        public CharacterSettingData GetSubSoulData()
        {
            if (SubSoulKey == null) return null;
            var dict = Main.Instance.DataManager.SettingData.CharacterSettingData.CharacterSettingDataDict;
            dict.TryGetValue(SubSoulKey, out CharacterSettingData data);
            return data;
        }

        public List<string> GetActiveSkillKeys()
        {
            var data = GetMainSoulData();
            if (data?.SkillKeys == null) return new List<string>();
            return new List<string>(data.SkillKeys);
        }

        public void InvokeOnSoulSwapped()
        {
            OnSoulSwapped?.Invoke();
        }
    }
}
