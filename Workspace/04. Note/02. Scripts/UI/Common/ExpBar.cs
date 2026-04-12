using MS.Field;
using MS.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MS.UI
{
    public class ExpBar : MonoBehaviour
    {
        private Image imgBar;


        public void InitExpBar()
        {
            imgBar = transform.FindChildComponentDeep<Image>("ResourceBar");
            imgBar.fillAmount = 0;
        }

        public void UpdateExpBar(float _ratio)
        {
            imgBar.fillAmount = _ratio;
        }
    }
}