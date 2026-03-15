using Cysharp.Threading.Tasks;
using System;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// ゲーム状態同期サービスインターフェース
    /// ターンベースゲームの同期処理を提供
    /// </summary>
    /// <remarks>
    /// Phase 5 Week 1で実装。
    /// Photon RPCを使用して、プレイヤー間で手番・移動・ゲーム状態を同期。
    /// Phase 4で実装された最適化（Direction-Based、Attack Map等）と
    /// シームレスに統合し、60fps維持したままオンラインプレイを実現。
    /// </remarks>
    public interface IGameSyncService
    {
        /// <summary>同期機能が有効か（ルーム参加中かつ2人以上）</summary>
        bool IsSyncEnabled { get; }

        /// <summary>現在のターンがローカルプレイヤーか</summary>
        bool IsMyTurn { get; }

        /// <summary>現在のターンプレイヤーのActor ID</summary>
        int CurrentPlayerId { get; }

        /// <summary>ゲームタイプ（TicTacToeHex, HexReversi等）</summary>
        string GameType { get; set; }

        /// <summary>
        /// 手番移動を送信（自分のターンのみ）
        /// </summary>
        /// <param name="from">移動元座標</param>
        /// <param name="to">移動先座標</param>
        /// <returns>送信成功でtrue</returns>
        UniTask<bool> SendMoveAsync(HexCoordinate from, HexCoordinate to);

        /// <summary>
        /// ゲーム開始を同期（全プレイヤーに通知）
        /// </summary>
        UniTask SyncGameStartAsync();

        /// <summary>
        /// ゲーム終了を同期（勝者IDを通知）
        /// </summary>
        /// <param name="winnerId">勝者のActor ID（引き分けは-1）</param>
        UniTask SyncGameEndAsync(int winnerId);

        /// <summary>
        /// ターンを次のプレイヤーに渡す
        /// </summary>
        UniTask PassTurnAsync();

        /// <summary>
        /// ゲーム状態をリセット（再戦準備）
        /// </summary>
        UniTask ResetGameStateAsync();

        /// <summary>対戦相手から移動を受信した時（from, to座標を通知）</summary>
        event Action<HexCoordinate, HexCoordinate>? OnMoveReceived;

        /// <summary>ゲーム開始通知受信時</summary>
        event Action? OnGameStarted;

        /// <summary>ゲーム終了通知受信時（勝者IDを通知）</summary>
        event Action<int>? OnGameEnded;

        /// <summary>ターン変更通知受信時（新しいターンプレイヤーIDを通知）</summary>
        event Action<int>? OnTurnChanged;

        /// <summary>対戦相手がゲームリセットを要求した時</summary>
        event Action? OnResetRequested;
    }
}
