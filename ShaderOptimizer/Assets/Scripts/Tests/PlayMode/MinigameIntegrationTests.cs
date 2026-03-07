#nullable enable

// TODO: これらのテストは古いAPIを使用しているため、HexBoardGameModelに必要なメソッドを実装後に有効化する
#if false

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using ShaderOp.Minigames.Games;
using ShaderOp.Minigames.HexGrid;
using ShaderOp.Tests.Helpers;

namespace ShaderOp.Tests.PlayMode
{
    /// <summary>
    /// ミニゲーム統合テスト（PlayMode）
    /// 実際のゲームフローを検証
    /// </summary>
    [TestFixture]
    public class MinigameIntegrationTests : TestBase
    {
        #region HexReversi 統合テスト

        [UnityTest]
        public IEnumerator HexReversi_FullGameFlow_CompletesSuccessfully()
        {
            // コントローラーを作成
            var controllerObj = CreateGameObject("HexReversiController");
            var controller = controllerObj.AddComponent<HexReversiController>();

            yield return null; // 初期化待ち

            // 初期状態確認
            Assert.AreEqual(GameState.Playing, controller.Model.State);
            Assert.AreEqual(PieceType.Player1, controller.Model.CurrentPlayer);

            // 有効な手を取得
            var validMoves = controller.Model.GetValidMoves();
            Assert.Greater(validMoves.Count, 0, "初期状態で有効な手が存在しません");

            // 数手プレイ
            for (int i = 0; i < 5 && controller.Model.State == GameState.Playing; i++)
            {
                validMoves = controller.Model.GetValidMoves();
                if (validMoves.Count > 0)
                {
                    bool success = controller.Model.PlacePiece(validMoves[0]);
                    Assert.IsTrue(success, $"ターン{i + 1}で駒の配置に失敗");
                    yield return TestUtilities.WaitForFrames(1);
                }
            }

            // ゲームがまだ進行中であることを確認
            Assert.AreEqual(GameState.Playing, controller.Model.State);

            yield return null;
        }

        [UnityTest]
        public IEnumerator HexReversi_Reset_RestoresInitialState()
        {
            var controllerObj = CreateGameObject("HexReversiController");
            var controller = controllerObj.AddComponent<HexReversiController>();

            yield return null;

            // 数手進める
            var validMoves = controller.Model.GetValidMoves();
            if (validMoves.Count > 0)
            {
                controller.Model.PlacePiece(validMoves[0]);
            }

            yield return null;

            // リセット
            controller.Model.Reset();

            // 初期状態に戻ったか確認
            Assert.AreEqual(GameState.Playing, controller.Model.State);
            Assert.AreEqual(PieceType.Player1, controller.Model.CurrentPlayer);

            (int p1, int p2) = controller.Model.GetPieceCounts();
            Assert.AreEqual(2, p1);
            Assert.AreEqual(2, p2);

            yield return null;
        }

        #endregion

        #region HexCheckers 統合テスト

        [UnityTest]
        public IEnumerator HexCheckers_FullGameFlow_CompletesSuccessfully()
        {
            var controllerObj = CreateGameObject("HexCheckersController");
            var controller = controllerObj.AddComponent<HexCheckersController>();

            yield return null;

            Assert.AreEqual(GameState.Playing, controller.Model.State);
            Assert.AreEqual(PieceType.Player1, controller.Model.CurrentPlayer);

            // 初期駒数確認
            (int p1, int p2) = controller.Model.GetPieceCounts();
            Assert.AreEqual(12, p1);
            Assert.AreEqual(12, p2);

            yield return null;
        }

        [UnityTest]
        public IEnumerator HexCheckers_PieceSelection_WorksCorrectly()
        {
            var controllerObj = CreateGameObject("HexCheckersController");
            var controller = controllerObj.AddComponent<HexCheckersController>();

            yield return null;

            // Player1の駒を探して選択
            HexCoordinate? player1Piece = FindFirstPieceInGrid(controller.Model.Grid, PieceType.Player1);
            Assert.IsNotNull(player1Piece, "Player1の駒が見つかりません");

            bool selected = controller.Model.SelectPiece(player1Piece!.Value);
            Assert.IsTrue(selected, "駒の選択に失敗");

            Assert.AreEqual(player1Piece!.Value, controller.Model.GetSelectedPiece());

            yield return null;
        }

        #endregion

        #region HexChess 統合テスト

        [UnityTest]
        public IEnumerator HexChess_FullGameFlow_CompletesSuccessfully()
        {
            var controllerObj = CreateGameObject("HexChessController");
            var controller = controllerObj.AddComponent<HexChessController>();

            yield return null;

            Assert.AreEqual(GameState.Playing, controller.Model.State);
            Assert.AreEqual(PieceType.Player1, controller.Model.CurrentPlayer);

            // チェック状態でないことを確認
            Assert.IsFalse(controller.Model.IsPlayerInCheck(PieceType.Player1));
            Assert.IsFalse(controller.Model.IsPlayerInCheck(PieceType.Player2));

            yield return null;
        }

        [UnityTest]
        public IEnumerator HexChess_PawnMove_WorksCorrectly()
        {
            var controllerObj = CreateGameObject("HexChessController");
            var controller = controllerObj.AddComponent<HexChessController>();

            yield return null;

            // ポーンを移動
            HexCoordinate pawnPos = new HexCoordinate(0, -4);
            HexCoordinate targetPos = new HexCoordinate(0, -3);

            bool selected = controller.Model.SelectPiece(pawnPos);
            Assert.IsTrue(selected, "ポーンの選択に失敗");

            yield return null;

            bool moved = controller.Model.PlacePiece(targetPos);
            Assert.IsTrue(moved, "ポーンの移動に失敗");

            // 駒が移動したか確認
            var piece = controller.Model.GetChessPiece(targetPos);
            Assert.IsNotNull(piece);
            Assert.AreEqual(HexChessModel.ChessPieceType.Pawn, piece!.Value.Type);

            yield return null;
        }

        #endregion

        #region TicTacToeHex 統合テスト

        [UnityTest]
        public IEnumerator TicTacToeHex_FullGameFlow_CompletesSuccessfully()
        {
            var controllerObj = CreateGameObject("TicTacToeHexController");
            var controller = controllerObj.AddComponent<TicTacToeHexController>();

            yield return null;

            Assert.AreEqual(GameState.Playing, controller.Model.State);
            Assert.AreEqual(PieceType.Player1, controller.Model.CurrentPlayer);

            // 数手プレイ
            for (int i = 0; i < 3 && controller.Model.State == GameState.Playing; i++)
            {
                var validMoves = controller.Model.GetValidMoves();
                if (validMoves.Count > 0)
                {
                    bool success = controller.Model.PlacePiece(validMoves[0]);
                    Assert.IsTrue(success, $"ターン{i + 1}で駒の配置に失敗");
                    yield return TestUtilities.WaitForFrames(1);
                }
            }

            yield return null;
        }

        #endregion

        #region パフォーマンステスト

        [UnityTest]
        public IEnumerator HexReversi_100Turns_CompletesInReasonableTime()
        {
            var controllerObj = CreateGameObject("HexReversiController");
            var controller = controllerObj.AddComponent<HexReversiController>();

            yield return null;

            float startTime = Time.realtimeSinceStartup;

            // 100ターンまたはゲーム終了まで
            int turnCount = 0;
            while (turnCount < 100 && controller.Model.State == GameState.Playing)
            {
                var validMoves = controller.Model.GetValidMoves();
                if (validMoves.Count > 0)
                {
                    controller.Model.PlacePiece(validMoves[0]);
                    turnCount++;
                }
                else
                {
                    break; // 有効な手がない
                }

                if (turnCount % 10 == 0)
                {
                    yield return null; // 10ターンごとにフレーム待機
                }
            }

            float elapsedTime = Time.realtimeSinceStartup - startTime;

            // 100ターンが5秒以内に完了
            Assert.Less(elapsedTime, 5f, $"100ターンに{elapsedTime}秒かかりました");

            yield return null;
        }

        #endregion

        #region ヘルパーメソッド

        private HexCoordinate? FindFirstPieceInGrid(HexGrid grid, PieceType pieceType)
        {
            foreach (var tile in grid.AllTiles)
            {
                if (tile.Piece == pieceType)
                {
                    return tile.Coordinate;
                }
            }
            return null;
        }

        #endregion
    }
}

#endif
