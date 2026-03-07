#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ShaderOp.Online.Models;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// チャット機能サービス
    /// </summary>
    /// <remarks>
    /// リアルタイムチャットメッセージの送受信を管理します。
    /// テキストメッセージとスタンプの両方に対応します。
    /// </remarks>
    public interface IChatService
    {
        /// <summary>
        /// テキストメッセージを送信します
        /// </summary>
        /// <param name="text">メッセージテキスト</param>
        /// <returns>成功時true</returns>
        UniTask<bool> SendMessageAsync(string text);

        /// <summary>
        /// スタンプを送信します
        /// </summary>
        /// <param name="stampId">スタンプID</param>
        /// <returns>成功時true</returns>
        UniTask<bool> SendStampAsync(int stampId);

        /// <summary>
        /// 最近のメッセージを取得します（ローカルキャッシュから）
        /// </summary>
        /// <param name="count">取得件数</param>
        /// <returns>メッセージリスト</returns>
        List<ChatMessage> GetRecentMessages(int count);

        /// <summary>
        /// すべてのメッセージをクリアします
        /// </summary>
        void ClearMessages();

        /// <summary>
        /// ユーザーをブロックします
        /// </summary>
        /// <param name="userId">ブロックするユーザーID</param>
        UniTask BlockUserAsync(string userId);

        /// <summary>
        /// ユーザーのブロックを解除します
        /// </summary>
        /// <param name="userId">ブロック解除するユーザーID</param>
        UniTask UnblockUserAsync(string userId);

        /// <summary>
        /// ユーザーがブロックされているか確認します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>ブロックされている場合true</returns>
        bool IsUserBlocked(string userId);

        /// <summary>メッセージを受信したときに発火</summary>
        event Action<ChatMessage>? OnMessageReceived;
    }
}
