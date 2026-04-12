using MS.Field;
using MS.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MS.UI
{
    public class HPBar : MonoBehaviour
    {
        private Image imgBar;
        private TextMeshProUGUI txtHPBar;
        private FieldCharacter owner;


        public void InitHPBar(FieldCharacter _owner)
        {
            if (imgBar == null) imgBar = transform.FindChildComponentDeep<Image>("ImgHPBar");
            if (txtHPBar == null) txtHPBar = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtHPBar");

            owner = _owner;
            imgBar.fillAmount = 1;
            _owner.SSC.OnHealthChanged += UpdateHPBar;
            _owner.SSC.OnDeadCallback += OnOwnerDead;

            float curHp = _owner.SSC.AttributeSet.Health;
            float maxHp = _owner.SSC.AttributeSet.GetStatValueByType(EStatType.MaxHealth);
            UpdateHPBar(curHp, maxHp);
        }

        private void UpdateHPBar(float _curHp, float _maxHp)
        {
            txtHPBar.text = _curHp.ToString("F0") + "/" + _maxHp.ToString("F0");
            imgBar.fillAmount = _curHp / _maxHp;
        }

        private void OnOwnerDead()
        {
            gameObject.SetActive(false);

            owner.SSC.OnHealthChanged -= UpdateHPBar;
            owner.SSC.OnDeadCallback -= OnOwnerDead;
        }
    }
}