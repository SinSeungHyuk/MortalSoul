using MS.Field;
using MS.Manager;
using System;
using UnityEngine;
using static MS.Field.FieldObject;


namespace MS.Core
{
    [RequireComponent(typeof(ParticleSystem))]
    public class MSEffect : MonoBehaviour
    {
        private ParticleSystem particle;
        private float duration;
        private float elapsedTime;
        private FieldObject traceTarget;
        private Vector3 targetOffset;

        public string PoolKey { get; set; }


        void Awake()
        {
            particle = GetComponent<ParticleSystem>();
            if (particle != null)
            {
                var main = particle.main;
                main.stopAction = ParticleSystemStopAction.Callback;
            }
        }

        void OnEnable()
        {
            if (particle != null)
            {
                particle.Play();
            }
        }

        void OnParticleSystemStopped()
        {
            ObjectPoolManager.Instance.Return(PoolKey, this.gameObject);
        }

        public void OnUpdate(float _deltaTime)
        {
            if (traceTarget != null)
            {
                if (traceTarget.ObjectLifeState == FieldObjectLifeState.Live)
                    transform.position = traceTarget.Position + targetOffset;
                else
                    ClearTraceTarget();
            }

            elapsedTime += _deltaTime;
            if (elapsedTime >= duration)
            {
                particle.Stop();
            }
        }

        public void InitEffect(string _poolKey)
        {
            PoolKey = _poolKey;
            duration = float.MaxValue;
            elapsedTime = 0f;
        }

        public void SetDuration(float _duration)
            => duration = _duration;

        public void SetTraceTarget(FieldObject _target, Vector3 _offset = default)
        {
            traceTarget = _target;
            targetOffset = _offset;
        }
        public void ClearTraceTarget()
            => traceTarget = null;

        public void StopEffect()
        {
            if (particle == null) return;
            particle.Stop();
        }
    }
}