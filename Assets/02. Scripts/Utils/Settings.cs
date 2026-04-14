using UnityEngine;


namespace MS.Utils
{
    public static class Settings
    {
        #region PLAYER CHARACTER SETTING
        // 이동
        public static float MoveSpeed = 5f;
        public static float AirControlMultiplier = 0.8f;

        // 점프 / 중력
        public static float JumpForce = 12f;
        public static float GravityScale = 3f;
        public static float FallMultiple = 2.5f;
        public static float MaxFallSpeed = -20f;

        // 대시
        public static float DashSpeed = 20f;
        public static float DashDuration = 0.2f;
        public static float DashCooldown = 1f;
        public static float DashEndFreezeDuration = 0.15f;

        // 지면 판정
        public static Vector2 GroundCheckSize = new Vector2(0.4f, 0.05f);
        public static float GroundCheckDistance = 0.1f;
        #endregion
        
        #region ANIMATION KEY SETTING
        public const string AnimIdle = "Wait1";
        public const string AnimRun  = "Run1";
        public const string AnimJump = "Wait4";
        public const string AnimDash = "Run3";
        public const int SpineMainTrack = 0;
        public const string AnimComboRdyEvent = "combo_ready";
        #endregion

        #region BATTLE SETTING
        public static int BattleScalingConstant = 100;
        public static float BasicAttackKnockback = 2f;
        public static float WeaknessAttributeMultiple = 1.3f;
        public static float LifeStealValue = 1f;
        #endregion

        #region MONSTER SETTING
        public static float MonsterDetectionRange = 8f;
        public static float MonsterLayerThresholdY = 2f;
        public static float MonsterPatrolWaitTime = 1f;
        #endregion

        #region LAYERMASK SETTING
        public static LayerMask MonsterLayer = LayerMask.GetMask("Monster");
        public static LayerMask PlayerLayer = LayerMask.GetMask("Player");
        public static LayerMask GroundLayer = LayerMask.GetMask("Ground");
        #endregion

        #region COLOR SETTING
        public static Color32 Green = new Color32(22, 135, 24, 255);
        public static Color32 Beige = new Color32(207, 182, 151, 255);
        public static Color32 Rare = new Color32(11, 110, 204, 255);
        public static Color32 Unique = new Color32(155, 61, 217, 255);
        public static Color32 Legend = new Color32(255, 112, 120, 255);
        public static Color32 Critical = new Color32(255, 102, 2, 255);
        public static Color32 Magnet = new Color32(255, 191, 0, 255);
        #endregion
    }
}