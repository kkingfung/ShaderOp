#nullable enable

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
// using Firebase;
// using Firebase.Auth;
// using Firebase.Extensions;

namespace ShaderOp.Core.Services.Online
{
    /// <summary>
    /// Firebase認証サービス実装
    /// </summary>
    /// <remarks>
    /// 現在はモック実装です。
    /// Firebase Unity SDKインストール後、コメントを解除して実装します。
    /// </remarks>
    public class FirebaseAuthService : MonoBehaviour, IFirebaseAuthService
    {
        // ============================================
        // モックフィールド
        // ============================================

        private bool _isAuthenticated = false;
        private string? _currentUserId = null;
        private string? _currentDisplayName = null;
        private string? _currentEmail = null;
        private string? _currentIdToken = null;

        // ============================================
        // プロパティ実装
        // ============================================

        /// <summary>認証済みか</summary>
        public bool IsAuthenticated => _isAuthenticated;

        /// <summary>現在のユーザーID</summary>
        public string? CurrentUserId => _currentUserId;

        /// <summary>現在のユーザー表示名</summary>
        public string? CurrentUserDisplayName => _currentDisplayName;

        /// <summary>現在のユーザーメールアドレス</summary>
        public string? CurrentUserEmail => _currentEmail;

        /// <summary>最後に取得したIDトークン</summary>
        public string? CurrentIdToken => _currentIdToken;

        // ============================================
        // イベント
        // ============================================

        /// <summary>認証状態変更イベント</summary>
        public event Action<bool>? OnAuthStateChanged;

        // ============================================
        // Firebase オブジェクト（SDK インストール後に有効化）
        // ============================================

        // private FirebaseAuth? _auth;
        // private FirebaseUser? _currentUser;

        // ============================================
        // 初期化
        // ============================================

        private void Awake()
        {
            // Firebase SDKインストール後の実装:
            /*
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError($"[FirebaseAuthService] Could not resolve Firebase dependencies: {dependencyStatus}");
                }
            });
            */

            Debug.LogWarning("[FirebaseAuthService] Firebase SDK not installed. Using mock implementation.");
        }

        /*
        private void InitializeFirebase()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _auth.StateChanged += OnFirebaseAuthStateChanged;

            if (_auth.CurrentUser != null)
            {
                _currentUser = _auth.CurrentUser;
                UpdateCurrentUserInfo();
            }

            Debug.Log("[FirebaseAuthService] Firebase initialized successfully.");
        }

        private void OnFirebaseAuthStateChanged(object sender, System.EventArgs e)
        {
            if (_auth!.CurrentUser != _currentUser)
            {
                bool signedIn = _auth.CurrentUser != null && _auth.CurrentUser != _currentUser;
                _currentUser = _auth.CurrentUser;

                if (signedIn)
                {
                    UpdateCurrentUserInfo();
                    OnAuthStateChanged?.Invoke(true);
                }
                else
                {
                    ClearCurrentUserInfo();
                    OnAuthStateChanged?.Invoke(false);
                }
            }
        }
        */

        // ============================================
        // 認証メソッド実装
        // ============================================

        /// <summary>
        /// メールアドレスとパスワードでサインイン
        /// </summary>
        public async UniTask<bool> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            Debug.Log($"[FirebaseAuthService] Sign in with email: {email}");

            // モック実装
            await UniTask.Delay(1000);
            _isAuthenticated = true;
            _currentUserId = "mock_user_001";
            _currentDisplayName = "Mock User";
            _currentEmail = email;
            _currentIdToken = "mock_id_token_" + Guid.NewGuid().ToString();

            OnAuthStateChanged?.Invoke(true);
            Debug.Log("[FirebaseAuthService] Sign in successful (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                var result = await _auth!.SignInWithEmailAndPasswordAsync(email, password);
                _currentUser = result.User;
                UpdateCurrentUserInfo();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Sign in failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// メールアドレスとパスワードで新規登録
        /// </summary>
        public async UniTask<bool> SignUpWithEmailAndPasswordAsync(string email, string password, string? displayName = null)
        {
            Debug.Log($"[FirebaseAuthService] Sign up with email: {email}");

            // モック実装
            await UniTask.Delay(1000);
            _isAuthenticated = true;
            _currentUserId = "mock_user_" + Guid.NewGuid().ToString().Substring(0, 8);
            _currentDisplayName = displayName ?? "New User";
            _currentEmail = email;
            _currentIdToken = "mock_id_token_" + Guid.NewGuid().ToString();

            OnAuthStateChanged?.Invoke(true);
            Debug.Log("[FirebaseAuthService] Sign up successful (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                var result = await _auth!.CreateUserWithEmailAndPasswordAsync(email, password);
                _currentUser = result.User;

                if (!string.IsNullOrEmpty(displayName))
                {
                    var profile = new UserProfile { DisplayName = displayName };
                    await _currentUser.UpdateUserProfileAsync(profile);
                }

                UpdateCurrentUserInfo();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Sign up failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// 匿名ログイン
        /// </summary>
        public async UniTask<bool> SignInAnonymouslyAsync()
        {
            Debug.Log("[FirebaseAuthService] Sign in anonymously");

            // モック実装
            await UniTask.Delay(500);
            _isAuthenticated = true;
            _currentUserId = "mock_anonymous_" + Guid.NewGuid().ToString().Substring(0, 8);
            _currentDisplayName = "Anonymous";
            _currentEmail = null;
            _currentIdToken = "mock_id_token_" + Guid.NewGuid().ToString();

            OnAuthStateChanged?.Invoke(true);
            Debug.Log("[FirebaseAuthService] Anonymous sign in successful (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                var result = await _auth!.SignInAnonymouslyAsync();
                _currentUser = result.User;
                UpdateCurrentUserInfo();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Anonymous sign in failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// サインアウト
        /// </summary>
        public void SignOut()
        {
            Debug.Log("[FirebaseAuthService] Sign out");

            // モック実装
            _isAuthenticated = false;
            _currentUserId = null;
            _currentDisplayName = null;
            _currentEmail = null;
            _currentIdToken = null;

            OnAuthStateChanged?.Invoke(false);

            /* Firebase SDKインストール後の実装:
            _auth?.SignOut();
            ClearCurrentUserInfo();
            */
        }

        /// <summary>
        /// IDトークンを取得
        /// </summary>
        public async UniTask<string?> GetIdTokenAsync(bool forceRefresh = false)
        {
            Debug.Log($"[FirebaseAuthService] Get ID token (forceRefresh: {forceRefresh})");

            // モック実装
            await UniTask.Delay(100);

            if (!_isAuthenticated)
            {
                Debug.LogWarning("[FirebaseAuthService] Not authenticated");
                return null;
            }

            if (forceRefresh)
            {
                _currentIdToken = "mock_id_token_refreshed_" + Guid.NewGuid().ToString();
            }

            return _currentIdToken;

            /* Firebase SDKインストール後の実装:
            try
            {
                if (_currentUser == null)
                {
                    Debug.LogWarning("[FirebaseAuthService] No current user");
                    return null;
                }

                var tokenResult = await _currentUser.TokenAsync(forceRefresh);
                _currentIdToken = tokenResult;
                return tokenResult;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Get ID token failed: {ex.Message}");
                return null;
            }
            */
        }

        /// <summary>
        /// パスワードリセットメール送信
        /// </summary>
        public async UniTask<bool> SendPasswordResetEmailAsync(string email)
        {
            Debug.Log($"[FirebaseAuthService] Send password reset email to: {email}");

            // モック実装
            await UniTask.Delay(1000);
            Debug.Log("[FirebaseAuthService] Password reset email sent (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                await _auth!.SendPasswordResetEmailAsync(email);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Send password reset email failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// 表示名を更新
        /// </summary>
        public async UniTask<bool> UpdateDisplayNameAsync(string displayName)
        {
            Debug.Log($"[FirebaseAuthService] Update display name to: {displayName}");

            // モック実装
            await UniTask.Delay(500);
            _currentDisplayName = displayName;
            Debug.Log("[FirebaseAuthService] Display name updated (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                if (_currentUser == null)
                {
                    Debug.LogWarning("[FirebaseAuthService] No current user");
                    return false;
                }

                var profile = new UserProfile { DisplayName = displayName };
                await _currentUser.UpdateUserProfileAsync(profile);
                _currentDisplayName = displayName;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Update display name failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// メールアドレスを更新
        /// </summary>
        public async UniTask<bool> UpdateEmailAsync(string newEmail)
        {
            Debug.Log($"[FirebaseAuthService] Update email to: {newEmail}");

            // モック実装
            await UniTask.Delay(500);
            _currentEmail = newEmail;
            Debug.Log("[FirebaseAuthService] Email updated (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                if (_currentUser == null)
                {
                    Debug.LogWarning("[FirebaseAuthService] No current user");
                    return false;
                }

                await _currentUser.UpdateEmailAsync(newEmail);
                _currentEmail = newEmail;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Update email failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// パスワードを更新
        /// </summary>
        public async UniTask<bool> UpdatePasswordAsync(string newPassword)
        {
            Debug.Log("[FirebaseAuthService] Update password");

            // モック実装
            await UniTask.Delay(500);
            Debug.Log("[FirebaseAuthService] Password updated (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                if (_currentUser == null)
                {
                    Debug.LogWarning("[FirebaseAuthService] No current user");
                    return false;
                }

                await _currentUser.UpdatePasswordAsync(newPassword);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Update password failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// アカウントを削除
        /// </summary>
        public async UniTask<bool> DeleteAccountAsync()
        {
            Debug.Log("[FirebaseAuthService] Delete account");

            // モック実装
            await UniTask.Delay(500);
            SignOut();
            Debug.Log("[FirebaseAuthService] Account deleted (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                if (_currentUser == null)
                {
                    Debug.LogWarning("[FirebaseAuthService] No current user");
                    return false;
                }

                await _currentUser.DeleteAsync();
                ClearCurrentUserInfo();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Delete account failed: {ex.Message}");
                return false;
            }
            */
        }

        /// <summary>
        /// 現在のユーザー情報を再読み込み
        /// </summary>
        public async UniTask<bool> ReloadUserAsync()
        {
            Debug.Log("[FirebaseAuthService] Reload user");

            // モック実装
            await UniTask.Delay(200);
            Debug.Log("[FirebaseAuthService] User reloaded (MOCK)");
            return true;

            /* Firebase SDKインストール後の実装:
            try
            {
                if (_currentUser == null)
                {
                    Debug.LogWarning("[FirebaseAuthService] No current user");
                    return false;
                }

                await _currentUser.ReloadAsync();
                UpdateCurrentUserInfo();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthService] Reload user failed: {ex.Message}");
                return false;
            }
            */
        }

        // ============================================
        // ヘルパーメソッド
        // ============================================

        /*
        private void UpdateCurrentUserInfo()
        {
            if (_currentUser == null)
            {
                ClearCurrentUserInfo();
                return;
            }

            _isAuthenticated = true;
            _currentUserId = _currentUser.UserId;
            _currentDisplayName = _currentUser.DisplayName;
            _currentEmail = _currentUser.Email;

            Debug.Log($"[FirebaseAuthService] User info updated: {_currentDisplayName} ({_currentEmail})");
        }

        private void ClearCurrentUserInfo()
        {
            _isAuthenticated = false;
            _currentUserId = null;
            _currentDisplayName = null;
            _currentEmail = null;
            _currentIdToken = null;
        }
        */

        // ============================================
        // クリーンアップ
        // ============================================

        private void OnDestroy()
        {
            // Firebase SDKインストール後の実装:
            /*
            if (_auth != null)
            {
                _auth.StateChanged -= OnFirebaseAuthStateChanged;
            }
            */
        }
    }
}
