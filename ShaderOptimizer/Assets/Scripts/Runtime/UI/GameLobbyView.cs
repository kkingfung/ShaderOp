#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.UI.ViewModels;
using ShaderOp.Core.Services;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// ゲームロビー画面のView
    /// </summary>
    /// <remarks>
    /// GameLobby_Portrait.uxml と GameLobbyViewModel を使用
    /// プレイヤーリスト表示、準備完了、ゲーム開始などを管理
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class GameLobbyView : MonoBehaviour
    {
        // ============================================
        // フィールド
        // ============================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;
        private GameLobbyViewModel? _viewModel;
        private readonly CompositeDisposable _disposables = new();

        // UI要素
        private Label? _gameTitleLabel;
        private Button? _backButton;
        private Button? _settingsButton;
        private Button? _readyButton;
        private Button? _chatToggleButton;
        private VisualElement? _playerListContainer;
        private Label? _player1NameLabel;
        private Label? _player1ScoreLabel;
        private Label? _player2NameLabel;
        private Label? _player2ScoreLabel;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _viewModel = new GameLobbyViewModel();
        }

        private void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[GameLobbyView] UIDocument または rootVisualElement が null です");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            // UI要素を取得
            QueryUIElements();

            // イベントハンドラ登録
            RegisterEventHandlers();

            // ViewModelをバインド
            BindViewModel();

            Debug.Log("[GameLobbyView] UI初期化完了");
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

            // ヘッダー
            _gameTitleLabel = _root.Q<Label>("GameTitle");
            _backButton = _root.Q<Button>("BackButton");
            _settingsButton = _root.Q<Button>("SettingsButton");

            // プレイヤー情報
            _player1NameLabel = _root.Q<Label>("Player1Name");
            _player1ScoreLabel = _root.Q<Label>("Player1Score");
            _player2NameLabel = _root.Q<Label>("Player2Name");
            _player2ScoreLabel = _root.Q<Label>("Player2Score");

            // コントロール
            _readyButton = _root.Q<Button>("PassButton"); // 仮想的にPassButtonをReadyButtonとして使用
            _chatToggleButton = _root.Q<Button>("ChatToggleButton");

            // デバッグ: 見つからないUI要素を警告
            if (_gameTitleLabel == null)
                Debug.LogWarning("[GameLobbyView] GameTitle が見つかりません");
            if (_backButton == null)
                Debug.LogWarning("[GameLobbyView] BackButton が見つかりません");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked += OnBackClicked;

            if (_settingsButton != null)
                _settingsButton.clicked += OnSettingsClicked;

            if (_readyButton != null)
                _readyButton.clicked += OnReadyClicked;

            if (_chatToggleButton != null)
                _chatToggleButton.clicked += OnChatToggleClicked;
        }

        private void UnregisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;

            if (_settingsButton != null)
                _settingsButton.clicked -= OnSettingsClicked;

            if (_readyButton != null)
                _readyButton.clicked -= OnReadyClicked;

            if (_chatToggleButton != null)
                _chatToggleButton.clicked -= OnChatToggleClicked;
        }

        // ============================================
        // ViewModelバインディング
        // ============================================

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // ゲーム名の変更を監視
            _viewModel.GameName.Subscribe(gameName =>
            {
                if (_gameTitleLabel != null)
                    _gameTitleLabel.text = gameName;
            }).AddTo(_disposables);

            // プレイヤーリストの変更を監視
            _viewModel.Players.ObserveCountChanged().Subscribe(_ =>
            {
                UpdatePlayerList();
            }).AddTo(_disposables);

            // 準備状態の変更を監視
            _viewModel.IsLocalPlayerReady.Subscribe(isReady =>
            {
                if (_readyButton != null)
                    _readyButton.text = isReady ? "Cancel Ready" : "Ready";
            }).AddTo(_disposables);

            // ViewModelのイベントを購読
            _viewModel.OnLeaveLobby += HandleLeaveLobby;
            _viewModel.OnGameStart += HandleGameStart;
            _viewModel.OnChatToggled += HandleChatToggled;

            // 初期表示更新
            UpdatePlayerList();
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.OnLeaveLobby -= HandleLeaveLobby;
                _viewModel.OnGameStart -= HandleGameStart;
                _viewModel.OnChatToggled -= HandleChatToggled;
            }

            _disposables.Clear();
        }

        // ============================================
        // UI更新
        // ============================================

        private void UpdatePlayerList()
        {
            if (_viewModel == null) return;

            // Player1の情報を更新
            if (_viewModel.Players.Count > 0)
            {
                var player1 = _viewModel.Players[0];
                if (_player1NameLabel != null)
                    _player1NameLabel.text = player1.Name;
                if (_player1ScoreLabel != null)
                    _player1ScoreLabel.text = $"Lv.{player1.Level} {(player1.IsReady ? "✓" : "")}";
            }

            // Player2の情報を更新
            if (_viewModel.Players.Count > 1)
            {
                var player2 = _viewModel.Players[1];
                if (_player2NameLabel != null)
                    _player2NameLabel.text = player2.Name;
                if (_player2ScoreLabel != null)
                    _player2ScoreLabel.text = $"Lv.{player2.Level} {(player2.IsReady ? "✓" : "")}";
            }
        }

        // ============================================
        // イベントハンドラ
        // ============================================

        private void OnBackClicked()
        {
            Debug.Log("[GameLobbyView] Back button clicked");
            _viewModel?.HandleLeaveLobby();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[GameLobbyView] Settings button clicked");
            // 設定画面表示（未実装）
        }

        private void OnReadyClicked()
        {
            Debug.Log("[GameLobbyView] Ready button clicked");
            _viewModel?.HandleReadyToggled();
        }

        private void OnChatToggleClicked()
        {
            Debug.Log("[GameLobbyView] Chat toggle clicked");
            _viewModel?.HandleChatToggle();
        }

        // ============================================
        // ViewModelイベントハンドラ
        // ============================================

        private void HandleLeaveLobby()
        {
            Debug.Log("[GameLobbyView] Leaving lobby...");

            // シーン遷移サービスを取得してメインメニューに戻る
            var sceneService = ServiceLocator.Instance.Get<ISceneLoaderService>();
            if (sceneService != null)
            {
                sceneService.LoadMainMenuAsync().Forget();
            }
        }

        private void HandleGameStart()
        {
            Debug.Log("[GameLobbyView] Game starting...");
            // ゲーム開始処理（未実装）
        }

        private void HandleChatToggled(bool isVisible)
        {
            Debug.Log($"[GameLobbyView] Chat visibility: {isVisible}");
            // チャットオーバーレイ表示切り替え（未実装）
        }
    }
}
