#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// ヘックスグリッドのAxial座標系（q, r）を表現
    /// </summary>
    /// <remarks>
    /// Axial Coordinate System: 2つの軸(q, r)で6角形グリッドを表現
    /// s = -q - r (3軸のうち2軸で表現可能)
    /// 参考: https://www.redblobgames.com/grids/hexagons/
    /// </remarks>
    [Serializable]
    public struct HexCoordinate : IEquatable<HexCoordinate>
    {
        /// <summary>Q軸座標（右上方向）</summary>
        public int Q;

        /// <summary>R軸座標（下方向）</summary>
        public int R;

        /// <summary>S軸座標（左上方向、計算プロパティ）</summary>
        public int S => -Q - R;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HexCoordinate(int q, int r)
        {
            Q = q;
            R = r;
        }

        /// <summary>
        /// 座標が有効かチェック（q + r + s = 0を満たすか）
        /// </summary>
        public bool IsValid() => Q + R + S == 0;

        /// <summary>
        /// 2つの座標間の距離を計算
        /// </summary>
        public int DistanceTo(HexCoordinate other)
        {
            return (Math.Abs(Q - other.Q) + Math.Abs(R - other.R) + Math.Abs(S - other.S)) / 2;
        }

        /// <summary>
        /// 隣接する6方向のオフセット（Flat-Top配置）
        /// </summary>
        public static readonly HexCoordinate[] Directions = new HexCoordinate[]
        {
            new HexCoordinate(1, 0),   // 右
            new HexCoordinate(1, -1),  // 右上
            new HexCoordinate(0, -1),  // 左上
            new HexCoordinate(-1, 0),  // 左
            new HexCoordinate(-1, 1),  // 左下
            new HexCoordinate(0, 1)    // 右下
        };

        /// <summary>
        /// 指定方向の隣接座標を取得
        /// </summary>
        public HexCoordinate GetNeighbor(int direction)
        {
            if (direction < 0 || direction >= Directions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(direction), "Direction must be 0-5");
            }

            HexCoordinate offset = Directions[direction];
            return new HexCoordinate(Q + offset.Q, R + offset.R);
        }

        /// <summary>
        /// すべての隣接座標を取得
        /// </summary>
        public HexCoordinate[] GetAllNeighbors()
        {
            HexCoordinate[] neighbors = new HexCoordinate[6];
            for (int i = 0; i < 6; i++)
            {
                neighbors[i] = GetNeighbor(i);
            }
            return neighbors;
        }

        /// <summary>
        /// ワールド座標に変換（Flat-Top配置、Y=0）
        /// </summary>
        /// <param name="hexSize">ヘックスのサイズ（半径）</param>
        public Vector3 ToWorldPosition(float hexSize)
        {
            float x = hexSize * (3.0f / 2.0f * Q);
            float z = hexSize * (Mathf.Sqrt(3.0f) / 2.0f * Q + Mathf.Sqrt(3.0f) * R);
            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// ワールド座標からヘックス座標に変換
        /// </summary>
        public static HexCoordinate FromWorldPosition(Vector3 worldPos, float hexSize)
        {
            float q = (2.0f / 3.0f * worldPos.x) / hexSize;
            float r = (-1.0f / 3.0f * worldPos.x + Mathf.Sqrt(3.0f) / 3.0f * worldPos.z) / hexSize;

            return RoundToHex(q, r);
        }

        /// <summary>
        /// 小数座標を最も近いヘックス座標に丸める
        /// </summary>
        private static HexCoordinate RoundToHex(float q, float r)
        {
            float s = -q - r;

            int roundedQ = Mathf.RoundToInt(q);
            int roundedR = Mathf.RoundToInt(r);
            int roundedS = Mathf.RoundToInt(s);

            float qDiff = Mathf.Abs(roundedQ - q);
            float rDiff = Mathf.Abs(roundedR - r);
            float sDiff = Mathf.Abs(roundedS - s);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                roundedQ = -roundedR - roundedS;
            }
            else if (rDiff > sDiff)
            {
                roundedR = -roundedQ - roundedS;
            }

            return new HexCoordinate(roundedQ, roundedR);
        }

        #region Equality & ToString

        public bool Equals(HexCoordinate other) => Q == other.Q && R == other.R;

        public override bool Equals(object? obj) => obj is HexCoordinate other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Q, R);

        public static bool operator ==(HexCoordinate left, HexCoordinate right) => left.Equals(right);

        public static bool operator !=(HexCoordinate left, HexCoordinate right) => !left.Equals(right);

        public override string ToString() => $"Hex({Q}, {R}, {S})";

        #endregion
    }
}
