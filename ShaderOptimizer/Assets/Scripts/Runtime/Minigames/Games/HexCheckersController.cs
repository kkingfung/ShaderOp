#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Checkers Controller
    /// </summary>
    public class HexCheckersController : HexBoardGameController
    {
        /// <summary>スコア表示テキスト</summary>
        [Header("Checkers UI")]
        [SerializeField] private TextMeshProUGUI? _player1ScoreText;
        [SerializeField] private TextMeshProUGUI? _player2ScoreText;
        [SerializeField] private TextMeshProUGUI? _currentPlayerText;
        [SerializeField] private TextMeshProUGUI? _statusText;

        /// <summary>リセットボタン</summary>
        [SerializeField] private Button? _resetButton;

        /// <summary>終了ターンボタン（連続ジャンプスキップ用）</summary>
        [SerializeField] private Button? _endTurnButton;

        protected override void Start()
        {
            // Model を作成
            _model = new HexCheckersModel();

            // View を自動検出
            _view = GetComponentInChildren<HexCheckersView>();

            // ViewにModelを設定
            HexCheckersView? checkersView = View as HexCheckersView;
            HexCheckersModel? checkersModel = Model as HexCheckersModel;
            if (checkersView != null && checkersModel != null)
            {
                checkersView.SetCheckersModel(checkersModel);
            }

            base.Start();

            // ボタンイベント設定
            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetClicked);
            }

            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);
                _endTurnButton.gameObject.SetActive(false);
            }

            UpdateScoreDisplay();
            UpdateStatusDisplay();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
            }

            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.RemoveAllListeners();
            }
        }

        protected override void OnTileClicked(HexCoordinate coord)
        {
            if (Model == null || View == null)
                return;

            HexCheckersModel? checkersModel = Model as HexCheckersModel;
            HexCheckersView? checkersView = View as HexCheckersView;

            if (checkersModel == null || checkersView == null)
                return;

            HexTile? clickedTile = checkersModel.Grid.GetTile(coord);
            if (clickedTile == null)
                return;

            HexCoordinate? selectedPiece = checkersModel.GetSelectedPiece();

            if (selectedPiece == null)
            {
                // 駒を選択
                if (!clickedTile.IsEmpty && clickedTile.Piece == checkersModel.CurrentPlayer)
                {
                    if (checkersModel.SelectPiece(coord))
                    {
                        checkersView.HighlightSelectedPiece(coord);
                        HighlightValidMovesForPiece(coord);
                        UpdateStatusDisplay();
                    }
                }
            }
            else
            {
                // 選択中の駒を移動または再選択
                if (clickedTile.IsEmpty)
                {
                    // 移動試行
                    if (checkersModel.PlacePiece(coord))
                    {
                        View.UpdateBoard(checkersModel.Grid);
                        checkersView.ClearAllHighlights();
                        UpdateScoreDisplay();
                        UpdateStatusDisplay();

                        // 連続ジャンプ中か確認
                        if (checkersModel.IsChainJumping())
                        {
                            HexCoordinate? newSelected = checkersModel.GetSelectedPiece();
                            if (newSelected != null)
                            {
                                checkersView.HighlightSelectedPiece(newSelected.Value);
                                HighlightValidMovesForPiece(newSelected.Value);
                            }

                            // 終了ターンボタンを表示
                            if (_endTurnButton != null)
                            {
                                _endTurnButton.gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            // 終了ターンボタンを非表示
                            if (_endTurnButton != null)
                            {
                                _endTurnButton.gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else if (clickedTile.Piece == checkersModel.CurrentPlayer)
                {
                    // 別の駒を再選択
                    if (checkersModel.SelectPiece(coord))
                    {
                        checkersView.HighlightSelectedPiece(coord);
                        HighlightValidMovesForPiece(coord);
                        UpdateStatusDisplay();
                    }
                }
                else
                {
                    // 選択解除
                    checkersModel.DeselectPiece();
                    checkersView.ClearAllHighlights();
                    UpdateStatusDisplay();
                }
            }
        }

        /// <summary>
        /// 指定座標の駒の有効な移動先をハイライト
        /// </summary>
        private void HighlightValidMovesForPiece(HexCoordinate from)
        {
            HexCheckersModel? checkersModel = Model as HexCheckersModel;
            HexCheckersView? checkersView = View as HexCheckersView;

            if (checkersModel == null || checkersView == null)
                return;

            List<HexCoordinate> normalMoves = new List<HexCoordinate>();
            List<HexCoordinate> jumpMoves = new List<HexCoordinate>();

            // 6方向それぞれをチェック
            for (int direction = 0; direction < 6; direction++)
            {
                // 通常移動（1マス）
                HexCoordinate normalTarget = from.GetNeighbor(direction);
                if (checkersModel.IsValidMove(from, normalTarget))
                {
                    HexTile? tile = checkersModel.Grid.GetTile(normalTarget);
                    if (tile != null && tile.IsEmpty)
                    {
                        normalMoves.Add(normalTarget);
                    }
                }

                // ジャンプ移動（2マス）
                HexCoordinate jumpTarget = from.GetNeighbor(direction).GetNeighbor(direction);
                if (checkersModel.IsValidMove(from, jumpTarget))
                {
                    HexTile? tile = checkersModel.Grid.GetTile(jumpTarget);
                    if (tile != null && tile.IsEmpty)
                    {
                        jumpMoves.Add(jumpTarget);
                    }
                }
            }

            checkersView.HighlightValidMoves(normalMoves, jumpMoves);
        }

        protected override void OnPiecePlaced(HexCoordinate coord, PieceType piece)
        {
            base.OnPiecePlaced(coord, piece);
            UpdateScoreDisplay();
        }

        /// <summary>
        /// スコア表示を更新
        /// </summary>
        private void UpdateScoreDisplay()
        {
            HexCheckersModel? checkersModel = Model as HexCheckersModel;
            if (checkersModel == null)
                return;

            (int player1Count, int player2Count) = checkersModel.GetPieceCounts();

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
        /// ステータス表示を更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            HexCheckersModel? checkersModel = Model as HexCheckersModel;

            if (_currentPlayerText != null && Model != null)
            {
                _currentPlayerText.text = $"Current Turn: {Model.CurrentPlayer}";
            }

            if (_statusText != null && checkersModel != null)
            {
                if (checkersModel.IsChainJumping())
                {
                    _statusText.text = "Chain Jump! Continue or End Turn";
                }
                else if (checkersModel.MustJump())
                {
                    _statusText.text = "Must Jump!";
                }
                else if (checkersModel.GetSelectedPiece() != null)
                {
                    _statusText.text = "Select destination";
                }
                else
                {
                    _statusText.text = "Select a piece to move";
                }
            }
        }

        /// <summary>
        /// リセットボタンクリック時
        /// </summary>
        private void OnResetClicked()
        {
            ResetGame();
            UpdateScoreDisplay();
            UpdateStatusDisplay();

            HexCheckersView? checkersView = View as HexCheckersView;
            if (checkersView != null)
            {
                checkersView.ClearAllHighlights();
            }

            if (_endTurnButton != null)
            {
                _endTurnButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 終了ターンボタンクリック時（連続ジャンプスキップ）
        /// </summary>
        private void OnEndTurnClicked()
        {
            HexCheckersModel? checkersModel = Model as HexCheckersModel;
            HexCheckersView? checkersView = View as HexCheckersView;

            if (checkersModel == null || checkersView == null)
                return;

            // 連続ジャンプを終了してターン交代
            checkersModel.DeselectPiece();

            // 強制的にターン交代させるため、リフレクションを使用
            checkersModel.GetType()
                .GetMethod("SwitchTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(checkersModel, null);

            checkersView.ClearAllHighlights();
            UpdateScoreDisplay();
            UpdateStatusDisplay();

            if (_endTurnButton != null)
            {
                _endTurnButton.gameObject.SetActive(false);
            }
        }

        protected override void OnGameStateChanged(GameState newState)
        {
            base.OnGameStateChanged(newState);

            if (newState == GameState.GameOver)
            {
                UpdateScoreDisplay();

                HexCheckersModel? checkersModel = Model as HexCheckersModel;
                if (checkersModel != null)
                {
                    (int player1Count, int player2Count) = checkersModel.GetPieceCounts();

                    if (player1Count == 0)
                    {
                        Debug.Log($"[HexCheckersController] Player 2 Wins! Final Score: {player1Count} vs {player2Count}");
                        if (_statusText != null)
                        {
                            _statusText.text = "Player 2 Wins!";
                        }
                    }
                    else if (player2Count == 0)
                    {
                        Debug.Log($"[HexCheckersController] Player 1 Wins! Final Score: {player1Count} vs {player2Count}");
                        if (_statusText != null)
                        {
                            _statusText.text = "Player 1 Wins!";
                        }
                    }
                    else
                    {
                        Debug.Log($"[HexCheckersController] Game Over! Final Score: {player1Count} vs {player2Count}");
                        if (_statusText != null)
                        {
                            _statusText.text = $"Game Over! {player1Count} vs {player2Count}";
                        }
                    }
                }
            }
        }
    }
}
