#nullable enable

using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using ShaderOp.Minigames.HexGrid;
using System.Diagnostics;

namespace ShaderOp.Tests.PlayMode
{
    /// <summary>
    /// HexGridBurstUtilities のパフォーマンステスト
    /// </summary>
    /// <remarks>
    /// Unity Performance Testing パッケージが利用できない場合は、
    /// Stopwatch を使用した簡易的なパフォーマンス測定を行います。
    /// </remarks>
    [TestFixture]
    [Category("Performance")]
    public class HexGridPerformanceTests
    {
        [Test]
        public void BatchDistanceJob_Performance_10000Operations()
        {
            const int count = 10000;
            var fromCoords = new NativeArray<HexCoordinate>(count, Allocator.TempJob);
            var toCoords = new NativeArray<HexCoordinate>(count, Allocator.TempJob);
            var distances = new NativeArray<int>(count, Allocator.TempJob);

            for (int i = 0; i < count; i++)
            {
                fromCoords[i] = new HexCoordinate(i % 100, i / 100);
                toCoords[i] = new HexCoordinate((i + 50) % 100, (i + 50) / 100);
            }

            // Stopwatch を使用した簡易パフォーマンス測定
            var stopwatch = Stopwatch.StartNew();

            var job = new HexGridBurstUtilities.BatchDistanceJob
            {
                FromCoordinates = fromCoords,
                ToCoordinates = toCoords,
                Distances = distances
            };
            job.Schedule(count, 64).Complete();

            stopwatch.Stop();
            UnityEngine.Debug.Log($"BatchDistanceJob: {count} operations completed in {stopwatch.ElapsedMilliseconds}ms");

            // パフォーマンスアサーション: 10000操作が100ms以内に完了すること
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "Performance regression detected");

            fromCoords.Dispose();
            toCoords.Dispose();
            distances.Dispose();
        }

        [Test]
        public void GenerateRadiusJob_Performance_Radius5()
        {
            int radius = 5;
            int expectedCount = HexGridBurstUtilities.GetRadiusCount(radius);
            var results = new NativeArray<HexCoordinate>(expectedCount, Allocator.TempJob);

            // Stopwatch を使用した簡易パフォーマンス測定
            var stopwatch = Stopwatch.StartNew();

            var job = new HexGridBurstUtilities.GenerateRadiusJob
            {
                Center = new HexCoordinate(0, 0),
                Radius = radius,
                Results = results
            };
            job.Schedule().Complete();

            stopwatch.Stop();
            UnityEngine.Debug.Log($"GenerateRadiusJob (radius={radius}): Completed in {stopwatch.ElapsedMilliseconds}ms");

            // パフォーマンスアサーション: 半径5の生成が10ms以内に完了すること
            Assert.Less(stopwatch.ElapsedMilliseconds, 10, "Performance regression detected");
            Assert.AreEqual(expectedCount, results.Length);

            results.Dispose();
        }

        [Test]
        public void BatchDistanceJob_Accuracy_MatchesNonBurstVersion()
        {
            const int count = 100;
            var fromCoords = new NativeArray<HexCoordinate>(count, Allocator.TempJob);
            var toCoords = new NativeArray<HexCoordinate>(count, Allocator.TempJob);
            var distances = new NativeArray<int>(count, Allocator.TempJob);

            for (int i = 0; i < count; i++)
            {
                fromCoords[i] = new HexCoordinate(i % 10, i / 10);
                toCoords[i] = new HexCoordinate((i + 5) % 10, (i + 5) / 10);
            }

            var job = new HexGridBurstUtilities.BatchDistanceJob
            {
                FromCoordinates = fromCoords,
                ToCoordinates = toCoords,
                Distances = distances
            };
            job.Schedule(count, 64).Complete();

            for (int i = 0; i < count; i++)
            {
                int expected = fromCoords[i].DistanceTo(toCoords[i]);
                Assert.AreEqual(expected, distances[i]);
            }

            fromCoords.Dispose();
            toCoords.Dispose();
            distances.Dispose();
        }
    }
}
