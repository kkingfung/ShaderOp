#nullable enable

using NUnit.Framework;
using ShaderOp.Minigames.Games;
using ShaderOp.Minigames.HexGrid;
using System.Collections.Generic;

namespace ShaderOp.Tests
{
    /// <summary>
    /// Hex Chessゲームロジックのテスト
    /// </summary>
    [TestFixture]
    public class HexChessTests
    {
        private HexChessModel? _model;

        [SetUp]
        public void Setup()
        {
            _model = new HexChessModel();
            _model.Initialize();
        }

        [TearDown]
        public void Teardown()
        {
            _model = null;
        }

        #region 初期化テスト

        [Test]
        public void Initialize_CreatesHexagonGrid()
        {
            Assert.IsNotNull(_model);
            Assert.IsNotNull(_model!.Grid);
            Assert.AreEqual(GridShape.Hexagon, _model.Grid.Shape);
        }

        [Test]
        public void Initialize_PlacesInitialPieces()
        {
            Assert.IsNotNull(_model);

            // Player1のキングが存在
            HexChessModel.ChessPiece? player1King = _model!.GetChessPiece(new HexCoordinate(0, -5));
            Assert.IsNotNull(player1King);
            Assert.AreEqual(PieceType.Player1, player1King!.Value.Player);
            Assert.AreEqual(HexChessModel.ChessPieceType.King, player1King.Value.Type);

            // Player2のキングが存在
            HexChessModel.ChessPiece? player2King = _model.GetChessPiece(new HexCoordinate(0, 5));
            Assert.IsNotNull(player2King);
            Assert.AreEqual(PieceType.Player2, player2King!.Value.Player);
            Assert.AreEqual(HexChessModel.ChessPieceType.King, player2King.Value.Type);
        }

        [Test]
        public void Initialize_StartWithPlayer1()
        {
            Assert.IsNotNull(_model);
            Assert.AreEqual(PieceType.Player1, _model!.CurrentPlayer);
        }

        [Test]
        public void Initialize_GameStateIsPlaying()
        {
            Assert.IsNotNull(_model);
            Assert.AreEqual(GameState.Playing, _model!.State);
        }

        [Test]
        public void Initialize_NoCheckAtStart()
        {
            Assert.IsNotNull(_model);
            Assert.IsFalse(_model!.IsPlayerInCheck(PieceType.Player1));
            Assert.IsFalse(_model.IsPlayerInCheck(PieceType.Player2));
        }

        #endregion

        #region 駒選択テスト

        [Test]
        public void SelectPiece_ValidPiece_ReturnsTrue()
        {
            Assert.IsNotNull(_model);

            // Player1のポーンを選択
            HexCoordinate pawnPos = new HexCoordinate(0, -4);
            bool success = _model!.SelectPiece(pawnPos);

            Assert.IsTrue(success);
            Assert.AreEqual(pawnPos, _model.GetSelectedPiece());
        }

        [Test]
        public void SelectPiece_OpponentPiece_ReturnsFalse()
        {
            Assert.IsNotNull(_model);

            // Player2の駒を選択（Player1のターン）
            HexCoordinate player2Piece = new HexCoordinate(0, 4);
            bool success = _model!.SelectPiece(player2Piece);

            Assert.IsFalse(success);
            Assert.IsNull(_model.GetSelectedPiece());
        }

        [Test]
        public void SelectPiece_EmptyTile_ReturnsFalse()
        {
            Assert.IsNotNull(_model);

            // 空のタイルを選択
            HexCoordinate emptyTile = new HexCoordinate(0, 0);
            bool success = _model!.SelectPiece(emptyTile);

            Assert.IsFalse(success);
        }

        [Test]
        public void DeselectPiece_ClearsSelection()
        {
            Assert.IsNotNull(_model);

            HexCoordinate piece = new HexCoordinate(0, -4);
            _model!.SelectPiece(piece);
            Assert.IsNotNull(_model.GetSelectedPiece());

            _model.DeselectPiece();
            Assert.IsNull(_model.GetSelectedPiece());
        }

        #endregion

        #region ポーン移動テスト

        [Test]
        public void PlacePiece_PawnForwardMove_Success()
        {
            Assert.IsNotNull(_model);

            // ポーンを1マス前進
            HexCoordinate from = new HexCoordinate(0, -4);
            HexCoordinate to = new HexCoordinate(0, -3);

            _model!.SelectPiece(from);
            bool success = _model.PlacePiece(to);

            Assert.IsTrue(success);

            // 駒が移動している
            HexChessModel.ChessPiece? movedPiece = _model.GetChessPiece(to);
            Assert.IsNotNull(movedPiece);
            Assert.AreEqual(HexChessModel.ChessPieceType.Pawn, movedPiece!.Value.Type);
        }

        [Test]
        public void PlacePiece_PawnDoubleMove_Success()
        {
            Assert.IsNotNull(_model);

            // 初回は2マス前進可能
            HexCoordinate from = new HexCoordinate(0, -4);
            HexCoordinate to = new HexCoordinate(0, -2);

            _model!.SelectPiece(from);
            bool success = _model.PlacePiece(to);

            Assert.IsTrue(success);
        }

        [Test]
        public void PlacePiece_PawnBackwardMove_Fails()
        {
            Assert.IsNotNull(_model);

            // 後退移動は無効
            HexCoordinate from = new HexCoordinate(0, -4);
            HexCoordinate backward = new HexCoordinate(0, -5);

            _model!.SelectPiece(from);
            bool success = _model.PlacePiece(backward);

            Assert.IsFalse(success);
        }

        #endregion

        #region ナイト移動テスト

        [Test]
        public void IsValidMove_KnightLShapeMove_ReturnsTrue()
        {
            Assert.IsNotNull(_model);

            // ナイトを配置
            HexCoordinate knightPos = new HexCoordinate(0, 0);
            _model!.Grid.GetTile(knightPos)?.PlacePiece(PieceType.Player1);
            var pieces = _model.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces[knightPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.Knight);
            }

            // L字移動（2-1）
            HexCoordinate target = new HexCoordinate(2, -1);
            bool valid = _model.IsValidMove(knightPos, target);

            Assert.IsTrue(valid);
        }

        #endregion

        #region ルーク移動テスト

        [Test]
        public void IsValidMove_RookStraightMove_ReturnsTrue()
        {
            Assert.IsNotNull(_model);

            // ルークを配置
            HexCoordinate rookPos = new HexCoordinate(0, 0);
            _model!.Grid.GetTile(rookPos)?.PlacePiece(PieceType.Player1);
            var pieces = _model.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces[rookPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.Rook);
            }

            // 直線移動
            HexCoordinate target = new HexCoordinate(0, 3);
            bool valid = _model.IsValidMove(rookPos, target);

            Assert.IsTrue(valid);
        }

        [Test]
        public void IsValidMove_RookBlockedPath_ReturnsFalse()
        {
            Assert.IsNotNull(_model);

            // ルークを配置
            HexCoordinate rookPos = new HexCoordinate(0, 0);
            _model!.Grid.GetTile(rookPos)?.PlacePiece(PieceType.Player1);
            var pieces = _model.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces[rookPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.Rook);

                // 経路上に駒を配置
                HexCoordinate blockPos = new HexCoordinate(0, 2);
                _model.Grid.GetTile(blockPos)?.PlacePiece(PieceType.Player2);
                pieces[blockPos] = new HexChessModel.ChessPiece(PieceType.Player2, HexChessModel.ChessPieceType.Pawn);
            }

            // 移動不可
            HexCoordinate target = new HexCoordinate(0, 3);
            bool valid = _model.IsValidMove(rookPos, target);

            Assert.IsFalse(valid);
        }

        #endregion

        #region ビショップ移動テスト

        [Test]
        public void IsValidMove_BishopDiagonalMove_ReturnsTrue()
        {
            Assert.IsNotNull(_model);

            // ビショップを配置
            HexCoordinate bishopPos = new HexCoordinate(0, 0);
            _model!.Grid.GetTile(bishopPos)?.PlacePiece(PieceType.Player1);
            var pieces = _model.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces[bishopPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.Bishop);
            }

            // 斜め移動（q一定）
            HexCoordinate target = new HexCoordinate(0, 3);
            bool valid = _model.IsValidMove(bishopPos, target);

            Assert.IsTrue(valid);
        }

        #endregion

        #region クイーン移動テスト

        [Test]
        public void IsValidMove_QueenStraightMove_ReturnsTrue()
        {
            Assert.IsNotNull(_model);

            // クイーンを配置
            HexCoordinate queenPos = new HexCoordinate(0, 0);
            _model!.Grid.GetTile(queenPos)?.PlacePiece(PieceType.Player1);
            var pieces = _model.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces[queenPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.Queen);
            }

            // 直線移動（ルークの動き）
            HexCoordinate target1 = new HexCoordinate(0, 3);
            bool valid1 = _model.IsValidMove(queenPos, target1);
            Assert.IsTrue(valid1);

            // 斜め移動（ビショップの動き）
            HexCoordinate target2 = new HexCoordinate(3, 0);
            bool valid2 = _model.IsValidMove(queenPos, target2);
            Assert.IsTrue(valid2);
        }

        #endregion

        #region キング移動テスト

        [Test]
        public void IsValidMove_KingOneStepMove_ReturnsTrue()
        {
            Assert.IsNotNull(_model);

            // キングを配置
            HexCoordinate kingPos = new HexCoordinate(0, 0);
            _model!.Grid.GetTile(kingPos)?.PlacePiece(PieceType.Player1);
            var pieces = _model.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces[kingPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.King);
            }

            // 1マス移動（6方向いずれか）
            HexCoordinate target = new HexCoordinate(1, 0);
            bool valid = _model.IsValidMove(kingPos, target);

            Assert.IsTrue(valid);
        }

        [Test]
        public void IsValidMove_KingTwoStepMove_ReturnsFalse()
        {
            Assert.IsNotNull(_model);

            // キングを配置
            HexCoordinate kingPos = new HexCoordinate(0, 0);
            _model!.Grid.GetTile(kingPos)?.PlacePiece(PieceType.Player1);
            var pieces = _model.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces[kingPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.King);
            }

            // 2マス移動（無効）
            HexCoordinate target = new HexCoordinate(2, 0);
            bool valid = _model.IsValidMove(kingPos, target);

            Assert.IsFalse(valid);
        }

        #endregion

        #region チェック判定テスト

        [Test]
        public void IsPlayerInCheck_KingUnderAttack_ReturnsTrue()
        {
            Assert.IsNotNull(_model);

            // すべての駒をクリア
            var pieces = _model!.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                pieces.Clear();
            }

            foreach (HexTile tile in _model.Grid.AllTiles)
            {
                tile.RemovePiece();
            }

            // Player1のキングを配置
            HexCoordinate kingPos = new HexCoordinate(0, 0);
            _model.Grid.GetTile(kingPos)?.PlacePiece(PieceType.Player1);
            if (pieces != null)
            {
                pieces[kingPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.King);
            }

            // キング位置を更新
            _model.GetType()
                .GetField("_player1KingPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .SetValue(_model, kingPos);

            // Player2のルークを配置（キングを攻撃可能）
            HexCoordinate rookPos = new HexCoordinate(0, 3);
            _model.Grid.GetTile(rookPos)?.PlacePiece(PieceType.Player2);
            if (pieces != null)
            {
                pieces[rookPos] = new HexChessModel.ChessPiece(PieceType.Player2, HexChessModel.ChessPieceType.Rook);
            }

            // チェック判定を更新
            _model.GetType()
                .GetMethod("UpdateCheckStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(_model, null);

            bool inCheck = _model.IsPlayerInCheck(PieceType.Player1);
            Assert.IsTrue(inCheck);
        }

        #endregion

        #region 駒取得テスト

        [Test]
        public void PlacePiece_CaptureOpponentPiece_Success()
        {
            Assert.IsNotNull(_model);

            // 駒を配置してキャプチャ可能にする
            var pieces = _model!.GetType()
                .GetField("_pieces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_model) as Dictionary<HexCoordinate, HexChessModel.ChessPiece>;

            if (pieces != null)
            {
                // Player1のクイーンを配置
                HexCoordinate queenPos = new HexCoordinate(0, 0);
                _model.Grid.GetTile(queenPos)?.PlacePiece(PieceType.Player1);
                pieces[queenPos] = new HexChessModel.ChessPiece(PieceType.Player1, HexChessModel.ChessPieceType.Queen);

                // Player2のポーンを配置（取られる駒）
                HexCoordinate targetPos = new HexCoordinate(0, 3);
                _model.Grid.GetTile(targetPos)?.PlacePiece(PieceType.Player2);
                pieces[targetPos] = new HexChessModel.ChessPiece(PieceType.Player2, HexChessModel.ChessPieceType.Pawn);
            }

            // キャプチャ実行
            _model.SelectPiece(new HexCoordinate(0, 0));
            bool success = _model.PlacePiece(new HexCoordinate(0, 3));

            Assert.IsTrue(success);

            // 駒が取られている
            HexChessModel.ChessPiece? capturedPiece = _model.GetChessPiece(new HexCoordinate(0, 3));
            Assert.IsNotNull(capturedPiece);
            Assert.AreEqual(PieceType.Player1, capturedPiece!.Value.Player);
        }

        #endregion

        #region 有効手取得テスト

        [Test]
        public void GetValidMoves_ReturnsOnlyLegalMoves()
        {
            Assert.IsNotNull(_model);

            // ポーンの有効手を取得
            HexCoordinate pawnPos = new HexCoordinate(0, -4);
            List<HexCoordinate> validMoves = _model!.GetValidMoves(pawnPos);

            // 初回は1マスまたは2マス前進可能
            Assert.GreaterOrEqual(validMoves.Count, 1);
            Assert.LessOrEqual(validMoves.Count, 2);
        }

        #endregion

        #region ゲームリセットテスト

        [Test]
        public void ResetGame_RestoresInitialState()
        {
            Assert.IsNotNull(_model);

            // ゲームを進める（駒を移動）
            HexCoordinate from = new HexCoordinate(0, -4);
            HexCoordinate to = new HexCoordinate(0, -3);
            _model!.SelectPiece(from);
            _model.PlacePiece(to);

            // リセット
            _model.Reset();

            // 初期状態に戻っているか確認
            Assert.AreEqual(PieceType.Player1, _model.CurrentPlayer);
            Assert.AreEqual(GameState.Playing, _model.State);

            // キングが初期位置にいる
            HexChessModel.ChessPiece? king = _model.GetChessPiece(new HexCoordinate(0, -5));
            Assert.IsNotNull(king);
            Assert.AreEqual(HexChessModel.ChessPieceType.King, king!.Value.Type);

            Assert.IsNull(_model.GetSelectedPiece());
            Assert.IsFalse(_model.IsPlayerInCheck(PieceType.Player1));
            Assert.IsFalse(_model.IsPlayerInCheck(PieceType.Player2));
        }

        #endregion

        #region エッジケーステスト

        [Test]
        public void PlacePiece_WithoutSelection_ReturnsFalse()
        {
            Assert.IsNotNull(_model);

            // 選択せずに配置試行
            HexCoordinate target = new HexCoordinate(0, 0);
            bool success = _model!.PlacePiece(target);

            Assert.IsFalse(success);
        }

        [Test]
        public void PlacePiece_OutOfBounds_ReturnsFalse()
        {
            Assert.IsNotNull(_model);

            HexCoordinate piece = new HexCoordinate(0, -4);
            _model!.SelectPiece(piece);

            // グリッド外の座標
            HexCoordinate outOfBounds = new HexCoordinate(10, 10);
            bool success = _model.PlacePiece(outOfBounds);

            Assert.IsFalse(success);
        }

        [Test]
        public void PlacePiece_ToOwnPiece_ReturnsFalse()
        {
            Assert.IsNotNull(_model);

            // 自分の駒に移動試行
            HexCoordinate from = new HexCoordinate(0, -4);
            HexCoordinate to = new HexCoordinate(1, -4); // 別の自分の駒

            _model!.SelectPiece(from);
            bool success = _model.PlacePiece(to);

            Assert.IsFalse(success);
        }

        #endregion
    }
}
