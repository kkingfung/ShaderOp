#nullable enable

using NUnit.Framework;
using UnityEngine;
using ShaderOp.Runtime.Core.Services.Online;

namespace ShaderOp.Tests
{
    /// <summary>
    /// PlayerIdServiceの単体テスト (Edit Mode)
    /// </summary>
    /// <remarks>
    /// PlayerId（GUID文字列） ↔ GameId（整数）の双方向マッピング機能を検証。
    /// GetHashCode()依存の問題を回避するための重要なサービス。
    /// </remarks>
    [TestFixture]
    public class PlayerIdServiceTests
    {
        private GameObject? _serviceObject;
        private PlayerIdService? _service;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectとサービスを作成
            _serviceObject = new GameObject("PlayerIdServiceTest");
            _service = _serviceObject.AddComponent<PlayerIdService>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_serviceObject != null)
            {
                Object.DestroyImmediate(_serviceObject);
            }
        }

        #region RegisterLocalPlayer Tests

        /// <summary>
        /// LocalGameIdプロパティがRegisterLocalPlayer後に正しい値を返すことを検証
        /// </summary>
        [Test]
        public void LocalGameId_AfterRegisterLocalPlayer_ReturnsCorrectId()
        {
            // Arrange
            const string playerId = "local-player-guid-12345";
            const int expectedGameId = 0;

            // Act
            _service!.RegisterLocalPlayer(playerId, expectedGameId);

            // Assert
            Assert.AreEqual(expectedGameId, _service.LocalGameId,
                "LocalGameIdが登録したGameIdと一致しません");
        }

        /// <summary>
        /// RegisterLocalPlayer呼び出し前のLocalGameIdが-1を返すことを検証
        /// </summary>
        [Test]
        public void LocalGameId_BeforeRegisterLocalPlayer_ReturnsMinusOne()
        {
            // Act
            int localGameId = _service!.LocalGameId;

            // Assert
            Assert.AreEqual(-1, localGameId,
                "ローカルプレイヤー未登録時は-1を返すべきです");
        }

        #endregion

        #region GetGameId Tests

        /// <summary>
        /// 登録済みのPlayerIdに対してGetGameIdが正しいGameIdを返すことを検証
        /// </summary>
        [Test]
        public void GetGameId_ForRegisteredPlayerId_ReturnsCorrectGameId()
        {
            // Arrange
            const string playerId = "test-player-guid-001";
            const int expectedGameId = 1;
            _service!.RegisterPlayer(playerId, expectedGameId);

            // Act
            int actualGameId = _service.GetGameId(playerId);

            // Assert
            Assert.AreEqual(expectedGameId, actualGameId,
                "登録したPlayerIdに対応するGameIdが返されませんでした");
        }

        /// <summary>
        /// 未登録のPlayerIdに対してGetGameIdが-1を返すことを検証
        /// </summary>
        [Test]
        public void GetGameId_ForUnregisteredPlayerId_ReturnsMinusOne()
        {
            // Arrange
            const string unregisteredPlayerId = "non-existent-player-id";

            // Act
            int gameId = _service!.GetGameId(unregisteredPlayerId);

            // Assert
            Assert.AreEqual(-1, gameId,
                "未登録のPlayerIdに対しては-1を返すべきです");
        }

        #endregion

        #region GetPlayerId Tests

        /// <summary>
        /// 登録済みのGameIdに対してGetPlayerIdが正しいPlayerIdを返すことを検証
        /// </summary>
        [Test]
        public void GetPlayerId_ForRegisteredGameId_ReturnsCorrectPlayerId()
        {
            // Arrange
            const string expectedPlayerId = "test-player-guid-002";
            const int gameId = 2;
            _service!.RegisterPlayer(expectedPlayerId, gameId);

            // Act
            string? actualPlayerId = _service.GetPlayerId(gameId);

            // Assert
            Assert.AreEqual(expectedPlayerId, actualPlayerId,
                "登録したGameIdに対応するPlayerIdが返されませんでした");
        }

        /// <summary>
        /// 未登録のGameIdに対してGetPlayerIdがnullを返すことを検証
        /// </summary>
        [Test]
        public void GetPlayerId_ForUnregisteredGameId_ReturnsNull()
        {
            // Arrange
            const int unregisteredGameId = 999;

            // Act
            string? playerId = _service!.GetPlayerId(unregisteredGameId);

            // Assert
            Assert.IsNull(playerId,
                "未登録のGameIdに対してはnullを返すべきです");
        }

        #endregion

        #region GetNextGameId Tests

        /// <summary>
        /// GetNextGameIdが未使用の最小のGameIdを返すことを検証（空のマッピング）
        /// </summary>
        [Test]
        public void GetNextGameId_WithEmptyMapping_ReturnsZero()
        {
            // Act
            int nextGameId = _service!.GetNextGameId();

            // Assert
            Assert.AreEqual(0, nextGameId,
                "空のマッピングでは0を返すべきです");
        }

        /// <summary>
        /// GetNextGameIdが未使用の最小のGameIdを返すことを検証（連続ID）
        /// </summary>
        [Test]
        public void GetNextGameId_WithConsecutiveIds_ReturnsNextId()
        {
            // Arrange: 0, 1, 2を登録
            _service!.RegisterPlayer("player-0", 0);
            _service.RegisterPlayer("player-1", 1);
            _service.RegisterPlayer("player-2", 2);

            // Act
            int nextGameId = _service.GetNextGameId();

            // Assert
            Assert.AreEqual(3, nextGameId,
                "次に使用可能なGameIdは3であるべきです");
        }

        /// <summary>
        /// GetNextGameIdが未使用の最小のGameIdを返すことを検証（歯抜けID）
        /// </summary>
        [Test]
        public void GetNextGameId_WithGapInIds_FindsFirstAvailable()
        {
            // Arrange: 0, 2を登録（1が抜けている）
            _service!.RegisterPlayer("player-0", 0);
            _service.RegisterPlayer("player-2", 2);

            // Act
            int nextGameId = _service.GetNextGameId();

            // Assert
            Assert.AreEqual(1, nextGameId,
                "歯抜けIDがある場合、最小の未使用IDを返すべきです");
        }

        #endregion

        #region RegisterPlayer Tests

        /// <summary>
        /// RegisterPlayerが双方向マッピングを正しく登録することを検証
        /// </summary>
        [Test]
        public void RegisterPlayer_CreatesBidirectionalMapping()
        {
            // Arrange
            const string playerId = "bidirectional-test-player";
            const int gameId = 5;

            // Act
            _service!.RegisterPlayer(playerId, gameId);

            // Assert: PlayerId → GameId
            Assert.AreEqual(gameId, _service.GetGameId(playerId),
                "PlayerId → GameIdのマッピングが正しく登録されていません");

            // Assert: GameId → PlayerId
            Assert.AreEqual(playerId, _service.GetPlayerId(gameId),
                "GameId → PlayerIdのマッピングが正しく登録されていません");
        }

        /// <summary>
        /// RegisterPlayerが同じPlayerIdの再登録を正しく処理することを検証
        /// </summary>
        [Test]
        public void RegisterPlayer_WithSamePlayerId_UpdatesMapping()
        {
            // Arrange
            const string playerId = "re-register-player";
            _service!.RegisterPlayer(playerId, 1);

            // Act: 同じPlayerIdで異なるGameIdを再登録
            _service.RegisterPlayer(playerId, 2);

            // Assert: 新しいGameIdが返される
            Assert.AreEqual(2, _service.GetGameId(playerId),
                "再登録時、新しいGameIdに更新されるべきです");

            // Assert: 古いGameIdからPlayerIdが削除されている
            Assert.IsNull(_service.GetPlayerId(1),
                "再登録時、古いGameIdからマッピングが削除されるべきです");
        }

        /// <summary>
        /// RegisterPlayerが同じGameIdの再登録を正しく処理することを検証
        /// </summary>
        [Test]
        public void RegisterPlayer_WithSameGameId_UpdatesMapping()
        {
            // Arrange
            const int gameId = 3;
            _service!.RegisterPlayer("old-player", gameId);

            // Act: 同じGameIdで異なるPlayerIdを再登録
            const string newPlayerId = "new-player";
            _service.RegisterPlayer(newPlayerId, gameId);

            // Assert: 新しいPlayerIdが返される
            Assert.AreEqual(newPlayerId, _service.GetPlayerId(gameId),
                "再登録時、新しいPlayerIdに更新されるべきです");

            // Assert: 古いPlayerIdからGameIdが削除されている
            Assert.AreEqual(-1, _service.GetGameId("old-player"),
                "再登録時、古いPlayerIdからマッピングが削除されるべきです");
        }

        #endregion

        #region RemovePlayer Tests

        /// <summary>
        /// RemovePlayerが双方向マッピングを正しく削除することを検証
        /// </summary>
        [Test]
        public void RemovePlayer_DeletesBidirectionalMapping()
        {
            // Arrange
            const string playerId = "remove-test-player";
            const int gameId = 7;
            _service!.RegisterPlayer(playerId, gameId);

            // Act
            _service.RemovePlayer(playerId);

            // Assert: PlayerId → GameIdが削除されている
            Assert.AreEqual(-1, _service.GetGameId(playerId),
                "RemovePlayer後、PlayerId → GameIdのマッピングが削除されるべきです");

            // Assert: GameId → PlayerIdが削除されている
            Assert.IsNull(_service.GetPlayerId(gameId),
                "RemovePlayer後、GameId → PlayerIdのマッピングが削除されるべきです");
        }

        /// <summary>
        /// RemovePlayerが未登録のPlayerIdに対して警告のみ出力することを検証
        /// </summary>
        [Test]
        public void RemovePlayer_ForUnregisteredPlayerId_LogsWarning()
        {
            // Arrange
            const string unregisteredPlayerId = "never-registered-player";

            // Act & Assert: 例外をスローせず、警告ログのみ
            Assert.DoesNotThrow(() => _service!.RemovePlayer(unregisteredPlayerId),
                "未登録のPlayerIdの削除は例外をスローすべきではありません");

            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning,
                $"[PlayerIdService] Cannot remove player - PlayerId not found: {unregisteredPlayerId}");
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// 空の文字列PlayerIdの処理を検証
        /// </summary>
        [Test]
        public void RegisterPlayer_WithEmptyPlayerId_HandlesGracefully()
        {
            // Arrange
            const string emptyPlayerId = "";
            const int gameId = 10;

            // Act
            _service!.RegisterPlayer(emptyPlayerId, gameId);

            // Assert: 空文字列でも登録される（仕様として許容）
            Assert.AreEqual(gameId, _service.GetGameId(emptyPlayerId));
            Assert.AreEqual(emptyPlayerId, _service.GetPlayerId(gameId));
        }

        /// <summary>
        /// 負のGameIdの処理を検証
        /// </summary>
        [Test]
        public void RegisterPlayer_WithNegativeGameId_HandlesGracefully()
        {
            // Arrange
            const string playerId = "negative-gameid-player";
            const int negativeGameId = -5;

            // Act
            _service!.RegisterPlayer(playerId, negativeGameId);

            // Assert: 負のGameIdでも登録される（仕様として許容）
            Assert.AreEqual(negativeGameId, _service.GetGameId(playerId));
            Assert.AreEqual(playerId, _service.GetPlayerId(negativeGameId));
        }

        /// <summary>
        /// 大量のプレイヤー登録時のパフォーマンスを検証
        /// </summary>
        [Test]
        public void RegisterPlayer_WithManyPlayers_PerformanceTest()
        {
            // Arrange
            const int playerCount = 1000;

            // Act: 1000人のプレイヤーを登録
            for (int i = 0; i < playerCount; i++)
            {
                _service!.RegisterPlayer($"player-{i}", i);
            }

            // Assert: すべてのプレイヤーが正しく登録されている
            for (int i = 0; i < playerCount; i++)
            {
                Assert.AreEqual(i, _service!.GetGameId($"player-{i}"),
                    $"Player-{i}のGameIdマッピングが正しくありません");
                Assert.AreEqual($"player-{i}", _service.GetPlayerId(i),
                    $"GameId={i}のPlayerIdマッピングが正しくありません");
            }

            // Assert: GetNextGameIdが正しく動作
            Assert.AreEqual(playerCount, _service!.GetNextGameId());
        }

        #endregion
    }
}
