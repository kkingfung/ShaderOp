#nullable enable

namespace ShaderOp.Runtime.Core.Services.Online
{
    /// <summary>
    /// PlayerId（GUID文字列）とGameId（整数）の双方向マッピングを提供するサービス
    /// </summary>
    /// <remarks>
    /// Unity Multiplayer ServicesのPlayerId（文字列GUID）を、
    /// ゲームロジックで使用する整数ベースのGameIdに変換します。
    ///
    /// - LocalPlayerは常にGameId=0
    /// - リモートプレイヤーは自動インクリメントされたGameIdを取得
    /// - GetHashCode()使用の問題を回避（毎回異なる値を返すため）
    /// </remarks>
    public interface IPlayerIdService
    {
        /// <summary>
        /// ローカルプレイヤーのGameId
        /// </summary>
        int LocalGameId { get; }

        /// <summary>
        /// PlayerIdからGameIdを取得
        /// </summary>
        /// <param name="playerId">Unity Multiplayer ServicesのPlayerId</param>
        /// <returns>対応するGameId。存在しない場合は-1</returns>
        int GetGameId(string playerId);

        /// <summary>
        /// GameIdからPlayerIdを取得
        /// </summary>
        /// <param name="gameId">ゲーム内のプレイヤーID</param>
        /// <returns>対応するPlayerId。存在しない場合はnull</returns>
        string? GetPlayerId(int gameId);

        /// <summary>
        /// ローカルプレイヤーを登録
        /// </summary>
        /// <param name="playerId">Unity Multiplayer ServicesのPlayerId</param>
        /// <param name="gameId">割り当てるGameId（通常は0）</param>
        void RegisterLocalPlayer(string playerId, int gameId);

        /// <summary>
        /// リモートプレイヤーを登録
        /// </summary>
        /// <param name="playerId">Unity Multiplayer ServicesのPlayerId</param>
        /// <param name="gameId">割り当てるGameId</param>
        void RegisterPlayer(string playerId, int gameId);

        /// <summary>
        /// プレイヤーをマッピングから削除
        /// </summary>
        /// <param name="playerId">削除するプレイヤーのPlayerId</param>
        void RemovePlayer(string playerId);

        /// <summary>
        /// 次に割り当てるべきGameIdを取得（auto-increment）
        /// </summary>
        /// <returns>未使用の最小のGameId</returns>
        int GetNextGameId();
    }
}
