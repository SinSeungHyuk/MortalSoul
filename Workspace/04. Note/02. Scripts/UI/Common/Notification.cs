using Cysharp.Threading.Tasks;
using DG.Tweening;
using MS.Manager;
using MS.Utils;
using System;
using TMPro;
using UnityEngine;


namespace MS.UI
{
    public class Notification : BaseUI
    {
        private CanvasGroup notificationGroup;
        private TextMeshProUGUI txtTitle;
        private TextMeshProUGUI txtSubtitle;


        public void InitNotification(string _titleKey, string _subtitleKey)
        {
            FindComponents();
            string titleText = StringTable.Instance.Get("Notification", _titleKey);
            string subtitleText = StringTable.Instance.Get("Notification", _subtitleKey);
            txtTitle.text = titleText;
            txtSubtitle.text = subtitleText;

            PlayNotificationAsync().Forget();
        }

        private void FindComponents()
        {
            if (notificationGroup != null) return;

            notificationGroup = transform.FindChildComponentDeep<CanvasGroup>("NotificationGroup");
            txtTitle = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtTitle");
            txtSubtitle = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtSubtitle");
        }

        private async UniTaskVoid PlayNotificationAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();

            DOTween.Kill(notificationGroup);
            notificationGroup.alpha = 0f;

            await notificationGroup.DOFade(1f, 1.5f).ToUniTask(cancellationToken: token);
            await UniTask.WaitForSeconds(2.5f, cancellationToken: token);
            await notificationGroup.DOFade(0f, 1.5f).ToUniTask(cancellationToken: token);
            gameObject.SetActive(false);
        }
    }
}