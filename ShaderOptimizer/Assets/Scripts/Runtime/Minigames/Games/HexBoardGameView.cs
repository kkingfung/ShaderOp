#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// ヘックスタイルのビジュアル表現（MonoBehaviour）
    /// </summary>
    public class HexTileView : MonoBehaviour
    {
        /// <summary>タイルの座標</summary>
        public HexCoordinate Coordinate { get; private set; }

        /// <summary>タイルのRenderer</summary>
        private SpriteRenderer? _renderer;

        /// <summary>駒のRenderer</summary>
        private SpriteRenderer? _pieceRenderer;

        /// <summary>タイルクリックイベント</summary>
        public event Action? OnClicked;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(HexCoordinate coordinate)
        {
            Coordinate = coordinate;
            _renderer = GetComponent<SpriteRenderer>();

            // 駒用のGameObjectを作成
            GameObject pieceObj = new GameObject("Piece");
            pieceObj.transform.SetParent(transform);
            pieceObj.transform.localPosition = new Vector3(0, 0, -0.1f);
            _pieceRenderer = pieceObj.AddComponent<SpriteRenderer>();
            _pieceRenderer.enabled = false;
        }

        /// <summary>
        /// ハイライト設定
        /// </summary>
        public void SetHighlight(Color color)
        {
            if (_renderer != null)
            {
                _renderer.color = color;
            }
        }

        /// <summary>
        /// ハイライト解除
        /// </summary>
        public void ClearHighlight()
        {
            if (_renderer != null)
            {
                _renderer.color = Color.white;
            }
        }

        /// <summary>
        /// 駒を表示
        /// </summary>
        public void ShowPiece(Sprite sprite, Color tint)
        {
            if (_pieceRenderer != null)
            {
                _pieceRenderer.sprite = sprite;
                _pieceRenderer.color = tint;
                _pieceRenderer.enabled = true;
            }
        }

        /// <summary>
        /// 駒を非表示
        /// </summary>
        public void HidePiece()
        {
            if (_pieceRenderer != null)
            {
                _pieceRenderer.enabled = false;
            }
        }

        /// <summary>
        /// マウスクリック検出
        /// </summary>
        private void OnMouseDown()
        {
            OnClicked?.Invoke();
        }
    }

    /// <summary>
    /// ヘックスボードゲームのView基底クラス
    /// </summary>
    public abstract class HexBoardGameView : MonoBehaviour
    {
        /// <summary>タイルPrefab</summary>
        [SerializeField] protected GameObject? _tilePrefab;

        /// <summary>ボードの親Transform</summary>
        [SerializeField] protected Transform? _boardParent;

        /// <summary>タイルのディクショナリ</summary>
        protected Dictionary<HexCoordinate, HexTileView> _tiles = new();

        /// <summary>タイルクリックイベント</summary>
        public event Action<HexCoordinate>? OnTileClicked;

        /// <summary>
        /// ボードを初期化
        /// </summary>
        public virtual void InitializeBoard(HexGrid.HexGrid grid)
        {
            // 既存のタイルをクリア
            ClearBoard();

            if (_tilePrefab == null || _boardParent == null)
            {
                Debug.LogError("[HexBoardGameView] TilePrefab or BoardParent is not assigned!");
                return;
            }

            // タイルを生成
            foreach (HexTile tile in grid.AllTiles)
            {
                GameObject tileObj = Instantiate(_tilePrefab, _boardParent);
                tileObj.transform.position = tile.WorldPosition;

                HexTileView tileView = tileObj.GetComponent<HexTileView>();
                if (tileView == null)
                {
                    tileView = tileObj.AddComponent<HexTileView>();
                }

                tileView.Initialize(tile.Coordinate);
                tileView.OnClicked += () => HandleTileClicked(tile.Coordinate);

                _tiles[tile.Coordinate] = tileView;
            }
        }

        /// <summary>
        /// ボードをクリア
        /// </summary>
        protected virtual void ClearBoard()
        {
            foreach (HexTileView tileView in _tiles.Values)
            {
                if (tileView != null)
                {
                    Destroy(tileView.gameObject);
                }
            }
            _tiles.Clear();
        }

        /// <summary>
        /// タイルクリック処理
        /// </summary>
        protected virtual void HandleTileClicked(HexCoordinate coord)
        {
            OnTileClicked?.Invoke(coord);
        }

        /// <summary>
        /// タイルハイライト
        /// </summary>
        public virtual void HighlightTile(HexCoordinate coord, Color color)
        {
            if (_tiles.TryGetValue(coord, out HexTileView? tile))
            {
                tile.SetHighlight(color);
            }
        }

        /// <summary>
        /// ハイライト解除
        /// </summary>
        public virtual void ClearHighlight(HexCoordinate coord)
        {
            if (_tiles.TryGetValue(coord, out HexTileView? tile))
            {
                tile.ClearHighlight();
            }
        }

        /// <summary>
        /// すべてのハイライトを解除
        /// </summary>
        public virtual void ClearAllHighlights()
        {
            foreach (HexTileView tile in _tiles.Values)
            {
                tile.ClearHighlight();
            }
        }

        /// <summary>
        /// 駒を表示
        /// </summary>
        public virtual void ShowPiece(HexCoordinate coord, Sprite sprite, Color tint)
        {
            if (_tiles.TryGetValue(coord, out HexTileView? tile))
            {
                tile.ShowPiece(sprite, tint);
            }
        }

        /// <summary>
        /// 駒を非表示
        /// </summary>
        public virtual void HidePiece(HexCoordinate coord)
        {
            if (_tiles.TryGetValue(coord, out HexTileView? tile))
            {
                tile.HidePiece();
            }
        }

        /// <summary>
        /// ボード全体を更新（Model同期）
        /// </summary>
        public abstract void UpdateBoard(HexGrid.HexGrid grid);

        private void OnDestroy()
        {
            ClearBoard();
        }
    }
}
