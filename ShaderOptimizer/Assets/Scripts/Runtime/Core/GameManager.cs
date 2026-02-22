#nullable enable

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShaderOp.Core
{
    /// <summary>
    /// ゲーム全体を管理するシングルトンマネージャー
    /// </summary>
    /// <remarks>
    /// シーン遷移、ゲーム状態管理、データ永続化を担当
    /// </remarks>
    public class GameManager : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static GameManager? _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>現在のゲームモード</summary>
        public enum GameMode
        {
            MainMenu,
            CharacterCustomization,
            RoomDecoration,
            Minigame
        }

        /// <summary>現在のモード</summary>
        public GameMode CurrentMode { get; private set; } = GameMode.MainMenu;

        /// <summary>モード変更イベント</summary>
        public event Action<GameMode>? OnGameModeChanged;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void LoadMainMenu()
        {
            ChangeMode(GameMode.MainMenu);
            SceneLoader.Instance.LoadSceneAsync("MainMenu");
        }

        /// <summary>
        /// キャラクターカスタマイズシーンをロード
        /// </summary>
        public void LoadCharacterCustomization()
        {
            ChangeMode(GameMode.CharacterCustomization);
            SceneLoader.Instance.LoadSceneAsync("MainCustomization");
        }

        /// <summary>
        /// 部屋デコレーションシーンをロード
        /// </summary>
        public void LoadRoomDecoration()
        {
            ChangeMode(GameMode.RoomDecoration);
            SceneLoader.Instance.LoadSceneAsync("RoomDecoration");
        }

        /// <summary>
        /// ミニゲームシーンをロード
        /// </summary>
        public void LoadMinigame(string minigameName)
        {
            ChangeMode(GameMode.Minigame);
            SceneLoader.Instance.LoadSceneAsync(minigameName);
        }

        /// <summary>
        /// Tic-Tac-Toe Hexをロード
        /// </summary>
        public void LoadTicTacToeHex()
        {
            LoadMinigame("TicTacToeHex");
        }

        /// <summary>
        /// ゲームモードを変更
        /// </summary>
        private void ChangeMode(GameMode newMode)
        {
            if (CurrentMode != newMode)
            {
                CurrentMode = newMode;
                OnGameModeChanged?.Invoke(newMode);
                Debug.Log($"[GameManager] Game mode changed to: {newMode}");
            }
        }

        /// <summary>
        /// アプリケーション終了
        /// </summary>
        public void QuitApplication()
        {
            Debug.Log("[GameManager] Quitting application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
