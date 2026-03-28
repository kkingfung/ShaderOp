#nullable enable

using UnityEngine;
using ShaderOp.Minigames.HexGrid;
using ShaderOp.Core.Services;
using Cysharp.Threading.Tasks;
using System;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// Tic-Tac-Toe Hex オンライン対応Controller
    /// Phase 5 Week 1で実装
    /// </summary>
    /// <remarks>
    /// IGameSyncServiceと統合してオンラインマルチプレイに対応。
    /// オフラインモードでも動作可能（IGameSyncServiceがnullまたはIsSyncEnabled=falseの場合）。
    /// Phase 4の最適化（Direction-Based、Attack Map等）を継承し、
    /// 60fps維持したままオンラインプレイを実現。
    /// </remarks>
    public class TicTacToeHexOnlineController : HexBoardGameController
    {
        /// <summary>Tic-Tac-Toe専用View</summary>
        private TicTacToeHexView? _ticTacToeView;

        /// <summary>ゲーム同期サービス</summary>
        private IGameSyncService? _gameSyncService;

        /// <summary>ネットワークサービス</summary>
        private INetworkService? _networkService;

        /// <summary>オンラインモードか</summary>
        private bool IsOnlineMode => _gameSyncService?.IsSyncEnabled == true;

        /// <summary>
        /// ゲーム初期化
        /// </summary>
        protected override void InitializeGame()
        {
            // サービス取得
            _gameSyncService = ServiceLocator.Instance.Get<IGameSyncService>();
            _networkService = ServiceLocator.Instance.Get<INetworkService>();

            if (_gameSyncService != null)
            {
                _gameSyncService.GameType = "TicTacToeHex";
                
                // イベント登録
                _gameSyncService.OnMoveReceived += OnOpponentMoveReceived;
                _gameSyncService.OnGameStarted += OnOnlineGameStarted;
                _gameSyncService.OnGameEnded += OnOnlineGameEnded;
                _gameSyncService.OnTurnChanged += OnTurnChanged;
                _gameSyncService.OnResetRequested += OnResetRequested;

                Debug.Log($"[TicTacToeHexOnline] IGameSyncService登録完了: IsOnline={IsOnlineMode}");
            }
            else
            {
                Debug.Log("[TicTacToeHexOnline] IGameSyncService未登録（オフラインモード）");
            }

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

            // オンラインモードでは自動ゲーム開始を待つ
            if (IsOnlineMode)
            {
                Debug.Log("[TicTacToeHexOnline] オンラインモード: ゲーム開始待機中...");
                // OnOnlineGameStarted()でゲーム開始される
            }
            else
            {
                // オフラインモードではすぐに開始
                _model?.StartGame();
                Debug.Log("[TicTacToeHexOnline] オフラインモード: ゲーム開始");
            }
        }

        /// <summary>
        /// タイルクリック処理（オンライン対応）
        /// </summary>
        protected override void OnTileClicked(HexCoordinate coord)
        {
            if (_model == null || _view == null) return;

            if (_model.State != GameState.Playing)
            {
                Debug.Log("[TicTacToeHexOnline] ゲーム中ではありません");
                return;
            }

            // オンラインモード: 自分のターンのみ操作可能
            if (IsOnlineMode)
            {
                if (!_gameSyncService!.IsMyTurn)
                {
                    Debug.Log("[TicTacToeHexOnline] 相手のターンです");
                    return;
                }
            }

            // ローカル処理（Phase 4最適化により<1ms）
            bool moveExecuted = _model.ExecuteMove(coord, coord);

            if (!moveExecuted)
            {
                Debug.Log($"[TicTacToeHexOnline] 無効な手: {coord}");
                return;
            }

            // オンライン同期
            if (IsOnlineMode)
            {
                SendMoveToOpponentAsync(coord, coord).Forget();
            }
        }

        /// <summary>
        /// 移動を相手に送信
        /// </summary>
        private async UniTaskVoid SendMoveToOpponentAsync(HexCoordinate from, HexCoordinate to)
        {
            if (_gameSyncService == null) return;

            bool sent = await _gameSyncService.SendMoveAsync(from, to);
            if (sent)
            {
                Debug.Log($"[TicTacToeHexOnline] 移動送信: {from} → {to}");

                // ターンを相手に渡す
                await _gameSyncService.PassTurnAsync();

                // 勝利判定
                if (_model?.State == GameState.Player1Won || _model?.State == GameState.Player2Won)
                {
                    int winnerId = _networkService?.LocalPlayerId ?? -1;
                    await _gameSyncService.SyncGameEndAsync(winnerId);
                }
                else if (_model?.State == GameState.Draw)
                {
                    await _gameSyncService.SyncGameEndAsync(-1); // 引き分けは-1
                }
            }
            else
            {
                Debug.LogError("[TicTacToeHexOnline] 移動送信失敗");
            }
        }

        /// <summary>
        /// 相手の移動を受信
        /// </summary>
        private void OnOpponentMoveReceived(HexCoordinate from, HexCoordinate to)
        {
            if (_model == null || _view == null)
            {
                Debug.LogWarning("[TicTacToeHexOnline] Model/Viewがnull");
                return;
            }

            Debug.Log($"[TicTacToeHexOnline] 相手の移動受信: {from} → {to}");

            // 相手の手を実行（Phase 4最適化により<1ms）
            bool moveExecuted = _model.ExecuteMove(to, to);

            if (!moveExecuted)
            {
                Debug.LogError($"[TicTacToeHexOnline] 相手の移動実行失敗: {to}");
            }
        }

        /// <summary>
        /// オンラインゲーム開始通知
        /// </summary>
        private void OnOnlineGameStarted()
        {
            Debug.Log("[TicTacToeHexOnline] オンラインゲーム開始!");

            // ゲーム開始
            _model?.StartGame();

            // ターン表示更新
            if (_gameSyncService != null)
            {
                bool isMyTurn = _gameSyncService.IsMyTurn;
                Debug.Log($"[TicTacToeHexOnline] 自分のターン: {isMyTurn}");
                // TODO: UI更新（ターンインジケーター表示）
            }
        }

        /// <summary>
        /// オンラインゲーム終了通知
        /// </summary>
        private void OnOnlineGameEnded(int winnerId)
        {
            int localPlayerId = _networkService?.LocalPlayerId ?? -1;

            if (winnerId == -1)
            {
                Debug.Log("[TicTacToeHexOnline] 引き分け!");
                // TODO: UI更新（引き分け表示）
            }
            else if (winnerId == localPlayerId)
            {
                Debug.Log("[TicTacToeHexOnline] 勝利!");
                // TODO: UI更新（勝利表示）
            }
            else
            {
                Debug.Log("[TicTacToeHexOnline] 敗北!");
                // TODO: UI更新（敗北表示）
            }
        }

        /// <summary>
        /// ターン変更通知
        /// </summary>
        protected override void OnTurnChanged(int newPlayerId)
        {
            bool isMyTurn = _gameSyncService?.IsMyTurn == true;
            Debug.Log($"[TicTacToeHexOnline] ターン変更: Player {newPlayerId} (自分のターン: {isMyTurn})");

            // TODO: UI更新（ターンインジケーター表示）
        }

        /// <summary>
        /// リセット要求受信
        /// </summary>
        private void OnResetRequested()
        {
            Debug.Log("[TicTacToeHexOnline] 相手がリセット要求");

            // ボードリセット
            ResetGame();
        }

        /// <summary>
        /// 有効手ハイライト（Tic-Tac-Toeでは不要）
        /// </summary>
        protected override void HighlightValidMoves(HexCoordinate from)
        {
            // Tic-Tac-Toeでは有効手のハイライトは不要
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

            switch (state)
            {
                case GameState.Player1Won:
                    Debug.Log("[TicTacToeHexOnline] Player 1 勝利!");
                    break;

                case GameState.Player2Won:
                    Debug.Log("[TicTacToeHexOnline] Player 2 勝利!");
                    break;

                case GameState.Draw:
                    Debug.Log("[TicTacToeHexOnline] 引き分け!");
                    break;
            }
        }

        /// <summary>
        /// リセットボタン処理
        /// </summary>
        public override void ResetGame()
        {
            base.ResetGame();

            // オンラインモードでは相手にもリセット通知
            if (IsOnlineMode && _gameSyncService != null)
            {
                _gameSyncService.ResetGameStateAsync().Forget();
                Debug.Log("[TicTacToeHexOnline] リセット通知送信");
            }
        }

        /// <summary>
        /// クリーンアップ
        /// </summary>
        protected override void OnDestroy()
        {
            // イベント解除
            if (_gameSyncService != null)
            {
                _gameSyncService.OnMoveReceived -= OnOpponentMoveReceived;
                _gameSyncService.OnGameStarted -= OnOnlineGameStarted;
                _gameSyncService.OnGameEnded -= OnOnlineGameEnded;
                _gameSyncService.OnTurnChanged -= OnTurnChanged;
                _gameSyncService.OnResetRequested -= OnResetRequested;
            }

            base.OnDestroy();
        }
    }
}
