#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace ShaderOp.UI
{
    /// <summary>
    /// メインメニュー画面のコントローラー
    /// </summary>
    /// <remarks>
    /// UI Toolkitを使用して縦画面向けメインメニューを実装します。
    /// すべてのゲームシーンへの入口として機能します。
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        // ========================================
        // フィールド
        // ========================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;

        // ボタン参照
        private Button? _playTicTacToeBtn;
        private Button? _playHexReversiBtn;
        private Button? _playHexCheckersBtn;
        private Button? _playHexChessBtn;
        private Button? _roomDecorationBtn;
        private Button? _characterCustomizationBtn;
        private Button? _settingsBtn;
        private Button? _quitBtn;

        // ラベル参照
        private Label? _versionLabel;

        // ========================================
        // Unity ライフサイクル
        // ========================================

        private void Awake()
        {
            // UIDocumentコンポーネント取得
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("[MainMenuController] UIDocumentコンポーネントが見つかりません", this);
                return;
            }

            Debug.Log("[MainMenuController] 初期化開始");
        }

        private void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[MainMenuController] ルートVisualElementが見つかりません", this);
                return;
            }

            _root = _uiDocument.rootVisualElement;

            // UI要素を取得
            GetUIElements();

            // イベントハンドラを登録
            RegisterEventHandlers();

            // バージョン情報を更新
            UpdateVersionInfo();

            Debug.Log("[MainMenuController] UI初期化完了");
        }

        private void OnDisable()
        {
            // イベントハンドラを解除
            UnregisterEventHandlers();

            Debug.Log("[MainMenuController] イベントハンドラ解除完了");
        }

        // ========================================
        // UI初期化
        // ========================================

        /// <summary>
        /// UI要素を取得します
        /// </summary>
        private void GetUIElements()
        {
            if (_root == null)
            {
                Debug.LogError("[MainMenuController] ルートが初期化されていません");
                return;
            }

            // ミニゲームボタン
            _playTicTacToeBtn = _root.Q<Button>("PlayTicTacToeBtn");
            _playHexReversiBtn = _root.Q<Button>("PlayHexReversiBtn");
            _playHexCheckersBtn = _root.Q<Button>("PlayHexCheckersBtn");
            _playHexChessBtn = _root.Q<Button>("PlayHexChessBtn");

            // カスタマイズボタン
            _roomDecorationBtn = _root.Q<Button>("RoomDecorationBtn");
            _characterCustomizationBtn = _root.Q<Button>("CharacterCustomizationBtn");

            // システムボタン
            _settingsBtn = _root.Q<Button>("SettingsBtn");
            _quitBtn = _root.Q<Button>("QuitBtn");

            // ラベル
            _versionLabel = _root.Q<Label>("VersionLabel");

            // nullチェック
            if (_playTicTacToeBtn == null) Debug.LogWarning("[MainMenuController] PlayTicTacToeBtnが見つかりません");
            if (_playHexReversiBtn == null) Debug.LogWarning("[MainMenuController] PlayHexReversiBtnが見つかりません");
            if (_roomDecorationBtn == null) Debug.LogWarning("[MainMenuController] RoomDecorationBtnが見つかりません");
            if (_settingsBtn == null) Debug.LogWarning("[MainMenuController] SettingsBtnが見つかりません");
            if (_quitBtn == null) Debug.LogWarning("[MainMenuController] QuitBtnが見つかりません");
            if (_versionLabel == null) Debug.LogWarning("[MainMenuController] VersionLabelが見つかりません");
        }

        /// <summary>
        /// イベントハンドラを登録します
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

            // システムボタン
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
        /// イベントハンドラを解除します
        /// </summary>
        private void UnregisterEventHandlers()
        {
            // ミニゲームボタン
            if (_playTicTacToeBtn != null)
            {
                _playTicTacToeBtn.clicked -= OnPlayTicTacToeClicked;
            }

            if (_playHexReversiBtn != null)
            {
                _playHexReversiBtn.clicked -= OnPlayHexReversiClicked;
            }

            if (_playHexCheckersBtn != null)
            {
                _playHexCheckersBtn.clicked -= OnPlayHexCheckersClicked;
            }

            if (_playHexChessBtn != null)
            {
                _playHexChessBtn.clicked -= OnPlayHexChessClicked;
            }

            // カスタマイズボタン
            if (_roomDecorationBtn != null)
            {
                _roomDecorationBtn.clicked -= OnRoomDecorationClicked;
            }

            if (_characterCustomizationBtn != null)
            {
                _characterCustomizationBtn.clicked -= OnCharacterCustomizationClicked;
            }

            // システムボタン
            if (_settingsBtn != null)
            {
                _settingsBtn.clicked -= OnSettingsClicked;
            }

            if (_quitBtn != null)
            {
                _quitBtn.clicked -= OnQuitClicked;
            }
        }

        // ========================================
        // イベントハンドラ
        // ========================================

        /// <summary>
        /// Tic-Tac-Toe Hexボタンクリック時の処理
        /// </summary>
        private void OnPlayTicTacToeClicked()
        {
            Debug.Log("[MainMenuController] TicTacToeHexシーンをロード");
            LoadScene("TicTacToeHex");
        }

        /// <summary>
        /// Hex Reversiボタンクリック時の処理
        /// </summary>
        private void OnPlayHexReversiClicked()
        {
            Debug.Log("[MainMenuController] HexReversiシーンをロード");
            LoadScene("HexReversi");
        }

        /// <summary>
        /// Hex Checkersボタンクリック時の処理
        /// </summary>
        private void OnPlayHexCheckersClicked()
        {
            Debug.Log("[MainMenuController] HexCheckersは未実装です (Phase 3で実装予定)");
            // TODO: Phase 3で実装
        }

        /// <summary>
        /// Hex Chessボタンクリック時の処理
        /// </summary>
        private void OnPlayHexChessClicked()
        {
            Debug.Log("[MainMenuController] HexChessは未実装です (Phase 3で実装予定)");
            // TODO: Phase 3で実装
        }

        /// <summary>
        /// Room Decorationボタンクリック時の処理
        /// </summary>
        private void OnRoomDecorationClicked()
        {
            Debug.Log("[MainMenuController] RoomDecorationシーンをロード");
            LoadScene("RoomDecoration");
        }

        /// <summary>
        /// Character Customizationボタンクリック時の処理
        /// </summary>
        private void OnCharacterCustomizationClicked()
        {
            Debug.Log("[MainMenuController] CharacterCustomizationは未実装です (Phase 2後半で実装予定)");
            // TODO: Phase 2後半で実装
        }

        /// <summary>
        /// Settingsボタンクリック時の処理
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuController] Settings機能は未実装です");
            // TODO: 将来的に実装
        }

        /// <summary>
        /// Quitボタンクリック時の処理
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuController] ゲームを終了");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ========================================
        // シーン遷移
        // ========================================

        /// <summary>
        /// 指定されたシーンをロードします
        /// </summary>
        /// <param name="sceneName">ロードするシーン名</param>
        private void LoadScene(string sceneName)
        {
            // Unity標準のSceneManagerを使用
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        // ========================================
        // UI更新
        // ========================================

        /// <summary>
        /// バージョン情報を更新します
        /// </summary>
        private void UpdateVersionInfo()
        {
            if (_versionLabel != null)
            {
                // PROJECT_STATUS.mdとROADMAP.mdの情報と一致
                _versionLabel.text = "v0.2.0 - Phase 2 (55%)";
            }
        }

        // ========================================
        // デバッグ機能
        // ========================================

#if UNITY_EDITOR
        [ContextMenu("Force Refresh UI")]
        private void ForceRefreshUI()
        {
            OnDisable();
            OnEnable();
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
    }
}
