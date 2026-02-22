#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// HexGridの3Dビジュアル化コンポーネント
    /// </summary>
    /// <remarks>
    /// HexGridデータをUnityシーン上に表示します
    /// タイルプレハブをインスタンス化して配置
    /// </remarks>
    public class HexGridVisualizer : MonoBehaviour
    {
        /// <summary>HexGridデータ</summary>
        private HexGrid? _grid;

        /// <summary>HexTileプレハブ</summary>
        [SerializeField] private GameObject? _tilePrefab;

        /// <summary>生成されたタイルオブジェクト</summary>
        private Dictionary<HexCoordinate, GameObject> _tileObjects = new();

        /// <summary>タイルのビジュアライザー</summary>
        private Dictionary<HexCoordinate, HexTileVisualizer> _tileVisualizers = new();

        /// <summary>
        /// HexGridを設定して表示
        /// </summary>
        public void SetGrid(HexGrid grid)
        {
            _grid = grid;
            GenerateVisuals();
        }

        /// <summary>
        /// ビジュアルを生成
        /// </summary>
        private void GenerateVisuals()
        {
            if (_grid == null || _tilePrefab == null)
            {
                Debug.LogError("[HexGridVisualizer] Grid or TilePrefab is null!");
                return;
            }

            // 既存のタイルをクリア
            ClearVisuals();

            // すべてのタイルをインスタンス化
            foreach (HexTile tile in _grid.AllTiles)
            {
                GameObject tileObject = Instantiate(_tilePrefab, transform);
                tileObject.transform.position = tile.WorldPosition;
                tileObject.name = $"HexTile_{tile.Coordinate.Q}_{tile.Coordinate.R}";

                // HexTileVisualizerを取得または追加
                HexTileVisualizer? visualizer = tileObject.GetComponent<HexTileVisualizer>();
                if (visualizer == null)
                {
                    visualizer = tileObject.AddComponent<HexTileVisualizer>();
                }

                visualizer.SetTile(tile);

                _tileObjects[tile.Coordinate] = tileObject;
                _tileVisualizers[tile.Coordinate] = visualizer;
            }

            Debug.Log($"[HexGridVisualizer] Generated {_tileObjects.Count} tile visuals");
        }

        /// <summary>
        /// すべてのビジュアルをクリア
        /// </summary>
        public void ClearVisuals()
        {
            foreach (GameObject tileObject in _tileObjects.Values)
            {
                if (tileObject != null)
                {
                    Destroy(tileObject);
                }
            }

            _tileObjects.Clear();
            _tileVisualizers.Clear();
        }

        /// <summary>
        /// 指定座標のタイルオブジェクトを取得
        /// </summary>
        public GameObject? GetTileObject(HexCoordinate coordinate)
        {
            _tileObjects.TryGetValue(coordinate, out GameObject? tileObject);
            return tileObject;
        }

        /// <summary>
        /// 指定座標のタイルビジュアライザーを取得
        /// </summary>
        public HexTileVisualizer? GetTileVisualizer(HexCoordinate coordinate)
        {
            _tileVisualizers.TryGetValue(coordinate, out HexTileVisualizer? visualizer);
            return visualizer;
        }

        /// <summary>
        /// すべてのビジュアルを更新
        /// </summary>
        public void UpdateAllVisuals()
        {
            foreach (HexTileVisualizer visualizer in _tileVisualizers.Values)
            {
                visualizer.UpdateVisuals();
            }
        }

        /// <summary>
        /// 特定のタイルのビジュアルを更新
        /// </summary>
        public void UpdateTileVisual(HexCoordinate coordinate)
        {
            if (_tileVisualizers.TryGetValue(coordinate, out HexTileVisualizer? visualizer))
            {
                visualizer.UpdateVisuals();
            }
        }

        private void OnDestroy()
        {
            ClearVisuals();
        }
    }
}
