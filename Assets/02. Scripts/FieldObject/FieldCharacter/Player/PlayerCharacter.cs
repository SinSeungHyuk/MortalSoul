using Core;
using Cysharp.Threading.Tasks;
using MS.Battle;
using MS.Data;
using UnityEngine;

namespace MS.Field
{
    public class PlayerCharacter : FieldCharacter
    {
        protected override void Awake()
        {
            base.Awake();
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

        protected override void Update()
        {
            base.Update();
        }

        public void InitPlayer(string _characKey)
        {
            var characDict = Main.Instance.DataManager.SettingData.CharacterSettingData?.CharacterSettingDataDict;
            if (characDict == null || !characDict.TryGetValue(_characKey, out CharacterSettingData characData))
            {
                Debug.LogError($"[PlayerCharacter] CharacterSettingData 없음: {_characKey}");
                return;
            }

            var attrSet = new PlayerAttributeSet();
            attrSet.InitAttributeSet(characData.AttributeSetSettingData);

            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, attrSet, characData.WeaponType);

            if (characData.SkinKeys != null && characData.SkinKeys.Count > 0)
            {
                var skinKeys = new string[characData.SkinKeys.Count];
                characData.SkinKeys.Values.CopyTo(skinKeys, 0);
                SpineController.SetCombinedSkin(skinKeys);
            }

            GetComponent<PlayerController>().InitController(BSC.WSC);
        }
    }
}
