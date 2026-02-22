#nullable enable

using System;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// ヘックスボードゲームのController基底クラス
    /// </summary>
    /// <remarks>
    /// ViewとModelの仲介を担当
    /// ゲーム固有のUIロジックは派生クラスで実装
    /// </remarks>
    public abstract class HexBoardGameController : MonoBehaviour
    {
        /// <summary>View参照</summary>
        [SerializeField] protected HexBoardGameView? _view;

        /// <summary>Model参照</summary>
        protected HexBoardGameModel? _model;

        /// <summary>選択中のタイル</summary>
        protected HexCoordinate? _selectedTile;

        /// <summary>ハイライトカラー（選択）</summary>
        [SerializeField] protected Color _selectionColor = Color.yellow;

        /// <summary>ハイライトカラー（有効手）</summary>
        [SerializeField] protected Color _validMoveColor = new Color(0.5f, 1.0f, 0.5f, 0.5f);

        /// <summary>ハイライトカラー（プレイヤー1）</summary>
        [SerializeField] protected Color _player1Color = Color.blue;

        /// <summary>ハイライトカラー（プレイヤー2）</summary>
        [SerializeField] protected Color _player2Color = Color.red;

        /// <summary>
        /// 初期化（派生クラスで_modelを設定）
        /// </summary>
        protected virtual void Start()
        {
            if (_view == null)
            {
                Debug.LogError("[HexBoardGameController] View is not assigned!");
                return;
            }

            // Viewイベント登録
            _view.OnTileClicked += OnTileClicked;

            // Modelイベント登録（派生クラスで_model設定後に呼ばれる想定）
            RegisterModelEvents();

            // ゲーム初期化
            InitializeGame();
        }

        /// <summary>
        /// Modelイベント登録
        /// </summary>
        protected virtual void RegisterModelEvents()
        {
            if (_model == null) return;

            _model.OnGameStateChanged += OnGameStateChanged;
            _model.OnTurnChanged += OnTurnChanged;
            _model.OnTileUpdated += OnTileUpdated;
        }

        /// <summary>
        /// Modelイベント解除
        /// </summary>
        protected virtual void UnregisterModelEvents()
        {
            if (_model == null) return;

            _model.OnGameStateChanged -= OnGameStateChanged;
            _model.OnTurnChanged -= OnTurnChanged;
            _model.OnTileUpdated -= OnTileUpdated;
        }

        /// <summary>
        /// ゲーム初期化（派生クラスで実装）
        /// </summary>
        protected abstract void InitializeGame();

        /// <summary>
        /// タイルクリック処理（派生クラスでオーバーライド可能）
        /// </summary>
        protected virtual void OnTileClicked(HexCoordinate coord)
        {
            if (_model == null || _view == null) return;

            if (_model.State != GameState.Playing)
            {
                return;
            }

            HexTile? clickedTile = _model.Grid.GetTile(coord);
            if (clickedTile == null) return;

            // 1回目: 駒選択
            if (_selectedTile == null)
            {
                if (clickedTile.Piece == _model.CurrentPlayer)
                {
                    _selectedTile = coord;
                    _view.HighlightTile(coord, _selectionColor);
                    HighlightValidMoves(coord);
                }
            }
            // 2回目: 移動先選択
            else
            {
                bool moveExecuted = _model.ExecuteMove(_selectedTile.Value, coord);

                // ハイライトクリア
                _view.ClearAllHighlights();
                _selectedTile = null;

                if (!moveExecuted)
                {
                    Debug.Log($"[HexBoardGameController] Invalid move: {_selectedTile} -> {coord}");
                }
            }
        }

        /// <summary>
        /// 有効手をハイライト（派生クラスでオーバーライド）
        /// </summary>
        protected virtual void HighlightValidMoves(HexCoordinate from)
        {
            // デフォルト実装: 隣接タイルをハイライト
            if (_model == null || _view == null) return;

            foreach (HexTile neighbor in _model.Grid.GetNeighbors(from))
            {
                if (_model.IsValidMove(from, neighbor.Coordinate))
                {
                    _view.HighlightTile(neighbor.Coordinate, _validMoveColor);
                }
            }
        }

        /// <summary>
        /// ゲーム状態変更時の処理（派生クラスでオーバーライド可能）
        /// </summary>
        protected virtual void OnGameStateChanged(GameState state)
        {
            Debug.Log($"[HexBoardGameController] Game State: {state}");

            switch (state)
            {
                case GameState.Player1Won:
                case GameState.Player2Won:
                case GameState.Player3Won:
                case GameState.Player4Won:
                    Debug.Log($"[HexBoardGameController] {state}!");
                    break;

                case GameState.Draw:
                    Debug.Log("[HexBoardGameController] Draw!");
                    break;
            }
        }

        /// <summary>
        /// ターン変更時の処理（派生クラスでオーバーライド可能）
        /// </summary>
        protected virtual void OnTurnChanged(int playerIndex)
        {
            Debug.Log($"[HexBoardGameController] Turn: Player {playerIndex + 1}");
            _view?.ClearAllHighlights();
            _selectedTile = null;
        }

        /// <summary>
        /// タイル更新時の処理（派生クラスでオーバーライド可能）
        /// </summary>
        protected virtual void OnTileUpdated(HexCoordinate coord)
        {
            if (_model == null || _view == null) return;

            HexTile? tile = _model.Grid.GetTile(coord);
            if (tile == null) return;

            // Viewを更新（駒表示/非表示）
            if (tile.IsEmpty)
            {
                _view.HidePiece(coord);
            }
            else
            {
                UpdatePieceView(coord, tile.Piece);
            }
        }

        /// <summary>
        /// 駒のビジュアル更新（派生クラスで実装）
        /// </summary>
        protected abstract void UpdatePieceView(HexCoordinate coord, PieceType piece);

        /// <summary>
        /// ゲームをリセット
        /// </summary>
        public virtual void ResetGame()
        {
            _model?.Reset();
            _view?.ClearAllHighlights();
            _selectedTile = null;

            if (_model != null && _view != null)
            {
                _view.UpdateBoard(_model.Grid);
            }
        }

        /// <summary>
        /// ゲームを開始
        /// </summary>
        public virtual void StartGame()
        {
            _model?.StartGame();
        }

        protected virtual void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnTileClicked -= OnTileClicked;
            }

            UnregisterModelEvents();
        }
    }
}
