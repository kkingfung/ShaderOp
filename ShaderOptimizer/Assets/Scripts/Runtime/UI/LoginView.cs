#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.UI.ViewModels;
using ShaderOp.Core.Services;
using ShaderOp.Core.Services.Online;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// ログイン画面のView
    /// </summary>
    /// <remarks>
    /// Login.uxml と LoginViewModel を使用
    /// メールアドレス・パスワードでのログイン、サインアップ、匿名ログインを提供
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class LoginView : MonoBehaviour
    {
        // ============================================
        // フィールド
        // ============================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;
        private LoginViewModel? _viewModel;
        private readonly CompositeDisposable _disposables = new();

        // UI要素
        private TextField? _emailField;
        private TextField? _passwordField;
        private Label? _errorLabel;
        private Button? _loginButton;
        private Button? _signUpButton;
        private Button? _guestButton;
        private Button? _forgotPasswordButton;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            // ServiceLocatorからFirebase認証サービスを取得
            var authService = ServiceLocator.Instance.Get<IFirebaseAuthService>();
            if (authService == null)
            {
                Debug.LogError("[LoginView] IFirebaseAuthService not found! Make sure GameBootstrap is initialized.");
                return;
            }

            _viewModel = new LoginViewModel(authService);
        }

        private void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[LoginView] UIDocument or rootVisualElement is null");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            QueryUIElements();
            RegisterEventHandlers();
            BindViewModel();

            Debug.Log("[LoginView] UI initialized");
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
            UnbindViewModel();
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            _disposables.Dispose();
        }

        // ============================================
        // UI要素取得
        // ============================================

        private void QueryUIElements()
        {
            if (_root == null) return;

            _emailField = _root.Q<TextField>("EmailField");
            _passwordField = _root.Q<TextField>("PasswordField");
            _errorLabel = _root.Q<Label>("ErrorLabel");
            _loginButton = _root.Q<Button>("LoginButton");
            _signUpButton = _root.Q<Button>("SignUpButton");
            _guestButton = _root.Q<Button>("GuestButton");
            _forgotPasswordButton = _root.Q<Button>("ForgotPasswordButton");

            if (_emailField == null)
                Debug.LogWarning("[LoginView] EmailField not found");
            if (_passwordField == null)
                Debug.LogWarning("[LoginView] PasswordField not found");
            if (_loginButton == null)
                Debug.LogWarning("[LoginView] LoginButton not found");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            if (_emailField != null)
            {
                _emailField.RegisterValueChangedCallback(evt =>
                {
                    if (_viewModel != null)
                        _viewModel.Email.Value = evt.newValue;
                });
            }

            if (_passwordField != null)
            {
                _passwordField.RegisterValueChangedCallback(evt =>
                {
                    if (_viewModel != null)
                        _viewModel.Password.Value = evt.newValue;
                });
            }

            if (_loginButton != null)
                _loginButton.clicked += OnLoginClicked;

            if (_signUpButton != null)
                _signUpButton.clicked += OnSignUpClicked;

            if (_guestButton != null)
                _guestButton.clicked += OnGuestClicked;

            if (_forgotPasswordButton != null)
                _forgotPasswordButton.clicked += OnForgotPasswordClicked;
        }

        private void UnregisterEventHandlers()
        {
            if (_loginButton != null)
                _loginButton.clicked -= OnLoginClicked;

            if (_signUpButton != null)
                _signUpButton.clicked -= OnSignUpClicked;

            if (_guestButton != null)
                _guestButton.clicked -= OnGuestClicked;

            if (_forgotPasswordButton != null)
                _forgotPasswordButton.clicked -= OnForgotPasswordClicked;
        }

        // ============================================
        // ViewModelバインディング
        // ============================================

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // エラーメッセージの変更を監視
            _viewModel.ErrorMessage.Subscribe(error =>
            {
                if (_errorLabel != null)
                {
                    _errorLabel.text = error;
                    _errorLabel.style.display = string.IsNullOrEmpty(error)
                        ? DisplayStyle.None
                        : DisplayStyle.Flex;
                }
            }).AddTo(_disposables);

            // ログイン処理中の状態を監視
            _viewModel.IsLoggingIn.Subscribe(isLoggingIn =>
            {
                UpdateButtonStates(!isLoggingIn);
            }).AddTo(_disposables);

            // サインアップ処理中の状態を監視
            _viewModel.IsSigningUp.Subscribe(isSigningUp =>
            {
                UpdateButtonStates(!isSigningUp);
            }).AddTo(_disposables);

            // ViewModelイベントを購読
            _viewModel.OnLoginSuccess += HandleLoginSuccess;
            _viewModel.OnSignUpSuccess += HandleSignUpSuccess;
            _viewModel.OnGuestLoginSuccess += HandleGuestLoginSuccess;
            _viewModel.OnPasswordResetEmailSent += HandlePasswordResetEmailSent;
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.OnLoginSuccess -= HandleLoginSuccess;
                _viewModel.OnSignUpSuccess -= HandleSignUpSuccess;
                _viewModel.OnGuestLoginSuccess -= HandleGuestLoginSuccess;
                _viewModel.OnPasswordResetEmailSent -= HandlePasswordResetEmailSent;
            }

            _disposables.Clear();
        }

        // ============================================
        // UI更新
        // ============================================

        private void UpdateButtonStates(bool enabled)
        {
            if (_loginButton != null)
                _loginButton.SetEnabled(enabled);

            if (_signUpButton != null)
                _signUpButton.SetEnabled(enabled);

            if (_guestButton != null)
                _guestButton.SetEnabled(enabled);

            if (_forgotPasswordButton != null)
                _forgotPasswordButton.SetEnabled(enabled);
        }

        // ============================================
        // イベントハンドラ
        // ============================================

        private void OnLoginClicked()
        {
            Debug.Log("[LoginView] Login button clicked");
            _viewModel?.Login();
        }

        private void OnSignUpClicked()
        {
            Debug.Log("[LoginView] Sign up button clicked");
            _viewModel?.SignUp();
        }

        private void OnGuestClicked()
        {
            Debug.Log("[LoginView] Guest button clicked");
            _viewModel?.LoginAsGuest();
        }

        private void OnForgotPasswordClicked()
        {
            Debug.Log("[LoginView] Forgot password button clicked");
            _viewModel?.SendPasswordResetEmail();
        }

        // ============================================
        // ViewModelイベントハンドラ
        // ============================================

        private void HandleLoginSuccess()
        {
            Debug.Log("[LoginView] Login successful - transitioning to main menu");
            NavigateToMainMenu();
        }

        private void HandleSignUpSuccess()
        {
            Debug.Log("[LoginView] Sign up successful - transitioning to main menu");
            NavigateToMainMenu();
        }

        private void HandleGuestLoginSuccess()
        {
            Debug.Log("[LoginView] Guest login successful - transitioning to main menu");
            NavigateToMainMenu();
        }

        private void HandlePasswordResetEmailSent()
        {
            Debug.Log("[LoginView] Password reset email sent");

            // 成功メッセージを表示
            if (_errorLabel != null)
            {
                _errorLabel.text = "Password reset email sent! Please check your inbox.";
                _errorLabel.style.display = DisplayStyle.Flex;
                _errorLabel.AddToClassList("success-message");
                _errorLabel.RemoveFromClassList("error-message");
            }
        }

        // ============================================
        // シーン遷移
        // ============================================

        private void NavigateToMainMenu()
        {
            var sceneService = ServiceLocator.Instance.Get<ISceneLoaderService>();
            if (sceneService != null)
            {
                sceneService.LoadMainMenuAsync().Forget();
            }
            else
            {
                Debug.LogError("[LoginView] ISceneLoaderService not found");
            }
        }
    }
}
