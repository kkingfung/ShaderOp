#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Reversi Controller
    /// </summary>
    public class HexReversiController : HexBoardGameController
    {
        /// <summary>スコア表示テキスト</summary>
        [Header("Reversi UI")]
        [SerializeField] private TextMeshProUGUI? _player1ScoreText;
        [SerializeField] private TextMeshProUGUI? _player2ScoreText;
        [SerializeField] private TextMeshProUGUI? _currentPlayerText;

        /// <summary>リセットボタン</summary>
        [SerializeField] private Button? _resetButton;

        /// <summary>ヒントボタン</summary>
        [SerializeField] private Button? _hintButton;

        /// <summary>ヒント表示中フラグ</summary>
        private bool _showingHint = false;

        protected override void Start()
        {
            // Model と View を自動検出
            Model = GetComponentInChildren<HexReversiModel>();
            View = GetComponentInChildren<HexReversiView>();

            if (Model == null)
            {
                Model = gameObject.AddComponent<HexReversiModel>();
            }

            base.Start();

            // ボタンイベント設定
            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetClicked);
            }

            if (_hintButton != null)
            {
                _hintButton.onClick.AddListener(OnHintClicked);
            }

            UpdateScoreDisplay();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
            }

            if (_hintButton != null)
            {
                _hintButton.onClick.RemoveAllListeners();
            }
        }

        protected override void OnTileClicked(HexCoordinate coord)
        {
            if (Model == null || View == null)
                return;

            HexReversiModel? reversiModel = Model as HexReversiModel;
            if (reversiModel == null)
                return;

            // 駒を配置
            if (reversiModel.PlacePiece(coord))
            {
                View.UpdateBoard(reversiModel.Grid);
                UpdateScoreDisplay();
                UpdateCurrentPlayerDisplay();

                // ヒントを非表示
                if (_showingHint)
                {
                    _showingHint = false;
                    View.ClearAllHighlights();
                }
            }
        }

        protected override void OnPiecePlaced(HexCoordinate coord, PieceType piece)
        {
            base.OnPiecePlaced(coord, piece);

            // 駒数を更新
            UpdateScoreDisplay();
        }

        /// <summary>
        /// スコア表示を更新
        /// </summary>
        private void UpdateScoreDisplay()
        {
            HexReversiModel? reversiModel = Model as HexReversiModel;
            if (reversiModel == null)
                return;

            (int player1Count, int player2Count) = reversiModel.GetPieceCounts();

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
        /// 現在のプレイヤー表示を更新
        /// </summary>
        private void UpdateCurrentPlayerDisplay()
        {
            if (_currentPlayerText != null && Model != null)
            {
                _currentPlayerText.text = $"Current Turn: {Model.CurrentPlayer}";
            }
        }

        /// <summary>
        /// リセットボタンクリック時
        /// </summary>
        private void OnResetClicked()
        {
            ResetGame();
            UpdateScoreDisplay();
            UpdateCurrentPlayerDisplay();

            if (_showingHint)
            {
                _showingHint = false;
                if (View != null)
                {
                    View.ClearAllHighlights();
                }
            }
        }

        /// <summary>
        /// ヒントボタンクリック時
        /// </summary>
        private void OnHintClicked()
        {
            HexReversiModel? reversiModel = Model as HexReversiModel;
            HexReversiView? reversiView = View as HexReversiView;

            if (reversiModel == null || reversiView == null)
                return;

            if (_showingHint)
            {
                // ヒントを非表示
                _showingHint = false;
                reversiView.ClearAllHighlights();
            }
            else
            {
                // ヒントを表示
                _showingHint = true;
                List<HexCoordinate> validMoves = reversiModel.GetValidMoves();
                reversiView.HighlightValidMoves(validMoves);
            }
        }

        protected override void OnGameStateChanged(GameState newState)
        {
            base.OnGameStateChanged(newState);

            if (newState == GameState.GameOver)
            {
                UpdateScoreDisplay();

                HexReversiModel? reversiModel = Model as HexReversiModel;
                if (reversiModel != null)
                {
                    (int player1Count, int player2Count) = reversiModel.GetPieceCounts();

                    if (player1Count > player2Count)
                    {
                        Debug.Log($"[HexReversiController] Player 1 Wins! Final Score: {player1Count} vs {player2Count}");
                    }
                    else if (player2Count > player1Count)
                    {
                        Debug.Log($"[HexReversiController] Player 2 Wins! Final Score: {player1Count} vs {player2Count}");
                    }
                    else
                    {
                        Debug.Log($"[HexReversiController] Draw! Final Score: {player1Count} vs {player2Count}");
                    }
                }
            }
        }
    }
}
