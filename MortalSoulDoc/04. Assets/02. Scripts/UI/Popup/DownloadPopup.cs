using MS.UI;
using MS.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MS.UI
{
    public class DownloadPopup : BasePopup
    {
        private Image imgBar;


        public void InitDownloadPopup(long downloadSize)
        {
            imgBar = transform.FindChildComponentDeep<Image>("ResourceBar");
            imgBar.fillAmount = 0;
        }

        public void UpdateProgress(float percent)
        {
            imgBar.fillAmount = percent;
        }
    }
}