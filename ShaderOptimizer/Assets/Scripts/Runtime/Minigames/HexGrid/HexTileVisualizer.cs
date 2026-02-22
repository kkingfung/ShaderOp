#nullable enable

using UnityEngine;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// Hexタイルのビジュアル表現コンポーネント
    /// </summary>
    /// <remarks>
    /// HexTileデータとUnity GameObjectを接続します
    /// タイルの状態に応じてマテリアルとスプライトを更新
    /// </remarks>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HexTileVisualizer : MonoBehaviour
    {
        /// <summary>対応するHexTile</summary>
        private HexTile? _tile;

        /// <summary>スプライトレンダラー</summary>
        private SpriteRenderer? _spriteRenderer;

        /// <summary>通常状態のマテリアル</summary>
        [SerializeField] private Material? _normalMaterial;

        /// <summary>ハイライト状態のマテリアル</summary>
        [SerializeField] private Material? _highlightedMaterial;

        /// <summary>選択状態のマテリアル</summary>
        [SerializeField] private Material? _selectedMaterial;

        /// <summary>Player1のピーススプライト</summary>
        [SerializeField] private Sprite? _player1PieceSprite;

        /// <summary>Player2のピーススプライト</summary>
        [SerializeField] private Sprite? _player2PieceSprite;

        /// <summary>空のタイルスプライト</summary>
        [SerializeField] private Sprite? _emptyTileSprite;

        /// <summary>ピース表示用の子オブジェクトのSpriteRenderer</summary>
        private SpriteRenderer? _pieceSpriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // ピース表示用の子オブジェクトを作成
            GameObject pieceObject = new GameObject("Piece");
            pieceObject.transform.SetParent(transform);
            pieceObject.transform.localPosition = new Vector3(0, 0, -0.1f); // タイルより手前に表示
            _pieceSpriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
            _pieceSpriteRenderer.sortingOrder = _spriteRenderer != null ? _spriteRenderer.sortingOrder + 1 : 1;
        }

        /// <summary>
        /// HexTileを設定
        /// </summary>
        public void SetTile(HexTile tile)
        {
            _tile = tile;
            UpdateVisuals();
        }

        /// <summary>
        /// ビジュアルを更新
        /// </summary>
        public void UpdateVisuals()
        {
            if (_tile == null || _spriteRenderer == null || _pieceSpriteRenderer == null)
                return;

            // タイルの状態に応じてマテリアルを変更
            if (_tile.IsSelected && _selectedMaterial != null)
            {
                _spriteRenderer.material = _selectedMaterial;
            }
            else if (_tile.IsHighlighted && _highlightedMaterial != null)
            {
                _spriteRenderer.material = _highlightedMaterial;
            }
            else if (_normalMaterial != null)
            {
                _spriteRenderer.material = _normalMaterial;
            }

            // タイルのベーススプライトを設定
            if (_emptyTileSprite != null)
            {
                _spriteRenderer.sprite = _emptyTileSprite;
            }

            // ピーススプライトを設定
            switch (_tile.Piece)
            {
                case PieceType.Player1:
                    _pieceSpriteRenderer.sprite = _player1PieceSprite;
                    _pieceSpriteRenderer.enabled = true;
                    break;

                case PieceType.Player2:
                    _pieceSpriteRenderer.sprite = _player2PieceSprite;
                    _pieceSpriteRenderer.enabled = true;
                    break;

                case PieceType.None:
                default:
                    _pieceSpriteRenderer.sprite = null;
                    _pieceSpriteRenderer.enabled = false;
                    break;
            }
        }

        /// <summary>
        /// タイルがクリックされたときの処理
        /// </summary>
        private void OnMouseDown()
        {
            if (_tile != null)
            {
                // ここでコントローラーにクリックイベントを通知
                // イベントシステムを使用する場合はここに実装
                Debug.Log($"[HexTileVisualizer] Tile clicked: {_tile.Coordinate}");
            }
        }

        /// <summary>
        /// マウスホバー時の処理
        /// </summary>
        private void OnMouseEnter()
        {
            if (_tile != null && !_tile.IsHighlighted)
            {
                // 一時的なホバーエフェクトを追加する場合はここに実装
            }
        }

        /// <summary>
        /// マウスホバー解除時の処理
        /// </summary>
        private void OnMouseExit()
        {
            // ホバーエフェクトを解除する場合はここに実装
        }

        private void Update()
        {
            // 毎フレームビジュアルを更新（必要に応じて最適化）
            UpdateVisuals();
        }
    }
}
