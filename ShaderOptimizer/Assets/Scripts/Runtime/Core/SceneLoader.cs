#nullable enable

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShaderOp.Core
{
    /// <summary>
    /// シーン読み込みユーティリティ
    /// </summary>
    /// <remarks>
    /// 非同期シーン読み込み、進捗管理、トランジション効果を提供
    /// </remarks>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static SceneLoader? _instance;
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneLoader");
                    _instance = go.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>読み込み進捗（0.0 - 1.0）</summary>
        public float LoadProgress { get; private set; } = 0f;

        /// <summary>読み込み中フラグ</summary>
        public bool IsLoading { get; private set; } = false;

        /// <summary>読み込み開始イベント</summary>
        public event Action<string>? OnLoadStarted;

        /// <summary>読み込み進捗更新イベント</summary>
        public event Action<float>? OnProgressChanged;

        /// <summary>読み込み完了イベント</summary>
        public event Action<string>? OnLoadCompleted;

        /// <summary>読み込み失敗イベント</summary>
        public event Action<string, string>? OnLoadFailed;

        /// <summary>現在の非同期操作</summary>
        private AsyncOperation? _currentOperation;

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
        /// シーンを非同期で読み込み
        /// </summary>
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] Already loading a scene. Cannot load {sceneName}");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName, mode));
        }

        /// <summary>
        /// シーン読み込みコルーチン
        /// </summary>
        private System.Collections.IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode mode)
        {
            IsLoading = true;
            LoadProgress = 0f;

            OnLoadStarted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");

            // 非同期読み込み開始
            _currentOperation = SceneManager.LoadSceneAsync(sceneName, mode);

            if (_currentOperation == null)
            {
                Debug.LogError($"[SceneLoader] Failed to start loading scene: {sceneName}");
                OnLoadFailed?.Invoke(sceneName, "Failed to start async operation");
                IsLoading = false;
                yield break;
            }

            // 自動的にシーンをアクティブにしない（遷移効果のため）
            _currentOperation.allowSceneActivation = false;

            // 進捗を監視
            while (!_currentOperation.isDone)
            {
                // 0.9までは読み込み進捗、0.9-1.0は手動でアクティベーション待ち
                float progress = Mathf.Clamp01(_currentOperation.progress / 0.9f);
                LoadProgress = progress;
                OnProgressChanged?.Invoke(progress);

                // 90%まで読み込んだらアクティベーション許可
                if (_currentOperation.progress >= 0.9f)
                {
                    LoadProgress = 1.0f;
                    OnProgressChanged?.Invoke(1.0f);

                    // 少し待ってからアクティベーション
                    yield return new WaitForSeconds(0.5f);
                    _currentOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
            OnLoadCompleted?.Invoke(sceneName);

            IsLoading = false;
            LoadProgress = 0f;
            _currentOperation = null;
        }

        /// <summary>
        /// 現在の読み込みをキャンセル（不可能だがフラグをリセット）
        /// </summary>
        public void CancelLoading()
        {
            if (_currentOperation != null)
            {
                _currentOperation.allowSceneActivation = true;
            }

            IsLoading = false;
            LoadProgress = 0f;
            _currentOperation = null;

            Debug.LogWarning("[SceneLoader] Loading cancelled");
        }

        /// <summary>
        /// シーンが存在するかチェック
        /// </summary>
        public static bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameFromPath == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// アクティブシーンの名前を取得
        /// </summary>
        public static string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// シーンをアンロード
        /// </summary>
        public void UnloadSceneAsync(string sceneName)
        {
            if (!IsLoading)
            {
                StartCoroutine(UnloadSceneCoroutine(sceneName));
            }
            else
            {
                Debug.LogWarning($"[SceneLoader] Cannot unload scene while loading: {sceneName}");
            }
        }

        /// <summary>
        /// シーンアンロードコルーチン
        /// </summary>
        private System.Collections.IEnumerator UnloadSceneCoroutine(string sceneName)
        {
            Debug.Log($"[SceneLoader] Unloading scene: {sceneName}");

            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation != null)
            {
                while (!operation.isDone)
                {
                    yield return null;
                }
                Debug.Log($"[SceneLoader] Scene unloaded: {sceneName}");
            }
        }
    }
}
