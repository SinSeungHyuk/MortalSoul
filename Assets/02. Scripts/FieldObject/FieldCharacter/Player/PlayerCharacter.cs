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


        private void Start()
        {
            InitTestAsync().Forget();
        }

        protected override void Update()
        {
            base.Update();
            if (pmc != null) pmc.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (pmc != null) pmc.OnFixedUpdate();
        }

        private async UniTaskVoid InitTestAsync()
        {
            await UniTask.WaitUntil(() => Main.Instance.IsBootCompleted);
            InitPlayer("test");
            GainSoul("test2");
        }

        public void InitPlayer(string _mainSoulKey)
        { 
            pmc = GetComponent<PlayerMovementController>();
            psc = new PlayerSoulController();
            psc.InitPSC(this, _mainSoulKey);

            var soulSettingData = Main.Instance.DataManager.SettingData.CharacterSettingData.GetSoulSettingData(_mainSoulKey);
            if (soulSettingData == null)
            {
                Debug.LogError($"[PlayerCharacter] CharacterSettingData 없음: {_mainSoulKey}");
                return;
            }

            var playerAttributeSet = new PlayerAttributeSet();
            playerAttributeSet.InitPlayerAttributeSet(soulSettingData.AttributeSetSettingData);

            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, playerAttributeSet, soulSettingData.WeaponType);

            if (soulSettingData.SkillKeys != null)
            {
                foreach (var skillKey in soulSettingData.SkillKeys)
                    BSC.SSC.GiveSkill(skillKey);
            }

            if (soulSettingData.SkinKeys != null && soulSettingData.SkinKeys.Count > 0)
                SpineController.SetCombinedSkin(soulSettingData.SkinKeys);

            pmc.InitController(BSC.WSC);
        }

        public void GainSoul(string _soulKey)
        {
            if (psc.SubSoulKey != null) return; // todo :: 영혼 교체 구현

            psc.SetSubSoul(_soulKey);

            var subData = Main.Instance.DataManager.SettingData.CharacterSettingData.GetSoulSettingData(_soulKey);
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

            var newSoulData = Main.Instance.DataManager.SettingData.CharacterSettingData.GetSoulSettingData(psc.SubSoulKey);
            var playerAttributeSet = (PlayerAttributeSet)BSC.AttributeSet;

            playerAttributeSet.SwapBaseValues(newSoulData.AttributeSetSettingData);
            BSC.WSC.ChangeWeaponType(newSoulData.WeaponType);
            SpineController.SetCombinedSkin(newSoulData.SkinKeys);
            pmc.SetPlayerState(EMoveState.Idle);

            float restoredHealth = psc.SwapSlots(playerAttributeSet.Health);
            playerAttributeSet.Health = Mathf.Min(restoredHealth, playerAttributeSet.MaxHealth.Value);
        }
    }
}
