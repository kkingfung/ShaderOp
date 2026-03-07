#nullable enable

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// Burstコンパイル対応のHexGrid高速計算ユーティリティ
    /// </summary>
    /// <remarks>
    /// Unity.Burstを使用してHexGridの距離計算やバッチ処理を高速化します。
    /// パフォーマンスクリティカルな処理に使用してください。
    /// </remarks>
    public static class HexGridBurstUtilities
    {
        /// <summary>
        /// 複数のHex座標間の距離をバッチ計算するJob
        /// </summary>
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public struct BatchDistanceJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<HexCoordinate> FromCoordinates;
            [ReadOnly] public NativeArray<HexCoordinate> ToCoordinates;
            [WriteOnly] public NativeArray<int> Distances;

            public void Execute(int index)
            {
                HexCoordinate from = FromCoordinates[index];
                HexCoordinate to = ToCoordinates[index];
                int dq = math.abs(from.Q - to.Q);
                int dr = math.abs(from.R - to.R);
                int ds = math.abs(from.S - to.S);
                Distances[index] = (dq + dr + ds) / 2;
            }
        }

        /// <summary>
        /// 指定半径内のHex座標を生成するJob
        /// </summary>
        /// <remarks>
        /// 結果配列は呼び出し側で適切なサイズ（GetRadiusCount()で計算）を事前に確保する必要があります。
        /// </remarks>
        [BurstCompile(CompileSynchronously = true)]
        public struct GenerateRadiusJob : IJob
        {
            [ReadOnly] public HexCoordinate Center;
            [ReadOnly] public int Radius;
            [WriteOnly] public NativeArray<HexCoordinate> Results;

            public void Execute()
            {
                int writeIndex = 0;
                for (int q = -Radius; q <= Radius; q++)
                {
                    int r1 = math.max(-Radius, -q - Radius);
                    int r2 = math.min(Radius, -q + Radius);
                    for (int r = r1; r <= r2; r++)
                    {
                        if (writeIndex < Results.Length)
                        {
                            Results[writeIndex++] = new HexCoordinate(Center.Q + q, Center.R + r);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 指定半径内のHex座標数を計算
        /// </summary>
        /// <param name="radius">半径</param>
        /// <returns>座標数</returns>
        public static int GetRadiusCount(int radius)
        {
            return 3 * radius * (radius + 1) + 1;
        }

        /// <summary>
        /// ワールド座標をHex座標にバッチ変換するJob
        /// </summary>
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public struct BatchWorldToHexJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> WorldPositions;
            [ReadOnly] public float HexSize;
            [WriteOnly] public NativeArray<HexCoordinate> HexCoordinates;

            public void Execute(int index)
            {
                float3 worldPos = WorldPositions[index];
                float q = (2.0f / 3.0f * worldPos.x) / HexSize;
                float r = (-1.0f / 3.0f * worldPos.x + math.sqrt(3.0f) / 3.0f * worldPos.z) / HexSize;
                float s = -q - r;
                
                int roundedQ = (int)math.round(q);
                int roundedR = (int)math.round(r);
                int roundedS = (int)math.round(s);
                
                float qDiff = math.abs(roundedQ - q);
                float rDiff = math.abs(roundedR - r);
                float sDiff = math.abs(roundedS - s);
                
                if (qDiff > rDiff && qDiff > sDiff)
                {
                    roundedQ = -roundedR - roundedS;
                }
                else if (rDiff > sDiff)
                {
                    roundedR = -roundedQ - roundedS;
                }
                
                HexCoordinates[index] = new HexCoordinate(roundedQ, roundedR);
            }
        }

        /// <summary>
        /// Hex座標をワールド座標にバッチ変換するJob
        /// </summary>
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public struct BatchHexToWorldJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<HexCoordinate> HexCoordinates;
            [ReadOnly] public float HexSize;
            [WriteOnly] public NativeArray<float3> WorldPositions;

            public void Execute(int index)
            {
                HexCoordinate coord = HexCoordinates[index];
                float x = HexSize * (3.0f / 2.0f * coord.Q);
                float z = HexSize * (math.sqrt(3.0f) / 2.0f * coord.Q + math.sqrt(3.0f) * coord.R);
                WorldPositions[index] = new float3(x, 0, z);
            }
        }

        /// <summary>
        /// 複数のHex座標の隣接座標をバッチ取得するJob
        /// </summary>
        /// <remarks>
        /// AllNeighborsのサイズはSourceCoordinates.Length * 6である必要があります。
        /// </remarks>
        [BurstCompile(CompileSynchronously = true)]
        public struct BatchGetNeighborsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<HexCoordinate> SourceCoordinates;
            [WriteOnly] public NativeArray<HexCoordinate> AllNeighbors;

            public void Execute(int index)
            {
                HexCoordinate source = SourceCoordinates[index];
                int baseIndex = index * 6;
                
                for (int direction = 0; direction < 6; direction++)
                {
                    int2 offset = GetDirectionOffset(direction);
                    AllNeighbors[baseIndex + direction] = new HexCoordinate(source.Q + offset.x, source.R + offset.y);
                }
            }

            /// <summary>
            /// 方向インデックスからオフセットを取得
            /// </summary>
            private int2 GetDirectionOffset(int direction)
            {
                switch (direction)
                {
                    case 0: return new int2(1, 0);    // 右
                    case 1: return new int2(1, -1);   // 右上
                    case 2: return new int2(0, -1);   // 左上
                    case 3: return new int2(-1, 0);   // 左
                    case 4: return new int2(-1, 1);   // 左下
                    case 5: return new int2(0, 1);    // 右下
                    default: return new int2(0, 0);
                }
            }
        }
    }
}
