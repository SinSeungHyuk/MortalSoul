using Cysharp.Threading.Tasks;
using MS.Core;
using MS.Data;
using MS.Field;
using MS.Manager;
using MS.UI;
using MS.Utils;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace MS.Mode
{
    public partial class SurvivalMode
    {
        private WaveSpawnInfo curWaveSpawnInfo; // 현재 진행중인 웨이브 스폰정보
        private float curWaveTime; // 현재 웨이브 총 시간

        private float elapsedSpawnTime;

        private bool isActivateNextFloor = false; // 다음 웨이브 연출 중인지 여부
        private bool isBossLive = false;          // 보스가 살아있는지 여부
        private MonsterCharacter curBoss;


        private void OnBattleStartEnter(int _prev, object[] _params)
        {
            curWaveSpawnInfo = stageSettingData.WaveSpawnInfoList[0];
            elapsedSpawnTime = 0f;
            curWaveTime = Settings.WaveTimer;
            CurWaveTimer.Value = curWaveTime;
            curBoss = null;
        }

        private void OnBattleStartUpdate(float _dt)
        {
            if (isActivateNextFloor) return; // 다음 웨이브로 넘어가는 중에는 스폰 X

            elapsedSpawnTime += _dt;
            if (elapsedSpawnTime >= curWaveSpawnInfo.SpawnInterval)
            {
                elapsedSpawnTime = 0f; 
                for (int i = 0; i < curWaveSpawnInfo.CountPerSpawn; i++)
                {
                    string monsterKey = "";
                    int totalRatio = curWaveSpawnInfo.MonsterSpawnInfoList.Sum(x => x.MonsterSpawnRate);
                    int ratioSum = 0;
                    int randomRate = Random.Range(0, totalRatio);

                    foreach (var monsterInfo in curWaveSpawnInfo.MonsterSpawnInfoList)
                    {
                        ratioSum += monsterInfo.MonsterSpawnRate;
                        if (randomRate < ratioSum)
                        {
                            monsterKey = monsterInfo.MonsterKey;
                            break;
                        }
                    }

                    Vector3 spawnPos = curFieldMap.GetRandomSpawnPoint(player.Position, CurWaveCount.Value);
                    MonsterCharacter monster = MonsterManager.Instance.SpawnMonster(monsterKey, spawnPos, Quaternion.identity);
                    monster.SSC.OnDeadCallback += OnMonsterDead;
                }
            }

            if (!isBossLive)
            {
                CurWaveTimer.Value -= _dt;
                if (CurWaveTimer.Value <= 0)
                {
                    EndWaveAsync().Forget();
                }
            }
        }

        private void OnBattleStartExit(int _next)
        {

        }


        private async UniTask EndWaveAsync()
        {
            isBossLive = true;
            Vector3 spawnPos = curFieldMap.GetRandomSpawnPoint(player.Position, CurWaveCount.Value);
            GameplayCueManager.Instance.PlayCue("GC_BossPortal", spawnPos);

            await UniTask.WaitForSeconds(1.5f);

            curBoss = MonsterManager.Instance.SpawnMonster(curWaveSpawnInfo.BossMonsterKey, spawnPos, Quaternion.identity);
            curBoss.SetBossMonster();
            curBoss.SSC.OnDeadCallback += OnBossMonsterDead;
            OnBossSpawned?.Invoke(curBoss);

            Notification notification = UIManager.Instance.ShowSystemUI<Notification>("Notification");
            if (notification)
            {
                notification.InitNotification("Warning", "BossAppear");
            }
        }

        private async UniTask ActivateNextWaveAsync()
        {
            isActivateNextFloor = true;

            await UniTask.WaitForSeconds(1.5f);
            await CurFieldMap.ActivateNextFloor(CurWaveCount.Value);

            curWaveTime += Settings.AddWaveTimePerWave;
            CurWaveTimer.Value = curWaveTime;
            elapsedSpawnTime = 0f;
            isActivateNextFloor = false;
            isBossLive = false;
            curWaveSpawnInfo = stageSettingData.WaveSpawnInfoList[CurWaveCount.Value]; // 웨이브 카운트를 증가시키기 전에 스폰정보부터 교체
            CurWaveCount.Value++;

            if (CurWaveCount.Value == Settings.MaxWaveCount)
            {
                modeStateMachine.TransitState((int)SurvivalModeState.LastWave);
            }
        }
    }
}