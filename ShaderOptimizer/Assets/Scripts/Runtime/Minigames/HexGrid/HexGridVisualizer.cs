#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Core.Services;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// HexGridの3Dビジュアル化コンポーネント
    /// </summary>
    /// <remarks>
    /// HexGridデータをUnityシーン上に表示します
    /// ObjectPoolServiceを使用してHexTileVisualizerを効率的に管理
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

        /// <summary>オブジェクトプールサービス</summary>
        private IObjectPoolService? _poolService;

        /// <summary>
        /// 初期化（ServiceLocatorからプールサービスを取得）
        /// </summary>
        private void Awake()
        {
            _poolService = ServiceLocator.Instance.Get<IObjectPoolService>();

            if (_poolService == null)
            {
                Debug.LogWarning("[HexGridVisualizer] IObjectPoolService not found. Pooling will be disabled (fallback to Instantiate).");
            }
        }

        /// <summary>
        /// HexGridを設定して表示
        /// </summary>
        public void SetGrid(HexGrid grid)
        {
            _grid = grid;
            GenerateVisuals();
        }

        /// <summary>
        /// ビジュアルを生成（オブジェクトプールを使用）
        /// </summary>
        private void GenerateVisuals()
        {
            if (_grid == null || _tilePrefab == null)
            {
                Debug.LogError("[HexGridVisualizer] Grid or TilePrefab is null!");
                return;
            }

            // 既存のタイルをクリア（プールに返却）
            ClearVisuals();

            // すべてのタイルを生成（プールから取得またはInstantiate）
            foreach (HexTile tile in _grid.AllTiles)
            {
                HexTileVisualizer? visualizer = null;
                GameObject? tileObject = null;

                // オブジェクトプールが利用可能な場合はプールから取得
                if (_poolService != null)
                {
                    visualizer = _poolService.Get<HexTileVisualizer>(tile.WorldPosition, Quaternion.identity);
                    tileObject = visualizer.gameObject;
                }
                else
                {
                    // フォールバック: 通常のInstantiate
                    tileObject = Instantiate(_tilePrefab, transform);
                    tileObject.transform.position = tile.WorldPosition;

                    visualizer = tileObject.GetComponent<HexTileVisualizer>();
                    if (visualizer == null)
                    {
                        visualizer = tileObject.AddComponent<HexTileVisualizer>();
                    }
                }

                tileObject.name = $"HexTile_{tile.Coordinate.Q}_{tile.Coordinate.R}";
                tileObject.transform.SetParent(transform);

                // タイルデータをビジュアライザーに設定
                visualizer.SetTile(tile);

                _tileObjects[tile.Coordinate] = tileObject;
                _tileVisualizers[tile.Coordinate] = visualizer;
            }

            Debug.Log($"[HexGridVisualizer] Generated {_tileObjects.Count} tile visuals (Pooling: {_poolService != null})");
        }

        /// <summary>
        /// すべてのビジュアルをクリア（プールに返却）
        /// </summary>
        public void ClearVisuals()
        {
            // オブジェクトプールが利用可能な場合はプールに返却
            if (_poolService != null)
            {
                foreach (var visualizer in _tileVisualizers.Values)
                {
                    if (visualizer != null)
                    {
                        _poolService.Return(visualizer);
                    }
                }
            }
            else
            {
                // フォールバック: 通常のDestroy
                foreach (GameObject tileObject in _tileObjects.Values)
                {
                    if (tileObject != null)
                    {
                        Destroy(tileObject);
                    }
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
