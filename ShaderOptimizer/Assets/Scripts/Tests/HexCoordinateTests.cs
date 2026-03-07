#nullable enable

using NUnit.Framework;
using ShaderOp.Minigames.HexGrid;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Tests
{
    /// <summary>
    /// HexCoordinate 構造体のテスト
    /// </summary>
    [TestFixture]
    public class HexCoordinateTests
    {
        #region 基本プロパティテスト

        [Test]
        public void Constructor_CreatesValidCoordinate()
        {
            HexCoordinate coord = new HexCoordinate(2, -1);

            Assert.AreEqual(2, coord.Q);
            Assert.AreEqual(-1, coord.R);
            Assert.AreEqual(-1, coord.S); // S = -Q - R
        }

        [Test]
        public void Zero_ReturnsOrigin()
        {
            HexCoordinate zero = HexCoordinate.Zero;

            Assert.AreEqual(0, zero.Q);
            Assert.AreEqual(0, zero.R);
            Assert.AreEqual(0, zero.S);
        }

        [Test]
        public void S_Property_CalculatesCorrectly()
        {
            HexCoordinate coord = new HexCoordinate(3, -5);

            // S = -Q - R = -3 - (-5) = 2
            Assert.AreEqual(2, coord.S);
        }

        [Test]
        public void S_AlwaysSatisfiesConstraint()
        {
            // Q + R + S = 0 の制約を満たすか
            HexCoordinate coord1 = new HexCoordinate(1, 1);
            Assert.AreEqual(0, coord1.Q + coord1.R + coord1.S);

            HexCoordinate coord2 = new HexCoordinate(-2, 3);
            Assert.AreEqual(0, coord2.Q + coord2.R + coord2.S);

            HexCoordinate coord3 = new HexCoordinate(0, 0);
            Assert.AreEqual(0, coord3.Q + coord3.R + coord3.S);
        }

        #endregion

        #region 距離計算テスト

        [Test]
        public void DistanceTo_ToSelf_ReturnsZero()
        {
            HexCoordinate coord = new HexCoordinate(5, -3);

            Assert.AreEqual(0, coord.DistanceTo(coord));
        }

        [Test]
        public void DistanceTo_AdjacentTiles_ReturnsOne()
        {
            HexCoordinate center = HexCoordinate.Zero;

            // 6方向の隣接タイル
            Assert.AreEqual(1, center.DistanceTo(new HexCoordinate(1, 0)));
            Assert.AreEqual(1, center.DistanceTo(new HexCoordinate(1, -1)));
            Assert.AreEqual(1, center.DistanceTo(new HexCoordinate(0, -1)));
            Assert.AreEqual(1, center.DistanceTo(new HexCoordinate(-1, 0)));
            Assert.AreEqual(1, center.DistanceTo(new HexCoordinate(-1, 1)));
            Assert.AreEqual(1, center.DistanceTo(new HexCoordinate(0, 1)));
        }

        [Test]
        public void DistanceTo_CalculatesCorrectManhattanDistance()
        {
            HexCoordinate from = new HexCoordinate(0, 0);
            HexCoordinate to = new HexCoordinate(3, -2);

            // マンハッタン距離 = (|dq| + |dr| + |ds|) / 2
            // dq = 3, dr = -2, ds = -1
            // (3 + 2 + 1) / 2 = 3
            Assert.AreEqual(3, from.DistanceTo(to));
        }

        [Test]
        public void DistanceTo_IsSymmetric()
        {
            HexCoordinate coord1 = new HexCoordinate(2, -1);
            HexCoordinate coord2 = new HexCoordinate(-3, 4);

            Assert.AreEqual(coord1.DistanceTo(coord2), coord2.DistanceTo(coord1));
        }

        #endregion

        #region 隣接タイル取得テスト

        [Test]
        public void GetAllNeighbors_ReturnsExactlySixNeighbors()
        {
            HexCoordinate coord = HexCoordinate.Zero;

            HexCoordinate[] neighbors = coord.GetAllNeighbors();

            Assert.AreEqual(6, neighbors.Length);
        }

        [Test]
        public void GetAllNeighbors_AllNeighborsAreDistanceOne()
        {
            HexCoordinate coord = new HexCoordinate(2, -3);

            HexCoordinate[] neighbors = coord.GetAllNeighbors();

            foreach (var neighbor in neighbors)
            {
                Assert.AreEqual(1, coord.DistanceTo(neighbor));
            }
        }

        [Test]
        public void GetAllNeighbors_CoversSixDirections()
        {
            HexCoordinate center = HexCoordinate.Zero;
            HexCoordinate[] neighbors = center.GetAllNeighbors();

            // 期待される6方向の隣接座標
            HexCoordinate[] expected = new HexCoordinate[]
            {
                new HexCoordinate(1, 0),
                new HexCoordinate(1, -1),
                new HexCoordinate(0, -1),
                new HexCoordinate(-1, 0),
                new HexCoordinate(-1, 1),
                new HexCoordinate(0, 1)
            };

            foreach (var exp in expected)
            {
                CollectionAssert.Contains(neighbors, exp);
            }
        }

        [Test]
        public void GetNeighbor_ValidDirection_ReturnsCorrectNeighbor()
        {
            HexCoordinate coord = HexCoordinate.Zero;

            // 方向0: (1, 0)
            Assert.AreEqual(new HexCoordinate(1, 0), coord.GetNeighbor(0));

            // 方向1: (1, -1)
            Assert.AreEqual(new HexCoordinate(1, -1), coord.GetNeighbor(1));

            // 方向5: (0, 1)
            Assert.AreEqual(new HexCoordinate(0, 1), coord.GetNeighbor(5));
        }

        [Test]
        public void GetNeighbor_InvalidDirection_ThrowsException()
        {
            HexCoordinate coord = new HexCoordinate(2, -1);

            // 無効な方向（0-5以外）は例外を投げる
            Assert.Throws<ArgumentOutOfRangeException>(() => coord.GetNeighbor(6));
            Assert.Throws<ArgumentOutOfRangeException>(() => coord.GetNeighbor(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => coord.GetNeighbor(10));
        }

        #endregion

        #region 方向ベクトルテスト

        [Test]
        public void Directions_ContainsSixDirections()
        {
            // 静的な方向配列が6つの要素を持つか確認
            Assert.AreEqual(6, HexCoordinate.Directions.Length);
        }

        [Test]
        public void Directions_AllAreDistanceOne()
        {
            HexCoordinate origin = HexCoordinate.Zero;

            // すべての方向ベクトルが原点から距離1
            foreach (var direction in HexCoordinate.Directions)
            {
                Assert.AreEqual(1, origin.DistanceTo(direction));
            }
        }

        #endregion

        #region 等価性テスト

        [Test]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            HexCoordinate coord1 = new HexCoordinate(3, -2);
            HexCoordinate coord2 = new HexCoordinate(3, -2);

            Assert.IsTrue(coord1.Equals(coord2));
            Assert.IsTrue(coord1 == coord2);
            Assert.IsFalse(coord1 != coord2);
        }

        [Test]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            HexCoordinate coord1 = new HexCoordinate(1, 0);
            HexCoordinate coord2 = new HexCoordinate(0, 1);

            Assert.IsFalse(coord1.Equals(coord2));
            Assert.IsFalse(coord1 == coord2);
            Assert.IsTrue(coord1 != coord2);
        }

        [Test]
        public void GetHashCode_SameCoordinates_ReturnsSameHash()
        {
            HexCoordinate coord1 = new HexCoordinate(5, -3);
            HexCoordinate coord2 = new HexCoordinate(5, -3);

            Assert.AreEqual(coord1.GetHashCode(), coord2.GetHashCode());
        }

        [Test]
        public void GetHashCode_CanBeUsedInDictionary()
        {
            Dictionary<HexCoordinate, int> dict = new Dictionary<HexCoordinate, int>();

            HexCoordinate key = new HexCoordinate(2, -1);
            dict[key] = 42;

            Assert.AreEqual(42, dict[new HexCoordinate(2, -1)]);
        }

        #endregion

        #region ToString テスト

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            HexCoordinate coord = new HexCoordinate(3, -2);

            string result = coord.ToString();

            Assert.IsTrue(result.Contains("3"));
            Assert.IsTrue(result.Contains("-2"));
        }

        #endregion

        #region エッジケース

        [Test]
        public void LargeCoordinates_WorkCorrectly()
        {
            HexCoordinate large = new HexCoordinate(1000, -500);

            Assert.AreEqual(1000, large.Q);
            Assert.AreEqual(-500, large.R);
            Assert.AreEqual(-500, large.S);
        }

        [Test]
        public void NegativeCoordinates_WorkCorrectly()
        {
            HexCoordinate negative = new HexCoordinate(-10, -5);

            Assert.AreEqual(-10, negative.Q);
            Assert.AreEqual(-5, negative.R);
            Assert.AreEqual(15, negative.S);
        }

        [Test]
        public void Distance_LargeValues_DoesNotOverflow()
        {
            HexCoordinate coord1 = new HexCoordinate(10000, -5000);
            HexCoordinate coord2 = new HexCoordinate(-10000, 5000);

            int distance = coord1.Distance(coord2);

            Assert.Greater(distance, 0);
            Assert.Less(distance, int.MaxValue);
        }

        #endregion

        #region 方向ベクトル一貫性テスト

        [Test]
        public void GetNeighbor_ReturnsConsistentWithGetAllNeighbors()
        {
            HexCoordinate coord = new HexCoordinate(3, -1);
            HexCoordinate[] neighbors = coord.GetAllNeighbors();

            for (int dir = 0; dir < 6; dir++)
            {
                HexCoordinate neighbor = coord.GetNeighbor(dir);
                CollectionAssert.Contains(neighbors, neighbor, $"Direction {dir} neighbor not in GetAllNeighbors list");
            }
        }

        [Test]
        public void OppositeDirections_AreSymmetric()
        {
            HexCoordinate center = HexCoordinate.Zero;

            // 方向0と方向3は反対
            HexCoordinate dir0 = center.GetNeighbor(0);
            HexCoordinate dir3 = center.GetNeighbor(3);

            Assert.AreEqual(center, new HexCoordinate(
                (dir0.Q + dir3.Q) / 2,
                (dir0.R + dir3.R) / 2
            ));
        }

        #endregion
    }
}
