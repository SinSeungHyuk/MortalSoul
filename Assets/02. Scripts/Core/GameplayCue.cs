using MS.Field;
using UnityEngine;


namespace Core
{
    [CreateAssetMenu(fileName = "GC_", menuName = "GameplayCue")]
    public class GameplayCue : ScriptableObject
    {
        [Header("Effects")]
        public string EffectKey;
        public Vector3 EffectOffset;
        public Vector3 EffectRotation;
        public float EffectDuration;
        public bool AttachToOwner;

        [Header("Sounds")]
        public string SoundKey;

        [Header("Camera")]
        public float CameraShakeIntensity;
        public float CameraShakeDuration;


        public void Play(FieldObject _owner)
        {
            if (!string.IsNullOrEmpty(EffectKey))
            {
                Vector3 spawnPosition =
                    _owner.Position +
                    _owner.transform.TransformDirection(EffectOffset);
                Quaternion spawnRotation =
                    _owner.Rotation * Quaternion.Euler(EffectRotation);

                MSEffect effect = Main.Instance.EffectManager.PlayEffect(
                    EffectKey,
                    spawnPosition,
                    spawnRotation
                );

                if (effect == null) return;
                if (EffectDuration > 0) effect.SetDuration(EffectDuration);
                if (AttachToOwner) effect.SetTraceTarget(_owner, EffectOffset);
            }

            if (!string.IsNullOrEmpty(SoundKey))
            {
                Main.Instance.SoundManager.PlaySFX(SoundKey);
            }

            // TODO: CameraManager 이주 후 연결
            // if (CameraShakeIntensity > 0)
            //     Main.Instance.CameraManager.ShakeCamera(CameraShakeIntensity, CameraShakeDuration);
        }

        public void Play(Vector3 _pos)
        {
            if (!string.IsNullOrEmpty(EffectKey))
            {
                Vector3 spawnPosition = _pos + EffectOffset;

                MSEffect effect = Main.Instance.EffectManager.PlayEffect(
                    EffectKey,
                    spawnPosition,
                    Quaternion.Euler(EffectRotation)
                );

                if (effect == null) return;
                if (EffectDuration > 0) effect.SetDuration(EffectDuration);
            }

            if (!string.IsNullOrEmpty(SoundKey))
            {
                Main.Instance.SoundManager.PlaySFX(SoundKey);
            }

            // TODO: CameraManager 이주 후 연결
            // if (CameraShakeIntensity > 0)
            //     Main.Instance.CameraManager.ShakeCamera(CameraShakeIntensity, CameraShakeDuration);
        }
    }
}
