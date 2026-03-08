#nullable enable

using UnityEngine;
using UnityEngine.UI;
using ShaderOp.Minigames.HexGrid;
using ShaderOp.Shaders;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Tic-Tac-Toe Hex 垂直スライス統合コンポーネント
    /// </summary>
    /// <remarks>
    /// Phase 1シェーダーシステムを実証するための完全な実装
    /// - HexTileInteractiveシェーダーでタイル状態制御
    /// - GamePiece2Dシェーダーでプレイヤー駒アニメーション
    /// - 縦画面向けUIレイアウト
    /// </remarks>
    public class TicTacToeHexVerticalSlice : MonoBehaviour
    {
        #region Inspector Fields

        [Header("ゲーム設定")]
        [SerializeField] private Transform? _boardParent;
        [SerializeField] private GameObject? _tilePrefab;
        [SerializeField] private GameObject? _player1PiecePrefab;
        [SerializeField] private GameObject? _player2PiecePrefab;

        [Header("マテリアル")]
        [SerializeField] private Material? _hexTileIdleMaterial;
        [SerializeField] private Material? _hexTileHoverMaterial;
        [SerializeField] private Material? _hexTileSelectedMaterial;
        [SerializeField] private Material? _player1PieceMaterial;
        [SerializeField] private Material? _player2PieceMaterial;

        [Header("レイアウト設定")]
        [SerializeField, Range(0.5f, 2f)] private float _tileSpacing = 1.2f;
        [SerializeField] private Vector2 _boardOffset = new Vector2(0, 2f);

        [Header("UI参照")]
        [SerializeField] private Canvas? _uiCanvas;
        [SerializeField] private Text? _turnIndicatorText;
        [SerializeField] private Text? _gameStatusText;
        [SerializeField] private Button? _resetButton;
        [SerializeField] private GameObject? _uiPanel;

        [Header("カラー設定")]
        [SerializeField] private Color _player1Color = new Color(0.2f, 0.5f, 1f); // 青
        [SerializeField] private Color _player2Color = new Color(1f, 0.3f, 0.3f); // 赤
        [SerializeField] private Color _hoverTint = new Color(1f, 1f, 0.8f);

        #endregion

        #region Private Fields

        private TicTacToeHexModel? _model;
        private Dictionary<HexCoordinate, GameObject> _tileObjects = new();
        private Dictionary<HexCoordinate, HexTileShaderController> _tileShaderControllers = new();
        private Dictionary<HexCoordinate, GameObject> _pieceObjects = new();
        private HexCoordinate? _hoveredTile;
        private bool _isGameActive = true;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeGame();
        }

        private void OnDestroy()
        {
            UnregisterModelEvents();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// ゲームを初期化
        /// </summary>
        private void InitializeGame()
        {
            // モデル作成
            _model = new TicTacToeHexModel();
            _model.Initialize();

            // イベント登録
            RegisterModelEvents();

            // ボード生成
            GenerateBoard();

            // UI初期化
            InitializeUI();

            // ゲーム開始
            _model.StartGame();
            UpdateUI();

            Debug.Log("[TicTacToeHexVerticalSlice] ゲーム初期化完了");
        }

        /// <summary>
        /// ヘックスボードを生成（3x3グリッド）
        /// </summary>
        private void GenerateBoard()
        {
            if (_model == null || _boardParent == null || _tilePrefab == null)
            {
                Debug.LogError("[TicTacToeHexVerticalSlice] 必要な参照が設定されていません");
                return;
            }

            // 既存のタイルをクリア
            ClearBoard();

            // ヘックスレイアウト設定（Flat-topped hexagon）
            float hexWidth = _tileSpacing;
            float hexHeight = _tileSpacing * Mathf.Sqrt(3f) / 2f;

            foreach (HexTile tile in _model.Grid.AllTiles)
            {
                // ヘックス座標をワールド座標に変換（3x3の長方形グリッド用）
                Vector3 worldPos = HexToWorldPosition(tile.Coordinate, hexWidth, hexHeight);
                worldPos += new Vector3(_boardOffset.x, _boardOffset.y, 0);

                // タイルGameObject生成
                GameObject tileObj = Instantiate(_tilePrefab, worldPos, Quaternion.identity, _boardParent);
                tileObj.name = $"HexTile_{tile.Coordinate}";

                // SpriteRendererとマテリアル設定
                SpriteRenderer? renderer = tileObj.GetComponent<SpriteRenderer>();
                if (renderer != null && _hexTileIdleMaterial != null)
                {
                    renderer.material = _hexTileIdleMaterial;
                }

                // HexTileShaderController追加/取得
                HexTileShaderController? shaderController = tileObj.GetComponent<HexTileShaderController>();
                if (shaderController == null)
                {
                    shaderController = tileObj.AddComponent<HexTileShaderController>();
                }

                // コライダー設定（クリック検出用）
                BoxCollider2D? collider = tileObj.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = tileObj.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(0.9f, 0.9f);
                }

                // タイルクリック用コンポーネント追加
                var clickHandler = tileObj.AddComponent<TileClickHandler>();
                clickHandler.Initialize(tile.Coordinate, this);

                // 辞書に登録
                _tileObjects[tile.Coordinate] = tileObj;
                _tileShaderControllers[tile.Coordinate] = shaderController;

                // 初期状態をNormalに設定
                shaderController.SetState(HexTileShaderController.TileState.Normal);
            }

            Debug.Log($"[TicTacToeHexVerticalSlice] {_tileObjects.Count}個のタイルを生成しました");
        }

        /// <summary>
        /// ヘックス座標をワールド座標に変換（長方形グリッド用）
        /// </summary>
        private Vector3 HexToWorldPosition(HexCoordinate coord, float hexWidth, float hexHeight)
        {
            // 長方形グリッド用の簡易変換（q, r座標系）
            float x = coord.Q * hexWidth * 0.75f;
            float y = (coord.R + coord.Q * 0.5f) * hexHeight;
            return new Vector3(x, y, 0);
        }

        /// <summary>
        /// ボードをクリア
        /// </summary>
        private void ClearBoard()
        {
            foreach (var tileObj in _tileObjects.Values)
            {
                if (tileObj != null)
                {
                    Destroy(tileObj);
                }
            }

            foreach (var pieceObj in _pieceObjects.Values)
            {
                if (pieceObj != null)
                {
                    Destroy(pieceObj);
                }
            }

            _tileObjects.Clear();
            _tileShaderControllers.Clear();
            _pieceObjects.Clear();
        }

        /// <summary>
        /// UIを初期化
        /// </summary>
        private void InitializeUI()
        {
            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetButtonClicked);
            }

            // キャンバスを縦画面向けに設定
            if (_uiCanvas != null)
            {
                var scaler = _uiCanvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920); // 縦画面
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
        }

        #endregion

        #region Model Events

        /// <summary>
        /// モデルイベントを登録
        /// </summary>
        private void RegisterModelEvents()
        {
            if (_model == null) return;

            _model.OnGameStateChanged += OnGameStateChanged;
            _model.OnTurnChanged += OnTurnChanged;
            _model.OnTileUpdated += OnTileUpdated;
        }

        /// <summary>
        /// モデルイベントを解除
        /// </summary>
        private void UnregisterModelEvents()
        {
            if (_model == null) return;

            _model.OnGameStateChanged -= OnGameStateChanged;
            _model.OnTurnChanged -= OnTurnChanged;
            _model.OnTileUpdated -= OnTileUpdated;
        }

        /// <summary>
        /// ゲーム状態変更時の処理
        /// </summary>
        private void OnGameStateChanged(GameState newState)
        {
            _isGameActive = (newState == GameState.Playing);

            switch (newState)
            {
                case GameState.Player1Won:
                    ShowGameResult("プレイヤー1の勝利！", _player1Color);
                    break;

                case GameState.Player2Won:
                    ShowGameResult("プレイヤー2の勝利！", _player2Color);
                    break;

                case GameState.Draw:
                    ShowGameResult("引き分け", Color.yellow);
                    break;

                case GameState.Playing:
                    if (_gameStatusText != null)
                    {
                        _gameStatusText.text = "";
                    }
                    break;
            }

            UpdateUI();
        }

        /// <summary>
        /// ターン変更時の処理
        /// </summary>
        private void OnTurnChanged(int newPlayerIndex)
        {
            UpdateUI();
        }

        /// <summary>
        /// タイル更新時の処理
        /// </summary>
        private void OnTileUpdated(HexCoordinate coord)
        {
            UpdateTileVisual(coord);
        }

        #endregion

        #region Tile Interaction

        /// <summary>
        /// タイルクリック時の処理
        /// </summary>
        public void OnTileClicked(HexCoordinate coord)
        {
            if (_model == null || !_isGameActive) return;

            if (_model.State != GameState.Playing) return;

            // タイルに駒を配置
            bool success = _model.ExecuteMove(coord, coord);

            if (success)
            {
                // 駒配置アニメーション
                PlacePieceAsync(coord, _model.CurrentPlayer == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1).Forget();
            }
            else
            {
                Debug.Log($"[TicTacToeHexVerticalSlice] 無効な手: {coord}");
            }
        }

        /// <summary>
        /// タイルホバー時の処理
        /// </summary>
        public void OnTileHovered(HexCoordinate coord)
        {
            if (_model == null || !_isGameActive) return;
            if (_hoveredTile == coord) return;

            // 前回のホバータイルをクリア
            if (_hoveredTile.HasValue && _tileShaderControllers.ContainsKey(_hoveredTile.Value))
            {
                var prevController = _tileShaderControllers[_hoveredTile.Value];
                var tile = _model.Grid.GetTile(_hoveredTile.Value);
                if (tile != null && tile.IsEmpty)
                {
                    prevController.SetState(HexTileShaderController.TileState.Normal);
                }
            }

            // 新しいホバータイルを設定
            _hoveredTile = coord;
            if (_tileShaderControllers.ContainsKey(coord))
            {
                var controller = _tileShaderControllers[coord];
                var tile = _model.Grid.GetTile(coord);
                if (tile != null && tile.IsEmpty && _model.State == GameState.Playing)
                {
                    controller.SetState(HexTileShaderController.TileState.Hover);
                    controller.SetHoverBrightness(1.2f);
                }
            }
        }

        /// <summary>
        /// タイルホバー解除時の処理
        /// </summary>
        public void OnTileHoverExit(HexCoordinate coord)
        {
            if (_hoveredTile == coord)
            {
                _hoveredTile = null;
            }

            if (_tileShaderControllers.ContainsKey(coord))
            {
                var controller = _tileShaderControllers[coord];
                var tile = _model?.Grid.GetTile(coord);
                if (tile != null && tile.IsEmpty)
                {
                    controller.SetState(HexTileShaderController.TileState.Normal);
                }
            }
        }

        #endregion

        #region Visual Updates

        /// <summary>
        /// タイルのビジュアルを更新
        /// </summary>
        private void UpdateTileVisual(HexCoordinate coord)
        {
            if (_model == null) return;

            HexTile? tile = _model.Grid.GetTile(coord);
            if (tile == null) return;

            // シェーダーコントローラーでタイル状態を更新
            if (_tileShaderControllers.ContainsKey(coord))
            {
                var controller = _tileShaderControllers[coord];

                if (!tile.IsEmpty)
                {
                    // 駒が配置されている場合は選択状態に
                    controller.SetState(HexTileShaderController.TileState.Selected);

                    // チームカラーを設定
                    Color teamColor = tile.Piece == PieceType.Player1 ? _player1Color : _player2Color;
                    controller.SetTeamColor(teamColor, 0.3f);
                }
            }
        }

        /// <summary>
        /// 駒を配置（アニメーション付き）
        /// </summary>
        private async UniTask PlacePieceAsync(HexCoordinate coord, PieceType pieceType)
        {
            if (!_tileObjects.ContainsKey(coord)) return;

            GameObject? piecePrefab = pieceType == PieceType.Player1 ? _player1PiecePrefab : _player2PiecePrefab;
            Material? pieceMaterial = pieceType == PieceType.Player1 ? _player1PieceMaterial : _player2PieceMaterial;
            Color pieceColor = pieceType == PieceType.Player1 ? _player1Color : _player2Color;

            if (piecePrefab == null || pieceMaterial == null) return;

            // タイルの位置に駒を生成
            Vector3 tilePos = _tileObjects[coord].transform.position;
            Vector3 piecePos = tilePos + new Vector3(0, 0, -0.5f); // タイルより手前に

            GameObject pieceObj = Instantiate(piecePrefab, piecePos, Quaternion.identity, _boardParent);
            pieceObj.name = $"Piece_{pieceType}_{coord}";

            // マテリアルとシェーダーアニメーター設定
            SpriteRenderer? renderer = pieceObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.material = pieceMaterial;
                renderer.sortingOrder = 10; // タイルより上に表示
            }

            GamePieceShaderAnimator? animator = pieceObj.GetComponent<GamePieceShaderAnimator>();
            if (animator == null)
            {
                animator = pieceObj.AddComponent<GamePieceShaderAnimator>();
            }

            // プレイヤーカラー設定
            animator.SetPlayerColor(pieceColor, 0.7f);

            // フェードインアニメーション
            await animator.FadeIn(0.3f);

            // 辞書に登録
            _pieceObjects[coord] = pieceObj;

            Debug.Log($"[TicTacToeHexVerticalSlice] 駒を配置: {coord}, {pieceType}");
        }

        /// <summary>
        /// UIを更新
        /// </summary>
        private void UpdateUI()
        {
            if (_model == null) return;

            // ターン表示
            if (_turnIndicatorText != null)
            {
                if (_model.State == GameState.Playing)
                {
                    string playerName = _model.CurrentPlayerIndex == 0 ? "プレイヤー1" : "プレイヤー2";
                    Color playerColor = _model.CurrentPlayerIndex == 0 ? _player1Color : _player2Color;

                    _turnIndicatorText.text = $"{playerName}のターン";
                    _turnIndicatorText.color = playerColor;
                }
            }
        }

        /// <summary>
        /// ゲーム結果を表示
        /// </summary>
        private void ShowGameResult(string message, Color color)
        {
            if (_gameStatusText != null)
            {
                _gameStatusText.text = message;
                _gameStatusText.color = color;
            }

            Debug.Log($"[TicTacToeHexVerticalSlice] ゲーム終了: {message}");
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// リセットボタンクリック時の処理
        /// </summary>
        private void OnResetButtonClicked()
        {
            Debug.Log("[TicTacToeHexVerticalSlice] ゲームをリセット");

            // モデルイベント解除
            UnregisterModelEvents();

            // ボードクリア
            ClearBoard();

            // 再初期化
            InitializeGame();
        }

        #endregion
    }

    /// <summary>
    /// タイルクリック検出用ヘルパーコンポーネント
    /// </summary>
    public class TileClickHandler : MonoBehaviour
    {
        private HexCoordinate _coordinate;
        private TicTacToeHexVerticalSlice? _gameController;

        public void Initialize(HexCoordinate coord, TicTacToeHexVerticalSlice controller)
        {
            _coordinate = coord;
            _gameController = controller;
        }

        private void OnMouseDown()
        {
            _gameController?.OnTileClicked(_coordinate);
        }

        private void OnMouseEnter()
        {
            _gameController?.OnTileHovered(_coordinate);
        }

        private void OnMouseExit()
        {
            _gameController?.OnTileHoverExit(_coordinate);
        }
    }
}
