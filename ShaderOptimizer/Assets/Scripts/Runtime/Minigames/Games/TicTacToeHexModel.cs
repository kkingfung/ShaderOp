#nullable enable

using System.Collections.Generic;
using ShaderOp.Minigames.HexGrid;
using UnityEngine;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Tic-Tac-Toe Hex用Model
    /// </summary>
    /// <remarks>
    /// 3×3ヘックスグリッド（9タイル）
    /// 直線3つ揃えたら勝利（6方向チェック）
    /// </remarks>
    public class TicTacToeHexModel : HexBoardGameModel
    {
        /// <summary>グリッドサイズ</summary>
        private const int GRID_SIZE = 3;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TicTacToeHexModel() : base(playerCount: 2)
        {
        }

        /// <summary>
        /// ゲームを初期化
        /// </summary>
        public override void Initialize()
        {
            // 3×3の長方形グリッドを生成
            Grid.GenerateRectangle(GRID_SIZE, GRID_SIZE);
            CurrentPlayerIndex = 0;
            SetGameState(GameState.WaitingToStart);
        }

        /// <summary>
        /// 有効な手かチェック
        /// </summary>
        public override bool IsValidMove(HexCoordinate from, HexCoordinate to)
        {
            // Tic-Tac-Toeでは駒は移動しない（配置のみ）
            // fromは無視、toが空きマスかのみチェック
            HexTile? tile = Grid.GetTile(to);
            return tile != null && tile.IsEmpty && tile.IsValid;
        }

        /// <summary>
        /// 手を実行（オーバーライド）
        /// </summary>
        public override bool ExecuteMove(HexCoordinate from, HexCoordinate to)
        {
            if (State != GameState.Playing)
            {
                return false;
            }

            if (!IsValidMove(from, to))
            {
                return false;
            }

            // 駒を配置
            HexTile? tile = Grid.GetTile(to);
            if (tile == null)
            {
                return false;
            }

            tile.Piece = CurrentPlayer;
            OnTileUpdated?.Invoke(to);

            // 勝利判定
            if (CheckWinCondition())
            {
                SetGameState(GetWinStateForPlayer(CurrentPlayerIndex));
                return true;
            }

            // 引き分け判定
            if (CheckDrawCondition())
            {
                SetGameState(GameState.Draw);
                return true;
            }

            // 次のプレイヤーへ
            NextTurn();
            return true;
        }

        /// <summary>
        /// 勝利条件チェック
        /// </summary>
        protected override bool CheckWinCondition()
        {
            PieceType currentPiece = CurrentPlayer;

            // すべてのタイルから3連続をチェック
            foreach (HexTile tile in Grid.AllTiles)
            {
                if (tile.Piece != currentPiece) continue;

                // 6方向それぞれで3連続チェック
                for (int direction = 0; direction < 6; direction++)
                {
                    if (CheckLineFromTile(tile.Coordinate, direction, currentPiece, 3))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 指定方向に連続した駒があるかチェック
        /// </summary>
        private bool CheckLineFromTile(HexCoordinate start, int direction, PieceType piece, int count)
        {
            HexCoordinate current = start;

            for (int i = 0; i < count; i++)
            {
                HexTile? tile = Grid.GetTile(current);
                if (tile == null || tile.Piece != piece)
                {
                    return false;
                }

                if (i < count - 1)
                {
                    current = current.GetNeighbor(direction);
                }
            }

            return true;
        }

        /// <summary>
        /// 引き分け条件チェック
        /// </summary>
        protected override bool CheckDrawCondition()
        {
            // すべてのタイルが埋まっている場合は引き分け
            foreach (HexTile tile in Grid.AllTiles)
            {
                if (tile.IsEmpty)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 有効な手をすべて取得（AI用）
        /// </summary>
        public List<HexCoordinate> GetAllValidMoves()
        {
            List<HexCoordinate> validMoves = new List<HexCoordinate>();

            foreach (HexTile tile in Grid.AllTiles)
            {
                if (tile.IsEmpty)
                {
                    validMoves.Add(tile.Coordinate);
                }
            }

            return validMoves;
        }
    }
}
