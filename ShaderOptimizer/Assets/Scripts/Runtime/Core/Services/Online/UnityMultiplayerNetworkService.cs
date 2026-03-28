#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using ShaderOp.Core.Services;

namespace ShaderOp.Runtime.Core.Services.Online
{
    /// <summary>
    /// Unity Multiplayer Services v2を使用したネットワークサービス実装
    /// </summary>
    /// <remarks>
    /// Unity Multiplayer Services v2.1.3の実装。
    /// セッション管理（作成・参加・離脱）のみ提供。
    ///
    /// **重要**: メッセージ送受信にはNetcode for GameObjectsパッケージが必要です。
    /// 現在はスタブ実装のみ。manifest.jsonに以下を追加してインストール:
    /// "com.unity.netcode.gameobjects": "2.0.0"
    ///
    /// 機能:
    /// - Unity Services初期化・匿名認証
    /// - セッション作成（CreateSessionAsync with Relay）
    /// - Join Code生成（6桁コード）
    /// - Join Codeでセッション参加（JoinSessionByCodeAsync）
    /// - プレイヤー参加/離脱イベント（session.PlayerJoined/PlayerLeft）
    /// </remarks>
    public class UnityMultiplayerNetworkService : MonoBehaviour, INetworkService
    {
        private IPlayerIdService? _playerIdService;
        private ISession? _currentSession;
        private bool _isInitialized;

        #region INetworkService Properties

        public bool IsConnected => _currentSession != null;

        public bool IsInRoom => _currentSession != null;

        public bool IsMasterClient =>
            _currentSession != null &&
            _currentSession.IsServer;

        public int LocalPlayerId => _playerIdService?.LocalGameId ?? -1;

        public int PlayerCount => _currentSession?.Players?.Count ?? 0;

        public string? RoomName => _currentSession?.Name;

        #endregion

        #region INetworkService Events

        public event Action<bool>? OnConnectedChanged;
        public event Action? OnConnectedToServer;
        public event Action<string>? OnRoomJoined;
        public event Action? OnRoomLeft;
        public event Action<int>? OnPlayerJoined;
        public event Action<int>? OnPlayerLeft;

        #endregion

        #region Initialization

        /// <summary>
        /// Unity Servicesを初期化し、匿名認証を実行
        /// </summary>
        public async UniTask<bool> InitializeAsync()
        {
            try
            {
                if (_isInitialized)
                {
                    Debug.Log("[NetworkService] Already initialized.");
                    return true;
                }

                Debug.Log("[NetworkService] Initializing Unity Services...");

                // Unity Services初期化
                await UnityServices.InitializeAsync();

                // PlayerIdServiceを取得
                _playerIdService = ServiceLocator.Instance.Get<IPlayerIdService>();
                if (_playerIdService == null)
                {
                    Debug.LogError("[NetworkService] PlayerIdService not found in ServiceLocator!");
                    return false;
                }

                // 匿名サインイン
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[NetworkService] Signed in anonymously: {AuthenticationService.Instance.PlayerId}");
                }

                // ローカルプレイヤーを登録（GameId=0）
                _playerIdService.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, gameId: 0);

                _isInitialized = true;
                OnConnectedToServer?.Invoke();
                OnConnectedChanged?.Invoke(true);

                Debug.Log("[NetworkService] Unity Services initialized successfully.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Initialization failed: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Room Management

        /// <summary>
        /// 新しいセッションを作成（Unity Multiplayer Services v2 API）
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="maxPlayers">最大プレイヤー数</param>
        /// <returns>作成成功でtrue</returns>
        public async UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2)
        {
            try
            {
                Debug.Log($"[NetworkService] Creating session: {roomName}, MaxPlayers: {maxPlayers}");

                var options = new SessionOptions
                {
                    Name = roomName,
                    MaxPlayers = maxPlayers
                }.WithRelayNetwork();

                _currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);

                // プレイヤーイベントを購読
                _currentSession.PlayerJoined += OnSessionPlayerJoined;
                _currentSession.PlayerLeaving += OnSessionPlayerLeft;

                // ローカルプレイヤーはホストなのでGameId=0
                _playerIdService?.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, gameId: 0);

                OnRoomJoined?.Invoke(roomName);
                OnConnectedChanged?.Invoke(true);

                Debug.Log($"[NetworkService] Session created successfully: {_currentSession.Id}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to create session: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 新しいセッションを作成してJoin Codeを取得
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="maxPlayers">最大プレイヤー数</param>
        /// <returns>6桁のJoin Code、失敗時はnull</returns>
        public async UniTask<string?> CreateRoomWithCodeAsync(string roomName, int maxPlayers = 2)
        {
            try
            {
                Debug.Log($"[NetworkService] Creating session with join code: {roomName}");

                var options = new SessionOptions
                {
                    Name = roomName,
                    MaxPlayers = maxPlayers
                }.WithRelayNetwork();

                _currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);

                // プレイヤーイベントを購読
                _currentSession.PlayerJoined += OnSessionPlayerJoined;
                _currentSession.PlayerLeaving += OnSessionPlayerLeft;

                // ローカルプレイヤーはホストなのでGameId=0
                _playerIdService?.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, gameId: 0);

                // Join Codeを取得（Unity Multiplayer Services v2ではsession.Codeプロパティ）
                string joinCode = _currentSession.Code;

                OnRoomJoined?.Invoke(joinCode);
                OnConnectedChanged?.Invoke(true);

                Debug.Log($"[NetworkService] Session created with join code: {joinCode}");
                return joinCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to create session with join code: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Join Codeでセッションに参加
        /// </summary>
        /// <param name="joinCode">6桁のJoin Code</param>
        /// <returns>参加成功でtrue</returns>
        public async UniTask<bool> JoinRoomAsync(string joinCode)
        {
            try
            {
                Debug.Log($"[NetworkService] Joining session with code: {joinCode}");

                _currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);

                // プレイヤーイベントを購読
                _currentSession.PlayerJoined += OnSessionPlayerJoined;
                _currentSession.PlayerLeaving += OnSessionPlayerLeft;

                // ゲストプレイヤーを登録（GameId=1）
                _playerIdService?.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, gameId: 1);

                // 既存プレイヤーを登録
                if (_currentSession.Players != null)
                {
                    int nextGameId = 2;
                    foreach (var player in _currentSession.Players)
                    {
                        // ローカルプレイヤー以外を登録
                        if (player.Id != AuthenticationService.Instance.PlayerId)
                        {
                            int gameId = nextGameId++;
                            _playerIdService?.RegisterPlayer(player.Id, gameId);
                            Debug.Log($"[NetworkService] Registered existing player: {player.Id} as GameId={gameId}");
                        }
                    }
                }

                OnRoomJoined?.Invoke(joinCode);
                OnConnectedChanged?.Invoke(true);

                Debug.Log($"[NetworkService] Joined session successfully: {_currentSession.Id}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to join session: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ランダムなセッションに参加（マッチメイキング）
        /// TODO: Unity Multiplayer Services v2.1.3ではQuickJoinが未実装の可能性
        /// </summary>
        public async UniTask<bool> JoinRandomRoomAsync()
        {
            // Unity Multiplayer Services v2.1.3にはJoinRandomSessionAsync相当のAPIが見つからない
            // QuickMatchmakingを使う必要があるかもしれないが、要調査
            Debug.LogWarning("[NetworkService] JoinRandomRoomAsync is not implemented yet. Use JoinRoomAsync with a join code.");
            await UniTask.Yield();
            return false;
        }

        /// <summary>
        /// セッションから退出
        /// </summary>
        public async UniTask LeaveRoomAsync()
        {
            if (_currentSession == null)
            {
                Debug.LogWarning("[NetworkService] Not in a session.");
                return;
            }

            try
            {
                Debug.Log("[NetworkService] Leaving session...");

                // イベント購読解除
                _currentSession.PlayerJoined -= OnSessionPlayerJoined;
                _currentSession.PlayerLeaving -= OnSessionPlayerLeft;

                // セッション離脱
                await _currentSession.LeaveAsync();

                _currentSession = null;

                OnRoomLeft?.Invoke();
                OnConnectedChanged?.Invoke(false);

                Debug.Log("[NetworkService] Left session successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to leave session: {e.Message}");
            }
        }

        #endregion

        #region Additional Interface Methods

        /// <summary>
        /// Photonマスターサーバーに接続（Unity Multiplayer Servicesでは不要）
        /// </summary>
        public async UniTask<bool> ConnectToServerAsync()
        {
            // Unity Multiplayer Servicesでは、InitializeAsync()で既に接続済み
            // このメソッドはPhoton互換性のため存在するが、何もしない
            await UniTask.Yield();
            return _isInitialized;
        }

        /// <summary>
        /// Photonサーバーから切断（Unity Multiplayer Servicesではセッション離脱のみ）
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            if (_currentSession != null)
            {
                await LeaveRoomAsync();
            }
            OnConnectedChanged?.Invoke(false);
        }

        /// <summary>
        /// Wire Protocolメッセージ受信ハンドラを登録
        /// TODO: Netcode for GameObjectsパッケージが必要
        /// </summary>
        public void RegisterMessageReceiver(Action<byte, ArraySegment<byte>> receiver)
        {
            // TODO: Netcode for GameObjectsのCustomMessagingManagerを使用する必要がある
            // 現在はスタブ実装
            Debug.LogWarning("[NetworkService] RegisterMessageReceiver: Netcode for GameObjects package required. Stub implementation.");
        }

        #endregion

        #region Player Events

        private void OnSessionPlayerJoined(string playerId)
        {
            Debug.Log($"[NetworkService] Player joined: {playerId}");

            // GameIdを割り当て
            int gameId = _playerIdService?.GetNextGameId() ?? -1;
            _playerIdService?.RegisterPlayer(playerId, gameId);

            OnPlayerJoined?.Invoke(gameId);
        }

        private void OnSessionPlayerLeft(string playerId)
        {
            Debug.Log($"[NetworkService] Player left: {playerId}");

            // GameIdを取得
            int gameId = _playerIdService?.GetGameId(playerId) ?? -1;

            // TODO: IPlayerIdService needs UnregisterPlayer method
            // For now, just invoke the event
            OnPlayerLeft?.Invoke(gameId);
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            if (_currentSession != null)
            {
                _currentSession.PlayerJoined -= OnSessionPlayerJoined;
                _currentSession.PlayerLeaving -= OnSessionPlayerLeft;
            }

            _currentSession = null;
            _isInitialized = false;

            Debug.Log("[NetworkService] Service destroyed.");
        }

        #endregion
    }
}
