using System;
using Core;
using Cysharp.Threading.Tasks;
using MS.Battle;
using MS.Data;
using UnityEngine;

namespace MS.Field
{
    public class PlayerCharacter : FieldCharacter
    {
        private PlayerMovementController pmc;
        private PlayerSoulController psc;

        public PlayerMovementController PMC => pmc;
        public PlayerSoulController PSC => psc;


        protected override void Awake()
        {
            base.Awake();
            pmc = GetComponent<PlayerMovementController>();
        }

        private void Start()
        {
            InitTestAsync().Forget();
        }

        private async UniTaskVoid InitTestAsync()
        {
            await UniTask.WaitUntil(() => Main.Instance.IsBootCompleted);
            InitPlayer("test");
        }

        public void InitPlayer(string _mainSoulKey)
        {
            psc = new PlayerSoulController();
            psc.InitPSC(this, _mainSoulKey);

            var mainData = psc.GetMainSoulData();
            if (mainData == null)
            {
                Debug.LogError($"[PlayerCharacter] CharacterSettingData 없음: {_mainSoulKey}");
                return;
            }

            var playerAttributeSet = new PlayerAttributeSet();
            playerAttributeSet.InitPlayerAttributeSet(mainData.AttributeSetSettingData);

            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, playerAttributeSet, mainData.WeaponType);

            if (mainData.SkillKeys != null)
            {
                foreach (var skillKey in mainData.SkillKeys)
                    BSC.SSC.GiveSkill(skillKey);
            }

            if (mainData.SkinKeys != null && mainData.SkinKeys.Count > 0)
                SpineController.SetCombinedSkin(mainData.SkinKeys);

            pmc.InitController(BSC.WSC);
        }

        public void AcquireSoul(string _soulKey)
        {
            if (psc.SubSoulKey != null) return;

            psc.SetSubSoul(_soulKey);

            var subData = psc.GetSubSoulData();
            if (subData == null) return;

            if (subData.SkillKeys != null)
            {
                foreach (var skillKey in subData.SkillKeys)
                    BSC.SSC.GiveSkill(skillKey);
            }

            psc.InitSubSoulHealth(subData.AttributeSetSettingData.MaxHealth);
        }

        public void SwapSoul()
        {
            if (!psc.CanSwap()) return;

            BSC.SSC.CancelAllSkills();

            var attrSet = (PlayerAttributeSet)BSC.AttributeSet;
            float restoredHealth = psc.SwapSlots(attrSet.Health);

            var newSoulData = psc.GetMainSoulData();

            attrSet.SwapBaseValues(newSoulData.AttributeSetSettingData);
            attrSet.Health = Mathf.Min(restoredHealth, attrSet.MaxHealth.Value);

            BSC.WSC.ChangeWeaponType(newSoulData.WeaponType);
            SpineController.SetCombinedSkin(newSoulData.SkinKeys);

            pmc.TransitToIdle();
            psc.InvokeOnSoulSwapped();
        }
    }
}
