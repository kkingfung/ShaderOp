#nullable enable

using ShaderOp.Minigames.HexGrid;
using System.Collections.Generic;

namespace ShaderOp.Tests.Mocks
{
    /// <summary>
    /// テスト用 HexGrid モック
    /// </summary>
    public class MockHexGrid
    {
        /// <summary>
        /// テスト用の簡易六角形グリッドを作成
        /// </summary>
        public static HexGrid CreateSimpleHexagonGrid(int radius = 3)
        {
            var grid = new HexGrid();
            grid.GenerateHexagon(radius);
            return grid;
        }

        /// <summary>
        /// テスト用の簡易矩形グリッドを作成
        /// </summary>
        public static HexGrid CreateSimpleRectangleGrid(int width = 8, int height = 8)
        {
            var grid = new HexGrid();
            grid.GenerateRectangle(width, height);
            return grid;
        }

        /// <summary>
        /// 特定の配置でタイルに駒を配置
        /// </summary>
        public static void PlacePiecesInPattern(HexGrid grid, PieceType pieceType, List<HexCoordinate> coordinates)
        {
            foreach (var coord in coordinates)
            {
                var tile = grid.GetTile(coord);
                if (tile != null)
                {
                    tile.PlacePiece(pieceType);
                }
            }
        }

        /// <summary>
        /// グリッドをクリア（全タイルの駒を削除）
        /// </summary>
        public static void ClearGrid(HexGrid grid)
        {
            foreach (var tile in grid.AllTiles)
            {
                tile.RemovePiece();
            }
        }

        /// <summary>
        /// 市松模様でタイルに駒を配置
        /// </summary>
        public static void PlaceCheckeredPattern(HexGrid grid, PieceType pieceType)
        {
            foreach (var tile in grid.AllTiles)
            {
                // 座標の合計が偶数の場合に配置
                if ((tile.Coordinate.Q + tile.Coordinate.R) % 2 == 0)
                {
                    tile.PlacePiece(pieceType);
                }
            }
        }
    }
}
