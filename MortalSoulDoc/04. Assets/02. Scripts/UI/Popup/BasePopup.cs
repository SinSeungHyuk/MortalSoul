using MS.Manager;
using UnityEngine;


namespace MS.UI
{
    public abstract class BasePopup : BaseUI
    {

        public override void Close()
        {
            UIManager.Instance.ClosePopup(this);
            base.Close();
        }
    }
}