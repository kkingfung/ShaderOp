#nullable enable

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using ShaderOp.Runtime.Core.Services.Online;
using ShaderOp.Core.Services;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Tests.PlayMode
{
    /// <summary>
    /// オンラインサービス統合テスト (Play Mode)
    /// </summary>
    /// <remarks>
    /// PlayerIdService、NetworkService、GameSyncServiceの連携を検証。
    /// 実際のゲームフローに近いシナリオでテスト。
    /// </remarks>
    [TestFixture]
    public class OnlineServicesIntegrationTests
    {
        private GameObject? _playerIdServiceObject;
        private GameObject? _networkServiceObject;
        private GameObject? _gameSyncServiceObject;
        private PlayerIdService? _playerIdService;
        private MockNetworkServiceForIntegration? _networkService;
        private UnityMultiplayerGameSyncService? _gameSyncService;

        [SetUp]
        public void SetUp()
        {
            // PlayerIdServiceを作成
            _playerIdServiceObject = new GameObject("PlayerIdService");
            _playerIdService = _playerIdServiceObject.AddComponent<PlayerIdService>();
            ServiceLocator.Instance.Register<IPlayerIdService>(_playerIdService);

            // MockNetworkServiceを作成
            _networkServiceObject = new GameObject("NetworkService");
            _networkService = _networkServiceObject.AddComponent<MockNetworkServiceForIntegration>();
            ServiceLocator.Instance.Register<INetworkService>(_networkService);

            // GameSyncServiceを作成
            _gameSyncServiceObject = new GameObject("GameSyncService");
            _gameSyncService = _gameSyncServiceObject.AddComponent<UnityMultiplayerGameSyncService>();
        }

        [TearDown]
        public void TearDown()
        {
            // ServiceLocatorからサービスを削除
            ServiceLocator.Instance.Unregister<IPlayerIdService>();
            ServiceLocator.Instance.Unregister<INetworkService>();

            // GameObjectを破棄
            if (_gameSyncServiceObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameSyncServiceObject);
            }

            if (_networkServiceObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_networkServiceObject);
            }

            if (_playerIdServiceObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_playerIdServiceObject);
            }
        }

        #region GameBootstrap Service Registration Test

        /// <summary>
        /// GameBootstrapでのサービス登録フローを検証
        /// </summary>
        [UnityTest]
        public IEnumerator GameBootstrap_ServiceRegistration_AllServicesAvailable()
        {
            // Act: ServiceLocatorからサービスを取得
            var playerIdService = ServiceLocator.Instance.Get<IPlayerIdService>();
            var networkService = ServiceLocator.Instance.Get<INetworkService>();

            yield return null;

            // Assert: すべてのサービスが登録されている
            Assert.IsNotNull(playerIdService,
                "IPlayerIdServiceがServiceLocatorに登録されているべきです");
            Assert.IsNotNull(networkService,
                "INetworkServiceがServiceLocatorに登録されているべきです");

            // Assert: サービスの型が正しい
            Assert.IsInstanceOf<PlayerIdService>(playerIdService,
                "PlayerIdServiceの実装型が正しくありません");
            Assert.IsInstanceOf<INetworkService>(networkService,
                "NetworkServiceがINetworkServiceを実装しているべきです");
        }

        #endregion

        #region PlayerIdService + NetworkService Integration

        /// <summary>
        /// PlayerIdServiceとNetworkServiceの連携動作を検証
        /// </summary>
        [UnityTest]
        public IEnumerator Integration_PlayerIdAndNetwork_LocalPlayerMapping()
        {
            // Arrange: NetworkServiceを初期化（ローカルプレイヤー登録）
            _networkService!.SimulateInitialize("local-player-guid");
            yield return null;

            // Act: NetworkServiceのLocalPlayerIdを取得
            int localPlayerId = _networkService.LocalPlayerId;

            // Assert: PlayerIdServiceと連携している
            Assert.AreEqual(0, localPlayerId,
                "ローカルプレイヤーのGameIdは0であるべきです");

            // Assert: PlayerIdServiceで逆引き可能
            string? playerId = _playerIdService!.GetPlayerId(0);
            Assert.AreEqual("local-player-guid", playerId,
                "PlayerIdServiceでGameId=0からPlayerIdを取得できるべきです");
        }

        /// <summary>
        /// リモートプレイヤー参加時のPlayerIdService連携を検証
        /// </summary>
        [UnityTest]
        public IEnumerator Integration_RemotePlayerJoin_AutoRegistersInPlayerIdService()
        {
            // Arrange: ローカルプレイヤーを初期化
            _networkService!.SimulateInitialize("local-player-guid");
            yield return null;

            // Act: リモートプレイヤーが参加
            _networkService.SimulatePlayerJoin("remote-player-guid-1");
            yield return null;

            // Assert: PlayerIdServiceに自動登録される
            int remoteGameId = _playerIdService!.GetGameId("remote-player-guid-1");
            Assert.AreNotEqual(-1, remoteGameId,
                "リモートプレイヤーがPlayerIdServiceに登録されるべきです");

            // Assert: GameId=1が割り当てられる（ローカルが0なので次は1）
            Assert.AreEqual(1, remoteGameId,
                "リモートプレイヤーはGameId=1を割り当てられるべきです");
        }

        /// <summary>
        /// プレイヤー退出時のPlayerIdServiceクリーンアップを検証
        /// </summary>
        [UnityTest]
        public IEnumerator Integration_PlayerLeave_RemovesFromPlayerIdService()
        {
            // Arrange: リモートプレイヤーを参加させる
            _networkService!.SimulateInitialize("local-player-guid");
            _networkService.SimulatePlayerJoin("remote-player-guid-1");
            yield return null;

            // 参加確認
            int remoteGameId = _playerIdService!.GetGameId("remote-player-guid-1");
            Assert.AreEqual(1, remoteGameId);

            // Act: リモートプレイヤーが退出
            _networkService.SimulatePlayerLeave("remote-player-guid-1");
            yield return null;

            // Assert: PlayerIdServiceから削除される
            int gameIdAfterLeave = _playerIdService.GetGameId("remote-player-guid-1");
            Assert.AreEqual(-1, gameIdAfterLeave,
                "プレイヤー退出後、PlayerIdServiceからマッピングが削除されるべきです");
        }

        #endregion

        #region End-to-End: Create Room → Join Room → Sync Enabled

        /// <summary>
        /// エンドツーエンド: ルーム作成→参加→同期有効化の完全フロー
        /// </summary>
        [UnityTest]
        public IEnumerator EndToEnd_CreateRoom_JoinRoom_EnableSync_FullFlow()
        {
            // Step 1: ローカルプレイヤー初期化
            _networkService!.SimulateInitialize("local-player-guid");
            yield return null;

            // Assert: ローカルプレイヤーが登録されている
            Assert.AreEqual(0, _playerIdService!.LocalGameId);

            // Step 2: ルーム作成
            _networkService.SimulateJoinRoom("test-room-001");
            yield return null;

            // Assert: ルーム内にいる
            Assert.IsTrue(_networkService.IsInRoom,
                "ルーム作成後、IsInRoomがtrueであるべきです");

            // Step 3: GameSyncServiceの同期を有効化
            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            // Assert: 同期が有効化される
            Assert.IsTrue(enableTask.GetAwaiter().GetResult(),
                "ルーム内で同期が有効化されるべきです");
            Assert.IsTrue(_gameSyncService.IsSyncEnabled,
                "IsSyncEnabledがtrueであるべきです");

            // Step 4: リモートプレイヤーが参加
            _networkService.SimulatePlayerJoin("remote-player-guid-1");
            yield return null;

            // Assert: PlayerIdServiceに登録される
            int remoteGameId = _playerIdService.GetGameId("remote-player-guid-1");
            Assert.AreEqual(1, remoteGameId,
                "リモートプレイヤーがGameId=1で登録されるべきです");

            // Step 5: ゲーム開始
            var startTask = _gameSyncService.SendGameStartAsync();
            yield return startTask.ToCoroutine();

            // Assert: ゲーム開始成功
            Assert.IsTrue(startTask.GetAwaiter().GetResult());

            // Step 6: 移動を送信
            var from = new HexCoordinate(0, 0);
            var to = new HexCoordinate(1, 1);
            var moveTask = _gameSyncService.SendMoveAsync(from, to);
            yield return moveTask.ToCoroutine();

            // Assert: 移動送信成功
            Assert.IsTrue(moveTask.GetAwaiter().GetResult());

            // Step 7: ルーム退出
            _networkService.SimulateLeaveRoom();
            yield return null;

            // Assert: 同期が無効化される
            Assert.IsFalse(_gameSyncService.IsSyncEnabled,
                "ルーム退出後、同期が無効化されるべきです");
        }

        /// <summary>
        /// エンドツーエンド: 2プレイヤーでのターン制ゲームフロー
        /// </summary>
        [UnityTest]
        public IEnumerator EndToEnd_TwoPlayerTurnBasedGame_FullFlow()
        {
            // Setup: ホストプレイヤー（GameId=0）
            _networkService!.SimulateInitialize("host-player-guid");
            _networkService.SimulateJoinRoom("turn-based-room");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();
            Assert.IsTrue(enableTask.GetAwaiter().GetResult());

            // Setup: ゲストプレイヤー参加（GameId=1）
            _networkService.SimulatePlayerJoin("guest-player-guid");
            yield return null;

            // Assert: 両プレイヤーが登録されている
            Assert.AreEqual(0, _playerIdService!.GetGameId("host-player-guid"));
            Assert.AreEqual(1, _playerIdService.GetGameId("guest-player-guid"));

            // ゲーム開始（ホストのターン）
            var startTask = _gameSyncService.SendGameStartAsync();
            yield return startTask.ToCoroutine();

            // Assert: ホストのターン
            Assert.IsTrue(_gameSyncService.IsMyTurn,
                "ゲーム開始時、ホスト（GameId=0）のターンであるべきです");

            // ホストが移動
            var move1Task = _gameSyncService.SendMoveAsync(
                new HexCoordinate(0, 0),
                new HexCoordinate(1, 0)
            );
            yield return move1Task.ToCoroutine();
            Assert.IsTrue(move1Task.GetAwaiter().GetResult());

            // ターンパス（ゲストのターンへ）
            var turnPassTask = _gameSyncService.SendTurnPassAsync(nextPlayerId: 1);
            yield return turnPassTask.ToCoroutine();
            Assert.IsTrue(turnPassTask.GetAwaiter().GetResult());

            // Assert: もうホストのターンではない
            Assert.IsFalse(_gameSyncService.IsMyTurn,
                "ターンパス後、ホストのターンではないべきです");

            // ゲーム終了（ホストが勝利）
            var endTask = _gameSyncService.SendGameEndAsync(winnerId: 0);
            yield return endTask.ToCoroutine();
            Assert.IsTrue(endTask.GetAwaiter().GetResult());
        }

        #endregion

        #region Service Lifecycle Tests

        /// <summary>
        /// サービスのライフサイクル（初期化→使用→破棄）を検証
        /// </summary>
        [UnityTest]
        public IEnumerator ServiceLifecycle_Initialize_Use_Destroy()
        {
            // Initialize
            _networkService!.SimulateInitialize("lifecycle-test-player");
            _networkService.SimulateJoinRoom("lifecycle-room");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            // Use
            var startTask = _gameSyncService.SendGameStartAsync();
            yield return startTask.ToCoroutine();
            Assert.IsTrue(startTask.GetAwaiter().GetResult());

            // Destroy
            UnityEngine.Object.DestroyImmediate(_gameSyncServiceObject);
            UnityEngine.Object.DestroyImmediate(_networkServiceObject);
            UnityEngine.Object.DestroyImmediate(_playerIdServiceObject);

            yield return null;

            // Assert: クリーンアップ成功（例外なし）
            Assert.Pass("サービスのライフサイクルが正常に完了しました");
        }

        #endregion
    }

    #region Mock Network Service for Integration

    /// <summary>
    /// 統合テスト用のモックネットワークサービス
    /// </summary>
    public class MockNetworkServiceForIntegration : MonoBehaviour, INetworkService
    {
        private bool _isInRoom;
        private string? _localPlayerId;
        private IPlayerIdService? _playerIdService;

        public bool IsConnected => true;
        public bool IsInRoom => _isInRoom;
        public bool IsMasterClient => true;
        public int LocalPlayerId => _playerIdService?.LocalGameId ?? -1;
        public int PlayerCount => 1;
        public string? RoomName { get; private set; }

        public event Action<bool>? OnConnectedChanged;
        public event Action? OnConnectedToServer;
        public event Action<string>? OnRoomJoined;
        public event Action? OnRoomLeft;
        public event Action<int>? OnPlayerJoined;
        public event Action<int>? OnPlayerLeft;

        private void Start()
        {
            _playerIdService = ServiceLocator.Instance.Get<IPlayerIdService>();
        }

        public UniTask<bool> InitializeAsync() => UniTask.FromResult(true);

        public UniTask<bool> ConnectToServerAsync()
        {
            OnConnectedToServer?.Invoke();
            OnConnectedChanged?.Invoke(true);
            return UniTask.FromResult(true);
        }

        public UniTask DisconnectAsync()
        {
            OnConnectedChanged?.Invoke(false);
            return UniTask.CompletedTask;
        }

        public void RegisterMessageReceiver(Action<byte, ArraySegment<byte>> receiver)
        {
            // Mock implementation - do nothing
        }

        public UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2)
        {
            _isInRoom = true;
            RoomName = roomName;
            OnRoomJoined?.Invoke(roomName);
            return UniTask.FromResult(true);
        }

        public UniTask<string?> CreateRoomWithCodeAsync(string roomName, int maxPlayers = 2)
        {
            _isInRoom = true;
            RoomName = roomName;
            OnRoomJoined?.Invoke(roomName);
            return UniTask.FromResult<string?>("123456");
        }

        public UniTask<bool> JoinRoomAsync(string joinCode)
        {
            _isInRoom = true;
            RoomName = joinCode;
            OnRoomJoined?.Invoke(joinCode);
            return UniTask.FromResult(true);
        }

        public UniTask<bool> JoinRandomRoomAsync()
        {
            _isInRoom = true;
            RoomName = "RandomRoom";
            OnRoomJoined?.Invoke("RandomRoom");
            return UniTask.FromResult(true);
        }

        public UniTask LeaveRoomAsync()
        {
            _isInRoom = false;
            RoomName = null;
            OnRoomLeft?.Invoke();
            return UniTask.CompletedTask;
        }

        // テストヘルパー
        public void SimulateInitialize(string localPlayerId)
        {
            _localPlayerId = localPlayerId;
            _playerIdService?.RegisterLocalPlayer(localPlayerId, gameId: 0);
            OnConnectedToServer?.Invoke();
        }

        public void SimulateJoinRoom(string roomName)
        {
            _isInRoom = true;
            RoomName = roomName;
            OnRoomJoined?.Invoke(roomName);
        }

        public void SimulateLeaveRoom()
        {
            _isInRoom = false;
            RoomName = null;
            OnRoomLeft?.Invoke();
        }

        public void SimulatePlayerJoin(string playerId)
        {
            int gameId = _playerIdService?.GetNextGameId() ?? -1;
            _playerIdService?.RegisterPlayer(playerId, gameId);
            OnPlayerJoined?.Invoke(gameId);
        }

        public void SimulatePlayerLeave(string playerId)
        {
            int gameId = _playerIdService?.GetGameId(playerId) ?? -1;
            _playerIdService?.RemovePlayer(playerId);
            OnPlayerLeft?.Invoke(gameId);
        }
    }

    #endregion
}
