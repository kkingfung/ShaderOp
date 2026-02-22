#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace ShaderOp.Core
{
    /// <summary>
    /// メインメニューUI制御
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button? _customizationButton;
        [SerializeField] private Button? _roomDecorationButton;
        [SerializeField] private Button? _minigamesButton;
        [SerializeField] private Button? _quitButton;

        private void Start()
        {
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

            if (_minigamesButton != null)
            {
                _minigamesButton.onClick.AddListener(OnMinigamesClicked);
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
            GameManager.Instance.LoadCharacterCustomization();
        }

        /// <summary>
        /// 部屋デコレーションボタンクリック時
        /// </summary>
        private void OnRoomDecorationClicked()
        {
            Debug.Log("[MainMenuUI] Loading Room Decoration...");
            GameManager.Instance.LoadRoomDecoration();
        }

        /// <summary>
        /// ミニゲームボタンクリック時
        /// </summary>
        private void OnMinigamesClicked()
        {
            Debug.Log("[MainMenuUI] Loading Minigames...");
            GameManager.Instance.LoadTicTacToeHex();
        }

        /// <summary>
        /// 終了ボタンクリック時
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuUI] Quitting application...");
            GameManager.Instance.QuitApplication();
        }

        private void OnDestroy()
        {
            if (_customizationButton != null) _customizationButton.onClick.RemoveAllListeners();
            if (_roomDecorationButton != null) _roomDecorationButton.onClick.RemoveAllListeners();
            if (_minigamesButton != null) _minigamesButton.onClick.RemoveAllListeners();
            if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
        }
    }
}
