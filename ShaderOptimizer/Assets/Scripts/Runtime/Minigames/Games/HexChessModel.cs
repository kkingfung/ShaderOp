#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Chess ゲームモデル
    /// </summary>
    /// <remarks>
    /// 六角形グリッド版のチェスゲーム
    /// 6種類の駒（King, Queen, Rook, Bishop, Knight, Pawn）を使用
    /// </remarks>
    public class HexChessModel : HexBoardGameModel
    {
        /// <summary>グリッド半径</summary>
        private const int GRID_RADIUS = 5;

        /// <summary>駒の種類</summary>
        public enum ChessPieceType
        {
            None = 0,
            Pawn = 1,
            Knight = 2,
            Bishop = 3,
            Rook = 4,
            Queen = 5,
            King = 6
        }

        /// <summary>駒情報</summary>
        public struct ChessPiece
        {
            public PieceType Player;
            public ChessPieceType Type;
            public bool HasMoved;

            public ChessPiece(PieceType player, ChessPieceType type)
            {
                Player = player;
                Type = type;
                HasMoved = false;
            }
        }

        /// <summary>盤面の駒情報</summary>
        private Dictionary<HexCoordinate, ChessPiece> _pieces = new();

        /// <summary>選択中の駒</summary>
        private HexCoordinate? _selectedPiece = null;

        /// <summary>チェック状態</summary>
        private bool _isPlayer1InCheck = false;
        private bool _isPlayer2InCheck = false;

        /// <summary>キングの位置</summary>
        private HexCoordinate _player1KingPosition;
        private HexCoordinate _player2KingPosition;

        /// <summary>
        /// ゲームを初期化
        /// </summary>
        public override void Initialize()
        {
            // 11x11の六角形グリッドを生成
            Grid = new HexGrid.HexGrid(1.0f);
            Grid.GenerateHexagon(GRID_RADIUS);

            // 初期配置
            SetupInitialPieces();

            CurrentPlayerIndex = 0; // Player1
            SetGameState(GameState.Playing);
            _selectedPiece = null;
            _isPlayer1InCheck = false;
            _isPlayer2InCheck = false;

            Debug.Log("[HexChessModel] Game initialized with hexagonal grid");
        }

        /// <summary>
        /// 初期駒配置
        /// </summary>
        private void SetupInitialPieces()
        {
            _pieces.Clear();

            // Player1の配置（下側）
            // 最下段：主要駒
            PlaceChessPiece(new HexCoordinate(0, -5), PieceType.Player1, ChessPieceType.King);
            _player1KingPosition = new HexCoordinate(0, -5);
            PlaceChessPiece(new HexCoordinate(1, -5), PieceType.Player1, ChessPieceType.Queen);
            PlaceChessPiece(new HexCoordinate(-1, -5), PieceType.Player1, ChessPieceType.Bishop);
            PlaceChessPiece(new HexCoordinate(2, -5), PieceType.Player1, ChessPieceType.Knight);
            PlaceChessPiece(new HexCoordinate(-2, -5), PieceType.Player1, ChessPieceType.Rook);

            // 2段目：ポーン
            for (int q = -3; q <= 3; q++)
            {
                HexCoordinate coord = new HexCoordinate(q, -4);
                if (Grid.GetTile(coord) != null)
                {
                    PlaceChessPiece(coord, PieceType.Player1, ChessPieceType.Pawn);
                }
            }

            // Player2の配置（上側）
            // 最上段：主要駒
            PlaceChessPiece(new HexCoordinate(0, 5), PieceType.Player2, ChessPieceType.King);
            _player2KingPosition = new HexCoordinate(0, 5);
            PlaceChessPiece(new HexCoordinate(-1, 5), PieceType.Player2, ChessPieceType.Queen);
            PlaceChessPiece(new HexCoordinate(1, 5), PieceType.Player2, ChessPieceType.Bishop);
            PlaceChessPiece(new HexCoordinate(-2, 5), PieceType.Player2, ChessPieceType.Knight);
            PlaceChessPiece(new HexCoordinate(2, 5), PieceType.Player2, ChessPieceType.Rook);

            // 2段目：ポーン
            for (int q = -3; q <= 3; q++)
            {
                HexCoordinate coord = new HexCoordinate(q, 4);
                if (Grid.GetTile(coord) != null)
                {
                    PlaceChessPiece(coord, PieceType.Player2, ChessPieceType.Pawn);
                }
            }
        }

        /// <summary>
        /// 駒を配置
        /// </summary>
        private void PlaceChessPiece(HexCoordinate coord, PieceType player, ChessPieceType type)
        {
            HexTile? tile = Grid.GetTile(coord);
            if (tile != null)
            {
                tile.PlacePiece(player);
                _pieces[coord] = new ChessPiece(player, type);
            }
        }

        /// <summary>
        /// 駒を選択
        /// </summary>
        public bool SelectPiece(HexCoordinate coord)
        {
            if (!_pieces.TryGetValue(coord, out ChessPiece piece))
                return false;

            // 現在のプレイヤーの駒のみ選択可能
            if (piece.Player != CurrentPlayer)
                return false;

            _selectedPiece = coord;
            Debug.Log($"[HexChessModel] {piece.Type} selected at {coord}");
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
        /// 駒情報を取得
        /// </summary>
        public ChessPiece? GetChessPiece(HexCoordinate coord)
        {
            if (_pieces.TryGetValue(coord, out ChessPiece piece))
                return piece;
            return null;
        }

        /// <summary>
        /// 移動が有効かチェック
        /// </summary>
        public override bool IsValidMove(HexCoordinate from, HexCoordinate to)
        {
            if (!_pieces.TryGetValue(from, out ChessPiece piece))
                return false;

            if (piece.Player != CurrentPlayer)
                return false;

            HexTile? toTile = Grid.GetTile(to);
            if (toTile == null)
                return false;

            // 自分の駒がいる場所には移動不可
            if (_pieces.TryGetValue(to, out ChessPiece targetPiece))
            {
                if (targetPiece.Player == piece.Player)
                    return false;
            }

            // 駒タイプごとの移動ルールチェック
            switch (piece.Type)
            {
                case ChessPieceType.Pawn:
                    return IsValidPawnMove(from, to, piece.Player, piece.HasMoved);
                case ChessPieceType.Knight:
                    return IsValidKnightMove(from, to);
                case ChessPieceType.Bishop:
                    return IsValidBishopMove(from, to);
                case ChessPieceType.Rook:
                    return IsValidRookMove(from, to);
                case ChessPieceType.Queen:
                    return IsValidQueenMove(from, to);
                case ChessPieceType.King:
                    return IsValidKingMove(from, to);
                default:
                    return false;
            }
        }

        /// <summary>
        /// ポーンの移動ルール
        /// </summary>
        private bool IsValidPawnMove(HexCoordinate from, HexCoordinate to, PieceType player, bool hasMoved)
        {
            int direction = player == PieceType.Player1 ? 1 : -1;

            // 前進（1マス）
            if (to.R == from.R + direction && to.Q == from.Q)
            {
                return !_pieces.ContainsKey(to);
            }

            // 初回は2マス前進可能
            if (!hasMoved && to.R == from.R + (direction * 2) && to.Q == from.Q)
            {
                HexCoordinate middle = new HexCoordinate(from.Q, from.R + direction);
                return !_pieces.ContainsKey(middle) && !_pieces.ContainsKey(to);
            }

            // 斜め前に敵駒がある場合のみ移動可能（取る）
            if (to.R == from.R + direction && System.Math.Abs(to.Q - from.Q) == 1)
            {
                if (_pieces.TryGetValue(to, out ChessPiece targetPiece))
                {
                    return targetPiece.Player != player;
                }
            }

            return false;
        }

        /// <summary>
        /// ナイトの移動ルール
        /// </summary>
        private bool IsValidKnightMove(HexCoordinate from, HexCoordinate to)
        {
            // ナイトは2-1のL字移動（6方向 × 2パターン = 12通り）
            int dq = to.Q - from.Q;
            int dr = to.R - from.R;
            int ds = to.S - from.S;

            // L字移動パターン
            return (System.Math.Abs(dq) == 2 && System.Math.Abs(dr) == 1) ||
                   (System.Math.Abs(dq) == 1 && System.Math.Abs(dr) == 2) ||
                   (System.Math.Abs(dr) == 2 && System.Math.Abs(ds) == 1) ||
                   (System.Math.Abs(dr) == 1 && System.Math.Abs(ds) == 2) ||
                   (System.Math.Abs(ds) == 2 && System.Math.Abs(dq) == 1) ||
                   (System.Math.Abs(ds) == 1 && System.Math.Abs(dq) == 2);
        }

        /// <summary>
        /// ビショップの移動ルール
        /// </summary>
        private bool IsValidBishopMove(HexCoordinate from, HexCoordinate to)
        {
            // 斜め方向の移動（3方向）
            int dq = to.Q - from.Q;
            int dr = to.R - from.R;
            int ds = to.S - from.S;

            // qが一定（s軸移動）、rが一定（q軸移動）、sが一定（r軸移動）
            if (dq == 0 || dr == 0 || ds == 0)
            {
                return IsPathClear(from, to);
            }

            return false;
        }

        /// <summary>
        /// ルークの移動ルール
        /// </summary>
        private bool IsValidRookMove(HexCoordinate from, HexCoordinate to)
        {
            // 直線方向の移動（6方向）
            int dq = to.Q - from.Q;
            int dr = to.R - from.R;
            int ds = to.S - from.S;

            // 2つの座標が変化し、1つが一定
            bool isValidDirection = (dq == -dr && ds == 0) ||
                                   (dr == -ds && dq == 0) ||
                                   (ds == -dq && dr == 0);

            if (isValidDirection)
            {
                return IsPathClear(from, to);
            }

            return false;
        }

        /// <summary>
        /// クイーンの移動ルール
        /// </summary>
        private bool IsValidQueenMove(HexCoordinate from, HexCoordinate to)
        {
            // ルーク + ビショップの動き
            return IsValidRookMove(from, to) || IsValidBishopMove(from, to);
        }

        /// <summary>
        /// キングの移動ルール
        /// </summary>
        private bool IsValidKingMove(HexCoordinate from, HexCoordinate to)
        {
            // 6方向に1マス移動
            return from.ManhattanDistance(to) == 1;
        }

        /// <summary>
        /// 経路が空いているかチェック
        /// </summary>
        private bool IsPathClear(HexCoordinate from, HexCoordinate to)
        {
            // fromからtoまでの経路上に駒がないかチェック
            int dq = to.Q - from.Q;
            int dr = to.R - from.R;
            int ds = to.S - from.S;

            int steps = System.Math.Max(System.Math.Abs(dq), System.Math.Max(System.Math.Abs(dr), System.Math.Abs(ds)));

            for (int i = 1; i < steps; i++)
            {
                int q = from.Q + (dq * i / steps);
                int r = from.R + (dr * i / steps);
                HexCoordinate intermediate = new HexCoordinate(q, r);

                if (_pieces.ContainsKey(intermediate))
                {
                    return false;
                }
            }

            return true;
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

            if (!_pieces.TryGetValue(from, out ChessPiece piece))
                return false;

            // 移動をシミュレートしてチェックになるか確認
            if (WouldMoveResultInCheck(from, coord, piece.Player))
            {
                Debug.Log($"[HexChessModel] Move would result in check");
                return false;
            }

            // 駒を移動
            HexTile? fromTile = Grid.GetTile(from);
            HexTile? toTile = Grid.GetTile(coord);

            if (fromTile == null || toTile == null)
                return false;

            // 取られる駒がある場合は削除
            bool isCapture = _pieces.ContainsKey(coord);
            if (isCapture)
            {
                _pieces.Remove(coord);
                Debug.Log($"[HexChessModel] Piece captured at {coord}");
            }

            // 駒を移動
            fromTile.RemovePiece();
            toTile.PlacePiece(piece.Player);

            _pieces.Remove(from);
            piece.HasMoved = true;
            _pieces[coord] = piece;

            // キングの位置を更新
            if (piece.Type == ChessPieceType.King)
            {
                if (piece.Player == PieceType.Player1)
                    _player1KingPosition = coord;
                else
                    _player2KingPosition = coord;
            }

            // OnPiecePlaced イベントは削除（コントローラー側で処理）

            _selectedPiece = null;

            // チェック状態を更新
            UpdateCheckStatus();

            // チェックメイト判定
            if (CheckWinCondition())
            {
                SetGameState(GameState.GameOver);
                return true;
            }

            // ターン交代
            NextTurn();

            return true;
        }

        /// <summary>
        /// 移動がチェックになるかシミュレート
        /// </summary>
        private bool WouldMoveResultInCheck(HexCoordinate from, HexCoordinate to, PieceType player)
        {
            // 現在の状態を保存
            ChessPiece movingPiece = _pieces[from];
            ChessPiece? capturedPiece = null;
            if (_pieces.TryGetValue(to, out ChessPiece captured))
                capturedPiece = captured;

            HexCoordinate kingPos = player == PieceType.Player1 ? _player1KingPosition : _player2KingPosition;

            // キングが移動する場合
            if (movingPiece.Type == ChessPieceType.King)
                kingPos = to;

            // 仮移動
            _pieces.Remove(from);
            _pieces[to] = movingPiece;

            // チェック判定
            bool inCheck = IsKingInCheck(kingPos, player);

            // 元に戻す
            _pieces.Remove(to);
            _pieces[from] = movingPiece;
            if (capturedPiece != null)
                _pieces[to] = capturedPiece.Value;

            return inCheck;
        }

        /// <summary>
        /// キングがチェックされているか判定
        /// </summary>
        private bool IsKingInCheck(HexCoordinate kingPos, PieceType player)
        {
            PieceType opponent = player == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1;

            // 相手の駒からキングへの攻撃をチェック
            foreach (var kvp in _pieces)
            {
                if (kvp.Value.Player == opponent)
                {
                    // この駒がキングを攻撃できるか
                    if (CanPieceAttack(kvp.Key, kingPos, kvp.Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 駒が指定座標を攻撃できるか
        /// </summary>
        private bool CanPieceAttack(HexCoordinate from, HexCoordinate to, ChessPiece piece)
        {
            switch (piece.Type)
            {
                case ChessPieceType.Pawn:
                    // ポーンは斜め前のみ攻撃可能
                    int direction = piece.Player == PieceType.Player1 ? 1 : -1;
                    return to.R == from.R + direction && System.Math.Abs(to.Q - from.Q) == 1;
                case ChessPieceType.Knight:
                    return IsValidKnightMove(from, to);
                case ChessPieceType.Bishop:
                    return IsValidBishopMove(from, to);
                case ChessPieceType.Rook:
                    return IsValidRookMove(from, to);
                case ChessPieceType.Queen:
                    return IsValidQueenMove(from, to);
                case ChessPieceType.King:
                    return IsValidKingMove(from, to);
                default:
                    return false;
            }
        }

        /// <summary>
        /// チェック状態を更新
        /// </summary>
        private void UpdateCheckStatus()
        {
            _isPlayer1InCheck = IsKingInCheck(_player1KingPosition, PieceType.Player1);
            _isPlayer2InCheck = IsKingInCheck(_player2KingPosition, PieceType.Player2);

            if (_isPlayer1InCheck)
                Debug.Log("[HexChessModel] Player 1 is in check!");
            if (_isPlayer2InCheck)
                Debug.Log("[HexChessModel] Player 2 is in check!");
        }

        /// <summary>
        /// 勝敗判定（チェックメイト）
        /// </summary>
        protected override bool CheckWinCondition()
        {
            bool currentPlayerInCheck = CurrentPlayer == PieceType.Player1 ? _isPlayer1InCheck : _isPlayer2InCheck;

            if (!currentPlayerInCheck)
                return false;

            // チェックメイト判定：すべての手を試して脱出できるか確認
            foreach (var kvp in _pieces)
            {
                if (kvp.Value.Player != CurrentPlayer)
                    continue;

                HexCoordinate from = kvp.Key;

                // この駒のすべての可能な移動をチェック
                foreach (HexTile tile in Grid.AllTiles)
                {
                    HexCoordinate to = tile.Coordinate;

                    if (IsValidMove(from, to))
                    {
                        if (!WouldMoveResultInCheck(from, to, CurrentPlayer))
                        {
                            // 脱出可能な手が見つかった
                            return false;
                        }
                    }
                }
            }

            // 脱出不可能 = チェックメイト
            Debug.Log($"[HexChessModel] Checkmate! {CurrentPlayer} loses");
            return true;
        }

        /// <summary>
        /// プレイヤーがチェックされているか
        /// </summary>
        public bool IsPlayerInCheck(PieceType player)
        {
            return player == PieceType.Player1 ? _isPlayer1InCheck : _isPlayer2InCheck;
        }

        /// <summary>
        /// 有効な移動先のリストを取得
        /// </summary>
        public List<HexCoordinate> GetValidMoves(HexCoordinate from)
        {
            List<HexCoordinate> validMoves = new List<HexCoordinate>();

            if (!_pieces.TryGetValue(from, out ChessPiece piece))
                return validMoves;

            foreach (HexTile tile in Grid.AllTiles)
            {
                if (IsValidMove(from, tile.Coordinate))
                {
                    if (!WouldMoveResultInCheck(from, tile.Coordinate, piece.Player))
                    {
                        validMoves.Add(tile.Coordinate);
                    }
                }
            }

            return validMoves;
        }
    }
}
