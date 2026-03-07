#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Hex Chess Controller
    /// </summary>
    public class HexChessController : HexBoardGameController
    {
        /// <summary>UI要素</summary>
        [Header("Chess UI")]
        [SerializeField] private TextMeshProUGUI? _currentPlayerText;
        [SerializeField] private TextMeshProUGUI? _statusText;
        [SerializeField] private TextMeshProUGUI? _capturedPiecesPlayer1Text;
        [SerializeField] private TextMeshProUGUI? _capturedPiecesPlayer2Text;

        /// <summary>リセットボタン</summary>
        [SerializeField] private Button? _resetButton;

        /// <summary>取られた駒のカウント</summary>
        private Dictionary<HexChessModel.ChessPieceType, int> _capturedByPlayer1 = new();
        private Dictionary<HexChessModel.ChessPieceType, int> _capturedByPlayer2 = new();

        protected override void Start()
        {
            // Model を作成
            _model = new HexChessModel();

            // View を自動検出
            _view = GetComponentInChildren<HexChessView>();

            // ViewにModelを設定
            HexChessView? chessView = View as HexChessView;
            HexChessModel? chessModel = Model as HexChessModel;
            if (chessView != null && chessModel != null)
            {
                chessView.SetChessModel(chessModel);
            }

            base.Start();

            // ボタンイベント設定
            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetClicked);
            }

            // 初期化
            InitializeCapturedPieces();
            UpdateStatusDisplay();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 取られた駒のカウントを初期化
        /// </summary>
        private void InitializeCapturedPieces()
        {
            _capturedByPlayer1.Clear();
            _capturedByPlayer2.Clear();

            foreach (HexChessModel.ChessPieceType type in System.Enum.GetValues(typeof(HexChessModel.ChessPieceType)))
            {
                if (type != HexChessModel.ChessPieceType.None)
                {
                    _capturedByPlayer1[type] = 0;
                    _capturedByPlayer2[type] = 0;
                }
            }

            UpdateCapturedPiecesDisplay();
        }

        protected override void OnTileClicked(HexCoordinate coord)
        {
            if (Model == null || View == null)
                return;

            HexChessModel? chessModel = Model as HexChessModel;
            HexChessView? chessView = View as HexChessView;

            if (chessModel == null || chessView == null)
                return;

            HexCoordinate? selectedPiece = chessModel.GetSelectedPiece();

            if (selectedPiece == null)
            {
                // 駒を選択
                HexChessModel.ChessPiece? piece = chessModel.GetChessPiece(coord);
                if (piece != null && piece.Value.Player == chessModel.CurrentPlayer)
                {
                    if (chessModel.SelectPiece(coord))
                    {
                        chessView.HighlightSelectedPiece(coord);

                        // 有効な移動先をハイライト
                        List<HexCoordinate> validMoves = chessModel.GetValidMoves(coord);
                        chessView.HighlightValidMoves(validMoves);

                        UpdateStatusDisplay();
                    }
                }
            }
            else
            {
                // 選択中の駒を移動または再選択
                HexChessModel.ChessPiece? targetPiece = chessModel.GetChessPiece(coord);

                if (targetPiece != null && targetPiece.Value.Player == chessModel.CurrentPlayer)
                {
                    // 別の駒を再選択
                    if (chessModel.SelectPiece(coord))
                    {
                        chessView.HighlightSelectedPiece(coord);

                        List<HexCoordinate> validMoves = chessModel.GetValidMoves(coord);
                        chessView.HighlightValidMoves(validMoves);

                        UpdateStatusDisplay();
                    }
                }
                else
                {
                    // 移動試行
                    HexChessModel.ChessPiece? capturedPiece = targetPiece;

                    if (chessModel.PlacePiece(coord))
                    {
                        // 取られた駒を記録
                        if (capturedPiece != null)
                        {
                            RecordCapturedPiece(capturedPiece.Value, chessModel.CurrentPlayer == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1);
                        }

                        View.UpdateBoard(chessModel.Grid);
                        chessView.ClearAllHighlights();
                        UpdateStatusDisplay();
                        UpdateCapturedPiecesDisplay();

                        // チェック状態のハイライト
                        HighlightCheckIfNeeded(chessModel);
                    }
                    else
                    {
                        // 無効な移動の場合は選択解除
                        chessModel.DeselectPiece();
                        chessView.ClearAllHighlights();
                        UpdateStatusDisplay();
                    }
                }
            }
        }

        /// <summary>
        /// 取られた駒を記録
        /// </summary>
        private void RecordCapturedPiece(HexChessModel.ChessPiece piece, PieceType capturedBy)
        {
            if (capturedBy == PieceType.Player1)
            {
                _capturedByPlayer1[piece.Type]++;
            }
            else
            {
                _capturedByPlayer2[piece.Type]++;
            }
        }

        /// <summary>
        /// チェック状態をハイライト
        /// </summary>
        private void HighlightCheckIfNeeded(HexChessModel chessModel)
        {
            HexChessView? chessView = View as HexChessView;
            if (chessView == null)
                return;

            // 現在のプレイヤーがチェックされている場合、キングをハイライト
            if (chessModel.IsPlayerInCheck(chessModel.CurrentPlayer))
            {
                // キングの位置を見つけてハイライト
                foreach (HexTile tile in chessModel.Grid.AllTiles)
                {
                    HexChessModel.ChessPiece? piece = chessModel.GetChessPiece(tile.Coordinate);
                    if (piece != null && piece.Value.Player == chessModel.CurrentPlayer && piece.Value.Type == HexChessModel.ChessPieceType.King)
                    {
                        chessView.HighlightCheck(tile.Coordinate);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// ステータス表示を更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            HexChessModel? chessModel = Model as HexChessModel;

            if (_currentPlayerText != null && Model != null)
            {
                _currentPlayerText.text = $"Current Turn: {Model.CurrentPlayer}";
            }

            if (_statusText != null && chessModel != null)
            {
                if (chessModel.State == GameState.Player1Won || chessModel.State == GameState.Player2Won)
                {
                    PieceType winner = chessModel.CurrentPlayer == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1;
                    _statusText.text = $"Checkmate! {winner} Wins!";
                }
                else if (chessModel.IsPlayerInCheck(chessModel.CurrentPlayer))
                {
                    _statusText.text = $"Check! {chessModel.CurrentPlayer} is in check!";
                }
                else if (chessModel.GetSelectedPiece() != null)
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
        /// 取られた駒の表示を更新
        /// </summary>
        private void UpdateCapturedPiecesDisplay()
        {
            if (_capturedPiecesPlayer1Text != null)
            {
                string captured = "Player 1 Captured:\n";
                foreach (var kvp in _capturedByPlayer1)
                {
                    if (kvp.Value > 0)
                    {
                        captured += $"{kvp.Key}: {kvp.Value}\n";
                    }
                }
                _capturedPiecesPlayer1Text.text = captured;
            }

            if (_capturedPiecesPlayer2Text != null)
            {
                string captured = "Player 2 Captured:\n";
                foreach (var kvp in _capturedByPlayer2)
                {
                    if (kvp.Value > 0)
                    {
                        captured += $"{kvp.Key}: {kvp.Value}\n";
                    }
                }
                _capturedPiecesPlayer2Text.text = captured;
            }
        }

        /// <summary>
        /// リセットボタンクリック時
        /// </summary>
        private void OnResetClicked()
        {
            ResetGame();
            InitializeCapturedPieces();
            UpdateStatusDisplay();

            HexChessView? chessView = View as HexChessView;
            if (chessView != null)
            {
                chessView.ClearAllHighlights();
            }
        }

        protected override void OnGameStateChanged(GameState newState)
        {
            base.OnGameStateChanged(newState);

            if (newState == GameState.GameOver)
            {
                UpdateStatusDisplay();

                HexChessModel? chessModel = Model as HexChessModel;
                if (chessModel != null)
                {
                    PieceType winner = chessModel.CurrentPlayer == PieceType.Player1 ? PieceType.Player2 : PieceType.Player1;
                    Debug.Log($"[HexChessController] Checkmate! {winner} Wins!");
                }
            }
        }

        protected override void OnPiecePlaced(HexCoordinate coord, PieceType piece)
        {
            base.OnPiecePlaced(coord, piece);
            UpdateStatusDisplay();
        }
    }
}
