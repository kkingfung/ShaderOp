#nullable enable

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShaderOp.Core
{
    /// <summary>
    /// ローディング画面UI制御
    /// </summary>
    /// <remarks>
    /// SceneLoaderと連携してローディング進捗を表示
    /// </remarks>
    public class LoadingScreenUI : MonoBehaviour
    {
        /// <summary>ローディングパネル</summary>
        [Header("UI Elements")]
        [SerializeField] private GameObject? _loadingPanel;

        /// <summary>進捗バー</summary>
        [SerializeField] private Slider? _progressBar;

        /// <summary>進捗テキスト</summary>
        [SerializeField] private TextMeshProUGUI? _progressText;

        /// <summary>ステータステキスト</summary>
        [SerializeField] private TextMeshProUGUI? _statusText;

        /// <summary>ローディングアニメーション用Image</summary>
        [SerializeField] private Image? _loadingIcon;

        /// <summary>アニメーション速度</summary>
        [SerializeField] private float _rotationSpeed = 100f;

        private void Start()
        {
            // SceneLoaderのイベントを購読
            SceneLoader.Instance.OnLoadStarted += OnLoadStarted;
            SceneLoader.Instance.OnProgressChanged += OnProgressChanged;
            SceneLoader.Instance.OnLoadCompleted += OnLoadCompleted;
            SceneLoader.Instance.OnLoadFailed += OnLoadFailed;

            // 初期状態では非表示
            HideLoadingScreen();
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnLoadStarted -= OnLoadStarted;
                SceneLoader.Instance.OnProgressChanged -= OnProgressChanged;
                SceneLoader.Instance.OnLoadCompleted -= OnLoadCompleted;
                SceneLoader.Instance.OnLoadFailed -= OnLoadFailed;
            }
        }

        private void Update()
        {
            // ローディングアイコンを回転
            if (_loadingIcon != null && _loadingPanel != null && _loadingPanel.activeSelf)
            {
                _loadingIcon.transform.Rotate(0, 0, -_rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 読み込み開始時の処理
        /// </summary>
        private void OnLoadStarted(string sceneName)
        {
            ShowLoadingScreen();
            UpdateStatus($"Loading {sceneName}...");
            UpdateProgress(0f);
        }

        /// <summary>
        /// 進捗更新時の処理
        /// </summary>
        private void OnProgressChanged(float progress)
        {
            UpdateProgress(progress);
        }

        /// <summary>
        /// 読み込み完了時の処理
        /// </summary>
        private void OnLoadCompleted(string sceneName)
        {
            UpdateProgress(1.0f);
            UpdateStatus($"Loaded {sceneName}");

            // 少し待ってから非表示
            Invoke(nameof(HideLoadingScreen), 0.5f);
        }

        /// <summary>
        /// 読み込み失敗時の処理
        /// </summary>
        private void OnLoadFailed(string sceneName, string error)
        {
            UpdateStatus($"Failed to load {sceneName}: {error}");
            Debug.LogError($"[LoadingScreenUI] Load failed: {sceneName} - {error}");

            // エラー表示後に非表示
            Invoke(nameof(HideLoadingScreen), 2.0f);
        }

        /// <summary>
        /// ローディング画面を表示
        /// </summary>
        public void ShowLoadingScreen()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }
        }

        /// <summary>
        /// ローディング画面を非表示
        /// </summary>
        public void HideLoadingScreen()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 進捗を更新
        /// </summary>
        private void UpdateProgress(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        /// <summary>
        /// ステータステキストを更新
        /// </summary>
        private void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }
    }
}
