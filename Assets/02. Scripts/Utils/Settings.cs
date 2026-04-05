using UnityEngine;


namespace MS.Utils
{
    public static class Settings
    {
        #region PLAYER CHARACTER SETTING
        public static float MoveSpeed = 5f;
        public static float JumpForce = 12f;
        public static float GravityScale = 3f;
        public static float FallMultiplier = 2.5f;
        public static float MaxFallSpeed = -20f;
        public static float DashSpeed = 30f;
        public static float DashDuration = 0.3f;
        public static float DashCooldown = 0.8f;
        public static float AirControlMultiplier = 0.8f;
        #endregion
        
        #region BATTLE SETTING
        public static int BattleScalingConstant = 100;
        #endregion

        #region LAYERMASK SETTING
        public static LayerMask MonsterLayer = LayerMask.GetMask("Monster"); // ���� ���̾�
        public static LayerMask PlayerLayer = LayerMask.GetMask("Player"); // �÷��̾� ���̾�
        #endregion

        #region COLOR SETTING
        public static Color32 Green = new Color32(22, 135, 24, 255);
        public static Color32 Beige = new Color32(207, 182, 151, 255);
        public static Color32 Rare = new Color32(11, 110, 204, 255); // �Ķ�
        public static Color32 Unique = new Color32(155, 61, 217, 255); // ����
        public static Color32 Legend = new Color32(255, 112, 120, 255); // ����
        public static Color32 Critical = new Color32(255, 102, 2, 255); // ��Ȳ
        public static Color32 Magnet = new Color32(255, 191, 0, 255); // Ȳ��
        #endregion

        #region ANIMATOR HASH
        public static readonly int AnimHashCasting = Animator.StringToHash("Casting");
        #endregion
    }
}