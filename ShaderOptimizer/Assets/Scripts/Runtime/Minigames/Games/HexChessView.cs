#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Chess View
    /// </summary>
    public class HexChessView : HexBoardGameView
    {
        /// <summary>Player1駒スプライト</summary>
        [Header("Player1 Sprites")]
        [SerializeField] private Sprite? _player1PawnSprite;
        [SerializeField] private Sprite? _player1KnightSprite;
        [SerializeField] private Sprite? _player1BishopSprite;
        [SerializeField] private Sprite? _player1RookSprite;
        [SerializeField] private Sprite? _player1QueenSprite;
        [SerializeField] private Sprite? _player1KingSprite;

        /// <summary>Player2駒スプライト</summary>
        [Header("Player2 Sprites")]
        [SerializeField] private Sprite? _player2PawnSprite;
        [SerializeField] private Sprite? _player2KnightSprite;
        [SerializeField] private Sprite? _player2BishopSprite;
        [SerializeField] private Sprite? _player2RookSprite;
        [SerializeField] private Sprite? _player2QueenSprite;
        [SerializeField] private Sprite? _player2KingSprite;

        /// <summary>選択中タイルハイライトカラー</summary>
        [Header("Highlight Colors")]
        [SerializeField] private Color _selectedHighlightColor = new Color(1.0f, 1.0f, 0.0f, 0.5f);

        /// <summary>移動可能マスハイライトカラー</summary>
        [SerializeField] private Color _validMoveHighlightColor = new Color(0.5f, 1.0f, 0.5f, 0.5f);

        /// <summary>攻撃可能マスハイライトカラー</summary>
        [SerializeField] private Color _attackMoveHighlightColor = new Color(1.0f, 0.3f, 0.3f, 0.5f);

        /// <summary>チェック状態ハイライトカラー</summary>
        [SerializeField] private Color _checkHighlightColor = new Color(1.0f, 0.0f, 0.0f, 0.7f);

        /// <summary>HexChessModelへの参照</summary>
        private HexChessModel? _chessModel;

        /// <summary>
        /// ボード全体を更新
        /// </summary>
        public override void UpdateBoard(HexGrid.HexGrid grid)
        {
            foreach (HexTile tile in grid.AllTiles)
            {
                UpdateTile(tile);
            }
        }

        /// <summary>
        /// 個別タイルを更新
        /// </summary>
        private void UpdateTile(HexTile tile)
        {
            if (!_tiles.TryGetValue(tile.Coordinate, out HexTileView? tileView))
                return;

            if (tile.IsEmpty)
            {
                tileView.HidePiece();
            }
            else
            {
                HexChessModel.ChessPiece? piece = _chessModel?.GetChessPiece(tile.Coordinate);
                if (piece == null)
                {
                    tileView.HidePiece();
                    return;
                }

                Sprite? sprite = GetSpriteForPiece(piece.Value);
                if (sprite != null)
                {
                    tileView.ShowPiece(sprite, Color.white);
                }
            }
        }

        /// <summary>
        /// 駒に対応するスプライトを取得
        /// </summary>
        private Sprite? GetSpriteForPiece(HexChessModel.ChessPiece piece)
        {
            if (piece.Player == PieceType.Player1)
            {
                return piece.Type switch
                {
                    HexChessModel.ChessPieceType.Pawn => _player1PawnSprite,
                    HexChessModel.ChessPieceType.Knight => _player1KnightSprite,
                    HexChessModel.ChessPieceType.Bishop => _player1BishopSprite,
                    HexChessModel.ChessPieceType.Rook => _player1RookSprite,
                    HexChessModel.ChessPieceType.Queen => _player1QueenSprite,
                    HexChessModel.ChessPieceType.King => _player1KingSprite,
                    _ => null
                };
            }
            else
            {
                return piece.Type switch
                {
                    HexChessModel.ChessPieceType.Pawn => _player2PawnSprite,
                    HexChessModel.ChessPieceType.Knight => _player2KnightSprite,
                    HexChessModel.ChessPieceType.Bishop => _player2BishopSprite,
                    HexChessModel.ChessPieceType.Rook => _player2RookSprite,
                    HexChessModel.ChessPieceType.Queen => _player2QueenSprite,
                    HexChessModel.ChessPieceType.King => _player2KingSprite,
                    _ => null
                };
            }
        }

        /// <summary>
        /// HexChessModelを設定
        /// </summary>
        public void SetChessModel(HexChessModel model)
        {
            _chessModel = model;
        }

        /// <summary>
        /// 選択中の駒をハイライト
        /// </summary>
        public void HighlightSelectedPiece(HexCoordinate coord)
        {
            ClearAllHighlights();
            HighlightTile(coord, _selectedHighlightColor);
        }

        /// <summary>
        /// 有効な移動先をハイライト
        /// </summary>
        public void HighlightValidMoves(List<HexCoordinate> validMoves)
        {
            foreach (HexCoordinate coord in validMoves)
            {
                // 敵駒がいる場合は攻撃色、いない場合は通常色
                HexChessModel.ChessPiece? targetPiece = _chessModel?.GetChessPiece(coord);
                if (targetPiece != null)
                {
                    HighlightTile(coord, _attackMoveHighlightColor);
                }
                else
                {
                    HighlightTile(coord, _validMoveHighlightColor);
                }
            }
        }

        /// <summary>
        /// チェック状態のキングをハイライト
        /// </summary>
        public void HighlightCheck(HexCoordinate kingPosition)
        {
            HighlightTile(kingPosition, _checkHighlightColor);
        }

        /// <summary>
        /// 駒取得アニメーション
        /// </summary>
        public void AnimateCapture(HexCoordinate coord)
        {
            // TODO: 取得アニメーション実装
            // 現在は即座に更新
            if (_tiles.TryGetValue(coord, out HexTileView? tileView))
            {
                tileView.HidePiece();
            }
        }

        /// <summary>
        /// 移動アニメーション
        /// </summary>
        public void AnimateMove(HexCoordinate from, HexCoordinate to)
        {
            // TODO: 移動アニメーション実装
            // 現在は即座に更新
            if (_chessModel != null && _chessModel.Grid != null)
            {
                HexTile? fromTile = _chessModel.Grid.GetTile(from);
                HexTile? toTile = _chessModel.Grid.GetTile(to);

                if (fromTile != null)
                    UpdateTile(fromTile);
                if (toTile != null)
                    UpdateTile(toTile);
            }
        }
    }
}
