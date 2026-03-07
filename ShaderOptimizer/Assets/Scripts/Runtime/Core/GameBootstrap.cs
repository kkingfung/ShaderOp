#nullable enable

using UnityEngine;
using ShaderOp.Core.Services;
using ShaderOp.Core.Services.Online;
using ShaderOp.Online.Services;

namespace ShaderOp.Core
{
    /// <summary>
    /// ゲーム起動時の初期化処理
    /// </summary>
    /// <remarks>
    /// ゲーム開始時に一度だけ実行され、すべてのサービスを初期化・登録します。
    /// このGameObjectはDontDestroyOnLoadでシーン遷移後も保持されます。
    /// </remarks>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Services")]
        [Tooltip("ネットワークサービスを有効化")]
        [SerializeField] private bool _enableNetworkService = true;

        [Tooltip("セーブデータサービスを有効化")]
        [SerializeField] private bool _enableSaveDataService = true;

        [Tooltip("Firebase認証サービスを有効化")]
        [SerializeField] private bool _enableFirebaseAuth = true;

        /// <summary>初期化済みフラグ</summary>
        private static bool _isInitialized = false;

        private void Awake()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[GameBootstrap] Already initialized. Destroying duplicate instance.");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            InitializeServices();
            _isInitialized = true;

            Debug.Log("[GameBootstrap] Initialization complete.");
        }

        /// <summary>
        /// すべてのサービスを初期化
        /// </summary>
        private void InitializeServices()
        {
            Debug.Log("[GameBootstrap] Initializing services...");

            // 1. ネットワークサービス
            if (_enableNetworkService)
            {
                var networkService = gameObject.AddComponent<PhotonNetworkService>();
                ServiceLocator.Instance.Register<INetworkService>(networkService);
                Debug.Log("[GameBootstrap] NetworkService registered.");
            }

            // 2. セーブデータサービス
            if (_enableSaveDataService)
            {
                var saveDataService = new LocalSaveDataService();
                ServiceLocator.Instance.Register<ISaveDataService>(saveDataService);
                Debug.Log("[GameBootstrap] SaveDataService registered.");
            }

            // 3. Firebase認証サービス
            if (_enableFirebaseAuth)
            {
                var firebaseAuthService = gameObject.AddComponent<FirebaseAuthService>();
                ServiceLocator.Instance.Register<IFirebaseAuthService>(firebaseAuthService);
                Debug.Log("[GameBootstrap] FirebaseAuthService registered.");

                // 4. HTTPクライアントサービス（Firebase認証に依存）
                var httpClientService = new HttpClientService(firebaseAuthService);
                ServiceLocator.Instance.Register<IHttpClientService>(httpClientService);
                Debug.Log("[GameBootstrap] HttpClientService registered.");
            }

            // 5. シーンローダーサービス（既存SceneLoaderをラップ）
            var sceneLoaderService = new SceneLoaderService();
            ServiceLocator.Instance.Register<ISceneLoaderService>(sceneLoaderService);
            Debug.Log("[GameBootstrap] SceneLoaderService registered.");

            Debug.Log($"[GameBootstrap] {CountRegisteredServices()} services registered successfully.");
        }

        /// <summary>
        /// 登録されたサービス数を取得
        /// </summary>
        private int CountRegisteredServices()
        {
            int count = 0;
            if (ServiceLocator.Instance.IsRegistered<INetworkService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<ISaveDataService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<IFirebaseAuthService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<IHttpClientService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<ISceneLoaderService>()) count++;
            return count;
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                Debug.Log("[GameBootstrap] Cleaning up services...");
                ServiceLocator.Instance.Clear();
                _isInitialized = false;
            }
        }
    }

    #region サービス実装（一時的に同じファイルに配置）

    /// <summary>
    /// ローカルセーブデータサービス実装
    /// </summary>
    internal class LocalSaveDataService : ISaveDataService
    {
        private const string SavePrefix = "ShaderOp_";

        public bool HasSaveData => !string.IsNullOrEmpty(CurrentPlayerId);
        public string? CurrentPlayerId { get; private set; }

        public LocalSaveDataService()
        {
            CurrentPlayerId = PlayerPrefs.GetString($"{SavePrefix}PlayerId", null);
            if (string.IsNullOrEmpty(CurrentPlayerId))
            {
                CurrentPlayerId = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString($"{SavePrefix}PlayerId", CurrentPlayerId);
                PlayerPrefs.Save();
            }
        }

        public async Cysharp.Threading.Tasks.UniTask<bool> SaveAsync<T>(string key, T data) where T : class
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString($"{SavePrefix}{key}", json);
                PlayerPrefs.Save();
                await Cysharp.Threading.Tasks.UniTask.Yield();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalSaveDataService] Save failed: {e.Message}");
                return false;
            }
        }

        public async Cysharp.Threading.Tasks.UniTask<T?> LoadAsync<T>(string key) where T : class
        {
            try
            {
                string json = PlayerPrefs.GetString($"{SavePrefix}{key}", string.Empty);
                if (string.IsNullOrEmpty(json)) return null;

                await Cysharp.Threading.Tasks.UniTask.Yield();
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalSaveDataService] Load failed: {e.Message}");
                return null;
            }
        }

        public async Cysharp.Threading.Tasks.UniTask<bool> DeleteAsync(string key)
        {
            PlayerPrefs.DeleteKey($"{SavePrefix}{key}");
            PlayerPrefs.Save();
            await Cysharp.Threading.Tasks.UniTask.Yield();
            return true;
        }

        public bool Exists(string key)
        {
            return PlayerPrefs.HasKey($"{SavePrefix}{key}");
        }

        public async Cysharp.Threading.Tasks.UniTask ClearAllAsync()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
    }

    /// <summary>
    /// シーンローダーサービス実装
    /// </summary>
    internal class SceneLoaderService : ISceneLoaderService
    {
        public bool IsLoading { get; private set; }
        public float LoadProgress { get; private set; }

        public event System.Action<string>? OnSceneLoadStarted;
        public event System.Action<string>? OnSceneLoadCompleted;

        public async Cysharp.Threading.Tasks.UniTask LoadSceneAsync(string sceneName)
        {
            IsLoading = true;
            LoadProgress = 0f;
            OnSceneLoadStarted?.Invoke(sceneName);

            // SceneLoader経由でロード
            await SceneLoader.Instance.LoadSceneAsync(sceneName);

            LoadProgress = 1f;
            IsLoading = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
        }

        public async Cysharp.Threading.Tasks.UniTask LoadMainMenuAsync()
        {
            await LoadSceneAsync("MainMenu");
        }

        public async Cysharp.Threading.Tasks.UniTask LoadCharacterCustomizationAsync()
        {
            await LoadSceneAsync("MainCustomization");
        }

        public async Cysharp.Threading.Tasks.UniTask LoadRoomDecorationAsync()
        {
            await LoadSceneAsync("RoomDecoration");
        }

        public async Cysharp.Threading.Tasks.UniTask LoadMinigameAsync(string gameName)
        {
            await LoadSceneAsync(gameName);
        }
    }

    #endregion
}
