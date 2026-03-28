#nullable enable

using UnityEngine;
using ShaderOp.Core.Services;
using ShaderOp.Core.Services.Online;
using ShaderOp.Online.Services;
using ShaderOp.Runtime.Core.Services.Online;

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

        [Tooltip("オブジェクトプールサービスを有効化")]
        [SerializeField] private bool _enableObjectPoolService = true;

        [Header("Network Service Settings (Phase 2)")]
        [Tooltip("ネットワークサービスの種類")]
        [SerializeField] private NetworkServiceType _networkServiceType = NetworkServiceType.UnityMultiplayer;

        [Header("Network Service Prefabs (Legacy)")]
        [Tooltip("PhotonNetworkServiceプレハブ (Legacy)")]
        [SerializeField] private GameObject? _photonNetworkServicePrefab;

        [Tooltip("PhotonGameSyncServiceプレハブ (Legacy)")]
        [SerializeField] private GameObject? _gameSyncServicePrefab;

        [Header("Object Pool Prefabs (Optional)")]
        [Tooltip("HexTileVisualizerプレハブ（ミニゲーム用）")]
        [SerializeField] private ShaderOp.Minigames.HexGrid.HexTileVisualizer? _hexTilePrefab;

        [Tooltip("Player1Pieceプレハブ（ゲーム駒）")]
        [SerializeField] private UnityEngine.Component? _player1PiecePrefab;

        [Tooltip("Player2Pieceプレハブ（ゲーム駒）")]
        [SerializeField] private UnityEngine.Component? _player2PiecePrefab;

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

            // 1. PlayerIdService（Phase 5 Week 2で追加 - ネットワークサービスより先に登録）
            var playerIdService = gameObject.AddComponent<PlayerIdService>();
            ServiceLocator.Instance.Register<IPlayerIdService>(playerIdService);
            Debug.Log("[GameBootstrap] PlayerIdService registered.");

            // 2. ネットワークサービス（Phase 5）
            if (_enableNetworkService)
            {
                RegisterNetworkServices();
            }

            // 3. セーブデータサービス
            if (_enableSaveDataService)
            {
                var saveDataService = new LocalSaveDataService();
                ServiceLocator.Instance.Register<ISaveDataService>(saveDataService);
                Debug.Log("[GameBootstrap] SaveDataService registered.");
            }

            // 4. Firebase認証サービス
            if (_enableFirebaseAuth)
            {
                var firebaseAuthService = gameObject.AddComponent<FirebaseAuthService>();
                ServiceLocator.Instance.Register<IFirebaseAuthService>(firebaseAuthService);
                Debug.Log("[GameBootstrap] FirebaseAuthService registered.");

                // 5. HTTPクライアントサービス（Firebase認証に依存）
                var httpClientService = new HttpClientService(firebaseAuthService);
                ServiceLocator.Instance.Register<IHttpClientService>(httpClientService);
                Debug.Log("[GameBootstrap] HttpClientService registered.");
            }

            // 6. シーンローダーサービス（既存SceneLoaderをラップ）
            var sceneLoaderService = new SceneLoaderService();
            ServiceLocator.Instance.Register<ISceneLoaderService>(sceneLoaderService);
            Debug.Log("[GameBootstrap] SceneLoaderService registered.");

            // 7. オブジェクトプールサービス
            if (_enableObjectPoolService)
            {
                var poolService = gameObject.AddComponent<ObjectPoolService>();
                ServiceLocator.Instance.Register<IObjectPoolService>(poolService);
                Debug.Log("[GameBootstrap] ObjectPoolService registered.");

                // プールの登録（prefabが設定されている場合のみ）
                RegisterObjectPools(poolService);
            }

            Debug.Log($"[GameBootstrap] {CountRegisteredServices()} services registered successfully.");
        }

        /// <summary>
        /// オブジェクトプールを登録
        /// </summary>
        private void RegisterObjectPools(IObjectPoolService poolService)
        {
            // HexTileVisualizerプール登録（HexChessが121タイルなので最大200に設定）
            if (_hexTilePrefab != null)
            {
                poolService.RegisterPool(_hexTilePrefab, defaultCapacity: 64, maxSize: 200);
                poolService.Prewarm<ShaderOp.Minigames.HexGrid.HexTileVisualizer>(64);
                Debug.Log("[GameBootstrap] HexTileVisualizer pool registered and prewarmed (64 tiles)");
            }
            else
            {
                Debug.LogWarning("[GameBootstrap] HexTilePrefab is not assigned. Pool will be registered later.");
            }

            // Player1Pieceプール登録（Component型として登録）
            // 注意: 実際のコンポーネント型がわからないため、prefab設定後に各ゲームで個別登録することを推奨
            // if (_player1PiecePrefab != null)
            // {
            //     poolService.RegisterPool(_player1PiecePrefab, defaultCapacity: 32, maxSize: 100);
            //     Debug.Log("[GameBootstrap] Player1Piece pool registered");
            // }

            // Player2Pieceプール登録（Component型として登録）
            // if (_player2PiecePrefab != null)
            // {
            //     poolService.RegisterPool(_player2PiecePrefab, defaultCapacity: 32, maxSize: 100);
            //     Debug.Log("[GameBootstrap] Player2Piece pool registered");
            // }
        }

        /// <summary>
        /// ネットワークサービスを登録（Phase 2: Unity Multiplayer Services）
        /// </summary>
        private void RegisterNetworkServices()
        {
            INetworkService? networkService = null;

            switch (_networkServiceType)
            {
                case NetworkServiceType.UnityMultiplayer:
                    // Unity Multiplayer Servicesを使用
                    var unityMultiplayerService = gameObject.AddComponent<UnityMultiplayerNetworkService>();
                    networkService = unityMultiplayerService;
                    Debug.Log("[GameBootstrap] INetworkService (Unity Multiplayer) registered.");
                    break;

                case NetworkServiceType.Photon:
                    // Photon PUN2を使用（レガシー - 現在無効化）
                    Debug.LogWarning("[GameBootstrap] Photon support disabled. PhotonNetworkService has been replaced with UnityMultiplayerNetworkService.");
                    networkService = null;
                    /* DISABLED - Photon removed
                    if (_photonNetworkServicePrefab != null)
                    {
                        GameObject networkServiceObj = Instantiate(_photonNetworkServicePrefab);
                        DontDestroyOnLoad(networkServiceObj);
                        networkServiceObj.name = "PhotonNetworkService";

                        var photonService = networkServiceObj.GetComponent<PhotonNetworkService>();
                        if (photonService != null)
                        {
                            networkService = photonService;
                            Debug.Log("[GameBootstrap] INetworkService (Photon) registered.");
                        }
                        else
                        {
                            Debug.LogError("[GameBootstrap] PhotonNetworkServiceコンポーネントが見つかりません");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[GameBootstrap] PhotonNetworkServicePrefabが設定されていません（オフラインモードで動作）");
                    }
                    */
                    break;

                case NetworkServiceType.None:
                    Debug.Log("[GameBootstrap] NetworkService disabled (offline mode).");
                    break;
            }

            if (networkService != null)
            {
                ServiceLocator.Instance.Register<INetworkService>(networkService);
            }

            // UnityMultiplayerGameSyncService登録（Phase 5 Week 2）
            if (_networkServiceType == NetworkServiceType.UnityMultiplayer && networkService != null)
            {
                var gameSyncService = gameObject.AddComponent<UnityMultiplayerGameSyncService>();
                ServiceLocator.Instance.Register<IGameSyncService>(gameSyncService);
                Debug.Log("[GameBootstrap] IGameSyncService (Unity Multiplayer) registered.");
            }
            else if (_networkServiceType == NetworkServiceType.Photon)
            {
                Debug.LogWarning("[GameBootstrap] Photon GameSyncService not implemented. Use UnityMultiplayer instead.");
            }
            else
            {
                Debug.Log("[GameBootstrap] GameSyncService not registered (offline mode).");
            }
        }

        /// <summary>
        /// 登録されたサービス数を取得
        /// </summary>
        private int CountRegisteredServices()
        {
            int count = 0;
            if (ServiceLocator.Instance.IsRegistered<IPlayerIdService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<INetworkService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<IGameSyncService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<ISaveDataService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<IFirebaseAuthService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<IHttpClientService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<ISceneLoaderService>()) count++;
            if (ServiceLocator.Instance.IsRegistered<IObjectPoolService>()) count++;
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

    /// <summary>
    /// ネットワークサービスの種類
    /// </summary>
    public enum NetworkServiceType
    {
        /// <summary>ネットワーク機能を無効化（オフラインモード）</summary>
        None,
        /// <summary>Unity Multiplayer Services (推奨)</summary>
        UnityMultiplayer,
        /// <summary>Photon PUN2 (レガシー)</summary>
        Photon
    }
}
