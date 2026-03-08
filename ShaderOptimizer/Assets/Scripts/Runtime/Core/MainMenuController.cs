#nullable enable

using System;
using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.Core.Services;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Core
{
    /// <summary>
    /// メインメニュー UI Toolkit コントローラー
    /// </summary>
    /// <remarks>
    /// Portrait モバイル最適化されたメインメニュー画面
    /// ServiceLocatorパターンでISceneLoaderServiceを使用
    /// 改善版: エラーハンドリング強化、UI検証追加
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument? _uiDocument;

        /// <summary>シーンローダーサービス</summary>
        private ISceneLoaderService? _sceneLoader;

        /// <summary>UI要素参照</summary>
        private VisualElement? _root;
        private Button? _playTicTacToeBtn;
        private Button? _playHexReversiBtn;
        private Button? _playHexCheckersBtn;
        private Button? _playHexChessBtn;
        private Button? _roomDecorationBtn;
        private Button? _characterCustomizationBtn;
        private Button? _settingsBtn;
        private Button? _quitBtn;
        private Label? _versionLabel;

        private void Awake()
        {
            // UIDocumentを取得
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument == null)
            {
                Debug.LogError("[MainMenuController] UIDocument component not found!", this);
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            // ServiceLocatorからシーンローダーサービスを取得
            _sceneLoader = ServiceLocator.Instance.Get<ISceneLoaderService>();

            if (_sceneLoader == null)
            {
                Debug.LogError("[MainMenuController] ISceneLoaderService not found! Make sure GameBootstrap is in the scene.");
            }

            // UI要素を取得してセットアップ
            SetupUI();
        }

        /// <summary>
        /// UI要素を取得してイベントハンドラを登録
        /// </summary>
        private void SetupUI()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("[MainMenuController] UIDocument is null in SetupUI!");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            if (_root == null)
            {
                Debug.LogError("[MainMenuController] Root VisualElement not found!");
                enabled = false;
                return;
            }

            // UI要素を取得
            GetUIElements();

            // 必須UI要素を検証
            if (!ValidateRequiredUIElements())
            {
                Debug.LogError("[MainMenuController] Required UI elements missing! Disabling controller.");
                enabled = false;
                return;
            }

            // イベントハンドラを登録
            RegisterEventHandlers();

            // バージョン情報を更新
            UpdateVersionInfo();

            // 無効化されたボタンを設定
            SetupDisabledButtons();

            Debug.Log("[MainMenuController] UI setup complete.");
        }

        /// <summary>
        /// UI要素を取得
        /// </summary>
        private void GetUIElements()
        {
            if (_root == null) return;

            // ミニゲームボタン
            _playTicTacToeBtn = _root.Q<Button>("PlayTicTacToeBtn");
            _playHexReversiBtn = _root.Q<Button>("PlayHexReversiBtn");
            _playHexCheckersBtn = _root.Q<Button>("PlayHexCheckersBtn");
            _playHexChessBtn = _root.Q<Button>("PlayHexChessBtn");

            // カスタマイズボタン
            _roomDecorationBtn = _root.Q<Button>("RoomDecorationBtn");
            _characterCustomizationBtn = _root.Q<Button>("CharacterCustomizationBtn");

            // フッターボタン
            _settingsBtn = _root.Q<Button>("SettingsBtn");
            _quitBtn = _root.Q<Button>("QuitBtn");

            // ラベル
            _versionLabel = _root.Q<Label>("VersionLabel");
        }

        /// <summary>
        /// 必須UI要素の検証
        /// </summary>
        private bool ValidateRequiredUIElements()
        {
            bool isValid = true;

            if (_playTicTacToeBtn == null)
            {
                Debug.LogError("[MainMenuController] PlayTicTacToeBtn not found!");
                isValid = false;
            }

            if (_playHexReversiBtn == null)
            {
                Debug.LogError("[MainMenuController] PlayHexReversiBtn not found!");
                isValid = false;
            }

            if (_roomDecorationBtn == null)
            {
                Debug.LogError("[MainMenuController] RoomDecorationBtn not found!");
                isValid = false;
            }

            if (_quitBtn == null)
            {
                Debug.LogError("[MainMenuController] QuitBtn not found!");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// イベントハンドラを登録
        /// </summary>
        private void RegisterEventHandlers()
        {
            // ミニゲームボタン
            if (_playTicTacToeBtn != null)
            {
                _playTicTacToeBtn.clicked += OnPlayTicTacToeClicked;
            }

            if (_playHexReversiBtn != null)
            {
                _playHexReversiBtn.clicked += OnPlayHexReversiClicked;
            }

            if (_playHexCheckersBtn != null)
            {
                _playHexCheckersBtn.clicked += OnPlayHexCheckersClicked;
            }

            if (_playHexChessBtn != null)
            {
                _playHexChessBtn.clicked += OnPlayHexChessClicked;
            }

            // カスタマイズボタン
            if (_roomDecorationBtn != null)
            {
                _roomDecorationBtn.clicked += OnRoomDecorationClicked;
            }

            if (_characterCustomizationBtn != null)
            {
                _characterCustomizationBtn.clicked += OnCharacterCustomizationClicked;
            }

            // フッターボタン
            if (_settingsBtn != null)
            {
                _settingsBtn.clicked += OnSettingsClicked;
            }

            if (_quitBtn != null)
            {
                _quitBtn.clicked += OnQuitClicked;
            }
        }

        /// <summary>
        /// イベントハンドラを解除
        /// </summary>
        private void UnregisterEventHandlers()
        {
            if (_playTicTacToeBtn != null) _playTicTacToeBtn.clicked -= OnPlayTicTacToeClicked;
            if (_playHexReversiBtn != null) _playHexReversiBtn.clicked -= OnPlayHexReversiClicked;
            if (_playHexCheckersBtn != null) _playHexCheckersBtn.clicked -= OnPlayHexCheckersClicked;
            if (_playHexChessBtn != null) _playHexChessBtn.clicked -= OnPlayHexChessClicked;
            if (_roomDecorationBtn != null) _roomDecorationBtn.clicked -= OnRoomDecorationClicked;
            if (_characterCustomizationBtn != null) _characterCustomizationBtn.clicked -= OnCharacterCustomizationClicked;
            if (_settingsBtn != null) _settingsBtn.clicked -= OnSettingsClicked;
            if (_quitBtn != null) _quitBtn.clicked -= OnQuitClicked;
        }

        /// <summary>
        /// 無効化されたボタンの設定
        /// </summary>
        private void SetupDisabledButtons()
        {
            // Phase 3 で実装予定のボタンは無効化
            SetButtonEnabled(_playHexCheckersBtn, false);
            SetButtonEnabled(_playHexChessBtn, false);
            SetButtonEnabled(_characterCustomizationBtn, false);
        }

        /// <summary>
        /// ボタンの有効/無効を設定
        /// </summary>
        private void SetButtonEnabled(Button? button, bool enabled)
        {
            if (button == null) return;

            button.SetEnabled(enabled);

            if (!enabled)
            {
                // 無効化時は "disabled" クラスを追加（USS で opacity を制御）
                if (!button.ClassListContains("disabled"))
                {
                    button.AddToClassList("disabled");
                }
            }
            else
            {
                button.RemoveFromClassList("disabled");
            }
        }

        /// <summary>
        /// バージョン情報を更新
        /// </summary>
        private void UpdateVersionInfo()
        {
            if (_versionLabel != null)
            {
                // PROJECT_STATUS.mdとROADMAP.mdの情報と一致
                _versionLabel.text = "v0.2.0 - Phase 2 (65%)";
            }
        }

        #region イベントハンドラ

        /// <summary>
        /// Tic-Tac-Toe Hex ボタンクリック時
        /// </summary>
        private void OnPlayTicTacToeClicked()
        {
            Debug.Log("[MainMenuController] Loading Tic-Tac-Toe Hex...");
            LoadSceneWithErrorHandlingAsync("TicTacToeHex").Forget();
        }

        /// <summary>
        /// Hex Reversi ボタンクリック時
        /// </summary>
        private void OnPlayHexReversiClicked()
        {
            Debug.Log("[MainMenuController] Loading Hex Reversi...");
            LoadSceneWithErrorHandlingAsync("HexReversi").Forget();
        }

        /// <summary>
        /// Hex Checkers ボタンクリック時 (Phase 3)
        /// </summary>
        private void OnPlayHexCheckersClicked()
        {
            ShowComingSoonMessage("Hex Checkers");
        }

        /// <summary>
        /// Hex Chess ボタンクリック時 (Phase 3)
        /// </summary>
        private void OnPlayHexChessClicked()
        {
            ShowComingSoonMessage("Hex Chess");
        }

        /// <summary>
        /// Room Decoration ボタンクリック時
        /// </summary>
        private void OnRoomDecorationClicked()
        {
            Debug.Log("[MainMenuController] Loading Room Decoration...");
            LoadRoomDecorationWithErrorHandlingAsync().Forget();
        }

        /// <summary>
        /// Character Customization ボタンクリック時 (Phase 2後半)
        /// </summary>
        private void OnCharacterCustomizationClicked()
        {
            ShowComingSoonMessage("Character Customization");
        }

        /// <summary>
        /// Settings ボタンクリック時
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuController] Settings - Not implemented yet");
            ShowComingSoonMessage("Settings");
        }

        /// <summary>
        /// Quit ボタンクリック時
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuController] Quit button clicked");
            ShowQuitConfirmation();
        }

        #endregion

        #region シーン遷移（エラーハンドリング付き）

        /// <summary>
        /// エラーハンドリング付きシーンロード（ミニゲーム）
        /// </summary>
        private async UniTask LoadSceneWithErrorHandlingAsync(string sceneName)
        {
            if (_sceneLoader == null)
            {
                Debug.LogError("[MainMenuController] SceneLoader not available!");
                return;
            }

            try
            {
                Debug.Log($"[MainMenuController] Loading scene: {sceneName}...");
                await _sceneLoader.LoadMinigameAsync(sceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MainMenuController] Failed to load scene {sceneName}: {e.Message}");
            }
        }

        /// <summary>
        /// エラーハンドリング付きシーンロード（Room Decoration）
        /// </summary>
        private async UniTask LoadRoomDecorationWithErrorHandlingAsync()
        {
            if (_sceneLoader == null)
            {
                Debug.LogError("[MainMenuController] SceneLoader not available!");
                return;
            }

            try
            {
                Debug.Log("[MainMenuController] Loading Room Decoration...");
                await _sceneLoader.LoadRoomDecorationAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MainMenuController] Failed to load Room Decoration: {e.Message}");
            }
        }

        #endregion

        #region ヘルパーメソッド

        /// <summary>
        /// "Coming Soon" メッセージを表示
        /// </summary>
        private void ShowComingSoonMessage(string featureName)
        {
            Debug.Log($"[MainMenuController] {featureName} - Coming Soon in Phase 3!");

            // TODO: UI Toolkit でモーダルダイアログを表示
            // 現在は簡易的にログ出力のみ
        }

        /// <summary>
        /// 終了確認ダイアログを表示
        /// </summary>
        private void ShowQuitConfirmation()
        {
            // TODO: UI Toolkit でモーダル確認ダイアログを表示
            // 現在は直接終了
            QuitApplication();
        }

        /// <summary>
        /// アプリケーションを終了
        /// </summary>
        private void QuitApplication()
        {
            Debug.Log("[MainMenuController] Quitting application...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Unity ライフサイクル

        private void OnDestroy()
        {
            UnregisterEventHandlers();
        }

        #endregion

        #region デバッグ機能

#if UNITY_EDITOR
        [ContextMenu("Force Refresh UI")]
        private void ForceRefreshUI()
        {
            UnregisterEventHandlers();
            SetupUI();
            Debug.Log("[MainMenuController] UI強制リフレッシュ完了");
        }

        [ContextMenu("Log Button States")]
        private void LogButtonStates()
        {
            Debug.Log($"[MainMenuController] PlayTicTacToeBtn: {(_playTicTacToeBtn != null ? "Found" : "Missing")}");
            Debug.Log($"[MainMenuController] PlayHexReversiBtn: {(_playHexReversiBtn != null ? "Found" : "Missing")}");
            Debug.Log($"[MainMenuController] RoomDecorationBtn: {(_roomDecorationBtn != null ? "Found" : "Missing")}");
            Debug.Log($"[MainMenuController] SettingsBtn: {(_settingsBtn != null ? "Found" : "Missing")}");
            Debug.Log($"[MainMenuController] QuitBtn: {(_quitBtn != null ? "Found" : "Missing")}");
        }
#endif

        #endregion
    }
}
