#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Checkers View
    /// </summary>
    public class HexCheckersView : HexBoardGameView
    {
        /// <summary>Player1駒スプライト（通常）</summary>
        [Header("Checkers Sprites")]
        [SerializeField] private Sprite? _player1NormalSprite;

        /// <summary>Player1駒スプライト（キング）</summary>
        [SerializeField] private Sprite? _player1KingSprite;

        /// <summary>Player2駒スプライト（通常）</summary>
        [SerializeField] private Sprite? _player2NormalSprite;

        /// <summary>Player2駒スプライト（キング）</summary>
        [SerializeField] private Sprite? _player2KingSprite;

        /// <summary>選択中タイルハイライトカラー</summary>
        [SerializeField] private Color _selectedHighlightColor = new Color(1.0f, 1.0f, 0.0f, 0.5f);

        /// <summary>移動可能マスハイライトカラー</summary>
        [SerializeField] private Color _validMoveHighlightColor = new Color(0.5f, 1.0f, 0.5f, 0.5f);

        /// <summary>ジャンプ可能マスハイライトカラー</summary>
        [SerializeField] private Color _validJumpHighlightColor = new Color(1.0f, 0.5f, 0.0f, 0.5f);

        /// <summary>HexCheckersModelへの参照</summary>
        private HexCheckersModel? _checkersModel;

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
                bool isKing = _checkersModel?.IsKing(tile.Coordinate) ?? false;

                if (tile.Piece == PieceType.Player1)
                {
                    Sprite? sprite = isKing ? _player1KingSprite : _player1NormalSprite;
                    if (sprite != null)
                    {
                        tileView.ShowPiece(sprite, Color.white);
                    }
                }
                else if (tile.Piece == PieceType.Player2)
                {
                    Sprite? sprite = isKing ? _player2KingSprite : _player2NormalSprite;
                    if (sprite != null)
                    {
                        tileView.ShowPiece(sprite, Color.white);
                    }
                }
            }
        }

        /// <summary>
        /// HexCheckersModelを設定
        /// </summary>
        public void SetCheckersModel(HexCheckersModel model)
        {
            _checkersModel = model;
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
        public void HighlightValidMoves(List<HexCoordinate> normalMoves, List<HexCoordinate> jumpMoves)
        {
            // 通常移動
            foreach (HexCoordinate coord in normalMoves)
            {
                HighlightTile(coord, _validMoveHighlightColor);
            }

            // ジャンプ移動（より強調）
            foreach (HexCoordinate coord in jumpMoves)
            {
                HighlightTile(coord, _validJumpHighlightColor);
            }
        }

        /// <summary>
        /// キング化アニメーション
        /// </summary>
        public void AnimatePromotion(HexCoordinate coord)
        {
            // TODO: キング化アニメーション実装
            // 現在は即座に更新
            HexTile? tile = _checkersModel?.Grid?.GetTile(coord);
            if (tile != null)
            {
                UpdateTile(tile);
            }
        }

        /// <summary>
        /// 駒捕獲アニメーション
        /// </summary>
        public void AnimateCapture(HexCoordinate coord)
        {
            // TODO: 捕獲アニメーション実装
            // 現在は即座に更新
            if (_tiles.TryGetValue(coord, out HexTileView? tileView))
            {
                tileView.HidePiece();
            }
        }
    }
}
