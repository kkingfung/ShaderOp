#nullable enable

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System;
using Cysharp.Threading.Tasks;
using ShaderOp.Runtime.Core.Services.Online;
using ShaderOp.Core.Services;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Tests.PlayMode
{
    /// <summary>
    /// UnityMultiplayerGameSyncServiceの統合テスト (Play Mode)
    /// </summary>
    /// <remarks>
    /// Wire Protocolを使用したゲーム同期機能を検証。
    /// バイナリシリアライゼーション、メッセージタイプ、ターン管理を中心にテスト。
    /// </remarks>
    [TestFixture]
    public class UnityMultiplayerGameSyncServiceTests
    {
        private GameObject? _gameSyncServiceObject;
        private GameObject? _networkServiceObject;
        private GameObject? _playerIdServiceObject;
        private UnityMultiplayerGameSyncService? _gameSyncService;
        private MockNetworkService? _mockNetworkService;
        private PlayerIdService? _playerIdService;

        [SetUp]
        public void SetUp()
        {
            // PlayerIdServiceを作成
            _playerIdServiceObject = new GameObject("PlayerIdService");
            _playerIdService = _playerIdServiceObject.AddComponent<PlayerIdService>();
            ServiceLocator.Instance.Register<IPlayerIdService>(_playerIdService);

            // MockNetworkServiceを作成
            _networkServiceObject = new GameObject("MockNetworkService");
            _mockNetworkService = _networkServiceObject.AddComponent<MockNetworkService>();
            ServiceLocator.Instance.Register<INetworkService>(_mockNetworkService);

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

        #region EnableSync Tests

        /// <summary>
        /// EnableSyncAsyncがルーム内で成功することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator EnableSyncAsync_WhenInRoom_ReturnsTrue()
        {
            // Arrange: ルームに参加した状態をモック
            _mockNetworkService!.SimulateJoinRoom("TestRoom");
            yield return null; // Start()実行待ち

            // Act
            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            // Assert
            Assert.IsTrue(enableTask.GetAwaiter().GetResult(),
                "ルーム内でEnableSyncAsyncはtrueを返すべきです");
            Assert.IsTrue(_gameSyncService.IsSyncEnabled,
                "IsSyncEnabledがtrueであるべきです");
        }

        /// <summary>
        /// EnableSyncAsyncがルーム外で失敗することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator EnableSyncAsync_WhenNotInRoom_ReturnsFalse()
        {
            // Arrange: ルーム未参加
            yield return null; // Start()実行待ち

            // Act
            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            // Assert
            Assert.IsFalse(enableTask.GetAwaiter().GetResult(),
                "ルーム外でEnableSyncAsyncはfalseを返すべきです");
            Assert.IsFalse(_gameSyncService.IsSyncEnabled,
                "IsSyncEnabledがfalseであるべきです");

            // Log確認
            LogAssert.Expect(LogType.Warning, "[GameSyncService] Not in a room. Cannot enable sync.");
        }

        #endregion

        #region Binary Serialization Tests

        /// <summary>
        /// SendMoveAsyncが16バイトに正しくシリアライズすることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator SendMoveAsync_SerializesTo16Bytes()
        {
            // Arrange: 同期を有効化
            _mockNetworkService!.SimulateJoinRoom("SerializationTestRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();
            Assert.IsTrue(enableTask.GetAwaiter().GetResult());

            // Act: 移動を送信
            var from = new HexCoordinate(1, 2);
            var to = new HexCoordinate(3, 4);
            var sendTask = _gameSyncService.SendMoveAsync(from, to);
            yield return sendTask.ToCoroutine();

            // Assert: MockNetworkServiceで送信されたデータを検証
            byte[]? sentData = _mockNetworkService.GetLastSentMessage();
            Assert.IsNotNull(sentData, "メッセージが送信されるべきです");
            Assert.AreEqual(16, sentData!.Length,
                "移動メッセージは16バイトであるべきです");

            // バイナリデータの検証
            int fromQ = BitConverter.ToInt32(sentData, 0);
            int fromR = BitConverter.ToInt32(sentData, 4);
            int toQ = BitConverter.ToInt32(sentData, 8);
            int toR = BitConverter.ToInt32(sentData, 12);

            Assert.AreEqual(1, fromQ, "fromQが正しくシリアライズされていません");
            Assert.AreEqual(2, fromR, "fromRが正しくシリアライズされていません");
            Assert.AreEqual(3, toQ, "toQが正しくシリアライズされていません");
            Assert.AreEqual(4, toR, "toRが正しくシリアライズされていません");
        }

        /// <summary>
        /// DeserializeMoveがHexCoordinateを正しく復元することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator DeserializeMove_ReconstructsHexCoordinateCorrectly()
        {
            // Arrange: 同期を有効化
            _mockNetworkService!.SimulateJoinRoom("DeserializeTestRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            // 受信イベントをセットアップ
            HexCoordinate? receivedFrom = null;
            HexCoordinate? receivedTo = null;
            _gameSyncService.OnMoveReceived += (from, to) =>
            {
                receivedFrom = from;
                receivedTo = to;
            };

            // Act: バイナリメッセージを受信シミュレート
            byte[] moveData = CreateMoveMessage(5, 6, 7, 8);
            _mockNetworkService.SimulateReceiveMessage(1, moveData); // MSG_MOVE = 1

            yield return null; // イベント処理待ち

            // Assert
            Assert.IsNotNull(receivedFrom, "OnMoveReceivedイベントが発火されるべきです");
            Assert.AreEqual(new HexCoordinate(5, 6), receivedFrom,
                "fromが正しくデシリアライズされていません");
            Assert.AreEqual(new HexCoordinate(7, 8), receivedTo,
                "toが正しくデシリアライズされていません");
        }

        #endregion

        #region Wire Protocol Message Types Tests

        /// <summary>
        /// MSG_GAME_START (2) メッセージの送受信を検証
        /// </summary>
        [UnityTest]
        public IEnumerator WireProtocol_MSG_GAME_START_SendsAndReceivesCorrectly()
        {
            // Arrange
            _mockNetworkService!.SimulateJoinRoom("GameStartRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            bool gameStarted = false;
            _gameSyncService.OnGameStarted += () => gameStarted = true;

            // Act: ゲーム開始送信
            var sendTask = _gameSyncService.SendGameStartAsync();
            yield return sendTask.ToCoroutine();

            // Assert: 送信成功
            Assert.IsTrue(sendTask.GetAwaiter().GetResult());

            // Act: 受信シミュレート
            _mockNetworkService.SimulateReceiveMessage(2, Array.Empty<byte>()); // MSG_GAME_START = 2
            yield return null;

            // Assert: イベント発火
            Assert.IsTrue(gameStarted, "OnGameStartedイベントが発火されるべきです");
        }

        /// <summary>
        /// MSG_GAME_END (3) メッセージの送受信を検証
        /// </summary>
        [UnityTest]
        public IEnumerator WireProtocol_MSG_GAME_END_SendsAndReceivesCorrectly()
        {
            // Arrange
            _mockNetworkService!.SimulateJoinRoom("GameEndRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            int? receivedWinnerId = null;
            _gameSyncService.OnGameEnded += (winnerId) => receivedWinnerId = winnerId;

            // Act: ゲーム終了送信
            var sendTask = _gameSyncService.SendGameEndAsync(winnerId: 1);
            yield return sendTask.ToCoroutine();

            // Assert: 送信成功
            Assert.IsTrue(sendTask.GetAwaiter().GetResult());

            // Act: 受信シミュレート
            byte[] winnerData = BitConverter.GetBytes(1);
            _mockNetworkService.SimulateReceiveMessage(3, winnerData); // MSG_GAME_END = 3
            yield return null;

            // Assert: 正しいwinnerIdを受信
            Assert.IsNotNull(receivedWinnerId);
            Assert.AreEqual(1, receivedWinnerId.Value,
                "正しいwinnerIdが受信されるべきです");
        }

        /// <summary>
        /// MSG_TURN_PASS (4) メッセージの送受信を検証
        /// </summary>
        [UnityTest]
        public IEnumerator WireProtocol_MSG_TURN_PASS_SendsAndReceivesCorrectly()
        {
            // Arrange
            _mockNetworkService!.SimulateJoinRoom("TurnPassRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            int? receivedNextPlayerId = null;
            _gameSyncService.OnTurnChanged += (nextPlayerId) => receivedNextPlayerId = nextPlayerId;

            // Act: ターンパス送信
            var sendTask = _gameSyncService.SendTurnPassAsync(nextPlayerId: 2);
            yield return sendTask.ToCoroutine();

            // Assert: 送信成功
            Assert.IsTrue(sendTask.GetAwaiter().GetResult());

            // Act: 受信シミュレート
            byte[] turnData = BitConverter.GetBytes(2);
            _mockNetworkService.SimulateReceiveMessage(4, turnData); // MSG_TURN_PASS = 4
            yield return null;

            // Assert: 正しいnextPlayerIdを受信
            Assert.IsNotNull(receivedNextPlayerId);
            Assert.AreEqual(2, receivedNextPlayerId.Value,
                "正しいnextPlayerIdが受信されるべきです");
        }

        /// <summary>
        /// MSG_RESET (5) メッセージの送受信を検証
        /// </summary>
        [UnityTest]
        public IEnumerator WireProtocol_MSG_RESET_SendsAndReceivesCorrectly()
        {
            // Arrange
            _mockNetworkService!.SimulateJoinRoom("ResetRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            bool gameReset = false;
            _gameSyncService.OnResetRequested += () => gameReset = true;

            // Act: リセット送信
            var sendTask = _gameSyncService.SendResetAsync();
            yield return sendTask.ToCoroutine();

            // Assert: 送信成功
            Assert.IsTrue(sendTask.GetAwaiter().GetResult());

            // Act: 受信シミュレート
            _mockNetworkService.SimulateReceiveMessage(5, Array.Empty<byte>()); // MSG_RESET = 5
            yield return null;

            // Assert: イベント発火
            Assert.IsTrue(gameReset, "OnGameResetイベントが発火されるべきです");
        }

        #endregion

        #region IsMyTurn Tests

        /// <summary>
        /// IsMyTurnプロパティのロジックを検証
        /// </summary>
        [UnityTest]
        public IEnumerator IsMyTurn_ReturnsTrue_WhenCurrentTurnIsLocalPlayer()
        {
            // Arrange: ローカルプレイヤーをGameId=0として登録
            _playerIdService!.RegisterLocalPlayer("local-player-id", 0);
            _mockNetworkService!.SimulateJoinRoom("TurnTestRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            // Act: ゲーム開始（ターンはGameId=0から始まる）
            var startTask = _gameSyncService.SendGameStartAsync();
            yield return startTask.ToCoroutine();

            // Assert: ローカルプレイヤーのターン
            Assert.IsTrue(_gameSyncService.IsMyTurn,
                "ゲーム開始時、GameId=0（ローカルプレイヤー）のターンであるべきです");
        }

        /// <summary>
        /// IsMyTurnがターン変更後に正しく更新されることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator IsMyTurn_UpdatesCorrectly_AfterTurnChange()
        {
            // Arrange
            _playerIdService!.RegisterLocalPlayer("local-player-id", 0);
            _mockNetworkService!.SimulateJoinRoom("TurnChangeRoom");
            yield return null;

            var enableTask = _gameSyncService!.EnableSyncAsync();
            yield return enableTask.ToCoroutine();

            // ゲーム開始（ターンはGameId=0）
            var startTask = _gameSyncService.SendGameStartAsync();
            yield return startTask.ToCoroutine();
            Assert.IsTrue(_gameSyncService.IsMyTurn);

            // Act: ターンをGameId=1に変更
            byte[] turnData = BitConverter.GetBytes(1);
            _mockNetworkService.SimulateReceiveMessage(4, turnData); // MSG_TURN_PASS
            yield return null;

            // Assert: もう自分のターンではない
            Assert.IsFalse(_gameSyncService.IsMyTurn,
                "ターン変更後、IsMyTurnはfalseになるべきです");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 移動メッセージのバイナリデータを作成
        /// </summary>
        private byte[] CreateMoveMessage(int fromQ, int fromR, int toQ, int toR)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(fromQ).CopyTo(bytes, 0);
            BitConverter.GetBytes(fromR).CopyTo(bytes, 4);
            BitConverter.GetBytes(toQ).CopyTo(bytes, 8);
            BitConverter.GetBytes(toR).CopyTo(bytes, 12);
            return bytes;
        }

        #endregion
    }

    #region Mock Network Service

    /// <summary>
    /// テスト用のモックネットワークサービス
    /// </summary>
    public class MockNetworkService : MonoBehaviour, INetworkService
    {
        private bool _isInRoom;
        private byte[]? _lastSentMessage;
        private Action<byte, ArraySegment<byte>>? _messageReceiver;

        public bool IsConnected => true;
        public bool IsInRoom => _isInRoom;
        public bool IsMasterClient => true;
        public int LocalPlayerId => 0;
        public int PlayerCount => 1;
        public string? RoomName { get; private set; }

        public event Action<bool>? OnConnectedChanged;
        public event Action? OnConnectedToServer;
        public event Action<string>? OnRoomJoined;
        public event Action? OnRoomLeft;
        public event Action<int>? OnPlayerJoined;
        public event Action<int>? OnPlayerLeft;

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

        public UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2)
        {
            _isInRoom = true;
            RoomName = roomName;
            return UniTask.FromResult(true);
        }

        public UniTask<string?> CreateRoomWithCodeAsync(string roomName, int maxPlayers = 2)
        {
            _isInRoom = true;
            RoomName = roomName;
            return UniTask.FromResult<string?>("123456");
        }

        public UniTask<bool> JoinRoomAsync(string joinCode)
        {
            _isInRoom = true;
            RoomName = joinCode;
            return UniTask.FromResult(true);
        }

        public UniTask<bool> JoinRandomRoomAsync()
        {
            _isInRoom = true;
            RoomName = "RandomRoom";
            return UniTask.FromResult(true);
        }

        public UniTask LeaveRoomAsync()
        {
            _isInRoom = false;
            RoomName = null;
            return UniTask.CompletedTask;
        }

        // テストヘルパー
        public void SimulateJoinRoom(string roomName)
        {
            _isInRoom = true;
            RoomName = roomName;
            OnRoomJoined?.Invoke(roomName);
        }

        public void SimulateReceiveMessage(byte messageCode, byte[] payload)
        {
            _messageReceiver?.Invoke(messageCode, new ArraySegment<byte>(payload));
        }

        public byte[]? GetLastSentMessage() => _lastSentMessage;

        public void RegisterMessageReceiver(Action<byte, ArraySegment<byte>> receiver)
        {
            _messageReceiver = receiver;
        }
    }

    #endregion
}
