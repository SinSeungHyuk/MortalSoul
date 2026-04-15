using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Utils;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MS.UI
{
    public class DialoguePopup : BasePopup
    {
        private Button btnBackdrop;
        private TextMeshProUGUI textSpeaker;
        private TextMeshProUGUI textContent;
        private SkeletonGraphic portraitLeft;
        private SkeletonGraphic portraitRight;

        private bool isTyping;
        private UniTaskCompletionSource touchTcs;

        private string curLeftSpineKey;
        private string curRightSpineKey;


        public async UniTask PlayDialogueAsync(string _dialogueKey)
        {
            FindComponents();

            if (!Main.Instance.DataManager.SettingData.DialogueDict.TryGetValue(_dialogueKey, out var data))
            {
                Debug.LogError($"[DialoguePopup] 대화 데이터 없음: {_dialogueKey}");
                return;
            }

            portraitLeft.gameObject.SetActive(false);
            portraitRight.gameObject.SetActive(false);

            for (int i = 0; i < data.DialogueList.Count; i++)
            {
                DialogueLineSettingData line = data.DialogueList[i];
                await ApplyPortraitAsync(line);
                textSpeaker.text = line.SpeakerKey;
                await TypeTextAsync(line.Text);
                await WaitForTouchAsync();
            }
        }

        private void FindComponents()
        {
            if (btnBackdrop != null) return;

            btnBackdrop = transform.FindChildComponentDeep<Button>("Backdrop");
            textSpeaker = transform.FindChildComponentDeep<TextMeshProUGUI>("SpeakerText");
            textContent = transform.FindChildComponentDeep<TextMeshProUGUI>("ContentText");
            portraitLeft = transform.FindChildComponentDeep<SkeletonGraphic>("PortraitLeft");
            portraitRight = transform.FindChildComponentDeep<SkeletonGraphic>("PortraitRight");

            btnBackdrop.onClick.AddListener(OnBtnBackdropClicked);
        }

        private async UniTask ApplyPortraitAsync(DialogueLineSettingData _line)
        {
            SkeletonGraphic target = _line.IsLeft ? portraitLeft : portraitRight;
            SkeletonGraphic other = _line.IsLeft ? portraitRight : portraitLeft;

            other.gameObject.SetActive(false);
            target.gameObject.SetActive(true);

            bool needReinit = _line.IsLeft
                ? curLeftSpineKey != _line.PortraitSpineKey
                : curRightSpineKey != _line.PortraitSpineKey;

            if (needReinit)
            {
                SkeletonDataAsset dataAsset = await Main.Instance.AddressableManager.LoadResourceAsync<SkeletonDataAsset>(_line.PortraitSpineKey);
                if (dataAsset == null)
                {
                    Debug.LogError($"[DialoguePopup] SkeletonDataAsset 로드 실패: {_line.PortraitSpineKey}");
                    return;
                }
                target.skeletonDataAsset = dataAsset;
                target.Initialize(true);

                if (_line.IsLeft) curLeftSpineKey = _line.PortraitSpineKey;
                else curRightSpineKey = _line.PortraitSpineKey;
            }

            ApplySkinCombined(target, _line.SkinKeyList);
            target.AnimationState.SetAnimation(0, Settings.AnimIdle, true);
        }

        private void ApplySkinCombined(SkeletonGraphic _graphic, List<string> _skinKeys)
        {
            Skeleton skeleton = _graphic.Skeleton;
            Skin combined = new Skin("combined");

            if (_skinKeys != null)
            {
                for (int i = 0; i < _skinKeys.Count; i++)
                {
                    Skin skin = skeleton.Data.FindSkin(_skinKeys[i]);
                    if (skin != null) combined.AddSkin(skin);
                }
            }

            skeleton.SetSkin(combined);
            skeleton.SetSlotsToSetupPose();
            _graphic.AnimationState.Apply(skeleton);
        }

        private async UniTask TypeTextAsync(string _text)
        {
            textContent.text = _text;
            textContent.maxVisibleCharacters = 0;
            isTyping = true;

            for (int i = 1; i <= _text.Length; i++)
            {
                if (!isTyping) break;
                textContent.maxVisibleCharacters = i;
                await UniTask.Delay(TimeSpan.FromSeconds(1f / 30f), ignoreTimeScale: true);
            }

            textContent.maxVisibleCharacters = _text.Length;
            isTyping = false;
        }

        private UniTask WaitForTouchAsync()
        {
            touchTcs = new UniTaskCompletionSource();
            return touchTcs.Task;
        }

        private void OnBtnBackdropClicked()
        {
            if (isTyping)
            {
                isTyping = false;
                return;
            }
            touchTcs?.TrySetResult();
        }
    }
}
