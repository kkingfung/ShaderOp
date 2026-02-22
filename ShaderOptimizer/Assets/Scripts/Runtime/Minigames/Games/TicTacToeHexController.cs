#nullable enable

using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Tic-Tac-Toe Hex Controller
    /// </summary>
    public class TicTacToeHexController : HexBoardGameController
    {
        /// <summary>Tic-Tac-Toe専用View</summary>
        private TicTacToeHexView? _ticTacToeView;

        /// <summary>
        /// ゲーム初期化
        /// </summary>
        protected override void InitializeGame()
        {
            // Model作成
            _model = new TicTacToeHexModel();
            _model.Initialize();

            _ticTacToeView = _view as TicTacToeHexView;

            // Modelイベント登録
            RegisterModelEvents();

            // ボード生成
            if (_view != null && _model != null)
            {
                _view.InitializeBoard(_model.Grid);
            }

            // ゲーム開始
            _model?.StartGame();
        }

        /// <summary>
        /// タイルクリック処理（オーバーライド）
        /// </summary>
        protected override void OnTileClicked(HexCoordinate coord)
        {
            if (_model == null || _view == null) return;

            if (_model.State != GameState.Playing)
            {
                return;
            }

            // Tic-Tac-Toeでは駒を配置するだけ（移動なし）
            // fromは現在のタイル座標をそのまま使用
            bool moveExecuted = _model.ExecuteMove(coord, coord);

            if (!moveExecuted)
            {
                Debug.Log($"[TicTacToeHexController] Invalid move: {coord}");
            }
        }

        /// <summary>
        /// 有効手ハイライト（Tic-Tac-Toeでは不要）
        /// </summary>
        protected override void HighlightValidMoves(HexCoordinate from)
        {
            // Tic-Tac-Toeでは有効手のハイライトは不要
            // すべての空きマスが有効手
        }

        /// <summary>
        /// 駒のビジュアル更新
        /// </summary>
        protected override void UpdatePieceView(HexCoordinate coord, PieceType piece)
        {
            if (_ticTacToeView != null)
            {
                _ticTacToeView.ShowPlayerPiece(coord, piece);
            }
        }

        /// <summary>
        /// ゲーム状態変更時の処理
        /// </summary>
        protected override void OnGameStateChanged(GameState state)
        {
            base.OnGameStateChanged(state);

            // UI更新やサウンド再生などを追加可能
            switch (state)
            {
                case GameState.Player1Won:
                    Debug.Log("[TicTacToeHex] Player 1 Wins!");
                    break;

                case GameState.Player2Won:
                    Debug.Log("[TicTacToeHex] Player 2 Wins!");
                    break;

                case GameState.Draw:
                    Debug.Log("[TicTacToeHex] It's a Draw!");
                    break;
            }
        }
    }
}
