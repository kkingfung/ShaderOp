#nullable enable

using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Tic-Tac-Toe Hex用View
    /// </summary>
    public class TicTacToeHexView : HexBoardGameView
    {
        /// <summary>Player 1用スプライト</summary>
        [SerializeField] private Sprite? _player1Sprite;

        /// <summary>Player 2用スプライト</summary>
        [SerializeField] private Sprite? _player2Sprite;

        /// <summary>Player 1カラー</summary>
        [SerializeField] private Color _player1Color = Color.blue;

        /// <summary>Player 2カラー</summary>
        [SerializeField] private Color _player2Color = Color.red;

        /// <summary>
        /// ボード全体を更新
        /// </summary>
        public override void UpdateBoard(HexGrid.HexGrid grid)
        {
            foreach (HexTile tile in grid.AllTiles)
            {
                if (tile.IsEmpty)
                {
                    HidePiece(tile.Coordinate);
                }
                else
                {
                    Sprite? sprite = tile.Piece == PieceType.Player1 ? _player1Sprite : _player2Sprite;
                    Color color = tile.Piece == PieceType.Player1 ? _player1Color : _player2Color;

                    if (sprite != null)
                    {
                        ShowPiece(tile.Coordinate, sprite, color);
                    }
                }
            }
        }

        /// <summary>
        /// プレイヤー駒を表示
        /// </summary>
        public void ShowPlayerPiece(HexCoordinate coord, PieceType piece)
        {
            Sprite? sprite = piece == PieceType.Player1 ? _player1Sprite : _player2Sprite;
            Color color = piece == PieceType.Player1 ? _player1Color : _player2Color;

            if (sprite != null)
            {
                ShowPiece(coord, sprite, color);
            }
        }
    }
}
