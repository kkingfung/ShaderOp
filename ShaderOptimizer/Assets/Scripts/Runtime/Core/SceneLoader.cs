#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        /// <summary>キャンセルトークンソース</summary>
        private CancellationTokenSource? _cancellationTokenSource;

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
        /// シーンを非同期で読み込み（UniTask版）
        /// </summary>
        public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, CancellationToken cancellationToken = default)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] Already loading a scene. Cannot load {sceneName}");
                return;
            }

            // キャンセルトークンソースを作成（外部トークンとリンク）
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                await LoadSceneAsyncInternal(sceneName, mode, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[SceneLoader] Scene loading cancelled: {sceneName}");
                OnLoadFailed?.Invoke(sceneName, "Loading cancelled");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// シーンを非同期で読み込み（レガシーコルーチン互換版）
        /// </summary>
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            LoadSceneAsync(sceneName, mode).Forget();
        }

        /// <summary>
        /// シーン読み込み内部実装（UniTask版）
        /// </summary>
        private async UniTask LoadSceneAsyncInternal(string sceneName, LoadSceneMode mode, CancellationToken cancellationToken)
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
                return;
            }

            // 自動的にシーンをアクティブにしない（遷移効果のため）
            _currentOperation.allowSceneActivation = false;

            // 進捗を監視（UniTaskでゼロアロケーション）
            while (!_currentOperation.isDone)
            {
                // キャンセルチェック
                cancellationToken.ThrowIfCancellationRequested();

                // 0.9までは読み込み進捗、0.9-1.0は手動でアクティベーション待ち
                float progress = Mathf.Clamp01(_currentOperation.progress / 0.9f);
                LoadProgress = progress;
                OnProgressChanged?.Invoke(progress);

                // 90%まで読み込んだらアクティベーション許可
                if (_currentOperation.progress >= 0.9f)
                {
                    LoadProgress = 1.0f;
                    OnProgressChanged?.Invoke(1.0f);

                    // 少し待ってからアクティベーション（UniTask.Delay使用でゼロアロケーション）
                    await UniTask.Delay(500, cancellationToken: cancellationToken);
                    _currentOperation.allowSceneActivation = true;
                }

                // 次フレームまで待機（ゼロアロケーション）
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
            OnLoadCompleted?.Invoke(sceneName);

            IsLoading = false;
            LoadProgress = 0f;
            _currentOperation = null;
        }

        /// <summary>
        /// 現在の読み込みをキャンセル
        /// </summary>
        public void CancelLoading()
        {
            // キャンセルトークンをキャンセル
            _cancellationTokenSource?.Cancel();

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
        /// シーンをアンロード（UniTask版）
        /// </summary>
        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] Cannot unload scene while loading: {sceneName}");
                return;
            }

            Debug.Log($"[SceneLoader] Unloading scene: {sceneName}");

            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation != null)
            {
                // UniTaskでゼロアロケーション待機
                await operation.ToUniTask(cancellationToken: cancellationToken);
                Debug.Log($"[SceneLoader] Scene unloaded: {sceneName}");
            }
        }

        /// <summary>
        /// シーンをアンロード（レガシー互換版）
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            UnloadSceneAsync(sceneName).Forget();
        }
    }
}
