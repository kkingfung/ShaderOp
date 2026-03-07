#nullable enable

using System;
using UniRx;
using Cysharp.Threading.Tasks;
using ShaderOp.Core.Services.Online;
using UnityEngine;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// ログイン画面のViewModel
    /// </summary>
    /// <remarks>
    /// Firebase認証を使用してユーザーのサインイン・サインアップを管理します。
    /// </remarks>
    public class LoginViewModel : IDisposable
    {
        // ============================================
        // フィールド
        // ============================================

        private readonly IFirebaseAuthService _authService;
        private readonly CompositeDisposable _disposables = new();

        // ============================================
        // プロパティ
        // ============================================

        /// <summary>メールアドレス</summary>
        public ReactiveProperty<string> Email { get; } = new("");

        /// <summary>パスワード</summary>
        public ReactiveProperty<string> Password { get; } = new("");

        /// <summary>エラーメッセージ</summary>
        public ReactiveProperty<string> ErrorMessage { get; } = new("");

        /// <summary>ログイン処理中フラグ</summary>
        public ReactiveProperty<bool> IsLoggingIn { get; } = new(false);

        /// <summary>サインアップ処理中フラグ</summary>
        public ReactiveProperty<bool> IsSigningUp { get; } = new(false);

        // ============================================
        // イベント
        // ============================================

        /// <summary>ログイン成功イベント</summary>
        public event Action? OnLoginSuccess;

        /// <summary>サインアップ成功イベント</summary>
        public event Action? OnSignUpSuccess;

        /// <summary>匿名ログイン成功イベント</summary>
        public event Action? OnGuestLoginSuccess;

        /// <summary>パスワードリセットメール送信イベント</summary>
        public event Action? OnPasswordResetEmailSent;

        // ============================================
        // コンストラクタ
        // ============================================

        public LoginViewModel(IFirebaseAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // 認証状態変更を監視
            _authService.OnAuthStateChanged += OnAuthStateChanged;
        }

        // ============================================
        // メソッド
        // ============================================

        /// <summary>
        /// ログインを実行
        /// </summary>
        public async void Login()
        {
            if (IsLoggingIn.Value || IsSigningUp.Value)
            {
                Debug.LogWarning("[LoginViewModel] Already processing login/signup");
                return;
            }

            if (!ValidateInput())
            {
                return;
            }

            IsLoggingIn.Value = true;
            ErrorMessage.Value = "";

            try
            {
                Debug.Log($"[LoginViewModel] Attempting login with email: {Email.Value}");

                bool success = await _authService.SignInWithEmailAndPasswordAsync(Email.Value, Password.Value);

                if (success)
                {
                    Debug.Log("[LoginViewModel] Login successful");
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    ErrorMessage.Value = "Login failed. Please check your credentials.";
                    Debug.LogWarning("[LoginViewModel] Login failed");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Value = $"Login error: {ex.Message}";
                Debug.LogError($"[LoginViewModel] Login exception: {ex}");
            }
            finally
            {
                IsLoggingIn.Value = false;
            }
        }

        /// <summary>
        /// サインアップを実行
        /// </summary>
        public async void SignUp()
        {
            if (IsLoggingIn.Value || IsSigningUp.Value)
            {
                Debug.LogWarning("[LoginViewModel] Already processing login/signup");
                return;
            }

            if (!ValidateInput())
            {
                return;
            }

            IsSigningUp.Value = true;
            ErrorMessage.Value = "";

            try
            {
                Debug.Log($"[LoginViewModel] Attempting sign up with email: {Email.Value}");

                bool success = await _authService.SignUpWithEmailAndPasswordAsync(Email.Value, Password.Value);

                if (success)
                {
                    Debug.Log("[LoginViewModel] Sign up successful");
                    OnSignUpSuccess?.Invoke();
                }
                else
                {
                    ErrorMessage.Value = "Sign up failed. Email may already be in use.";
                    Debug.LogWarning("[LoginViewModel] Sign up failed");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Value = $"Sign up error: {ex.Message}";
                Debug.LogError($"[LoginViewModel] Sign up exception: {ex}");
            }
            finally
            {
                IsSigningUp.Value = false;
            }
        }

        /// <summary>
        /// ゲストログイン（匿名ログイン）を実行
        /// </summary>
        public async void LoginAsGuest()
        {
            if (IsLoggingIn.Value || IsSigningUp.Value)
            {
                Debug.LogWarning("[LoginViewModel] Already processing login/signup");
                return;
            }

            IsLoggingIn.Value = true;
            ErrorMessage.Value = "";

            try
            {
                Debug.Log("[LoginViewModel] Attempting guest login");

                bool success = await _authService.SignInAnonymouslyAsync();

                if (success)
                {
                    Debug.Log("[LoginViewModel] Guest login successful");
                    OnGuestLoginSuccess?.Invoke();
                }
                else
                {
                    ErrorMessage.Value = "Guest login failed. Please try again.";
                    Debug.LogWarning("[LoginViewModel] Guest login failed");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Value = $"Guest login error: {ex.Message}";
                Debug.LogError($"[LoginViewModel] Guest login exception: {ex}");
            }
            finally
            {
                IsLoggingIn.Value = false;
            }
        }

        /// <summary>
        /// パスワードリセットメールを送信
        /// </summary>
        public async void SendPasswordResetEmail()
        {
            if (string.IsNullOrWhiteSpace(Email.Value))
            {
                ErrorMessage.Value = "Please enter your email address";
                return;
            }

            if (!IsValidEmail(Email.Value))
            {
                ErrorMessage.Value = "Please enter a valid email address";
                return;
            }

            ErrorMessage.Value = "";

            try
            {
                Debug.Log($"[LoginViewModel] Sending password reset email to: {Email.Value}");

                bool success = await _authService.SendPasswordResetEmailAsync(Email.Value);

                if (success)
                {
                    Debug.Log("[LoginViewModel] Password reset email sent");
                    OnPasswordResetEmailSent?.Invoke();
                }
                else
                {
                    ErrorMessage.Value = "Failed to send password reset email";
                    Debug.LogWarning("[LoginViewModel] Password reset email send failed");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Value = $"Error sending password reset email: {ex.Message}";
                Debug.LogError($"[LoginViewModel] Password reset email exception: {ex}");
            }
        }

        // ============================================
        // バリデーション
        // ============================================

        /// <summary>
        /// 入力値をバリデーション
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Email.Value))
            {
                ErrorMessage.Value = "Email is required";
                return false;
            }

            if (!IsValidEmail(Email.Value))
            {
                ErrorMessage.Value = "Please enter a valid email address";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password.Value))
            {
                ErrorMessage.Value = "Password is required";
                return false;
            }

            if (Password.Value.Length < 6)
            {
                ErrorMessage.Value = "Password must be at least 6 characters";
                return false;
            }

            return true;
        }

        /// <summary>
        /// メールアドレスの形式をチェック
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // ============================================
        // イベントハンドラ
        // ============================================

        /// <summary>
        /// 認証状態変更ハンドラ
        /// </summary>
        private void OnAuthStateChanged(bool isAuthenticated)
        {
            Debug.Log($"[LoginViewModel] Auth state changed: {isAuthenticated}");

            if (isAuthenticated)
            {
                // ログイン後にメインメニューに遷移
                // （View側でOnLoginSuccessイベントをハンドリング）
            }
        }

        // ============================================
        // クリーンアップ
        // ============================================

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= OnAuthStateChanged;

            _disposables.Dispose();
            Email.Dispose();
            Password.Dispose();
            ErrorMessage.Dispose();
            IsLoggingIn.Dispose();
            IsSigningUp.Dispose();

            Debug.Log("[LoginViewModel] Disposed");
        }
    }
}
