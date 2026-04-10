using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class MSImage : Image
    {
        private Tweener curTween;
        private Color originalColor;

        protected override void Awake()
        {
            base.Awake();
            originalColor = color;
        }

        public void ShowFade(float _duration)
        {
            KillTween();
            curTween = this.DOFade(1f, _duration);
        }

        public void HideFade(float _duration, Action _onComplete = null)
        {
            KillTween();
            curTween = this.DOFade(0f, _duration);
            if (_onComplete != null)
                curTween.OnComplete(() => _onComplete.Invoke());
        }

        public void DoBlink(float _speed)
        {
            KillTween();
            curTween = this.DOFade(0f, _speed).SetLoops(-1, LoopType.Yoyo);
        }

        public void DoSetColor(Color _target, float _duration)
        {
            KillTween();
            curTween = this.DOColor(_target, _duration);
        }

        public void StopTween()
        {
            KillTween();
            color = originalColor;
        }

        private void KillTween()
        {
            curTween?.Kill();
            curTween = null;
        }
    }
}
