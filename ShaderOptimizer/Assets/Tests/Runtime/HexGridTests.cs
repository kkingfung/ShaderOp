#nullable enable

using NUnit.Framework;
using ShaderOp.Minigames.HexGrid;
using UnityEngine;

namespace ShaderOp.Tests.Runtime
{
    /// <summary>
    /// HexCoordinateのテスト
    /// </summary>
    public class HexCoordinateTests
    {
        [Test]
        public void HexCoordinate_IsValid_ReturnsTrue()
        {
            // Arrange
            HexCoordinate coord = new HexCoordinate(1, -1);

            // Act & Assert
            Assert.IsTrue(coord.IsValid(), "Coordinate should be valid (q + r + s = 0)");
        }

        [Test]
        public void HexCoordinate_DistanceTo_CalculatesCorrectly()
        {
            // Arrange
            HexCoordinate a = new HexCoordinate(0, 0);
            HexCoordinate b = new HexCoordinate(2, -2);

            // Act
            int distance = a.DistanceTo(b);

            // Assert
            Assert.AreEqual(2, distance, "Distance should be 2");
        }

        [Test]
        public void HexCoordinate_GetNeighbor_ReturnsCorrectNeighbor()
        {
            // Arrange
            HexCoordinate center = new HexCoordinate(0, 0);

            // Act
            HexCoordinate right = center.GetNeighbor(0);

            // Assert
            Assert.AreEqual(new HexCoordinate(1, 0), right, "Right neighbor should be (1, 0)");
        }

        [Test]
        public void HexCoordinate_GetAllNeighbors_ReturnsSixNeighbors()
        {
            // Arrange
            HexCoordinate center = new HexCoordinate(0, 0);

            // Act
            HexCoordinate[] neighbors = center.GetAllNeighbors();

            // Assert
            Assert.AreEqual(6, neighbors.Length, "Should have 6 neighbors");
        }

        [Test]
        public void HexCoordinate_ToWorldPosition_ReturnsCorrectPosition()
        {
            // Arrange
            HexCoordinate coord = new HexCoordinate(1, 0);
            float hexSize = 1.0f;

            // Act
            Vector3 worldPos = coord.ToWorldPosition(hexSize);

            // Assert
            Assert.AreEqual(1.5f, worldPos.x, 0.01f, "X position should be 1.5");
            Assert.AreEqual(0.0f, worldPos.y, 0.01f, "Y position should be 0");
        }

        [Test]
        public void HexCoordinate_Equality_WorksCorrectly()
        {
            // Arrange
            HexCoordinate a = new HexCoordinate(1, -1);
            HexCoordinate b = new HexCoordinate(1, -1);
            HexCoordinate c = new HexCoordinate(0, 0);

            // Assert
            Assert.IsTrue(a == b, "Equal coordinates should be equal");
            Assert.IsFalse(a == c, "Different coordinates should not be equal");
        }
    }

    /// <summary>
    /// HexGridのテスト
    /// </summary>
    public class HexGridTests
    {
        [Test]
        public void HexGrid_GenerateRectangle_CreatesCorrectNumberOfTiles()
        {
            // Arrange
            HexGrid grid = new HexGrid(1.0f);

            // Act
            grid.GenerateRectangle(3, 3);

            // Assert
            Assert.AreEqual(9, grid.TileCount, "3x3 grid should have 9 tiles");
        }

        [Test]
        public void HexGrid_GenerateHexagon_CreatesCorrectNumberOfTiles()
        {
            // Arrange
            HexGrid grid = new HexGrid(1.0f);

            // Act
            grid.GenerateHexagon(2);

            // Assert
            Assert.AreEqual(19, grid.TileCount, "Hexagon with radius 2 should have 19 tiles");
        }

        [Test]
        public void HexGrid_GetTile_ReturnsCorrectTile()
        {
            // Arrange
            HexGrid grid = new HexGrid(1.0f);
            grid.GenerateRectangle(3, 3);
            HexCoordinate coord = new HexCoordinate(1, 1);

            // Act
            HexTile? tile = grid.GetTile(coord);

            // Assert
            Assert.IsNotNull(tile, "Tile should exist");
            Assert.AreEqual(coord, tile!.Coordinate, "Tile coordinate should match");
        }

        [Test]
        public void HexGrid_GetNeighbors_ReturnsCorrectNeighbors()
        {
            // Arrange
            HexGrid grid = new HexGrid(1.0f);
            grid.GenerateRectangle(3, 3);
            HexCoordinate center = new HexCoordinate(1, 1);

            // Act
            var neighbors = grid.GetNeighbors(center);

            // Assert
            Assert.IsTrue(neighbors.Count > 0, "Center tile should have neighbors");
            Assert.IsTrue(neighbors.Count <= 6, "Should have at most 6 neighbors");
        }

        [Test]
        public void HexGrid_FindEmptyTiles_ReturnsAllTiles()
        {
            // Arrange
            HexGrid grid = new HexGrid(1.0f);
            grid.GenerateRectangle(3, 3);

            // Act
            var emptyTiles = grid.FindEmptyTiles();

            // Assert
            Assert.AreEqual(9, emptyTiles.Count, "All tiles should be empty initially");
        }

        [Test]
        public void HexGrid_ResetAll_ClearsAllTiles()
        {
            // Arrange
            HexGrid grid = new HexGrid(1.0f);
            grid.GenerateRectangle(3, 3);
            HexTile? tile = grid.GetTile(new HexCoordinate(0, 0));
            tile!.Piece = PieceType.Player1;

            // Act
            grid.ResetAll();

            // Assert
            Assert.IsTrue(tile.IsEmpty, "Tile should be empty after reset");
        }
    }

    /// <summary>
    /// HexTileのテスト
    /// </summary>
    public class HexTileTests
    {
        [Test]
        public void HexTile_PlacePiece_SucceedsOnEmptyTile()
        {
            // Arrange
            HexCoordinate coord = new HexCoordinate(0, 0);
            HexTile tile = new HexTile(coord, Vector3.zero);

            // Act
            bool result = tile.PlacePiece(PieceType.Player1);

            // Assert
            Assert.IsTrue(result, "Placing piece on empty tile should succeed");
            Assert.AreEqual(PieceType.Player1, tile.Piece, "Piece should be Player1");
        }

        [Test]
        public void HexTile_PlacePiece_FailsOnOccupiedTile()
        {
            // Arrange
            HexCoordinate coord = new HexCoordinate(0, 0);
            HexTile tile = new HexTile(coord, Vector3.zero);
            tile.PlacePiece(PieceType.Player1);

            // Act
            bool result = tile.PlacePiece(PieceType.Player2);

            // Assert
            Assert.IsFalse(result, "Placing piece on occupied tile should fail");
            Assert.AreEqual(PieceType.Player1, tile.Piece, "Piece should still be Player1");
        }

        [Test]
        public void HexTile_RemovePiece_ClearsTile()
        {
            // Arrange
            HexCoordinate coord = new HexCoordinate(0, 0);
            HexTile tile = new HexTile(coord, Vector3.zero);
            tile.PlacePiece(PieceType.Player1);

            // Act
            tile.RemovePiece();

            // Assert
            Assert.IsTrue(tile.IsEmpty, "Tile should be empty after removing piece");
        }

        [Test]
        public void HexTile_Reset_ClearsAllState()
        {
            // Arrange
            HexCoordinate coord = new HexCoordinate(0, 0);
            HexTile tile = new HexTile(coord, Vector3.zero);
            tile.PlacePiece(PieceType.Player1);
            tile.IsHighlighted = true;
            tile.IsSelected = true;

            // Act
            tile.Reset();

            // Assert
            Assert.IsTrue(tile.IsEmpty, "Tile should be empty");
            Assert.IsFalse(tile.IsHighlighted, "Tile should not be highlighted");
            Assert.IsFalse(tile.IsSelected, "Tile should not be selected");
        }
    }
}
