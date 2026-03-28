#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;
using ShaderOp.Runtime.Minigames.HexGrid;

namespace ShaderOp.Runtime.Core.Services.Online
{
    /// <summary>
    /// Unity Multiplayer Services Wire Protocolを使用したゲーム同期サービス
    /// </summary>
    /// <remarks>
    /// ターンベースヘックスボードゲームのリアルタイム同期を実装。
    ///
    /// Wire Protocolメッセージタイプ:
    /// - MSG_MOVE (1): 駒の移動 (16 bytes: fromQ, fromR, toQ, toR)
    /// - MSG_GAME_START (2): ゲーム開始 (0 bytes)
    /// - MSG_GAME_END (3): ゲーム終了 (4 bytes: winnerId)
    /// - MSG_TURN_PASS (4): ターンパス (4 bytes: nextPlayerId)
    /// - MSG_RESET (5): ゲームリセット (0 bytes)
    ///
    /// バイナリシリアライゼーションを使用してパフォーマンスを最適化。
    /// 移動メッセージは16バイト（JSONの100+バイトと比較）。
    /// </remarks>
    public class UnityMultiplayerGameSyncService : MonoBehaviour, IGameSyncService
    {
        #region Message Type Constants

        private const byte MSG_MOVE = 1;
        private const byte MSG_GAME_START = 2;
        private const byte MSG_GAME_END = 3;
        private const byte MSG_TURN_PASS = 4;
        private const byte MSG_RESET = 5;

        #endregion

        #region Fields

        private INetworkService? _networkService;
        private IPlayerIdService? _playerIdService;
        private ISession? _currentSession;
        private int _currentTurnPlayerId = 0;
        private bool _isSyncEnabled;

        #endregion

        #region IGameSyncService Properties

        public bool IsSyncEnabled => _isSyncEnabled && _currentSession != null;

        public bool IsMyTurn =>
            IsSyncEnabled &&
            _playerIdService != null &&
            _currentTurnPlayerId == _playerIdService.LocalGameId;

        #endregion

        #region IGameSyncService Events

        public event Action<HexCoordinate, HexCoordinate>? OnMoveReceived;
        public event Action? OnGameStarted;
        public event Action<int>? OnGameEnded;
        public event Action? OnGameReset;
        public event Action<int>? OnTurnChanged;

        #endregion

        #region Initialization

        private void Start()
        {
            // サービス取得
            _networkService = ServiceLocator.Instance.Get<INetworkService>();
            _playerIdService = ServiceLocator.Instance.Get<IPlayerIdService>();

            if (_networkService == null)
            {
                Debug.LogError("[GameSyncService] INetworkService not found!");
                return;
            }

            if (_playerIdService == null)
            {
                Debug.LogError("[GameSyncService] IPlayerIdService not found!");
                return;
            }

            // ネットワークイベントを購読
            _networkService.OnJoinedRoom += OnJoinedRoom;
            _networkService.OnLeftRoom += OnLeftRoom;

            Debug.Log("[GameSyncService] Service initialized.");
        }

        /// <summary>
        /// 同期を有効化
        /// </summary>
        public async UniTask<bool> EnableSyncAsync()
        {
            try
            {
                if (_networkService == null || !_networkService.IsInRoom)
                {
                    Debug.LogWarning("[GameSyncService] Not in a room. Cannot enable sync.");
                    return false;
                }

                // Unity Multiplayer Servicesのセッションを取得
                _currentSession = MultiplayerService.Instance.GetSession(_networkService.RoomName ?? "");
                if (_currentSession == null)
                {
                    Debug.LogError("[GameSyncService] Could not get current session.");
                    return false;
                }

                // Wire Protocolメッセージハンドラを登録
                RegisterMessageHandlers();

                _isSyncEnabled = true;
                Debug.Log("[GameSyncService] Sync enabled.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSyncService] Failed to enable sync: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 同期を無効化
        /// </summary>
        public void DisableSync()
        {
            if (_currentSession != null)
            {
                UnregisterMessageHandlers();
                _currentSession = null;
            }

            _isSyncEnabled = false;
            Debug.Log("[GameSyncService] Sync disabled.");
        }

        #endregion

        #region Message Handlers Registration

        /// <summary>
        /// Wire Protocolメッセージハンドラを登録
        /// </summary>
        private void RegisterMessageHandlers()
        {
            if (_currentSession == null) return;

            _currentSession.OnMessage += OnMessageReceived;
            Debug.Log("[GameSyncService] Message handlers registered.");
        }

        /// <summary>
        /// Wire Protocolメッセージハンドラを解除
        /// </summary>
        private void UnregisterMessageHandlers()
        {
            if (_currentSession == null) return;

            _currentSession.OnMessage -= OnMessageReceived;
            Debug.Log("[GameSyncService] Message handlers unregistered.");
        }

        #endregion

        #region Message Receiving

        /// <summary>
        /// Wire Protocolメッセージを受信
        /// </summary>
        private void OnMessageReceived(IPlayer sender, byte messageCode, ArraySegment<byte> payload)
        {
            Debug.Log($"[GameSyncService] Message received: Code={messageCode}, Size={payload.Count} bytes, Sender={sender.Id}");

            switch (messageCode)
            {
                case MSG_MOVE:
                    HandleMoveMessage(payload);
                    break;

                case MSG_GAME_START:
                    HandleGameStartMessage();
                    break;

                case MSG_GAME_END:
                    HandleGameEndMessage(payload);
                    break;

                case MSG_TURN_PASS:
                    HandleTurnPassMessage(payload);
                    break;

                case MSG_RESET:
                    HandleResetMessage();
                    break;

                default:
                    Debug.LogWarning($"[GameSyncService] Unknown message code: {messageCode}");
                    break;
            }
        }

        /// <summary>
        /// 移動メッセージを処理
        /// </summary>
        private void HandleMoveMessage(ArraySegment<byte> payload)
        {
            if (payload.Count < 16)
            {
                Debug.LogError($"[GameSyncService] Invalid move message size: {payload.Count} bytes (expected 16)");
                return;
            }

            var (fromQ, fromR, toQ, toR) = DeserializeMove(payload);
            var from = new HexCoordinate(fromQ, fromR);
            var to = new HexCoordinate(toQ, toR);

            Debug.Log($"[GameSyncService] Move received: {from} → {to}");
            OnMoveReceived?.Invoke(from, to);
        }

        /// <summary>
        /// ゲーム開始メッセージを処理
        /// </summary>
        private void HandleGameStartMessage()
        {
            Debug.Log("[GameSyncService] Game start received.");
            _currentTurnPlayerId = 0; // ホストがターン開始
            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// ゲーム終了メッセージを処理
        /// </summary>
        private void HandleGameEndMessage(ArraySegment<byte> payload)
        {
            if (payload.Count < 4)
            {
                Debug.LogError($"[GameSyncService] Invalid game end message size: {payload.Count} bytes");
                return;
            }

            int winnerId = BitConverter.ToInt32(payload.Array!, payload.Offset);
            Debug.Log($"[GameSyncService] Game end received: Winner={winnerId}");
            OnGameEnded?.Invoke(winnerId);
        }

        /// <summary>
        /// ターンパスメッセージを処理
        /// </summary>
        private void HandleTurnPassMessage(ArraySegment<byte> payload)
        {
            if (payload.Count < 4)
            {
                Debug.LogError($"[GameSyncService] Invalid turn pass message size: {payload.Count} bytes");
                return;
            }

            int nextPlayerId = BitConverter.ToInt32(payload.Array!, payload.Offset);
            _currentTurnPlayerId = nextPlayerId;
            Debug.Log($"[GameSyncService] Turn changed to player: {nextPlayerId}");
            OnTurnChanged?.Invoke(nextPlayerId);
        }

        /// <summary>
        /// リセットメッセージを処理
        /// </summary>
        private void HandleResetMessage()
        {
            Debug.Log("[GameSyncService] Game reset received.");
            _currentTurnPlayerId = 0;
            OnGameReset?.Invoke();
        }

        #endregion

        #region Message Sending

        /// <summary>
        /// 駒の移動を送信
        /// </summary>
        public async UniTask<bool> SendMoveAsync(HexCoordinate from, HexCoordinate to)
        {
            if (!IsSyncEnabled || _currentSession == null)
            {
                Debug.LogWarning("[GameSyncService] Sync not enabled. Cannot send move.");
                return false;
            }

            try
            {
                byte[] payload = SerializeMove(from.Q, from.R, to.Q, to.R);

                await _currentSession.SendMessageAsync(MSG_MOVE, new ArraySegment<byte>(payload));

                Debug.Log($"[GameSyncService] Move sent: {from} → {to}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSyncService] Failed to send move: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ゲーム開始を送信
        /// </summary>
        public async UniTask<bool> SendGameStartAsync()
        {
            if (!IsSyncEnabled || _currentSession == null)
            {
                Debug.LogWarning("[GameSyncService] Sync not enabled.");
                return false;
            }

            try
            {
                await _currentSession.SendMessageAsync(MSG_GAME_START, new ArraySegment<byte>(Array.Empty<byte>()));

                _currentTurnPlayerId = 0;
                Debug.Log("[GameSyncService] Game start sent.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSyncService] Failed to send game start: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ゲーム終了を送信
        /// </summary>
        public async UniTask<bool> SendGameEndAsync(int winnerId)
        {
            if (!IsSyncEnabled || _currentSession == null)
            {
                Debug.LogWarning("[GameSyncService] Sync not enabled.");
                return false;
            }

            try
            {
                byte[] payload = BitConverter.GetBytes(winnerId);

                await _currentSession.SendMessageAsync(MSG_GAME_END, new ArraySegment<byte>(payload));

                Debug.Log($"[GameSyncService] Game end sent: Winner={winnerId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSyncService] Failed to send game end: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ターンパスを送信
        /// </summary>
        public async UniTask<bool> SendTurnPassAsync(int nextPlayerId)
        {
            if (!IsSyncEnabled || _currentSession == null)
            {
                Debug.LogWarning("[GameSyncService] Sync not enabled.");
                return false;
            }

            try
            {
                byte[] payload = BitConverter.GetBytes(nextPlayerId);

                await _currentSession.SendMessageAsync(MSG_TURN_PASS, new ArraySegment<byte>(payload));

                _currentTurnPlayerId = nextPlayerId;
                Debug.Log($"[GameSyncService] Turn pass sent: Next player={nextPlayerId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSyncService] Failed to send turn pass: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ゲームリセットを送信
        /// </summary>
        public async UniTask<bool> SendResetAsync()
        {
            if (!IsSyncEnabled || _currentSession == null)
            {
                Debug.LogWarning("[GameSyncService] Sync not enabled.");
                return false;
            }

            try
            {
                await _currentSession.SendMessageAsync(MSG_RESET, new ArraySegment<byte>(Array.Empty<byte>()));

                _currentTurnPlayerId = 0;
                Debug.Log("[GameSyncService] Reset sent.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSyncService] Failed to send reset: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Binary Serialization

        /// <summary>
        /// 移動データをバイナリシリアライズ (16 bytes)
        /// </summary>
        private byte[] SerializeMove(int fromQ, int fromR, int toQ, int toR)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(fromQ).CopyTo(bytes, 0);
            BitConverter.GetBytes(fromR).CopyTo(bytes, 4);
            BitConverter.GetBytes(toQ).CopyTo(bytes, 8);
            BitConverter.GetBytes(toR).CopyTo(bytes, 12);
            return bytes;
        }

        /// <summary>
        /// バイナリデータから移動を復元
        /// </summary>
        private (int fromQ, int fromR, int toQ, int toR) DeserializeMove(ArraySegment<byte> payload)
        {
            if (payload.Array == null)
            {
                throw new InvalidOperationException("Payload array is null");
            }

            int fromQ = BitConverter.ToInt32(payload.Array, payload.Offset + 0);
            int fromR = BitConverter.ToInt32(payload.Array, payload.Offset + 4);
            int toQ = BitConverter.ToInt32(payload.Array, payload.Offset + 8);
            int toR = BitConverter.ToInt32(payload.Array, payload.Offset + 12);

            return (fromQ, fromR, toQ, toR);
        }

        #endregion

        #region Network Events

        /// <summary>
        /// ルーム参加時の処理
        /// </summary>
        private async void OnJoinedRoom(string roomName)
        {
            Debug.Log($"[GameSyncService] Joined room: {roomName}");

            // 同期を自動的に有効化
            await EnableSyncAsync();
        }

        /// <summary>
        /// ルーム退出時の処理
        /// </summary>
        private void OnLeftRoom()
        {
            Debug.Log("[GameSyncService] Left room.");

            // 同期を無効化
            DisableSync();
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            if (_networkService != null)
            {
                _networkService.OnJoinedRoom -= OnJoinedRoom;
                _networkService.OnLeftRoom -= OnLeftRoom;
            }

            DisableSync();

            Debug.Log("[GameSyncService] Service destroyed.");
        }

        #endregion
    }
}
