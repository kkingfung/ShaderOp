#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ShaderOp.Online.Models;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// フレンド管理サービス
    /// </summary>
    /// <remarks>
    /// フレンドリストの管理、フレンドリクエストの送受信、
    /// オンライン状態の監視を提供します。
    /// </remarks>
    public interface IFriendService
    {
        /// <summary>
        /// フレンドリストを取得します（REST API経由）
        /// </summary>
        /// <returns>フレンドリスト</returns>
        UniTask<List<FriendData>> GetFriendsAsync();

        /// <summary>
        /// フレンドリクエストを送信します
        /// </summary>
        /// <param name="targetUserId">ターゲットユーザーID</param>
        /// <returns>成功時true</returns>
        UniTask<bool> SendFriendRequestAsync(string targetUserId);

        /// <summary>
        /// フレンドリクエストを承認します
        /// </summary>
        /// <param name="requestId">リクエストID</param>
        /// <returns>成功時true</returns>
        UniTask<bool> AcceptFriendRequestAsync(string requestId);

        /// <summary>
        /// フレンドリクエストを拒否します
        /// </summary>
        /// <param name="requestId">リクエストID</param>
        /// <returns>成功時true</returns>
        UniTask<bool> RejectFriendRequestAsync(string requestId);

        /// <summary>
        /// フレンドを削除します
        /// </summary>
        /// <param name="friendUserId">フレンドユーザーID</param>
        /// <returns>成功時true</returns>
        UniTask<bool> RemoveFriendAsync(string friendUserId);

        /// <summary>
        /// ユーザーを検索します
        /// </summary>
        /// <param name="query">検索クエリ（表示名またはユーザーID）</param>
        /// <returns>検索結果リスト</returns>
        UniTask<List<PlayerProfile>> SearchUsersAsync(string query);

        /// <summary>
        /// 受信したフレンドリクエストリストを取得します
        /// </summary>
        /// <returns>フレンドリクエストリスト</returns>
        UniTask<List<FriendRequest>> GetPendingRequestsAsync();

        /// <summary>フレンドのオンライン状態が変更されたときに発火</summary>
        event Action<string, bool>? OnFriendOnlineStatusChanged;

        /// <summary>フレンドリクエストを受信したときに発火</summary>
        event Action<FriendRequest>? OnFriendRequestReceived;
    }
}
