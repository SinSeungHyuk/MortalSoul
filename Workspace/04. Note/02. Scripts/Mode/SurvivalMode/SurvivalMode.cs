using Cysharp.Threading.Tasks;
using DG.Tweening;
using MS.Core;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.UI;
using MS.Utils;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;

namespace MS.Mode
{
    public partial class SurvivalMode : GameModeBase
    {
        public event Action<MonsterCharacter> OnBossSpawned;

        private StageSettingData stageSettingData;
        private PlayerCharacter player;
        private BattlePanel battlePanel;

        public MSReactProp<int> KillCount { get; private set; } = new MSReactProp<int>(0);
        public MSReactProp<int> CurWaveCount { get; private set; } = new MSReactProp<int>(1);
        public MSReactProp<float> CurWaveTimer { get; private set; } = new MSReactProp<float>(1f);


        public SurvivalMode(StageSettingData _stageSettingData) : base()
        {
            stageSettingData = _stageSettingData;
        }

        public enum SurvivalModeState
        {
            Load,
            BattleStart,
            LastWave,
            BattleEnd,
        }

        protected override void OnRegisterStates()
        {
            modeStateMachine.RegisterState((int)SurvivalModeState.Load, OnLoadEnter, OnLoadUpdate, OnLoadExit);
            modeStateMachine.RegisterState((int)SurvivalModeState.BattleStart, OnBattleStartEnter, OnBattleStartUpdate, OnBattleStartExit);
            modeStateMachine.RegisterState((int)SurvivalModeState.LastWave, OnLastWaveEnter, OnLastWaveUpdate, OnLastWaveExit);
            modeStateMachine.TransitState((int)SurvivalModeState.Load);
        }

        public override void EndMode() 
        {
            SkillObjectManager.Instance.ClearSkillObject();
            EffectManager.Instance.ClearEffect();
            MonsterManager.Instance.ClearMonster();
            FieldItemManager.Instance.ClearFieldItem();
            PlayerManager.Instance.ClearPlayerCharacter();
            CameraManager.Instance.StopShake();

            curFieldMap.Destroy();

            ObjectPoolManager.Instance.ClearAllPools();
            SoundManager.Instance.StopBGM();
        }

        public override void OnUpdate(float _dt)
        {
            base.OnUpdate(_dt);

            SkillObjectManager.Instance.OnUpdate(_dt);
            MonsterManager.Instance.OnUpdate(_dt);
            EffectManager.Instance.OnUpdate(_dt);
            FieldItemManager.Instance.OnUpdate();
            if (battlePanel != null)
            {
                battlePanel.OnUpdate(_dt);
            }
        }


        #region Mode Callback
        private void OnMonsterDead()
        {
            if (MathUtils.IsSuccess(stageSettingData.WaveSpawnInfoList[CurWaveCount.Value - 1].FieldItemSpawnChance))
            {
                Vector3 spawnPos = curFieldMap.GetRandomSpawnPoint(player.Position, CurWaveCount.Value);
                FieldItemManager.Instance.SpawnRandomFieldItem(spawnPos);
            }

            KillCount.Value++;
        }

        private void OnBossMonsterDead()
        {
            // 보스를 처치하고 유물이 나타나는 연출
            KillCount.Value++;
            ActivateNextWaveAsync().Forget();

            Notification notification = UIManager.Instance.ShowSystemUI<Notification>("Notification");
            if (notification)
            {
                notification.InitNotification("Info", "Artifact");
            }

            Vector3 targetPos = curFieldMap.GetRandomSpawnPoint(curBoss.Position, CurWaveCount.Value);
            FieldItem artifact = FieldItemManager.Instance.SpawnFieldItem("Artifact", curBoss.Position);
            artifact.GetComponent<Collider>().enabled = false;

            artifact.transform.DOJump(targetPos, 5f, 1, 1.5f)
                .SetEase(Ease.Linear)
                .onComplete = () =>
                {
                    artifact.GetComponent<Collider>().enabled = true;
                };

            curBoss = null;
        }

        private void OnLastBossMonsterDead()
        {
            curBoss = null;
            EffectManager.Instance.PlayEffect("Eff_Firework", player.Position, Quaternion.identity);
            SoundManager.Instance.PlaySFX("FX_StageClear");

            StageStatisticsData stageData = new StageStatisticsData()
            {
                KillCount = KillCount.Value,
                Gold = player.LevelSystem.Gold.Value,
                PlayerLevel = player.LevelSystem.CurLevel.Value,
                SkillStatList = player.SSC.GetSkillStatistics(),
                IsClear = true
            };
            var popup = UIManager.Instance.ShowPopup<StageEndPopup>("StageEndPopup");
            popup.InitStageEndPopup(stageData, player);
        }

        private void OnPlayerLevelUpCallback(int _prevLv, int _curLv)
        {
            EffectManager.Instance.PlayEffect("Eff_Firework", player.Position, Quaternion.identity);
            SoundManager.Instance.PlaySFX("FX_LevelUp");
            
            List<StatRewardSettingData> selectedRewards = GetRandomStatRewards(4);

            var popup = UIManager.Instance.ShowPopup<StatRewardPopup>("StatRewardPopup");
            popup.InitStatRewardPopup(selectedRewards, player);
        }

        private void OnPlayerDeadCallback()
        {
            SoundManager.Instance.PlaySFX("FX_StageClear");
            StageStatisticsData stageData = new StageStatisticsData()
            {
                KillCount = KillCount.Value,
                Gold = player.LevelSystem.Gold.Value,
                PlayerLevel = player.LevelSystem.CurLevel.Value,
                SkillStatList = player.SSC.GetSkillStatistics(),
                IsClear = false
            };
            var popup = UIManager.Instance.ShowPopup<StageEndPopup>("StageEndPopup");
            popup.InitStageEndPopup(stageData, player);
        }
        #endregion


        private List<StatRewardSettingData> GetRandomStatRewards(int _count)
        {
            List<StatRewardSettingData> results = new List<StatRewardSettingData>();
            var statRewardDict = DataManager.Instance.StatRewardSettingDataDict;
            var statTypes = (EStatType[])Enum.GetValues(typeof(EStatType));

            while (results.Count < _count)
            {
                EGrade rndGrade = MathUtils.GetRandomGrade();
                EStatType rndStat = statTypes[UnityEngine.Random.Range(0, statTypes.Length)];

                string key = rndStat.ToString() + rndGrade.ToString(); // 스탯과 등급을 조합해서 키값 생성

                if (statRewardDict.TryGetValue(key, out StatRewardSettingData data))
                {
                    if (!results.Contains(data))
                    {
                        results.Add(data);
                    }
                }
            }

            return results;
        }
    }
}