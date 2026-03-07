#nullable enable

using NUnit.Framework;
using ShaderOp.Minigames.HexGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShaderOp.Tests
{
    /// <summary>
    /// HexGrid クラスのテスト
    /// </summary>
    [TestFixture]
    public class HexGridTests
    {
        private HexGrid? _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new HexGrid();
        }

        [TearDown]
        public void Teardown()
        {
            _grid = null;
        }

        #region 初期化テスト

        [Test]
        public void Constructor_CreatesEmptyGrid()
        {
            HexGrid grid = new HexGrid();

            Assert.AreEqual(0, grid.TileCount);
            Assert.AreEqual(GridShape.Rectangle, grid.Shape);
        }

        [Test]
        public void Constructor_WithHexSize_SetsHexSize()
        {
            HexGrid grid = new HexGrid(2.5f);

            Assert.AreEqual(2.5f, grid.HexSize);
        }

        #endregion

        #region Rectangle グリッド生成テスト

        [Test]
        public void GenerateRectangle_CreatesCorrectNumberOfTiles()
        {
            _grid!.GenerateRectangle(width: 5, height: 4);

            Assert.AreEqual(20, _grid.TileCount);
            Assert.AreEqual(GridShape.Rectangle, _grid.Shape);
        }

        [Test]
        public void GenerateRectangle_TilesAreAccessible()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            // すべての座標でタイルが取得できる
            for (int r = 0; r < 3; r++)
            {
                for (int q = 0; q < 3; q++)
                {
                    HexCoordinate coord = new HexCoordinate(q, r);
                    HexTile? tile = _grid.GetTile(coord);
                    Assert.IsNotNull(tile, $"Tile at {coord} is null");
                }
            }
        }

        [Test]
        public void GenerateRectangle_SingleTile_CreatesOneByOneGrid()
        {
            _grid!.GenerateRectangle(width: 1, height: 1);

            Assert.AreEqual(1, _grid.TileCount);
            Assert.IsNotNull(_grid.GetTile(new HexCoordinate(0, 0)));
        }

        #endregion

        #region Hexagon グリッド生成テスト

        [Test]
        public void GenerateHexagon_Radius0_CreatesOneTile()
        {
            _grid!.GenerateHexagon(radius: 0);

            Assert.AreEqual(1, _grid.TileCount);
            Assert.AreEqual(GridShape.Hexagon, _grid.Shape);
        }

        [Test]
        public void GenerateHexagon_Radius1_Creates7Tiles()
        {
            _grid!.GenerateHexagon(radius: 1);

            Assert.AreEqual(7, _grid.TileCount);
        }

        [Test]
        public void GenerateHexagon_Radius2_Creates19Tiles()
        {
            _grid!.GenerateHexagon(radius: 2);

            // 半径2の六角形は19タイル (1 + 6 + 12)
            Assert.AreEqual(19, _grid.TileCount);
        }

        [Test]
        public void GenerateHexagon_Radius3_Creates37Tiles()
        {
            _grid!.GenerateHexagon(radius: 3);

            // 半径3の六角形は37タイル (1 + 6 + 12 + 18)
            Assert.AreEqual(37, _grid.TileCount);
        }

        [Test]
        public void GenerateHexagon_CenterTileExists()
        {
            _grid!.GenerateHexagon(radius: 2);

            HexTile? centerTile = _grid.GetTile(HexCoordinate.Zero);
            Assert.IsNotNull(centerTile);
        }

        #endregion

        #region Parallelogram グリッド生成テスト

        [Test]
        public void GenerateParallelogram_CreatesCorrectNumberOfTiles()
        {
            _grid!.GenerateParallelogram(width: 5, height: 6);

            Assert.AreEqual(30, _grid.TileCount);
            Assert.AreEqual(GridShape.Parallelogram, _grid.Shape);
        }

        #endregion

        #region Triangle グリッド生成テスト

        [Test]
        public void GenerateTriangle_Size0_CreatesOneTile()
        {
            _grid!.GenerateTriangle(size: 0);

            Assert.AreEqual(1, _grid.TileCount);
            Assert.AreEqual(GridShape.Triangle, _grid.Shape);
        }

        [Test]
        public void GenerateTriangle_Size2_Creates6Tiles()
        {
            _grid!.GenerateTriangle(size: 2);

            // サイズ2の三角形は6タイル (3 + 2 + 1)
            Assert.AreEqual(6, _grid.TileCount);
        }

        #endregion

        #region タイル取得テスト

        [Test]
        public void GetTile_ValidCoordinate_ReturnsTile()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            HexTile? tile = _grid.GetTile(new HexCoordinate(1, 1));

            Assert.IsNotNull(tile);
            Assert.AreEqual(new HexCoordinate(1, 1), tile!.Coordinate);
        }

        [Test]
        public void GetTile_InvalidCoordinate_ReturnsNull()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            HexTile? tile = _grid.GetTile(new HexCoordinate(100, 100));

            Assert.IsNull(tile);
        }

        [Test]
        public void HasTile_ValidCoordinate_ReturnsTrue()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            bool exists = _grid.HasTile(new HexCoordinate(1, 1));

            Assert.IsTrue(exists);
        }

        [Test]
        public void HasTile_InvalidCoordinate_ReturnsFalse()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            bool exists = _grid.HasTile(new HexCoordinate(100, 100));

            Assert.IsFalse(exists);
        }

        #endregion

        #region 隣接タイル取得テスト

        [Test]
        public void GetNeighbors_CenterOfHexagon_Returns6Neighbors()
        {
            _grid!.GenerateHexagon(radius: 2);

            List<HexTile> neighbors = _grid.GetNeighbors(HexCoordinate.Zero);

            Assert.AreEqual(6, neighbors.Count);
        }

        [Test]
        public void GetNeighbors_CornerOfRectangle_ReturnsLessThan6()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            List<HexTile> neighbors = _grid.GetNeighbors(new HexCoordinate(0, 0));

            // コーナーは隣接タイルが6未満
            Assert.Less(neighbors.Count, 6);
        }

        [Test]
        public void GetNeighbors_EdgeTile_ReturnsOnlyValidNeighbors()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            List<HexTile> neighbors = _grid.GetNeighbors(new HexCoordinate(0, 1));

            // すべての隣接タイルがグリッド内に存在
            foreach (var neighbor in neighbors)
            {
                Assert.IsTrue(_grid.HasTile(neighbor.Coordinate));
            }
        }

        [Test]
        public void GetNeighbor_ValidDirection_ReturnsTile()
        {
            _grid!.GenerateHexagon(radius: 2);

            HexTile? neighbor = _grid.GetNeighbor(HexCoordinate.Zero, direction: 0);

            Assert.IsNotNull(neighbor);
            Assert.AreEqual(new HexCoordinate(1, 0), neighbor!.Coordinate);
        }

        [Test]
        public void GetNeighbor_OutOfBounds_ReturnsNull()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            // グリッド外の隣接タイル
            HexTile? neighbor = _grid.GetNeighbor(new HexCoordinate(0, 0), direction: 3);

            // direction 3 = (-1, 0) で範囲外
            Assert.IsNull(neighbor);
        }

        #endregion

        #region パス取得テスト

        [Test]
        public void GetLinePath_SameCoordinate_ReturnsSingleTile()
        {
            _grid!.GenerateHexagon(radius: 3);

            HexCoordinate start = new HexCoordinate(1, 0);
            List<HexTile> path = _grid.GetLinePath(start, start);

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(start, path[0].Coordinate);
        }

        [Test]
        public void GetLinePath_AdjacentTiles_ReturnsTwoTiles()
        {
            _grid!.GenerateHexagon(radius: 3);

            HexCoordinate start = HexCoordinate.Zero;
            HexCoordinate end = new HexCoordinate(1, 0);

            List<HexTile> path = _grid.GetLinePath(start, end);

            Assert.AreEqual(2, path.Count);
        }

        [Test]
        public void GetLinePath_DistantTiles_ReturnsCorrectPath()
        {
            _grid!.GenerateHexagon(radius: 3);

            HexCoordinate start = new HexCoordinate(0, 0);
            HexCoordinate end = new HexCoordinate(2, 0);

            List<HexTile> path = _grid.GetLinePath(start, end);

            // 距離2なので3タイル（スタート、中間、エンド）
            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(start, path[0].Coordinate);
            Assert.AreEqual(end, path[path.Count - 1].Coordinate);
        }

        #endregion

        #region グリッド操作テスト

        [Test]
        public void Clear_RemovesAllTiles()
        {
            _grid!.GenerateRectangle(width: 5, height: 5);
            Assert.AreEqual(25, _grid.TileCount);

            _grid.Clear();

            Assert.AreEqual(0, _grid.TileCount);
        }

        [Test]
        public void ResetAll_ResetsAllTilePieces()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            // いくつかのタイルに駒を配置
            HexTile? tile1 = _grid.GetTile(new HexCoordinate(0, 0));
            HexTile? tile2 = _grid.GetTile(new HexCoordinate(1, 1));
            tile1!.Piece = PieceType.Player1;
            tile2!.Piece = PieceType.Player2;

            _grid.ResetAll();

            // すべてのタイルがリセットされている
            foreach (var tile in _grid.AllTiles)
            {
                Assert.AreEqual(PieceType.None, tile.Piece);
            }
        }

        [Test]
        public void FindTilesByPiece_ReturnsMatchingTiles()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            // Player1の駒を配置
            _grid.GetTile(new HexCoordinate(0, 0))!.Piece = PieceType.Player1;
            _grid.GetTile(new HexCoordinate(1, 1))!.Piece = PieceType.Player1;
            _grid.GetTile(new HexCoordinate(2, 2))!.Piece = PieceType.Player2;

            List<HexTile> player1Tiles = _grid.FindTilesByPiece(PieceType.Player1);

            Assert.AreEqual(2, player1Tiles.Count);
            Assert.IsTrue(player1Tiles.All(t => t.Piece == PieceType.Player1));
        }

        [Test]
        public void FindEmptyTiles_ReturnsOnlyEmptyTiles()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);

            // いくつかのタイルに駒を配置
            _grid.GetTile(new HexCoordinate(0, 0))!.Piece = PieceType.Player1;
            _grid.GetTile(new HexCoordinate(1, 1))!.Piece = PieceType.Player2;

            List<HexTile> emptyTiles = _grid.FindEmptyTiles();

            Assert.AreEqual(7, emptyTiles.Count); // 9 - 2 = 7
            Assert.IsTrue(emptyTiles.All(t => t.IsEmpty));
        }

        #endregion

        #region AllTiles プロパティテスト

        [Test]
        public void AllTiles_ReturnsAllGeneratedTiles()
        {
            _grid!.GenerateRectangle(width: 4, height: 3);

            int count = 0;
            foreach (var tile in _grid.AllTiles)
            {
                count++;
            }

            Assert.AreEqual(12, count);
        }

        [Test]
        public void AllTiles_EmptyGrid_ReturnsEmpty()
        {
            var grid = new HexGrid();

            int count = 0;
            foreach (var tile in grid.AllTiles)
            {
                count++;
            }

            Assert.AreEqual(0, count);
        }

        #endregion

        #region ワールド座標変換テスト

        [Test]
        public void GetTileFromWorldPosition_ReturnsCorrectTile()
        {
            _grid!.GenerateHexagon(radius: 3);

            HexCoordinate expected = new HexCoordinate(1, 0);
            Vector3 worldPos = expected.ToWorldPosition(_grid.HexSize);

            HexTile? tile = _grid.GetTileFromWorldPosition(worldPos);

            Assert.IsNotNull(tile);
            Assert.AreEqual(expected, tile!.Coordinate);
        }

        [Test]
        public void GetTileFromWorldPosition_OutOfBounds_ReturnsNull()
        {
            _grid!.GenerateHexagon(radius: 1);

            Vector3 farAwayPos = new Vector3(1000, 0, 1000);

            HexTile? tile = _grid.GetTileFromWorldPosition(farAwayPos);

            Assert.IsNull(tile);
        }

        #endregion

        #region グリッド再生成テスト

        [Test]
        public void GenerateMultipleTimes_ClearsPreviousGrid()
        {
            _grid!.GenerateRectangle(width: 5, height: 5);
            Assert.AreEqual(25, _grid.TileCount);

            _grid.GenerateHexagon(radius: 2);
            Assert.AreEqual(19, _grid.TileCount);

            // 以前のタイルは存在しない
            Assert.IsFalse(_grid.HasTile(new HexCoordinate(4, 4)));
        }

        [Test]
        public void GenerateMultipleTimes_UpdatesShape()
        {
            _grid!.GenerateRectangle(width: 3, height: 3);
            Assert.AreEqual(GridShape.Rectangle, _grid.Shape);

            _grid.GenerateHexagon(radius: 2);
            Assert.AreEqual(GridShape.Hexagon, _grid.Shape);

            _grid.GenerateTriangle(size: 3);
            Assert.AreEqual(GridShape.Triangle, _grid.Shape);
        }

        #endregion
    }
}
