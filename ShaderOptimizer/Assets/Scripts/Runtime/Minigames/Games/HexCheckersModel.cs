#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Checkers (チェッカー) ゲームモデル
    /// </summary>
    /// <remarks>
    /// 六角形グリッド版のチェッカーゲーム
    /// 駒のジャンプ、キング化、連続ジャンプをサポート
    /// </remarks>
    public class HexCheckersModel : HexBoardGameModel
    {
        /// <summary>グリッドサイズ（幅）</summary>
        private const int GRID_WIDTH = 8;

        /// <summary>グリッドサイズ（高さ）</summary>
        private const int GRID_HEIGHT = 8;

        /// <summary>キング化された駒を管理</summary>
        private HashSet<HexCoordinate> _kingPieces = new();

        /// <summary>選択中の駒</summary>
        private HexCoordinate? _selectedPiece = null;

        /// <summary>強制ジャンプフラグ</summary>
        private bool _mustJump = false;

        /// <summary>連続ジャンプ中フラグ</summary>
        private bool _isChainJumping = false;

        /// <summary>
        /// ゲームを初期化
        /// </summary>
        public override void Initialize()
        {
            // 8x8の長方形グリッドを生成
            Grid = new HexGrid.HexGrid(1.0f);
            Grid.GenerateRectangle(GRID_WIDTH, GRID_HEIGHT);

            // 初期配置
            SetupInitialPieces();

            CurrentPlayerIndex = 0; // Player1
            SetGameState(GameState.Playing);
            _kingPieces.Clear();
            _selectedPiece = null;
            _mustJump = false;
            _isChainJumping = false;

            Debug.Log("[HexCheckersModel] Game initialized with 8x8 grid");
        }

        /// <summary>
        /// 初期駒配置
        /// </summary>
        private void SetupInitialPieces()
        {
            // Player1の駒を下3列に配置（市松模様）
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < GRID_WIDTH; col++)
                {
                    // 市松模様で配置
                    if ((row + col) % 2 == 0)
                    {
                        HexCoordinate coord = new HexCoordinate(col, row);
                        HexTile? tile = Grid.GetTile(coord);
                        if (tile != null)
                        {
                            tile.PlacePiece(PieceType.Player1);
                        }
                    }
                }
            }

            // Player2の駒を上3列に配置（市松模様）
            for (int row = GRID_HEIGHT - 3; row < GRID_HEIGHT; row++)
            {
                for (int col = 0; col < GRID_WIDTH; col++)
                {
                    // 市松模様で配置
                    if ((row + col) % 2 == 0)
                    {
                        HexCoordinate coord = new HexCoordinate(col, row);
                        HexTile? tile = Grid.GetTile(coord);
                        if (tile != null)
                        {
                            tile.PlacePiece(PieceType.Player2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 駒を選択
        /// </summary>
        public bool SelectPiece(HexCoordinate coord)
        {
            HexTile? tile = Grid.GetTile(coord);
            if (tile == null || tile.IsEmpty)
                return false;

            // 現在のプレイヤーの駒のみ選択可能
            if (tile.Piece != CurrentPlayer)
                return false;

            _selectedPiece = coord;
            Debug.Log($"[HexCheckersModel] Piece selected at {coord}");
            return true;
        }

        /// <summary>
        /// 選択を解除
        /// </summary>
        public void DeselectPiece()
        {
            _selectedPiece = null;
        }

        /// <summary>
        /// 選択中の駒を取得
        /// </summary>
        public HexCoordinate? GetSelectedPiece()
        {
            return _selectedPiece;
        }

        /// <summary>
        /// 駒がキングかどうか
        /// </summary>
        public bool IsKing(HexCoordinate coord)
        {
            return _kingPieces.Contains(coord);
        }

        /// <summary>
        /// 移動が有効かチェック
        /// </summary>
        public override bool IsValidMove(HexCoordinate from, HexCoordinate to)
        {
            HexTile? fromTile = Grid.GetTile(from);
            HexTile? toTile = Grid.GetTile(to);

            if (fromTile == null || toTile == null)
                return false;

            if (fromTile.IsEmpty || !toTile.IsEmpty)
                return false;

            if (fromTile.Piece != CurrentPlayer)
                return false;

            // 強制ジャンプ中は、ジャンプ移動のみ許可
            if (_mustJump)
            {
                return IsValidJump(from, to);
            }

            // 通常移動かジャンプ移動か判定
            int distance = from.ManhattanDistance(to);

            if (distance == 1)
            {
                // 通常移動（1マス）
                return IsValidNormalMove(from, to);
            }
            else if (distance == 2)
            {
                // ジャンプ移動（2マス）
                return IsValidJump(from, to);
            }

            return false;
        }

        /// <summary>
        /// 通常移動が有効かチェック
        /// </summary>
        private bool IsValidNormalMove(HexCoordinate from, HexCoordinate to)
        {
            bool isKing = IsKing(from);

            // キングでない場合は前進のみ
            if (!isKing)
            {
                if (CurrentPlayer == PieceType.Player1)
                {
                    // Player1は上方向（r が増える方向）
                    if (to.R <= from.R)
                        return false;
                }
                else
                {
                    // Player2は下方向（r が減る方向）
                    if (to.R >= from.R)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ジャンプ移動が有効かチェック
        /// </summary>
        private bool IsValidJump(HexCoordinate from, HexCoordinate to)
        {
            // 中間の駒を取得
            HexCoordinate? middleCoord = GetMiddleCoordinate(from, to);
            if (middleCoord == null)
                return false;

            HexTile? middleTile = Grid.GetTile(middleCoord.Value);
            if (middleTile == null || middleTile.IsEmpty)
                return false;

            // 相手の駒を飛び越える
            PieceType opponent = CurrentPlayer == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1;
            if (middleTile.Piece != opponent)
                return false;

            bool isKing = IsKing(from);

            // キングでない場合は前進ジャンプのみ
            if (!isKing)
            {
                if (CurrentPlayer == PieceType.Player1)
                {
                    if (to.R <= from.R)
                        return false;
                }
                else
                {
                    if (to.R >= from.R)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 2つの座標の中間座標を取得
        /// </summary>
        private HexCoordinate? GetMiddleCoordinate(HexCoordinate from, HexCoordinate to)
        {
            // 6方向のいずれかで距離2の場合のみ有効
            for (int direction = 0; direction < 6; direction++)
            {
                HexCoordinate neighbor = from.GetNeighbor(direction);
                HexCoordinate nextNeighbor = neighbor.GetNeighbor(direction);

                if (nextNeighbor == to)
                {
                    return neighbor;
                }
            }

            return null;
        }

        /// <summary>
        /// 駒を配置（移動）
        /// </summary>
        public override bool PlacePiece(HexCoordinate coord)
        {
            if (_selectedPiece == null)
                return false;

            HexCoordinate from = _selectedPiece.Value;

            if (!IsValidMove(from, coord))
                return false;

            // 移動元のタイル
            HexTile? fromTile = Grid.GetTile(from);
            HexTile? toTile = Grid.GetTile(coord);

            if (fromTile == null || toTile == null)
                return false;

            PieceType piece = fromTile.Piece;
            bool wasKing = IsKing(from);

            // ジャンプ移動の場合
            bool isJump = from.ManhattanDistance(coord) == 2;
            if (isJump)
            {
                // 中間の駒を取得して削除
                HexCoordinate? middleCoord = GetMiddleCoordinate(from, coord);
                if (middleCoord != null)
                {
                    HexTile? middleTile = Grid.GetTile(middleCoord.Value);
                    if (middleTile != null)
                    {
                        middleTile.RemovePiece();
                        _kingPieces.Remove(middleCoord.Value);
                        Debug.Log($"[HexCheckersModel] Captured piece at {middleCoord.Value}");
                    }
                }
            }

            // 駒を移動
            fromTile.RemovePiece();
            toTile.PlacePiece(piece);

            // キング状態を移動
            if (wasKing)
            {
                _kingPieces.Remove(from);
                _kingPieces.Add(coord);
            }

            // キング化判定
            if (!wasKing && ShouldPromoteToKing(coord, piece))
            {
                _kingPieces.Add(coord);
                Debug.Log($"[HexCheckersModel] Piece promoted to King at {coord}");
            }

            // OnPiecePlaced イベントは削除（コントローラー側で処理）

            // 連続ジャンプ判定
            if (isJump)
            {
                List<HexCoordinate> nextJumps = GetValidJumps(coord);
                if (nextJumps.Count > 0)
                {
                    // 連続ジャンプ可能
                    _selectedPiece = coord;
                    _mustJump = true;
                    _isChainJumping = true;
                    Debug.Log($"[HexCheckersModel] Chain jump available");
                    return true;
                }
            }

            // ターン終了処理
            _selectedPiece = null;
            _mustJump = false;
            _isChainJumping = false;

            // 勝敗判定
            if (CheckWinCondition())
            {
                SetGameState(GameState.GameOver);
                return true;
            }

            // ターン交代
            NextTurn();

            // 次のプレイヤーがジャンプ可能な駒を持っているか確認
            UpdateMustJumpFlag();

            return true;
        }

        /// <summary>
        /// キング化すべきか判定
        /// </summary>
        private bool ShouldPromoteToKing(HexCoordinate coord, PieceType piece)
        {
            if (piece == PieceType.Player1)
            {
                // Player1は最上段（r == GRID_HEIGHT - 1）
                return coord.R == GRID_HEIGHT - 1;
            }
            else
            {
                // Player2は最下段（r == 0）
                return coord.R == 0;
            }
        }

        /// <summary>
        /// 強制ジャンプフラグを更新
        /// </summary>
        private void UpdateMustJumpFlag()
        {
            foreach (HexTile tile in Grid.AllTiles)
            {
                if (tile.Piece == CurrentPlayer)
                {
                    List<HexCoordinate> jumps = GetValidJumps(tile.Coordinate);
                    if (jumps.Count > 0)
                    {
                        _mustJump = true;
                        Debug.Log($"[HexCheckersModel] Must jump detected");
                        return;
                    }
                }
            }

            _mustJump = false;
        }

        /// <summary>
        /// 有効なジャンプ移動のリストを取得
        /// </summary>
        private List<HexCoordinate> GetValidJumps(HexCoordinate from)
        {
            List<HexCoordinate> jumps = new List<HexCoordinate>();

            // 6方向それぞれで2マス先をチェック
            for (int direction = 0; direction < 6; direction++)
            {
                HexCoordinate neighbor = from.GetNeighbor(direction);
                HexCoordinate target = neighbor.GetNeighbor(direction);

                HexTile? targetTile = Grid.GetTile(target);
                if (targetTile == null || !targetTile.IsEmpty)
                    continue;

                if (IsValidJump(from, target))
                {
                    jumps.Add(target);
                }
            }

            return jumps;
        }

        /// <summary>
        /// 勝敗判定
        /// </summary>
        protected override bool CheckWinCondition()
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

            // どちらかの駒が0になったら終了
            if (player1Count == 0)
            {
                Debug.Log($"[HexCheckersModel] Player2 wins! All Player1 pieces captured");
                return true;
            }

            if (player2Count == 0)
            {
                Debug.Log($"[HexCheckersModel] Player1 wins! All Player2 pieces captured");
                return true;
            }

            // 現在のプレイヤーが移動できない場合も負け
            if (!HasValidMoves(CurrentPlayer))
            {
                Debug.Log($"[HexCheckersModel] {CurrentPlayer} has no valid moves and loses");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 有効な手があるかチェック
        /// </summary>
        private bool HasValidMoves(PieceType player)
        {
            foreach (HexTile tile in Grid.AllTiles)
            {
                if (tile.Piece == player)
                {
                    // ジャンプ可能か
                    List<HexCoordinate> jumps = GetValidJumps(tile.Coordinate);
                    if (jumps.Count > 0)
                        return true;

                    // 通常移動可能か
                    if (!_mustJump)
                    {
                        for (int direction = 0; direction < 6; direction++)
                        {
                            HexCoordinate target = tile.Coordinate.GetNeighbor(direction);
                            if (IsValidNormalMove(tile.Coordinate, target))
                            {
                                HexTile? targetTile = Grid.GetTile(target);
                                if (targetTile != null && targetTile.IsEmpty)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
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
        /// 連続ジャンプ中かどうか
        /// </summary>
        public bool IsChainJumping()
        {
            return _isChainJumping;
        }

        /// <summary>
        /// 強制ジャンプ中かどうか
        /// </summary>
        public bool MustJump()
        {
            return _mustJump;
        }
    }
}
