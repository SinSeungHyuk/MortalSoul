using System;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.UI;
using UnityEngine;

namespace MS.Mode
{
    public partial class SurvivalMode
    {
        private void OnLoadEnter(int _prev, object[] _params)
        {
            SoundManager.Instance.PlayBGM("BGM_Stage");
            LoadSurvivalModeAsync().Forget();
        }

        private void OnLoadUpdate(float _dt)
        {

        }

        private void OnLoadExit(int _next)
        {

        }
        
        private async UniTask LoadSurvivalModeAsync()
        {
            try
            {
                UIManager.Instance.ShowSystemUI<BaseUI>("LoadingPanel");
                
                await EffectManager.Instance.LoadAllEffectAsync();
                await SkillObjectManager.Instance.LoadAllSkillObjectAsync();
                await FieldItemManager.Instance.LoadAllFieldItemAsync();
                await MonsterManager.Instance.LoadAllMonsterAsync(stageSettingData);

                GameObject map = await AddressableManager.Instance.LoadResourceAsync<GameObject>(stageSettingData.MapKey);
                curFieldMap = GameObject.Instantiate(map,Vector3.zero, Quaternion.identity).GetComponent<FieldMap>();

                player = await PlayerManager.Instance.SpawnPlayerCharacter("TestCharacter");

                battlePanel = UIManager.Instance.ShowView<BattlePanel>("BattlePanel");
                BattlePanelViewModel data = new BattlePanelViewModel(this, player);
                battlePanel.InitBattlePanel(data);

                player.InitPlayer("TestCharacter");
                player.LevelSystem.CurLevel.Subscribe(OnPlayerLevelUpCallback);
                player.SSC.OnDeadCallback += OnPlayerDeadCallback;

                modeStateMachine.TransitState((int)SurvivalModeState.BattleStart);
            }
            catch (Exception e)
            {
                Debug.LogError($"SurvivalMode::LoadSurvivalModeAsync() Failed: {e.Message}");
            }
            finally
            {
                UIManager.Instance.CloseUI("LoadingPanel");
            }
        }
    }
}