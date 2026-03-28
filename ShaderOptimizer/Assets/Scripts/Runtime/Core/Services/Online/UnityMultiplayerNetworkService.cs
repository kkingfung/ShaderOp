#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ShaderOp.Runtime.Core.Services.Online
{
    /// <summary>
    /// Unity Multiplayer Services v2を使用したネットワークサービス実装
    /// </summary>
    /// <remarks>
    /// Photon PUN 2からUnity Multiplayer Servicesへの移行版。
    /// Session-based APIを使用してルーム管理、プレイヤー管理を実装。
    ///
    /// 主要機能:
    /// - 匿名認証（AnonymousSignIn）
    /// - セッション作成・参加（CreateSessionAsync / JoinSessionByCodeAsync）
    /// - Join Code生成（6桁コード）
    /// - プレイヤー参加/離脱イベント
    /// </remarks>
    public class UnityMultiplayerNetworkService : MonoBehaviour, INetworkService
    {
        private IPlayerIdService? _playerIdService;
        private ISession? _currentSession;
        private string? _sessionHostId;
        private bool _isInitialized;

        #region INetworkService Properties

        public bool IsConnected => _currentSession != null;

        public bool IsInRoom => _currentSession != null;

        public bool IsMasterClient =>
            _sessionHostId != null &&
            _sessionHostId == AuthenticationService.Instance.PlayerId;

        public int LocalPlayerId => _playerIdService?.LocalGameId ?? -1;

        public int PlayerCount => _currentSession?.Players?.Count ?? 0;

        public string? RoomName => _currentSession?.Name;

        #endregion

        #region INetworkService Events

        public event Action? OnConnectedToServer;
        public event Action<string>? OnDisconnected;
        public event Action<string>? OnJoinedRoom;
        public event Action? OnLeftRoom;
        public event Action<int>? OnPlayerJoined;
        public event Action<int>? OnPlayerLeft;
        public event Action<string>? OnRoomJoined;
        public event Action<string>? OnRoomLeft;
        public event Action<string>? OnRoomCreated;

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
        /// 新しいセッションを作成
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="maxPlayers">最大プレイヤー数</param>
        /// <returns>作成成功でtrue</returns>
        public async UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2)
        {
            try
            {
                Debug.Log($"[NetworkService] Creating room: {roomName}, MaxPlayers: {maxPlayers}");

                var sessionOptions = new SessionOptions
                {
                    Name = roomName,
                    MaxPlayers = maxPlayers
                };

                _currentSession = await MultiplayerService.Instance.CreateSessionAsync(sessionOptions);

                // プレイヤーイベントを購読
                _currentSession.Players.PlayerJoined += OnSessionPlayerJoined;
                _currentSession.Players.PlayerLeft += OnSessionPlayerLeft;

                // セッションホストIDを保存
                _sessionHostId = _currentSession.HostId;

                // ローカルプレイヤーはホストなのでGameId=0
                _playerIdService?.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, gameId: 0);

                OnRoomCreated?.Invoke(roomName);
                OnJoinedRoom?.Invoke(roomName);

                Debug.Log($"[NetworkService] Room created successfully: {_currentSession.Id}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to create room: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ルームを作成してJoin Codeを生成
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="maxPlayers">最大プレイヤー数</param>
        /// <returns>6桁のjoin code（失敗時はnull）</returns>
        public async UniTask<string?> CreateRoomWithCodeAsync(string roomName, int maxPlayers = 2)
        {
            try
            {
                Debug.Log($"[NetworkService] Creating room with join code: {roomName}");

                var sessionOptions = new SessionOptions
                {
                    Name = roomName,
                    MaxPlayers = maxPlayers
                };

                _currentSession = await MultiplayerService.Instance.CreateSessionAsync(sessionOptions);

                // プレイヤーイベントを購読
                _currentSession.Players.PlayerJoined += OnSessionPlayerJoined;
                _currentSession.Players.PlayerLeft += OnSessionPlayerLeft;

                // セッションホストIDを保存
                _sessionHostId = _currentSession.HostId;

                // ローカルプレイヤーはホストなのでGameId=0
                _playerIdService?.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, gameId: 0);

                // Join Codeを生成
                string joinCode = await _currentSession.GetJoinCodeAsync();

                OnRoomCreated?.Invoke(joinCode);
                OnJoinedRoom?.Invoke(joinCode);

                Debug.Log($"[NetworkService] Room created with join code: {joinCode}");
                return joinCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to create room with code: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Join Codeを使用してセッションに参加
        /// </summary>
        /// <param name="joinCode">6桁のjoin code</param>
        /// <returns>参加成功でtrue</returns>
        public async UniTask<bool> JoinRoomAsync(string joinCode)
        {
            try
            {
                Debug.Log($"[NetworkService] Joining session by code: {joinCode}");

                // Join Codeでセッションに参加
                _currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);

                // プレイヤーイベントを購読
                _currentSession.Players.PlayerJoined += OnSessionPlayerJoined;
                _currentSession.Players.PlayerLeft += OnSessionPlayerLeft;

                // セッションホストIDを保存
                _sessionHostId = _currentSession.HostId;

                // ローカルプレイヤーのGameIdを決定（ホスト=0、ゲスト=1）
                int localGameId = (AuthenticationService.Instance.PlayerId == _sessionHostId) ? 0 : 1;
                _playerIdService?.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, localGameId);

                // 既存プレイヤーを登録
                if (_currentSession.Players != null)
                {
                    foreach (var player in _currentSession.Players)
                    {
                        if (player.Id != AuthenticationService.Instance.PlayerId)
                        {
                            int gameId = _playerIdService?.GetNextGameId() ?? -1;
                            _playerIdService?.RegisterPlayer(player.Id, gameId);
                            Debug.Log($"[NetworkService] Registered existing player: {player.Id} as GameId={gameId}");
                        }
                    }
                }

                OnRoomJoined?.Invoke(joinCode);
                OnJoinedRoom?.Invoke(joinCode);

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
        /// ランダムなセッションに参加
        /// </summary>
        public async UniTask<bool> JoinRandomRoomAsync()
        {
            try
            {
                Debug.Log("[NetworkService] Joining random session...");

                _currentSession = await MultiplayerService.Instance.JoinRandomSessionAsync();

                // プレイヤーイベントを購読
                _currentSession.Players.PlayerJoined += OnSessionPlayerJoined;
                _currentSession.Players.PlayerLeft += OnSessionPlayerLeft;

                // セッションホストIDを保存
                _sessionHostId = _currentSession.HostId;

                // ローカルプレイヤーのGameIdを決定
                int localGameId = _playerIdService?.GetNextGameId() ?? 1;
                _playerIdService?.RegisterLocalPlayer(AuthenticationService.Instance.PlayerId, localGameId);

                // 既存プレイヤーを登録
                if (_currentSession.Players != null)
                {
                    foreach (var player in _currentSession.Players)
                    {
                        if (player.Id != AuthenticationService.Instance.PlayerId)
                        {
                            int gameId = _playerIdService?.GetNextGameId() ?? -1;
                            _playerIdService?.RegisterPlayer(player.Id, gameId);
                        }
                    }
                }

                OnRoomJoined?.Invoke(_currentSession.Name ?? "RandomRoom");
                OnJoinedRoom?.Invoke(_currentSession.Name ?? "RandomRoom");

                Debug.Log($"[NetworkService] Joined random session: {_currentSession.Id}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to join random session: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 現在のセッションから退出
        /// </summary>
        public async UniTask LeaveRoomAsync()
        {
            try
            {
                if (_currentSession == null)
                {
                    Debug.LogWarning("[NetworkService] Not in a session.");
                    return;
                }

                Debug.Log("[NetworkService] Leaving session...");

                // イベント購読解除
                _currentSession.Players.PlayerJoined -= OnSessionPlayerJoined;
                _currentSession.Players.PlayerLeft -= OnSessionPlayerLeft;

                string roomName = _currentSession.Name ?? "Unknown";

                await _currentSession.LeaveAsync();
                _currentSession = null;
                _sessionHostId = null;

                OnLeftRoom?.Invoke();
                OnRoomLeft?.Invoke(roomName);

                Debug.Log("[NetworkService] Left session successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkService] Failed to leave session: {e.Message}");
            }
        }

        #endregion

        #region Player Events

        /// <summary>
        /// プレイヤーがセッションに参加したときの処理
        /// </summary>
        private void OnSessionPlayerJoined(IPlayer player)
        {
            Debug.Log($"[NetworkService] Player joined session: {player.Id}");

            // 新しいGameIdを割り当て
            int gameId = _playerIdService?.GetNextGameId() ?? -1;
            _playerIdService?.RegisterPlayer(player.Id, gameId);

            // イベント発火
            OnPlayerJoined?.Invoke(gameId);

            Debug.Log($"[NetworkService] Player registered: PlayerId={player.Id}, GameId={gameId}");
        }

        /// <summary>
        /// プレイヤーがセッションから退出したときの処理
        /// </summary>
        private void OnSessionPlayerLeft(IPlayer player)
        {
            Debug.Log($"[NetworkService] Player left session: {player.Id}");

            // GameIdを取得してから削除
            int gameId = _playerIdService?.GetGameId(player.Id) ?? -1;
            _playerIdService?.RemovePlayer(player.Id);

            // イベント発火
            OnPlayerLeft?.Invoke(gameId);

            Debug.Log($"[NetworkService] Player unregistered: PlayerId={player.Id}, GameId={gameId}");
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            if (_currentSession != null)
            {
                _currentSession.Players.PlayerJoined -= OnSessionPlayerJoined;
                _currentSession.Players.PlayerLeft -= OnSessionPlayerLeft;
            }

            _currentSession = null;
            _sessionHostId = null;
            _isInitialized = false;

            Debug.Log("[NetworkService] Service destroyed.");
        }

        #endregion
    }
}
