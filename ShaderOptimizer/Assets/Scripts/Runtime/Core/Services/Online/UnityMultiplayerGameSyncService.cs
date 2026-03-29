#nullable enable

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ShaderOp.Core.Services;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Runtime.Core.Services.Online
{
    /// <summary>
    /// Unity Multiplayer Services Wire Protocolを使用したゲーム同期サービス
    /// </summary>
    /// <remarks>
    /// **重要**: Wire Protocolメッセージ送受信にはNetcode for GameObjectsパッケージが必要です。
    /// 現在はスタブ実装のみ。manifest.jsonに以下を追加してインストール:
    /// "com.unity.netcode.gameobjects": "2.0.0"
    ///
    /// Netcodeインストール後、CustomMessagingManagerを使用して以下を実装:
    /// - SendMoveAsync(): NetworkManager.CustomMessagingManager.SendNamedMessage()
    /// - RegisterMessageHandlers(): NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler()
    ///
    /// Wire Protocolメッセージタイプ:
    /// - MSG_MOVE (1): 駒の移動 (16 bytes: fromQ, fromR, toQ, toR)
    /// - MSG_GAME_START (2): ゲーム開始 (0 bytes)
    /// - MSG_GAME_END (3): ゲーム終了 (4 bytes: winnerId)
    /// - MSG_TURN_PASS (4): ターンパス (4 bytes: nextPlayerId)
    /// - MSG_RESET (5): ゲームリセット (0 bytes)
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
        private int _currentTurnPlayerId = 0;
        private bool _isSyncEnabled;
        private string _gameType = "HexGame";

        #endregion

        #region IGameSyncService Properties

        public bool IsSyncEnabled => _isSyncEnabled && _networkService != null && _networkService.IsInRoom;

        public bool IsMyTurn =>
            IsSyncEnabled &&
            _playerIdService != null &&
            _currentTurnPlayerId == _playerIdService.LocalGameId;

        public int CurrentPlayerId => _currentTurnPlayerId;

        public string GameType
        {
            get => _gameType;
            set => _gameType = value;
        }

        #endregion

        #region IGameSyncService Events

        public event Action<HexCoordinate, HexCoordinate>? OnMoveReceived;
        public event Action? OnGameStarted;
        public event Action<int>? OnGameEnded;
        public event Action<int>? OnTurnChanged;
        public event Action? OnResetRequested;

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
            _networkService.OnRoomJoined += OnRoomJoined;
            _networkService.OnRoomLeft += OnRoomLeft;

            Debug.Log("[GameSyncService] Service initialized.");
        }

        /// <summary>
        /// 同期を有効化
        /// </summary>
        public async UniTask<bool> EnableSyncAsync()
        {
            if (_networkService == null || !_networkService.IsInRoom)
            {
                Debug.LogWarning("[GameSyncService] Not in a room. Cannot enable sync.");
                return false;
            }

            // TODO: Netcode for GameObjects がインストールされたら実装
            // NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler() でメッセージハンドラを登録

            _isSyncEnabled = true;
            Debug.Log("[GameSyncService] Sync enabled (stub implementation - Netcode required for actual message passing).");

            await UniTask.Yield();
            return true;
        }

        /// <summary>
        /// 同期を無効化
        /// </summary>
        private void DisableSync()
        {
            if (!_isSyncEnabled)
            {
                return;
            }

            // TODO: Netcode for GameObjects がインストールされたら実装
            // メッセージハンドラの登録解除

            _isSyncEnabled = false;
            _currentTurnPlayerId = 0;

            Debug.Log("[GameSyncService] Sync disabled.");
        }

        #endregion

        #region Message Sending (STUB - Requires Netcode)

        /// <summary>
        /// 手番移動を送信
        /// TODO: Netcode for GameObjectsのCustomMessagingManager.SendNamedMessage()を使用
        /// </summary>
        public async UniTask<bool> SendMoveAsync(HexCoordinate from, HexCoordinate to)
        {
            if (!IsSyncEnabled || !IsMyTurn)
            {
                Debug.LogWarning("[GameSyncService] Cannot send move: sync disabled or not my turn.");
                return false;
            }

            Debug.LogWarning($"[GameSyncService] SendMoveAsync STUB: {from} -> {to} (Netcode package required)");
            await UniTask.Yield();
            return false;
        }

        /// <summary>
        /// ゲーム開始を送信
        /// TODO: Netcode for GameObjectsのCustomMessagingManager.SendNamedMessage()を使用
        /// </summary>
        public async UniTask<bool> SendGameStartAsync()
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[GameSyncService] Cannot send game start: sync disabled.");
                return false;
            }

            Debug.LogWarning("[GameSyncService] SendGameStartAsync STUB (Netcode package required)");
            await UniTask.Yield();
            return false;
        }

        /// <summary>
        /// ゲーム終了を送信
        /// TODO: Netcode for GameObjectsのCustomMessagingManager.SendNamedMessage()を使用
        /// </summary>
        public async UniTask<bool> SendGameEndAsync(int winnerId)
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[GameSyncService] Cannot send game end: sync disabled.");
                return false;
            }

            Debug.LogWarning($"[GameSyncService] SendGameEndAsync STUB: winnerId={winnerId} (Netcode package required)");
            await UniTask.Yield();
            return false;
        }

        /// <summary>
        /// ターンパスを送信
        /// TODO: Netcode for GameObjectsのCustomMessagingManager.SendNamedMessage()を使用
        /// </summary>
        public async UniTask<bool> SendTurnPassAsync(int nextPlayerId)
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[GameSyncService] Cannot send turn pass: sync disabled.");
                return false;
            }

            Debug.LogWarning($"[GameSyncService] SendTurnPassAsync STUB: nextPlayerId={nextPlayerId} (Netcode package required)");
            await UniTask.Yield();
            return false;
        }

        /// <summary>
        /// ゲームリセットを送信
        /// TODO: Netcode for GameObjectsのCustomMessagingManager.SendNamedMessage()を使用
        /// </summary>
        public async UniTask<bool> SendResetAsync()
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[GameSyncService] Cannot send reset: sync disabled.");
                return false;
            }

            Debug.LogWarning("[GameSyncService] SendResetAsync STUB (Netcode package required)");
            await UniTask.Yield();
            return false;
        }

        #endregion

        #region Network Events

        /// <summary>
        /// ルーム参加時の処理
        /// </summary>
        private void OnRoomJoined(string roomName)
        {
            Debug.Log($"[GameSyncService] Joined room: {roomName}");

            // 同期を自動的に有効化（エラーハンドリング付き）
            EnableSyncAsync().Forget(ex =>
            {
                Debug.LogError($"[GameSyncService] Failed to enable sync on room join: {ex.Message}\n{ex.StackTrace}");
            });
        }

        /// <summary>
        /// ルーム退出時の処理
        /// </summary>
        private void OnRoomLeft()
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
                _networkService.OnRoomJoined -= OnRoomJoined;
                _networkService.OnRoomLeft -= OnRoomLeft;
            }

            DisableSync();

            Debug.Log("[GameSyncService] Service destroyed.");
        }

        #endregion
    }
}
