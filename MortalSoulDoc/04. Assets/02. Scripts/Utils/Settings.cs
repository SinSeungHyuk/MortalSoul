using UnityEngine;


namespace MS.Utils
{
    public static class Settings
    {
        #region BATTLE SETTING
        public static int MaxWaveCount =5;
        public static int BattleScalingConstant = 100;
        public static float WeaknessAttributeMultiple = 1.3f; // 약점속성 추가 데미지
        public static int LifeStealValue = 1;

        public static float DefaultMinSpawnDistance = 5f; // 스폰시 플레이어와 최소 간격
        public static float DefaultMaxSpawnDistance = 10f;
        public static float BossScaleMultiple = 1.5f;

        public static float WaveTimer = 30f; // 웨이브 당 시간
        public static float AddWaveTimePerWave = 5f; // 웨이브마다 추가되는 시간
        #endregion

        #region LAYERMASK SETTING
        public static LayerMask MonsterLayer = LayerMask.GetMask("Monster"); // 몬스터 레이어
        public static LayerMask PlayerLayer = LayerMask.GetMask("Player"); // 플레이어 레이어
        #endregion

        #region ANIMATION
        public static int AnimHashAttack = Animator.StringToHash("Attack01");
        public static int AnimHashCasting = Animator.StringToHash("Attack02Casting");

        public static int AnimHashSpeed = Animator.StringToHash("Speed");
        public static int AnimHashRun = Animator.StringToHash("Run");
        public static int AnimHashIdle = Animator.StringToHash("Idle");
        public static int AnimHashDead = Animator.StringToHash("Dead");
        #endregion

        #region COLOR SETTING
        public static Color32 Green = new Color32(22, 135, 24, 255);
        public static Color32 Beige = new Color32(207, 182, 151, 255);
        public static Color32 Rare = new Color32(11, 110, 204, 255); // 파랑
        public static Color32 Unique = new Color32(155, 61, 217, 255); // 보라
        public static Color32 Legend = new Color32(255, 112, 120, 255); // 빨강
        public static Color32 Critical = new Color32(255, 102, 2, 255); // 주황
        public static Color32 Magnet = new Color32(255, 191, 0, 255); // 황금
        #endregion
    }
}