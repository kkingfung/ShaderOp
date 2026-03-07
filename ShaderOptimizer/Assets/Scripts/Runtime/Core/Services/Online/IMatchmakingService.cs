#nullable enable

using System;
using Cysharp.Threading.Tasks;
using ShaderOp.Online.Models;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// マッチメイキングサービス
    /// </summary>
    /// <remarks>
    /// ゲームマッチングの作成・参加を管理します。
    /// ランダムマッチング、プライベートマッチ、招待コードに対応します。
    /// </remarks>
    public interface IMatchmakingService
    {
        /// <summary>現在のマッチセッション</summary>
        MatchSession? CurrentMatch { get; }

        /// <summary>マッチメイキング中かどうか</summary>
        bool IsSearching { get; }

        /// <summary>
        /// ランダムマッチングを開始します
        /// </summary>
        /// <param name="gameType">ゲームタイプ</param>
        /// <returns>マッチセッション</returns>
        UniTask<MatchSession?> StartQuickMatchAsync(GameType gameType);

        /// <summary>
        /// プライベートマッチを作成します
        /// </summary>
        /// <param name="gameType">ゲームタイプ</param>
        /// <param name="friendIds">招待するフレンドIDリスト</param>
        /// <returns>マッチセッションと招待コード</returns>
        UniTask<(MatchSession? session, string? inviteCode)> CreatePrivateMatchAsync(
            GameType gameType,
            string[]? friendIds = null);

        /// <summary>
        /// 招待コードでマッチに参加します
        /// </summary>
        /// <param name="inviteCode">招待コード</param>
        /// <returns>マッチセッション</returns>
        UniTask<MatchSession?> JoinMatchByCodeAsync(string inviteCode);

        /// <summary>
        /// マッチメイキングをキャンセルします
        /// </summary>
        UniTask CancelMatchmakingAsync();

        /// <summary>
        /// 現在のマッチから退出します
        /// </summary>
        UniTask LeaveMatchAsync();

        /// <summary>
        /// プレイヤーの準備完了状態を設定します
        /// </summary>
        /// <param name="isReady">準備完了状態</param>
        UniTask SetPlayerReadyAsync(bool isReady);

        /// <summary>マッチが見つかったときに発火</summary>
        event Action<MatchSession>? OnMatchFound;

        /// <summary>マッチが開始されたときに発火</summary>
        event Action<MatchSession>? OnMatchStarted;

        /// <summary>対戦相手が退出したときに発火</summary>
        event Action<string>? OnOpponentLeft;

        /// <summary>マッチがキャンセルされたときに発火</summary>
        event Action? OnMatchCancelled;
    }
}
