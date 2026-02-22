#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// グリッド形状タイプ
    /// </summary>
    public enum GridShape
    {
        Rectangle,      // 長方形グリッド
        Hexagon,        // 六角形グリッド
        Triangle,       // 三角形グリッド
        Parallelogram   // 平行四辺形グリッド
    }

    /// <summary>
    /// ヘックスグリッド管理クラス
    /// </summary>
    /// <remarks>
    /// ヘックスタイルの生成、座標管理、タイルアクセスを担当
    /// </remarks>
    public class HexGrid
    {
        /// <summary>タイルのディクショナリ（座標ベースアクセス）</summary>
        private readonly Dictionary<HexCoordinate, HexTile> _tiles;

        /// <summary>ヘックスサイズ（半径）</summary>
        public float HexSize { get; private set; }

        /// <summary>グリッド形状</summary>
        public GridShape Shape { get; private set; }

        /// <summary>すべてのタイル</summary>
        public IEnumerable<HexTile> AllTiles => _tiles.Values;

        /// <summary>タイル数</summary>
        public int TileCount => _tiles.Count;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HexGrid(float hexSize = 1.0f)
        {
            _tiles = new Dictionary<HexCoordinate, HexTile>();
            HexSize = hexSize;
            Shape = GridShape.Rectangle;
        }

        /// <summary>
        /// 長方形グリッドを生成（Tic-Tac-Toe, Reversi用）
        /// </summary>
        public void GenerateRectangle(int width, int height)
        {
            _tiles.Clear();
            Shape = GridShape.Rectangle;

            for (int r = 0; r < height; r++)
            {
                for (int q = 0; q < width; q++)
                {
                    HexCoordinate coord = new HexCoordinate(q, r);
                    Vector3 worldPos = coord.ToWorldPosition(HexSize);
                    _tiles[coord] = new HexTile(coord, worldPos);
                }
            }
        }

        /// <summary>
        /// 六角形グリッドを生成（中国式チェッカー用）
        /// </summary>
        public void GenerateHexagon(int radius)
        {
            _tiles.Clear();
            Shape = GridShape.Hexagon;

            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Math.Max(-radius, -q - radius);
                int r2 = Math.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    HexCoordinate coord = new HexCoordinate(q, r);
                    Vector3 worldPos = coord.ToWorldPosition(HexSize);
                    _tiles[coord] = new HexTile(coord, worldPos);
                }
            }
        }

        /// <summary>
        /// 三角形グリッドを生成
        /// </summary>
        public void GenerateTriangle(int size)
        {
            _tiles.Clear();
            Shape = GridShape.Triangle;

            for (int q = 0; q <= size; q++)
            {
                for (int r = 0; r <= size - q; r++)
                {
                    HexCoordinate coord = new HexCoordinate(q, r);
                    Vector3 worldPos = coord.ToWorldPosition(HexSize);
                    _tiles[coord] = new HexTile(coord, worldPos);
                }
            }
        }

        /// <summary>
        /// 平行四辺形グリッドを生成（Checkers用）
        /// </summary>
        public void GenerateParallelogram(int width, int height)
        {
            _tiles.Clear();
            Shape = GridShape.Parallelogram;

            for (int q = 0; q < width; q++)
            {
                for (int r = 0; r < height; r++)
                {
                    HexCoordinate coord = new HexCoordinate(q, r);
                    Vector3 worldPos = coord.ToWorldPosition(HexSize);
                    _tiles[coord] = new HexTile(coord, worldPos);
                }
            }
        }

        /// <summary>
        /// 座標からタイルを取得
        /// </summary>
        public HexTile? GetTile(HexCoordinate coordinate)
        {
            return _tiles.TryGetValue(coordinate, out HexTile? tile) ? tile : null;
        }

        /// <summary>
        /// ワールド座標から最も近いタイルを取得
        /// </summary>
        public HexTile? GetTileFromWorldPosition(Vector3 worldPosition)
        {
            HexCoordinate coord = HexCoordinate.FromWorldPosition(worldPosition, HexSize);
            return GetTile(coord);
        }

        /// <summary>
        /// タイルが存在するかチェック
        /// </summary>
        public bool HasTile(HexCoordinate coordinate)
        {
            return _tiles.ContainsKey(coordinate);
        }

        /// <summary>
        /// 隣接タイルを取得
        /// </summary>
        public List<HexTile> GetNeighbors(HexCoordinate coordinate)
        {
            List<HexTile> neighbors = new List<HexTile>();
            HexCoordinate[] neighborCoords = coordinate.GetAllNeighbors();

            foreach (HexCoordinate neighborCoord in neighborCoords)
            {
                HexTile? tile = GetTile(neighborCoord);
                if (tile != null)
                {
                    neighbors.Add(tile);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// 指定方向の隣接タイルを取得
        /// </summary>
        public HexTile? GetNeighbor(HexCoordinate coordinate, int direction)
        {
            HexCoordinate neighborCoord = coordinate.GetNeighbor(direction);
            return GetTile(neighborCoord);
        }

        /// <summary>
        /// 2点間のパスを取得（直線）
        /// </summary>
        public List<HexTile> GetLinePath(HexCoordinate start, HexCoordinate end)
        {
            List<HexTile> path = new List<HexTile>();
            int distance = start.DistanceTo(end);

            for (int i = 0; i <= distance; i++)
            {
                float t = distance == 0 ? 0.0f : i / (float)distance;
                HexCoordinate coord = LerpHex(start, end, t);
                HexTile? tile = GetTile(coord);
                if (tile != null)
                {
                    path.Add(tile);
                }
            }

            return path;
        }

        /// <summary>
        /// ヘックス座標の線形補間
        /// </summary>
        private HexCoordinate LerpHex(HexCoordinate a, HexCoordinate b, float t)
        {
            float q = a.Q + (b.Q - a.Q) * t;
            float r = a.R + (b.R - a.R) * t;
            return HexCoordinate.RoundToHex(q, r);
        }

        /// <summary>
        /// すべてのタイルをリセット
        /// </summary>
        public void ResetAll()
        {
            foreach (HexTile tile in _tiles.Values)
            {
                tile.Reset();
            }
        }

        /// <summary>
        /// グリッドをクリア
        /// </summary>
        public void Clear()
        {
            _tiles.Clear();
        }

        /// <summary>
        /// 特定の駒を配置しているタイルを検索
        /// </summary>
        public List<HexTile> FindTilesByPiece(PieceType piece)
        {
            return _tiles.Values.Where(t => t.Piece == piece).ToList();
        }

        /// <summary>
        /// 空のタイルを検索
        /// </summary>
        public List<HexTile> FindEmptyTiles()
        {
            return _tiles.Values.Where(t => t.IsEmpty).ToList();
        }
    }

    /// <summary>
    /// HexCoordinate拡張（RoundToHexをpublicにするためのヘルパー）
    /// </summary>
    internal static class HexCoordinateExtensions
    {
        internal static HexCoordinate RoundToHex(float q, float r)
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
    }
}
