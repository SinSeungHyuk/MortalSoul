using Cysharp.Threading.Tasks;
using MS.Manager;
using MS.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace MS.UI
{
    public class TitlePanel : BaseUI
    {
        private Button btnTitle;
        private TextMeshProUGUI txtTitle;
        private long downloadSize;
        private bool isPatchProcessFinished = false;


        private void Awake()
        {
            btnTitle = transform.FindChildComponentDeep<Button>("BtnTitle");
            txtTitle = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtTitle"); 
            btnTitle.onClick.AddListener(OnBtnTitleClicked);
            btnTitle.interactable = false; // 패치 체크 전까지 클릭 방지
        }

        private void Start()
        {
            PlayTitleBGMAsync().Forget();
            CheckPatchInfoAsync().Forget();
        }

        private async UniTask CheckPatchInfoAsync()
        {
            try
            {
                txtTitle.text = "리소스 정보를 확인 중입니다...";

                // 1. 어드레서블 초기화
                await Addressables.InitializeAsync();

                // 2. 카탈로그 업데이트 확인 (CCD 변경 사항 감지)
                var checkHandle = Addressables.CheckForCatalogUpdates(false);
                List<string> catalogs = await checkHandle.ToUniTask();
                Addressables.Release(checkHandle);

                if (catalogs != null && catalogs.Count > 0)
                {
                    txtTitle.text = "리소스 목록을 갱신 중입니다...";
                    var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                    await updateHandle.ToUniTask();
                    Addressables.Release(updateHandle);
                }

                // 3. 다운로드 사이즈 체크
                downloadSize = await Addressables.GetDownloadSizeAsync("Remote");
                if (downloadSize > 0)
                {
                    float mbSize = downloadSize / (1024f * 1024f);
                    txtTitle.text = $"터치하여 데이터 다운로드 ({mbSize:F2}MB)";
                }
                else
                {
                    txtTitle.text = "터치하여 게임 시작";
                }

                isPatchProcessFinished = true;
                btnTitle.interactable = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Patch Check Failed: {e.Message}");
                txtTitle.text = "네트워크 오류";
                btnTitle.interactable = true;
            }
        }

        private void OnBtnTitleClicked()
        {
            if (downloadSize > 0)
                DownloadPatchAsync().Forget();
            else
                GameManager.Instance.StartGameAsync().Forget();
        }

        private async UniTask DownloadPatchAsync()
        {
            btnTitle.interactable = false;

            DownloadPopup downloadPanel = UIManager.Instance.ShowPopup<DownloadPopup>("DownloadPopup");
            downloadPanel.InitDownloadPopup(downloadSize);

            var downloadHandle = Addressables.DownloadDependenciesAsync("Remote", false);

            while (!downloadHandle.IsDone)
            {
                DownloadStatus status = downloadHandle.GetDownloadStatus();
                float percent = status.Percent;

                downloadPanel.UpdateProgress(percent);
                await UniTask.Yield();
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(downloadHandle);
                downloadPanel.Close();
                downloadSize = 0;
                GameManager.Instance.StartGameAsync().Forget();
            }
            else
            {
                Addressables.Release(downloadHandle);
                Debug.LogError("Download Failed");
            }
        }

        private async UniTask PlayTitleBGMAsync()
        {
            await SoundManager.Instance.InitSoundAsync();
            SoundManager.Instance.PlayBGM("BGM_Title");
        }
    }
}