using MS.Data;
using MS.Manager;
using MS.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MS.UI
{
    public class ArtifactPopup : BasePopup
    {
        private Image imgArtifactIcon;
        private TextMeshProUGUI txtArtifactName;
        private TextMeshProUGUI txtArtifactDesc;
        private Button btnClose;

        public void InitArtifactPopup(string _key)
        {
            FindComponents();

            Time.timeScale = 0f;

            if (DataManager.Instance.ArtifactSettingDataDict.TryGetValue(_key, out ArtifactSettingData _data))
            {
                Sprite icon = AddressableManager.Instance.LoadResource<Sprite>(_data.IconKey);
                imgArtifactIcon.sprite = icon;

                txtArtifactName.text = StringTable.Instance.Get("ArtifactName", _key);
                txtArtifactDesc.text = StringTable.Instance.Get("ArtifactDesc", _key);
            }
        }

        private void FindComponents()
        {
            if (btnClose != null) return;

            imgArtifactIcon = transform.FindChildComponentDeep<Image>("ImgArtifactIcon");
            txtArtifactName = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtArtifactName");
            txtArtifactDesc = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtArtifactDesc");
            btnClose = transform.FindChildComponentDeep<Button>("BtnClose");

            btnClose.onClick.AddListener(() => Close());
        }

        public override void Close()
        {
            base.Close();

            Time.timeScale = 1f;
        }
    }
}