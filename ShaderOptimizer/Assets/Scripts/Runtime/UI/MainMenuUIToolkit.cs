#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.Core;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// メインメニュー UI Toolkit コントローラー
    /// </summary>
    /// <remarks>
    /// MainMenu.uxml を使用したモダンな UI 実装
    /// レガシー MainMenuUI.cs (uGUI) の代替として設計
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuUIToolkit : MonoBehaviour
    {
        // ============================================
        // UI 要素参照
        // ============================================

        private VisualElement? _root;

        // カスタマイズボタン
        private Button? _characterCustomizationButton;
        private Button? _roomDecorationButton;

        // ミニゲームボタン
        private Button? _ticTacToeButton;
        private Button? _hexReversiButton;
        private Button? _hexCheckersButton;
        private Button? _hexChessButton;

        // システムボタン
        private Button? _settingsButton;
        private Button? _quitButton;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void OnEnable()
        {
            // UIDocument からルート要素取得
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[MainMenuUIToolkit] UIDocument コンポーネントが見つかりません");
                return;
            }

            _root = uiDocument.rootVisualElement;
            if (_root == null)
            {
                Debug.LogError("[MainMenuUIToolkit] Root VisualElement が null です");
                return;
            }

            // UI 要素を取得
            QueryUIElements();

            // イベントハンドラ登録
            RegisterEventHandlers();

            Debug.Log("[MainMenuUIToolkit] UI 初期化完了");
        }

        private void OnDisable()
        {
            // イベントハンドラ解除
            UnregisterEventHandlers();
        }

        // ============================================
        // UI 要素取得
        // ============================================

        private void QueryUIElements()
        {
            if (_root == null) return;

            // カスタマイズボタン
            _characterCustomizationButton = _root.Q<Button>("CharacterCustomizationButton");
            _roomDecorationButton = _root.Q<Button>("RoomDecorationButton");

            // ミニゲームボタン
            _ticTacToeButton = _root.Q<Button>("TicTacToeButton");
            _hexReversiButton = _root.Q<Button>("HexReversiButton");
            _hexCheckersButton = _root.Q<Button>("HexCheckersButton");
            _hexChessButton = _root.Q<Button>("HexChessButton");

            // システムボタン
            _settingsButton = _root.Q<Button>("SettingsButton");
            _quitButton = _root.Q<Button>("QuitButton");

            // デバッグ: ボタンが見つからない場合の警告
            if (_characterCustomizationButton == null)
                Debug.LogWarning("[MainMenuUIToolkit] CharacterCustomizationButton が見つかりません");
            if (_quitButton == null)
                Debug.LogWarning("[MainMenuUIToolkit] QuitButton が見つかりません");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            // カスタマイズボタン
            if (_characterCustomizationButton != null)
                _characterCustomizationButton.clicked += OnCharacterCustomizationClicked;
            if (_roomDecorationButton != null)
                _roomDecorationButton.clicked += OnRoomDecorationClicked;

            // ミニゲームボタン
            if (_ticTacToeButton != null)
                _ticTacToeButton.clicked += OnTicTacToeClicked;
            if (_hexReversiButton != null)
                _hexReversiButton.clicked += OnHexReversiClicked;
            if (_hexCheckersButton != null)
                _hexCheckersButton.clicked += OnHexCheckersClicked;
            if (_hexChessButton != null)
                _hexChessButton.clicked += OnHexChessClicked;

            // システムボタン
            if (_settingsButton != null)
                _settingsButton.clicked += OnSettingsClicked;
            if (_quitButton != null)
                _quitButton.clicked += OnQuitClicked;
        }

        private void UnregisterEventHandlers()
        {
            // カスタマイズボタン
            if (_characterCustomizationButton != null)
                _characterCustomizationButton.clicked -= OnCharacterCustomizationClicked;
            if (_roomDecorationButton != null)
                _roomDecorationButton.clicked -= OnRoomDecorationClicked;

            // ミニゲームボタン
            if (_ticTacToeButton != null)
                _ticTacToeButton.clicked -= OnTicTacToeClicked;
            if (_hexReversiButton != null)
                _hexReversiButton.clicked -= OnHexReversiClicked;
            if (_hexCheckersButton != null)
                _hexCheckersButton.clicked -= OnHexCheckersClicked;
            if (_hexChessButton != null)
                _hexChessButton.clicked -= OnHexChessClicked;

            // システムボタン
            if (_settingsButton != null)
                _settingsButton.clicked -= OnSettingsClicked;
            if (_quitButton != null)
                _quitButton.clicked -= OnQuitClicked;
        }

        // ============================================
        // ボタンクリックイベントハンドラ
        // ============================================

        private void OnCharacterCustomizationClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Character Customization ボタンがクリックされました");
            GameManager.Instance.LoadCharacterCustomization();
        }

        private void OnRoomDecorationClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Room Decoration ボタンがクリックされました");
            GameManager.Instance.LoadRoomDecoration();
        }

        private void OnTicTacToeClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Tic-Tac-Toe Hex ボタンがクリックされました");
            GameManager.Instance.LoadTicTacToeHex();
        }

        private void OnHexReversiClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Hex Reversi ボタンがクリックされました");
            GameManager.Instance.LoadHexReversi();
        }

        private void OnHexCheckersClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Hex Checkers ボタンがクリックされました");
            GameManager.Instance.LoadHexCheckers();
        }

        private void OnHexChessClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Hex Chess ボタンがクリックされました");
            GameManager.Instance.LoadHexChess();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Settings ボタンがクリックされました");
            // 設定画面への遷移 (未実装)
            Debug.LogWarning("[MainMenuUIToolkit] Settings 画面は未実装です");
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuUIToolkit] Quit ボタンがクリックされました");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
