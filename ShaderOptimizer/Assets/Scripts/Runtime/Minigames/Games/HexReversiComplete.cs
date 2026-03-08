#nullable enable

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShaderOp.Minigames.HexGrid;
using ShaderOp.Shaders;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// HexReversi完全統合コントローラー
    /// </summary>
    /// <remarks>
    /// 7x7グリッド（半径3の六角形 = 37タイル）での六角形リバーシゲーム
    /// シェーダー統合、アニメーション、UI、AI機能を含む完全実装
    /// </remarks>
    public class HexReversiComplete : MonoBehaviour
    {
        #region Prefab References

        [Header("Prefab")]
        [SerializeField] private GameObject? _hexTilePrefab;
        [SerializeField] private GameObject? _gamePiecePrefab;

        [Header("Materials")]
        [SerializeField] private Material? _hexTileMaterial;
        [SerializeField] private Material? _player1PieceMaterial;
        [SerializeField] private Material? _player2PieceMaterial;

        [Header("Grid Settings")]
        [SerializeField, Range(1, 5)] private int _gridRadius = 3;
        [SerializeField] private float _hexSize = 1.0f;
        [SerializeField] private float _tileSpacing = 0.1f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI? _player1ScoreText;
        [SerializeField] private TextMeshProUGUI? _player2ScoreText;
        [SerializeField] private TextMeshProUGUI? _turnIndicatorText;
        [SerializeField] private TextMeshProUGUI? _gameResultText;
        [SerializeField] private Button? _resetButton;
        [SerializeField] private Toggle? _showHintsToggle;
        [SerializeField] private Button? _backToMenuButton;

        [Header("Animation Settings")]
        [SerializeField] private float _flipAnimationDuration = 0.3f;
        [SerializeField] private float _placePieceAnimationDuration = 0.5f;

        [Header("Color Settings")]
        [SerializeField] private Color _player1Color = new Color(0.2f, 0.5f, 1.0f); // 青
        [SerializeField] private Color _player2Color = new Color(1.0f, 0.3f, 0.3f); // 赤
        [SerializeField] private Color _validMoveGlowColor = new Color(0.5f, 1.0f, 0.5f); // 緑

        #endregion

        #region Private Fields

        private HexReversiModel? _model;
        private HexGrid.HexGrid? _grid;
        private Transform? _gridContainer;
        private Transform? _piecesContainer;

        // タイルとシェーダーコントローラーのマッピング
        private readonly Dictionary<HexCoordinate, GameObject> _tileObjects = new();
        private readonly Dictionary<HexCoordinate, HexTileShaderController> _tileShaders = new();
        private readonly Dictionary<HexCoordinate, GameObject> _pieceObjects = new();
        private readonly Dictionary<HexCoordinate, GamePieceShaderAnimator> _pieceAnimators = new();

        private bool _isAnimating = false;
        private bool _showHints = false;

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            InitializeGame();
            SetupUI();
        }

        void OnDestroy()
        {
            CleanupUI();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// ゲームを初期化
        /// </summary>
        private void InitializeGame()
        {
            // コンテナ作成
            _gridContainer = new GameObject("GridContainer").transform;
            _gridContainer.SetParent(transform);
            _gridContainer.localPosition = Vector3.zero;

            _piecesContainer = new GameObject("PiecesContainer").transform;
            _piecesContainer.SetParent(transform);
            _piecesContainer.localPosition = Vector3.zero;

            // モデル作成
            _model = new HexReversiModel();
            _model.Initialize();
            _grid = _model.Grid;

            // グリッド生成
            GenerateGridVisuals();

            // 初期駒を配置
            UpdateAllPieces();

            // UI更新
            UpdateScoreDisplay();
            UpdateTurnDisplay();
            UpdateGameResultDisplay();

            Debug.Log($"[HexReversiComplete] Game initialized with {_grid?.TileCount} tiles");
        }

        /// <summary>
        /// グリッドのビジュアルを生成
        /// </summary>
        private void GenerateGridVisuals()
        {
            if (_grid == null || _hexTilePrefab == null || _gridContainer == null)
            {
                Debug.LogError("[HexReversiComplete] Grid or prefab not initialized");
                return;
            }

            foreach (HexTile tile in _grid.AllTiles)
            {
                // タイルオブジェクト生成
                GameObject tileObj = Instantiate(_hexTilePrefab, _gridContainer);
                tileObj.name = $"HexTile_{tile.Coordinate}";
                tileObj.transform.position = tile.WorldPosition;

                // マテリアル設定
                if (_hexTileMaterial != null)
                {
                    Renderer renderer = tileObj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = _hexTileMaterial;
                    }
                }

                // シェーダーコントローラー取得/追加
                HexTileShaderController? shaderController = tileObj.GetComponent<HexTileShaderController>();
                if (shaderController == null)
                {
                    shaderController = tileObj.AddComponent<HexTileShaderController>();
                }

                // クリックイベント設定
                HexTileInteractive? interactive = tileObj.GetComponent<HexTileInteractive>();
                if (interactive == null)
                {
                    interactive = tileObj.AddComponent<HexTileInteractive>();
                }
                interactive.Initialize(tile.Coordinate, OnTileClicked, OnTileHoverEnter, OnTileHoverExit);

                // マッピングに追加
                _tileObjects[tile.Coordinate] = tileObj;
                _tileShaders[tile.Coordinate] = shaderController;
            }

            Debug.Log($"[HexReversiComplete] Generated {_tileObjects.Count} tile visuals");
        }

        /// <summary>
        /// UIイベントを設定
        /// </summary>
        private void SetupUI()
        {
            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetClicked);
            }

            if (_showHintsToggle != null)
            {
                _showHintsToggle.onValueChanged.AddListener(OnShowHintsToggled);
            }

            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            }
        }

        /// <summary>
        /// UIイベントをクリーンアップ
        /// </summary>
        private void CleanupUI()
        {
            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
            }

            if (_showHintsToggle != null)
            {
                _showHintsToggle.onValueChanged.RemoveAllListeners();
            }

            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.RemoveAllListeners();
            }
        }

        #endregion

        #region Tile Events

        /// <summary>
        /// タイルクリック時
        /// </summary>
        private async void OnTileClicked(HexCoordinate coord)
        {
            if (_model == null || _isAnimating)
                return;

            if (_model.State != GameState.Playing)
            {
                Debug.Log("[HexReversiComplete] Game is not in playing state");
                return;
            }

            // 有効な手かチェック
            if (!_model.IsValidMove(HexCoordinate.Zero, coord))
            {
                Debug.Log($"[HexReversiComplete] Invalid move: {coord}");
                return;
            }

            _isAnimating = true;

            // 反転される駒のリストを取得
            List<HexCoordinate> flippedTiles = GetFlippedTiles(coord, _model.CurrentPlayer);

            // 駒を配置（モデル更新）
            PieceType currentPlayer = _model.CurrentPlayer;
            bool success = _model.PlacePiece(coord);

            if (success)
            {
                // 配置アニメーション
                await PlacePieceAsync(coord, currentPlayer);

                // 反転アニメーション（同時実行）
                if (flippedTiles.Count > 0)
                {
                    await FlipPiecesAsync(flippedTiles, currentPlayer);
                }

                // UI更新
                UpdateScoreDisplay();
                UpdateTurnDisplay();

                // ヒント再表示
                if (_showHints)
                {
                    ShowValidMoveHints();
                }

                // ゲーム終了チェック
                if (_model.State == GameState.GameOver)
                {
                    UpdateGameResultDisplay();
                }
            }

            _isAnimating = false;
        }

        /// <summary>
        /// タイルホバー開始時
        /// </summary>
        private void OnTileHoverEnter(HexCoordinate coord)
        {
            if (_model == null || _isAnimating)
                return;

            if (_model.State != GameState.Playing)
                return;

            // 有効な手の場合のみホバー表示
            if (_model.IsValidMove(HexCoordinate.Zero, coord))
            {
                if (_tileShaders.TryGetValue(coord, out HexTileShaderController? shader))
                {
                    shader.SetState(HexTileShaderController.TileState.Hover);
                }
            }
        }

        /// <summary>
        /// タイルホバー終了時
        /// </summary>
        private void OnTileHoverExit(HexCoordinate coord)
        {
            if (_tileShaders.TryGetValue(coord, out HexTileShaderController? shader))
            {
                shader.SetState(HexTileShaderController.TileState.Normal);

                // ヒント表示中の場合は再度グロー表示
                if (_showHints && _model != null && _model.IsValidMove(HexCoordinate.Zero, coord))
                {
                    shader.ShowValidMoveGlow(true, 1.0f);
                }
            }
        }

        #endregion

        #region Game Logic

        /// <summary>
        /// 反転される駒のリストを取得
        /// </summary>
        private List<HexCoordinate> GetFlippedTiles(HexCoordinate coord, PieceType player)
        {
            List<HexCoordinate> flipped = new();

            if (_model == null || _grid == null)
                return flipped;

            PieceType opponent = player == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1;

            // 6方向それぞれでチェック
            for (int direction = 0; direction < 6; direction++)
            {
                List<HexCoordinate> directionFlipped = new();
                HexCoordinate current = coord.GetNeighbor(direction);

                while (true)
                {
                    HexTile? tile = _grid.GetTile(current);
                    if (tile == null || tile.IsEmpty)
                        break;

                    if (tile.Piece == opponent)
                    {
                        directionFlipped.Add(current);
                        current = current.GetNeighbor(direction);
                    }
                    else if (tile.Piece == player)
                    {
                        // 挟めた場合、このリストを追加
                        flipped.AddRange(directionFlipped);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return flipped;
        }

        #endregion

        #region Piece Management

        /// <summary>
        /// すべての駒を更新
        /// </summary>
        private void UpdateAllPieces()
        {
            if (_grid == null)
                return;

            foreach (HexTile tile in _grid.AllTiles)
            {
                if (!tile.IsEmpty)
                {
                    CreatePieceVisual(tile.Coordinate, tile.Piece);
                }
            }
        }

        /// <summary>
        /// 駒のビジュアルを作成
        /// </summary>
        private void CreatePieceVisual(HexCoordinate coord, PieceType piece)
        {
            if (_gamePiecePrefab == null || _piecesContainer == null)
                return;

            // 既存の駒を削除
            if (_pieceObjects.TryGetValue(coord, out GameObject? existingPiece))
            {
                Destroy(existingPiece);
                _pieceObjects.Remove(coord);
                _pieceAnimators.Remove(coord);
            }

            // タイルのワールド座標取得
            if (!_tileObjects.TryGetValue(coord, out GameObject? tileObj))
                return;

            Vector3 piecePosition = tileObj.transform.position + Vector3.up * 0.1f;

            // 駒オブジェクト生成
            GameObject pieceObj = Instantiate(_gamePiecePrefab, _piecesContainer);
            pieceObj.name = $"GamePiece_{coord}_{piece}";
            pieceObj.transform.position = piecePosition;

            // マテリアル設定
            Material? pieceMaterial = piece == PieceType.Player1 ? _player1PieceMaterial : _player2PieceMaterial;
            if (pieceMaterial != null)
            {
                Renderer renderer = pieceObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = pieceMaterial;
                }
            }

            // アニメーター取得/追加
            GamePieceShaderAnimator? animator = pieceObj.GetComponent<GamePieceShaderAnimator>();
            if (animator == null)
            {
                animator = pieceObj.AddComponent<GamePieceShaderAnimator>();
            }

            // プレイヤーカラー設定
            Color playerColor = piece == PieceType.Player1 ? _player1Color : _player2Color;
            animator.SetPlayerColor(playerColor, 0.6f);

            // マッピングに追加
            _pieceObjects[coord] = pieceObj;
            _pieceAnimators[coord] = animator;
        }

        /// <summary>
        /// 駒を配置（アニメーション付き）
        /// </summary>
        private async UniTask PlacePieceAsync(HexCoordinate coord, PieceType piece)
        {
            CreatePieceVisual(coord, piece);

            if (_pieceAnimators.TryGetValue(coord, out GamePieceShaderAnimator? animator))
            {
                await animator.FadeIn(_placePieceAnimationDuration);
            }
        }

        /// <summary>
        /// 駒を反転（アニメーション付き）
        /// </summary>
        private async UniTask FlipPiecesAsync(List<HexCoordinate> coords, PieceType newPiece)
        {
            List<UniTask> flipTasks = new();

            foreach (HexCoordinate coord in coords)
            {
                flipTasks.Add(FlipSinglePieceAsync(coord, newPiece));
            }

            // すべての反転アニメーションを同時実行
            await UniTask.WhenAll(flipTasks);
        }

        /// <summary>
        /// 単一駒の反転アニメーション
        /// </summary>
        private async UniTask FlipSinglePieceAsync(HexCoordinate coord, PieceType newPiece)
        {
            if (!_pieceAnimators.TryGetValue(coord, out GamePieceShaderAnimator? animator))
                return;

            // フェードアウト
            await animator.FadeOut(_flipAnimationDuration / 2);

            // 駒を再生成（新しいプレイヤーの色）
            CreatePieceVisual(coord, newPiece);

            // フェードイン
            if (_pieceAnimators.TryGetValue(coord, out GamePieceShaderAnimator? newAnimator))
            {
                await newAnimator.FadeIn(_flipAnimationDuration / 2);
            }
        }

        #endregion

        #region Hints

        /// <summary>
        /// 有効手のヒントを表示
        /// </summary>
        private void ShowValidMoveHints()
        {
            if (_model == null)
                return;

            List<HexCoordinate> validMoves = _model.GetValidMoves();

            foreach (HexCoordinate coord in validMoves)
            {
                if (_tileShaders.TryGetValue(coord, out HexTileShaderController? shader))
                {
                    shader.ShowValidMoveGlow(true, 1.5f);
                    shader.SetGlowSpeed(2.0f);
                }
            }

            Debug.Log($"[HexReversiComplete] Showing {validMoves.Count} valid move hints");
        }

        /// <summary>
        /// ヒントを非表示
        /// </summary>
        private void HideValidMoveHints()
        {
            foreach (var shader in _tileShaders.Values)
            {
                shader.ShowValidMoveGlow(false, 0f);
            }
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// スコア表示を更新
        /// </summary>
        private void UpdateScoreDisplay()
        {
            if (_model == null)
                return;

            (int player1Count, int player2Count) = _model.GetPieceCounts();

            if (_player1ScoreText != null)
            {
                _player1ScoreText.text = $"Player 1: {player1Count}";
            }

            if (_player2ScoreText != null)
            {
                _player2ScoreText.text = $"Player 2: {player2Count}";
            }
        }

        /// <summary>
        /// ターン表示を更新
        /// </summary>
        private void UpdateTurnDisplay()
        {
            if (_model == null || _turnIndicatorText == null)
                return;

            if (_model.State == GameState.Playing)
            {
                string playerName = _model.CurrentPlayer == PieceType.Player1 ? "Player 1" : "Player 2";
                _turnIndicatorText.text = $"Turn: {playerName}";
            }
        }

        /// <summary>
        /// ゲーム結果表示を更新
        /// </summary>
        private void UpdateGameResultDisplay()
        {
            if (_model == null || _gameResultText == null)
                return;

            if (_model.State == GameState.GameOver)
            {
                (int player1Count, int player2Count) = _model.GetPieceCounts();

                if (player1Count > player2Count)
                {
                    _gameResultText.text = $"Player 1 Wins!\n{player1Count} - {player2Count}";
                }
                else if (player2Count > player1Count)
                {
                    _gameResultText.text = $"Player 2 Wins!\n{player1Count} - {player2Count}";
                }
                else
                {
                    _gameResultText.text = $"Draw!\n{player1Count} - {player2Count}";
                }

                _gameResultText.gameObject.SetActive(true);
            }
            else
            {
                _gameResultText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region UI Events

        /// <summary>
        /// リセットボタンクリック時
        /// </summary>
        private void OnResetClicked()
        {
            Debug.Log("[HexReversiComplete] Reset button clicked");

            // すべての駒を削除
            foreach (var piece in _pieceObjects.Values)
            {
                Destroy(piece);
            }
            _pieceObjects.Clear();
            _pieceAnimators.Clear();

            // ヒントを非表示
            HideValidMoveHints();

            // モデルを再初期化
            _model?.Initialize();

            // 初期駒を配置
            UpdateAllPieces();

            // UI更新
            UpdateScoreDisplay();
            UpdateTurnDisplay();
            UpdateGameResultDisplay();
        }

        /// <summary>
        /// ヒント表示トグル時
        /// </summary>
        private void OnShowHintsToggled(bool isOn)
        {
            _showHints = isOn;

            if (_showHints)
            {
                ShowValidMoveHints();
            }
            else
            {
                HideValidMoveHints();
            }
        }

        /// <summary>
        /// メニューに戻るボタンクリック時
        /// </summary>
        private void OnBackToMenuClicked()
        {
            Debug.Log("[HexReversiComplete] Back to menu button clicked");
            // TODO: シーン遷移実装（ServiceLocatorを使用）
            // var sceneLoader = ServiceLocator.Instance.Get<ISceneLoaderService>();
            // sceneLoader?.LoadMainMenu();
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でのテスト用メソッド
        /// </summary>
        [ContextMenu("Force Update All Pieces")]
        private void ForceUpdateAllPieces()
        {
            UpdateAllPieces();
        }

        [ContextMenu("Show Valid Moves")]
        private void ForceShowHints()
        {
            ShowValidMoveHints();
        }

        [ContextMenu("Hide Valid Moves")]
        private void ForceHideHints()
        {
            HideValidMoveHints();
        }
#endif
    }

    /// <summary>
    /// ヘックスタイルのインタラクティブ処理
    /// </summary>
    public class HexTileInteractive : MonoBehaviour
    {
        private HexCoordinate _coordinate;
        private System.Action<HexCoordinate>? _onClicked;
        private System.Action<HexCoordinate>? _onHoverEnter;
        private System.Action<HexCoordinate>? _onHoverExit;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(
            HexCoordinate coordinate,
            System.Action<HexCoordinate> onClicked,
            System.Action<HexCoordinate> onHoverEnter,
            System.Action<HexCoordinate> onHoverExit)
        {
            _coordinate = coordinate;
            _onClicked = onClicked;
            _onHoverEnter = onHoverEnter;
            _onHoverExit = onHoverExit;
        }

        void OnMouseDown()
        {
            _onClicked?.Invoke(_coordinate);
        }

        void OnMouseEnter()
        {
            _onHoverEnter?.Invoke(_coordinate);
        }

        void OnMouseExit()
        {
            _onHoverExit?.Invoke(_coordinate);
        }
    }
}
