#nullable enable

using UnityEngine;
using UnityEngine.UI;
using ShaderOp.Core.Services;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Core
{
    /// <summary>
    /// メインメニューUI制御
    /// </summary>
    /// <remarks>
    /// ServiceLocatorパターンを使用してISceneLoaderServiceを取得し、
    /// シーン遷移を実行します。
    /// </remarks>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button? _customizationButton;
        [SerializeField] private Button? _roomDecorationButton;
        [SerializeField] private Button? _quitButton;

        [Header("Minigame Buttons")]
        [SerializeField] private Button? _ticTacToeButton;
        [SerializeField] private Button? _reversiButton;
        [SerializeField] private Button? _checkersButton;
        [SerializeField] private Button? _chessButton;

        /// <summary>シーンローダーサービス</summary>
        private ISceneLoaderService? _sceneLoader;

        private void Start()
        {
            // ServiceLocatorからシーンローダーサービスを取得
            _sceneLoader = ServiceLocator.Instance.Get<ISceneLoaderService>();

            if (_sceneLoader == null)
            {
                Debug.LogError("[MainMenuUI] ISceneLoaderService not found! Make sure GameBootstrap is in the scene.");
            }

            SetupButtons();
        }

        /// <summary>
        /// ボタンをセットアップ
        /// </summary>
        private void SetupButtons()
        {
            if (_customizationButton != null)
            {
                _customizationButton.onClick.AddListener(OnCustomizationClicked);
            }

            if (_roomDecorationButton != null)
            {
                _roomDecorationButton.onClick.AddListener(OnRoomDecorationClicked);
            }

            if (_ticTacToeButton != null)
            {
                _ticTacToeButton.onClick.AddListener(OnTicTacToeClicked);
            }

            if (_reversiButton != null)
            {
                _reversiButton.onClick.AddListener(OnReversiClicked);
            }

            if (_checkersButton != null)
            {
                _checkersButton.onClick.AddListener(OnCheckersClicked);
            }

            if (_chessButton != null)
            {
                _chessButton.onClick.AddListener(OnChessClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        /// <summary>
        /// キャラクターカスタマイズボタンクリック時
        /// </summary>
        private void OnCustomizationClicked()
        {
            Debug.Log("[MainMenuUI] Loading Character Customization...");
            _sceneLoader?.LoadCharacterCustomizationAsync().Forget();
        }

        /// <summary>
        /// 部屋デコレーションボタンクリック時
        /// </summary>
        private void OnRoomDecorationClicked()
        {
            Debug.Log("[MainMenuUI] Loading Room Decoration...");
            _sceneLoader?.LoadRoomDecorationAsync().Forget();
        }

        /// <summary>
        /// Tic-Tac-Toeボタンクリック時
        /// </summary>
        private void OnTicTacToeClicked()
        {
            Debug.Log("[MainMenuUI] Loading Tic-Tac-Toe Hex...");
            _sceneLoader?.LoadMinigameAsync("TicTacToeHex").Forget();
        }

        /// <summary>
        /// Reversiボタンクリック時
        /// </summary>
        private void OnReversiClicked()
        {
            Debug.Log("[MainMenuUI] Loading Hex Reversi...");
            _sceneLoader?.LoadMinigameAsync("HexReversi").Forget();
        }

        /// <summary>
        /// Checkersボタンクリック時
        /// </summary>
        private void OnCheckersClicked()
        {
            Debug.Log("[MainMenuUI] Loading Hex Checkers...");
            _sceneLoader?.LoadMinigameAsync("HexCheckers").Forget();
        }

        /// <summary>
        /// Chessボタンクリック時
        /// </summary>
        private void OnChessClicked()
        {
            Debug.Log("[MainMenuUI] Loading Hex Chess...");
            _sceneLoader?.LoadMinigameAsync("HexChess").Forget();
        }

        /// <summary>
        /// 終了ボタンクリック時
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuUI] Quitting application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (_customizationButton != null) _customizationButton.onClick.RemoveAllListeners();
            if (_roomDecorationButton != null) _roomDecorationButton.onClick.RemoveAllListeners();
            if (_ticTacToeButton != null) _ticTacToeButton.onClick.RemoveAllListeners();
            if (_reversiButton != null) _reversiButton.onClick.RemoveAllListeners();
            if (_checkersButton != null) _checkersButton.onClick.RemoveAllListeners();
            if (_chessButton != null) _chessButton.onClick.RemoveAllListeners();
            if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
        }
    }
}
