using MS.Field;
using MS.Manager;
using UnityEngine;


namespace MS.Core
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

                MSEffect effect = EffectManager.Instance.PlayEffect(
                    EffectKey,
                    spawnPosition,
                    spawnRotation
                );

                if (EffectDuration > 0) effect.SetDuration(EffectDuration); 
                if (AttachToOwner) effect.SetTraceTarget(_owner, EffectOffset);
            }

            if (!string.IsNullOrEmpty(SoundKey))
            {
                SoundManager.Instance.PlaySFX(SoundKey);
            }

            if (CameraShakeIntensity > 0)
            {
                CameraManager.Instance.ShakeCamera(CameraShakeIntensity, CameraShakeDuration);
            }
        }

        public void Play(Vector3 _pos)
        {
            if (!string.IsNullOrEmpty(EffectKey))
            {
                Vector3 spawnPosition = _pos + EffectOffset;

                MSEffect effect = EffectManager.Instance.PlayEffect(
                    EffectKey,
                    spawnPosition,
                    Quaternion.Euler(EffectRotation)
                );

                if (EffectDuration > 0) effect.SetDuration(EffectDuration);
            }

            if (!string.IsNullOrEmpty(SoundKey))
            {
                SoundManager.Instance.PlaySFX(SoundKey);
            }

            if (CameraShakeIntensity > 0)
            {
                CameraManager.Instance.ShakeCamera(CameraShakeIntensity, CameraShakeDuration);
            }
        }
    }
}