using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace MS.Manager
{
    public partial class GameManager
    {
        private bool isGooglePlayAutoLoginStarted;

        private void InitializeGooglePlayAutoLogin()
        {
            if (isGooglePlayAutoLoginStarted)
                return;

            isGooglePlayAutoLoginStarted = true;
            AutoLoginWithGooglePlayAsync().Forget();
        }

        private async UniTaskVoid AutoLoginWithGooglePlayAsync()
        {
            try
            {
                DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != DependencyStatus.Available)
                    return;

                FirebaseAuth auth = FirebaseAuth.DefaultInstance;
                if (auth.CurrentUser != null)
                    return;

                PlayGamesPlatform.Activate();

                SignInStatus signInStatus = await AuthenticatePlayGamesAsync();
                if (signInStatus != SignInStatus.Success)
                    return;

                string serverAuthCode = await RequestServerAuthCodeAsync();
                if (string.IsNullOrEmpty(serverAuthCode))
                    return;

                Credential credential = PlayGamesAuthProvider.GetCredential(serverAuthCode);
                await auth.SignInWithCredentialAsync(credential);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static UniTask<SignInStatus> AuthenticatePlayGamesAsync()
        {
            var tcs = new UniTaskCompletionSource<SignInStatus>();
            PlayGamesPlatform.Instance.Authenticate(status => tcs.TrySetResult(status));
            return tcs.Task;
        }

        private static UniTask<string> RequestServerAuthCodeAsync()
        {
            var tcs = new UniTaskCompletionSource<string>();
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, code => tcs.TrySetResult(code));
            return tcs.Task;
        }
    }
}
