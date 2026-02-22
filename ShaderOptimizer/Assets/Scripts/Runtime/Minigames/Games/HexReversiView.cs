#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Reversi View
    /// </summary>
    public class HexReversiView : HexBoardGameView
    {
        /// <summary>Player1駒スプライト</summary>
        [Header("Reversi Sprites")]
        [SerializeField] private Sprite? _player1Sprite;

        /// <summary>Player2駒スプライト</summary>
        [SerializeField] private Sprite? _player2Sprite;

        /// <summary>有効手ハイライトカラー</summary>
        [SerializeField] private Color _validMoveHighlightColor = new Color(0.5f, 1.0f, 0.5f, 0.5f);

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
            else if (tile.Piece == PieceType.Player1 && _player1Sprite != null)
            {
                tileView.ShowPiece(_player1Sprite, Color.white);
            }
            else if (tile.Piece == PieceType.Player2 && _player2Sprite != null)
            {
                tileView.ShowPiece(_player2Sprite, Color.white);
            }
        }

        /// <summary>
        /// 有効な手をハイライト
        /// </summary>
        public void HighlightValidMoves(List<HexCoordinate> validMoves)
        {
            // すべてのハイライトをクリア
            ClearAllHighlights();

            // 有効な手をハイライト
            foreach (HexCoordinate coord in validMoves)
            {
                HighlightTile(coord, _validMoveHighlightColor);
            }
        }

        /// <summary>
        /// 駒を反転アニメーション
        /// </summary>
        public void AnimateFlip(HexCoordinate coord, PieceType newPiece)
        {
            // TODO: 反転アニメーション実装
            // 現在は即座に更新
            HexTile? tile = _tiles.ContainsKey(coord) ? Grid?.GetTile(coord) : null;
            if (tile != null)
            {
                UpdateTile(tile);
            }
        }
    }
}
