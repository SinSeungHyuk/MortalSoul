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
        private float curSubSoulHealth;

        public void InitPSC(PlayerCharacter _owner, string _mainSoulKey)
        {
            owner = _owner;
            MainSoulKey = _mainSoulKey;
            SubSoulKey = null;
            curSubSoulHealth = 0f;
        }

        public bool CanSwap()
        {
            if (SubSoulKey == null) return false;
            if (owner.BSC.AttributeSet.Health <= 0f) return false;
            return true;
        }

        public float SwapSlots(float _curHealth)
        {
            string tempSoulKey = MainSoulKey;
            MainSoulKey = SubSoulKey;
            SubSoulKey = tempSoulKey;

            float curSoulHealth = curSubSoulHealth;
            curSubSoulHealth = _curHealth;

            OnSoulSwapped?.Invoke();

            return curSoulHealth;
        }

        public void SetSubSoul(string _soulKey)
        {
            SubSoulKey = _soulKey;
        }

        public void InitSubSoulHealth(float _maxHealth)
        {
            curSubSoulHealth = _maxHealth;
        }

        public List<string> GetActiveSkillKeys()
        {
            var data = Main.Instance.DataManager.SettingData.CharacterSettingData.GetSoulSettingData(MainSoulKey);
            if (data?.SkillKeys == null) return new List<string>();
            return new List<string>(data.SkillKeys);
        }
    }
}
