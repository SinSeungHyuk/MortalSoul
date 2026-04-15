using Core;

namespace MS.UI
{
    public abstract class BasePopup : BaseUI
    {
        public override void Close()
        {
            Main.Instance.UIManager.ClosePopup(this);
            base.Close();
        }
    }
}
