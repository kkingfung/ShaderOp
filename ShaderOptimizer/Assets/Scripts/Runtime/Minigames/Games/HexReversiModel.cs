#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Reversi (オセロ) ゲームモデル
    /// </summary>
    /// <remarks>
    /// 六角形グリッド版のリバーシゲーム
    /// 6方向に挟んだ駒を反転させる
    /// </remarks>
    public class HexReversiModel : HexBoardGameModel
    {
        /// <summary>グリッドサイズ（半径）</summary>
        private const int GRID_RADIUS = 3;

        /// <summary>
        /// ゲームを初期化
        /// </summary>
        public override void Initialize()
        {
            // 六角形グリッドを生成
            Grid = new HexGrid.HexGrid(1.0f);
            Grid.GenerateHexagon(GRID_RADIUS);

            // 初期配置（中央4マスに交互配置）
            SetupInitialPieces();

            CurrentPlayer = PieceType.Player1;
            GameState = GameState.Playing;

            Debug.Log("[HexReversiModel] Game initialized with hexagon grid");
        }

        /// <summary>
        /// 初期駒配置
        /// </summary>
        private void SetupInitialPieces()
        {
            // 中央付近に初期配置
            // Player1: (0,0), (1,-1)
            // Player2: (0,-1), (1,0)
            PlacePieceInternal(new HexCoordinate(0, 0), PieceType.Player1);
            PlacePieceInternal(new HexCoordinate(1, -1), PieceType.Player1);
            PlacePieceInternal(new HexCoordinate(0, -1), PieceType.Player2);
            PlacePieceInternal(new HexCoordinate(1, 0), PieceType.Player2);
        }

        /// <summary>
        /// 内部用駒配置（反転なし）
        /// </summary>
        private void PlacePieceInternal(HexCoordinate coord, PieceType piece)
        {
            HexTile? tile = Grid.GetTile(coord);
            if (tile != null)
            {
                tile.PlacePiece(piece);
            }
        }

        /// <summary>
        /// 移動が有効かチェック
        /// </summary>
        public override bool IsValidMove(HexCoordinate from, HexCoordinate to)
        {
            // Reversiでは「from」は使用しない（直接配置）
            // 空のタイルで、かつ少なくとも1方向で駒を反転できる場合のみ有効
            HexTile? tile = Grid.GetTile(to);
            if (tile == null || !tile.IsEmpty)
                return false;

            // 6方向それぞれで反転可能な駒があるかチェック
            for (int direction = 0; direction < 6; direction++)
            {
                if (CanFlipInDirection(to, direction, CurrentPlayer))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 駒を配置
        /// </summary>
        public override bool PlacePiece(HexCoordinate coord)
        {
            if (!IsValidMove(HexCoordinate.Zero, coord))
                return false;

            // 駒を配置
            HexTile? tile = Grid.GetTile(coord);
            if (tile != null)
            {
                tile.PlacePiece(CurrentPlayer);

                // 6方向それぞれで駒を反転
                for (int direction = 0; direction < 6; direction++)
                {
                    FlipPiecesInDirection(coord, direction, CurrentPlayer);
                }

                OnPiecePlaced?.Invoke(coord, CurrentPlayer);

                // 勝敗判定
                if (CheckWinCondition())
                {
                    SetGameState(GameState.GameOver);
                    return true;
                }

                // ターン交代
                SwitchTurn();

                // 次のプレイヤーが置ける場所がない場合はスキップ
                if (!HasValidMoves(CurrentPlayer))
                {
                    Debug.Log($"[HexReversiModel] {CurrentPlayer} has no valid moves, skipping turn");
                    SwitchTurn();

                    // 両者とも置けない場合はゲーム終了
                    if (!HasValidMoves(CurrentPlayer))
                    {
                        SetGameState(GameState.GameOver);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 特定方向に駒を反転できるかチェック
        /// </summary>
        private bool CanFlipInDirection(HexCoordinate start, int direction, PieceType player)
        {
            PieceType opponent = player == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1;
            HexCoordinate current = start.GetNeighbor(direction);
            bool foundOpponent = false;

            while (true)
            {
                HexTile? tile = Grid.GetTile(current);
                if (tile == null)
                    return false; // グリッド外

                if (tile.IsEmpty)
                    return false; // 空マス

                if (tile.Piece == opponent)
                {
                    foundOpponent = true;
                    current = current.GetNeighbor(direction);
                }
                else if (tile.Piece == player)
                {
                    return foundOpponent; // 自分の駒で挟めた
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 特定方向の駒を反転
        /// </summary>
        private void FlipPiecesInDirection(HexCoordinate start, int direction, PieceType player)
        {
            if (!CanFlipInDirection(start, direction, player))
                return;

            PieceType opponent = player == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1;
            HexCoordinate current = start.GetNeighbor(direction);

            while (true)
            {
                HexTile? tile = Grid.GetTile(current);
                if (tile == null || tile.IsEmpty)
                    break;

                if (tile.Piece == opponent)
                {
                    tile.PlacePiece(player); // 反転
                    current = current.GetNeighbor(direction);
                }
                else
                {
                    break; // 自分の駒に到達
                }
            }
        }

        /// <summary>
        /// 有効な手があるかチェック
        /// </summary>
        private bool HasValidMoves(PieceType player)
        {
            PieceType previousPlayer = CurrentPlayer;
            CurrentPlayer = player;

            foreach (HexTile tile in Grid.AllTiles)
            {
                if (IsValidMove(HexCoordinate.Zero, tile.Coordinate))
                {
                    CurrentPlayer = previousPlayer;
                    return true;
                }
            }

            CurrentPlayer = previousPlayer;
            return false;
        }

        /// <summary>
        /// 勝敗判定
        /// </summary>
        protected override bool CheckWinCondition()
        {
            // すべてのタイルが埋まった、または両者とも置けない場合
            List<HexTile> emptyTiles = Grid.FindEmptyTiles();
            if (emptyTiles.Count == 0)
            {
                DetermineWinner();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 勝者を判定
        /// </summary>
        private void DetermineWinner()
        {
            int player1Count = 0;
            int player2Count = 0;

            foreach (HexTile tile in Grid.AllTiles)
            {
                if (tile.Piece == PieceType.Player1)
                    player1Count++;
                else if (tile.Piece == PieceType.Player2)
                    player2Count++;
            }

            if (player1Count > player2Count)
            {
                Debug.Log($"[HexReversiModel] Player1 wins! ({player1Count} vs {player2Count})");
            }
            else if (player2Count > player1Count)
            {
                Debug.Log($"[HexReversiModel] Player2 wins! ({player2Count} vs {player1Count})");
            }
            else
            {
                Debug.Log($"[HexReversiModel] Draw! ({player1Count} vs {player2Count})");
            }
        }

        /// <summary>
        /// 各プレイヤーの駒数を取得
        /// </summary>
        public (int player1Count, int player2Count) GetPieceCounts()
        {
            int player1Count = 0;
            int player2Count = 0;

            foreach (HexTile tile in Grid.AllTiles)
            {
                if (tile.Piece == PieceType.Player1)
                    player1Count++;
                else if (tile.Piece == PieceType.Player2)
                    player2Count++;
            }

            return (player1Count, player2Count);
        }

        /// <summary>
        /// 有効な手のリストを取得
        /// </summary>
        public List<HexCoordinate> GetValidMoves()
        {
            List<HexCoordinate> validMoves = new List<HexCoordinate>();

            foreach (HexTile tile in Grid.AllTiles)
            {
                if (IsValidMove(HexCoordinate.Zero, tile.Coordinate))
                {
                    validMoves.Add(tile.Coordinate);
                }
            }

            return validMoves;
        }
    }
}
