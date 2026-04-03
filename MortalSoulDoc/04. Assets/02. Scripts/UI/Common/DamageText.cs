using MS.Manager;
using MS.Utils;
using System;
using System.Collections;
using TMPro;
using UnityEngine;


namespace MS.UI
{
    public class DamageText : MonoBehaviour
    {
        private TextMeshProUGUI txtDamage;
        private float moveSpeed = 2.0f;
        private float duration = 0.8f;
        private Vector3 baseScale = new Vector3(0.1f, 0.1f, 0.1f);
        private static Camera mainCam;
        private Coroutine damageTextEffectCoroutine;


        private void Awake()
        {
            if (txtDamage == null) txtDamage = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtDamage");
            if (mainCam == null) mainCam = Camera.main;
        }

        public void InitDamageText(int _damage, bool _isCritic)
        {
            transform.localScale = _isCritic ? baseScale * 1.2f : baseScale;

            if (mainCam != null)
                transform.rotation = mainCam.transform.rotation;

            txtDamage.text = _damage.ToString();
            txtDamage.color = _isCritic ? Settings.Critical : Color.white;

            StartDamageTextEffect();
        }

        public void InitEvasionText()
        {
            transform.localScale = baseScale;
            if (mainCam != null)
                transform.rotation = mainCam.transform.rotation;

            txtDamage.text = StringTable.Instance.Get("Battle", "Evasion");
            txtDamage.color = Settings.Green;

            StartDamageTextEffect();
        }

        private void StartDamageTextEffect()
        {
            if (damageTextEffectCoroutine != null)
            {
                StopCoroutine(damageTextEffectCoroutine);
            }

            damageTextEffectCoroutine = StartCoroutine(DamageTextEffectCoroutine());
        }
        
        private IEnumerator DamageTextEffectCoroutine()
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                if (!gameObject.activeInHierarchy)
                {
                    damageTextEffectCoroutine = null;
                    yield break;
                }

                float dt = Time.deltaTime;
                elapsedTime += dt;

                transform.Translate(Vector3.up * (moveSpeed * dt), Space.World);

                yield return null;
            }

            damageTextEffectCoroutine = null;
            ObjectPoolManager.Instance.Return("DamageText", this.gameObject);
        }
    }
}
