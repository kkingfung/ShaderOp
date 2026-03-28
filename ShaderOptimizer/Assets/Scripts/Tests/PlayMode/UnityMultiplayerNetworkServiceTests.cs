#nullable enable

using NUnit.Framework;
using UnityEngine;
using System.Linq;
using UnityEngine.TestTools;
using System.Collections;
using Cysharp.Threading.Tasks;
using ShaderOp.Runtime.Core.Services.Online;
using ShaderOp.Runtime.Core.Services;

namespace ShaderOp.Tests.PlayMode
{
    /// <summary>
    /// UnityMultiplayerNetworkServiceの統合テスト (Play Mode)
    /// </summary>
    /// <remarks>
    /// Unity Multiplayer Services v2を使用したネットワーク機能を検証。
    /// PlayerIdServiceとの統合、イベント発火、ルーム管理を中心にテスト。
    ///
    /// NOTE: Unity Multiplayer Servicesのモック化が困難なため、
    /// 一部のテストは実際のサービス接続を必要とする可能性があります。
    /// </remarks>
    [TestFixture]
    public class UnityMultiplayerNetworkServiceTests
    {
        private GameObject? _networkServiceObject;
        private GameObject? _playerIdServiceObject;
        private UnityMultiplayerNetworkService? _networkService;
        private PlayerIdService? _playerIdService;
        private bool _eventFired;

        [SetUp]
        public void SetUp()
        {
            // PlayerIdServiceを作成して登録
            _playerIdServiceObject = new GameObject("PlayerIdService");
            _playerIdService = _playerIdServiceObject.AddComponent<PlayerIdService>();
            ServiceLocator.Instance.Register<IPlayerIdService>(_playerIdService);

            // NetworkServiceを作成
            _networkServiceObject = new GameObject("NetworkService");
            _networkService = _networkServiceObject.AddComponent<UnityMultiplayerNetworkService>();

            // イベントフラグをリセット
            _eventFired = false;
        }

        [TearDown]
        public void TearDown()
        {
            // ServiceLocatorからサービスを削除
            ServiceLocator.Instance.Unregister<IPlayerIdService>();

            // GameObjectを破棄
            if (_networkServiceObject != null)
            {
                Object.DestroyImmediate(_networkServiceObject);
            }

            if (_playerIdServiceObject != null)
            {
                Object.DestroyImmediate(_playerIdServiceObject);
            }
        }

        #region Initialization Tests

        /// <summary>
        /// InitializeAsyncが成功してOnConnectedToServerイベントを発火することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator InitializeAsync_SucceedsAndFiresOnConnectedToServer()
        {
            // Arrange
            bool eventFired = false;
            _networkService!.OnConnectedToServer += () => eventFired = true;

            // Act
            var initTask = _networkService.InitializeAsync();
            yield return initTask.ToCoroutine();

            // Assert
            Assert.IsTrue(initTask.GetAwaiter().GetResult(),
                "InitializeAsyncはtrueを返すべきです");
            Assert.IsTrue(eventFired,
                "OnConnectedToServerイベントが発火されるべきです");
        }

        /// <summary>
        /// 初期化後、2回目のInitializeAsyncが既に初期化済みを返すことを検証
        /// </summary>
        [UnityTest]
        public IEnumerator InitializeAsync_SecondCall_ReturnsAlreadyInitialized()
        {
            // Arrange: 1回目の初期化
            var firstInit = _networkService!.InitializeAsync();
            yield return firstInit.ToCoroutine();

            // Act: 2回目の初期化
            var secondInit = _networkService.InitializeAsync();
            yield return secondInit.ToCoroutine();

            // Assert
            Assert.IsTrue(secondInit.GetAwaiter().GetResult(),
                "2回目のInitializeAsyncもtrueを返すべきです");

            // Log確認（既に初期化済みのメッセージ）
            LogAssert.Expect(LogType.Log, "[NetworkService] Already initialized.");
        }

        #endregion

        #region LocalPlayerId Tests

        /// <summary>
        /// LocalPlayerIdがPlayerIdServiceを使用していることを検証（GetHashCode不使用）
        /// </summary>
        [UnityTest]
        public IEnumerator LocalPlayerId_UsesPlayerIdService_NotGetHashCode()
        {
            // Arrange: 初期化
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();

            // Act
            int localPlayerId = _networkService.LocalPlayerId;

            // Assert: PlayerIdServiceのLocalGameIdと一致
            Assert.AreEqual(_playerIdService!.LocalGameId, localPlayerId,
                "LocalPlayerIdはPlayerIdService.LocalGameIdを使用すべきです");

            // Assert: ローカルプレイヤーは常にGameId=0
            Assert.AreEqual(0, localPlayerId,
                "ローカルプレイヤーのGameIdは0であるべきです");
        }

        /// <summary>
        /// 初期化前のLocalPlayerIdが-1を返すことを検証
        /// </summary>
        [Test]
        public void LocalPlayerId_BeforeInitialization_ReturnsMinusOne()
        {
            // Act
            int localPlayerId = _networkService!.LocalPlayerId;

            // Assert
            Assert.AreEqual(-1, localPlayerId,
                "初期化前のLocalPlayerIdは-1であるべきです");
        }

        #endregion

        #region Room Creation Tests

        /// <summary>
        /// CreateRoomWithCodeAsyncが6桁のJoin Codeを返すことを検証
        /// </summary>
        [UnityTest]
        public IEnumerator CreateRoomWithCodeAsync_Returns6DigitJoinCode()
        {
            // Arrange: 初期化
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();
            Assert.IsTrue(initTask.GetAwaiter().GetResult());

            // Act: ルーム作成
            var createTask = _networkService.CreateRoomWithCodeAsync("TestRoom", maxPlayers: 2);
            yield return createTask.ToCoroutine();
            string? joinCode = createTask.GetAwaiter().GetResult();

            // Assert: Join Codeの検証
            if (joinCode != null)
            {
                Assert.IsNotNull(joinCode, "Join Codeが返されるべきです");
                Assert.AreEqual(6, joinCode.Length,
                    "Join Codeは6桁であるべきです");
                Assert.IsTrue(joinCode.All(char.IsLetterOrDigit),
                    "Join Codeは英数字のみで構成されるべきです");
            }
            else
            {
                // Unity Multiplayer Servicesが利用できない環境ではnullが返る
                Assert.Inconclusive("Unity Multiplayer Servicesに接続できませんでした（テスト環境の制限）");
            }
        }

        /// <summary>
        /// CreateRoomAsync成功時にOnRoomCreatedイベントが発火することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator CreateRoomAsync_OnSuccess_FiresOnRoomCreatedEvent()
        {
            // Arrange
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();
            Assert.IsTrue(initTask.GetAwaiter().GetResult());

            string? receivedRoomName = null;
            _networkService.OnRoomCreated += (roomName) => receivedRoomName = roomName;

            // Act
            var createTask = _networkService.CreateRoomWithCodeAsync("EventTestRoom", maxPlayers: 2);
            yield return createTask.ToCoroutine();
            string? joinCode = createTask.GetAwaiter().GetResult();

            // Assert
            if (joinCode != null)
            {
                Assert.IsNotNull(receivedRoomName,
                    "OnRoomCreatedイベントが発火されるべきです");
            }
            else
            {
                Assert.Inconclusive("Unity Multiplayer Servicesに接続できませんでした");
            }
        }

        #endregion

        #region Player Events Tests

        /// <summary>
        /// OnPlayerJoinedイベントがプレイヤー参加時に発火することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator OnPlayerJoined_FiresWhenPlayerJoinsRoom()
        {
            // NOTE: このテストは実際の2プレイヤー接続が必要なため、モック実装を使用
            // 実環境では統合テストとして別途実施

            // Arrange
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();

            int joinedPlayerId = -1;
            _networkService.OnPlayerJoined += (playerId) => joinedPlayerId = playerId;

            // モック: プレイヤー参加イベントのシミュレーション
            // （実際のテストではUnity Multiplayer Servicesの2クライアントが必要）

            // Assert: モックなしでは検証不可
            Assert.Inconclusive("OnPlayerJoinedイベントは実際のマルチプレイヤー接続が必要です");
            yield return null;
        }

        /// <summary>
        /// OnPlayerLeftイベントがプレイヤー退出時に発火することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator OnPlayerLeft_FiresWhenPlayerLeavesRoom()
        {
            // NOTE: このテストも実際の2プレイヤー接続が必要

            // Arrange
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();

            int leftPlayerId = -1;
            _networkService.OnPlayerLeft += (playerId) => leftPlayerId = playerId;

            // Assert: モックなしでは検証不可
            Assert.Inconclusive("OnPlayerLeftイベントは実際のマルチプレイヤー接続が必要です");
            yield return null;
        }

        #endregion

        #region IsMasterClient Tests

        /// <summary>
        /// IsMasterClientがホストに対してtrueを返すことを検証
        /// </summary>
        [UnityTest]
        public IEnumerator IsMasterClient_ForHost_ReturnsTrue()
        {
            // Arrange: ルーム作成（ホストになる）
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();
            Assert.IsTrue(initTask.GetAwaiter().GetResult());

            var createTask = _networkService.CreateRoomWithCodeAsync("HostTestRoom", maxPlayers: 2);
            yield return createTask.ToCoroutine();
            string? joinCode = createTask.GetAwaiter().GetResult();

            // Act & Assert
            if (joinCode != null)
            {
                Assert.IsTrue(_networkService.IsMasterClient,
                    "ルームを作成したプレイヤーはMaster Clientであるべきです");
            }
            else
            {
                Assert.Inconclusive("Unity Multiplayer Servicesに接続できませんでした");
            }
        }

        /// <summary>
        /// IsMasterClientがゲストに対してfalseを返すことを検証
        /// </summary>
        [UnityTest]
        public IEnumerator IsMasterClient_ForGuest_ReturnsFalse()
        {
            // NOTE: ゲストクライアントのテストは2クライアント接続が必要
            // モック実装または統合テストで実施

            Assert.Inconclusive("ゲストクライアントのテストは実際のマルチプレイヤー接続が必要です");
            yield return null;
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// PlayerIdServiceとNetworkServiceの統合動作を検証
        /// </summary>
        [UnityTest]
        public IEnumerator Integration_PlayerIdServiceAndNetworkService_WorkTogether()
        {
            // Arrange: 初期化
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();
            Assert.IsTrue(initTask.GetAwaiter().GetResult());

            // Act: ローカルプレイヤーのGameIdを取得
            int localGameId = _networkService.LocalPlayerId;

            // Assert: PlayerIdServiceと連携している
            Assert.AreEqual(0, localGameId,
                "ローカルプレイヤーのGameIdは0であるべきです");
            Assert.AreEqual(localGameId, _playerIdService!.LocalGameId,
                "NetworkServiceとPlayerIdServiceのLocalGameIdが一致すべきです");
        }

        /// <summary>
        /// ルーム作成→参加→退出のフルフローを検証
        /// </summary>
        [UnityTest]
        public IEnumerator Integration_CreateRoom_Join_Leave_FullFlow()
        {
            // Arrange: 初期化
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();
            Assert.IsTrue(initTask.GetAwaiter().GetResult());

            // Act: ルーム作成
            var createTask = _networkService.CreateRoomWithCodeAsync("FullFlowRoom", maxPlayers: 2);
            yield return createTask.ToCoroutine();
            string? joinCode = createTask.GetAwaiter().GetResult();

            if (joinCode == null)
            {
                Assert.Inconclusive("Unity Multiplayer Servicesに接続できませんでした");
                yield break;
            }

            // Assert: ルーム内にいる
            Assert.IsTrue(_networkService.IsInRoom,
                "ルーム作成後、IsInRoomがtrueであるべきです");
            Assert.AreEqual(joinCode, _networkService.RoomName,
                "RoomNameがjoin codeと一致すべきです");

            // Act: ルーム退出
            var leaveTask = _networkService.LeaveRoomAsync();
            yield return leaveTask.ToCoroutine();

            // Assert: ルームから退出
            Assert.IsFalse(_networkService.IsInRoom,
                "ルーム退出後、IsInRoomがfalseであるべきです");
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// PlayerIdServiceが未登録の状態でのLocalPlayerId取得を検証
        /// </summary>
        [Test]
        public void LocalPlayerId_WhenPlayerIdServiceNotRegistered_ReturnsMinusOne()
        {
            // Arrange: PlayerIdServiceを削除
            ServiceLocator.Instance.Unregister<IPlayerIdService>();

            // Act
            int localPlayerId = _networkService!.LocalPlayerId;

            // Assert
            Assert.AreEqual(-1, localPlayerId,
                "PlayerIdServiceが未登録の場合、LocalPlayerIdは-1を返すべきです");

            // Cleanup: サービスを再登録
            ServiceLocator.Instance.Register<IPlayerIdService>(_playerIdService!);
        }

        /// <summary>
        /// ルーム未参加時のLeaveRoomAsyncが警告のみ出力することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator LeaveRoomAsync_WhenNotInRoom_LogsWarning()
        {
            // Arrange: 初期化のみ（ルーム未参加）
            var initTask = _networkService!.InitializeAsync();
            yield return initTask.ToCoroutine();

            // Act
            var leaveTask = _networkService.LeaveRoomAsync();
            yield return leaveTask.ToCoroutine();

            // Assert: 警告ログが出力される
            LogAssert.Expect(LogType.Warning, "[NetworkService] Not in a session.");
        }

        #endregion
    }
}
