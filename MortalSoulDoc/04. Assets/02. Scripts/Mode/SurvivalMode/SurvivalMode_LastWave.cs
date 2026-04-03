using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MS.Core;
using MS.Field;
using MS.Manager;
using MS.Skill;
using MS.UI;
using MS.Utils;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MS.Mode
{
    public partial class SurvivalMode
    {
        private const float METEOR_INTERVAL_MIN = 2f;
        private const float METEOR_INTERVAL_MAX = 4f;
        private const int METEOR_COUNT_MIN = 2;
        private const int METEOR_COUNT_MAX = 4;

        private Action<Vector3> OnWaveMeteor;
        private float randMeteorInterval;
        private int randMeteorCount;
        private float elapsedMeteorTime;


        private void OnLastWaveEnter(int _prev, object[] _params)
        {
            Notification notification = UIManager.Instance.ShowSystemUI<Notification>("Notification");
            if (notification)
            {
                notification.InitNotification("Warning", "LastWave");
            }

            elapsedMeteorTime = 0f;
            OnWaveMeteor = ExecuteWaveMeteor;
            SetRandMeteorParams();
        }

        private void OnLastWaveUpdate(float _dt)
        {
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

            elapsedMeteorTime += _dt;
            if (elapsedMeteorTime >= randMeteorInterval)
            {
                elapsedMeteorTime = 0f;
                for (int i = 0; i < randMeteorCount; i++)
                {
                    Vector3 spawnPos = BattleUtils.GetRandomPoint(player.Position, 15f);
                    SkillObjectManager.Instance.SpawnIndicator(spawnPos,4f, 1f, OnWaveMeteor);
                }
                SetRandMeteorParams();
            }

            if (!isBossLive)
            {
                CurWaveTimer.Value -= _dt;
                if (CurWaveTimer.Value <= 0)
                {
                    EndLastWaveAsync().Forget();
                }
            }
        }

        private void OnLastWaveExit(int _next)
        {

        }

        private async UniTask EndLastWaveAsync()
        {
            isBossLive = true;
            Vector3 spawnPos = curFieldMap.BossSpawnPoint.position;
            GameplayCueManager.Instance.PlayCue("GC_BossPortal", spawnPos);

            await UniTask.WaitForSeconds(1.5f);

            MonsterCharacter boss = MonsterManager.Instance.SpawnMonster(curWaveSpawnInfo.BossMonsterKey, spawnPos, Quaternion.identity);
            boss.SetBossMonster();
            boss.SSC.OnDeadCallback += OnLastBossMonsterDead;
            OnBossSpawned?.Invoke(boss);

            Notification notification = UIManager.Instance.ShowSystemUI<Notification>("Notification");
            if (notification)
            {
                notification.InitNotification("Warning", "BossAppear");
            }
        }

        private async UniTaskVoid WaveMeteorAsync(Vector3 _pos)
        {
            Vector3 spawnPos = new Vector3(_pos.x, _pos.y + 10f, _pos.z);

            MSEffect meteor = EffectManager.Instance.PlayEffect("Eff_WaveMeteor", spawnPos, Quaternion.identity);
            Vector3 endPos = _pos;

            await meteor.transform.DOMove(endPos, 0.5f).SetEase(Ease.InQuad)
                .ToUniTask();

            ObjectPoolManager.Instance.Return("Eff_WaveMeteor", meteor.gameObject);
            EffectManager.Instance.PlayEffect("Eff_WaveMeteorHit", _pos, Quaternion.identity);

            Collider[] hitColliders = Physics.OverlapSphere(_pos, 2f, Settings.PlayerLayer);
            foreach (var hit in hitColliders)
            {
                if (hit.TryGetComponent<PlayerCharacter>(out var player))
                {
                    DamageInfo damageInfo = new DamageInfo(
                        _attacker: null,
                        _target: player,
                        _attributeType: EDamageAttributeType.None,
                        _damage: 50f,
                        _isCritic: false,
                        _knockbackForce: 0f
                    );
                    player.SSC.TakeDamage(damageInfo);
                }
            }
        }

        private void ExecuteWaveMeteor(Vector3 _pos)
        {
            WaveMeteorAsync(_pos).Forget();
        }

        private void SetRandMeteorParams()
        {
            randMeteorInterval = Random.Range(METEOR_INTERVAL_MIN, METEOR_INTERVAL_MAX);
            randMeteorCount = Random.Range(METEOR_COUNT_MIN, METEOR_COUNT_MAX + 1);
        }
    }
}