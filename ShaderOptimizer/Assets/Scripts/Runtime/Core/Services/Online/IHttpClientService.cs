#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Core.Services.Online
{
    /// <summary>
    /// HTTPクライアントサービスインターフェース
    /// </summary>
    /// <remarks>
    /// バックエンドAPIへのHTTPリクエストを管理します。
    /// Firebase IDトークンを自動的にAuthorizationヘッダーに追加します。
    /// </remarks>
    public interface IHttpClientService
    {
        /// <summary>
        /// GETリクエストを送信
        /// </summary>
        /// <typeparam name="T">レスポンスの型</typeparam>
        /// <param name="endpoint">APIエンドポイント（例: "/v1/profile"）</param>
        /// <param name="queryParams">クエリパラメータ（オプション）</param>
        /// <returns>デシリアライズされたレスポンスデータ</returns>
        UniTask<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null) where T : class;

        /// <summary>
        /// POSTリクエストを送信
        /// </summary>
        /// <typeparam name="TRequest">リクエストボディの型</typeparam>
        /// <typeparam name="TResponse">レスポンスの型</typeparam>
        /// <param name="endpoint">APIエンドポイント</param>
        /// <param name="body">リクエストボディ</param>
        /// <returns>デシリアライズされたレスポンスデータ</returns>
        UniTask<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        /// PUTリクエストを送信
        /// </summary>
        /// <typeparam name="TRequest">リクエストボディの型</typeparam>
        /// <typeparam name="TResponse">レスポンスの型</typeparam>
        /// <param name="endpoint">APIエンドポイント</param>
        /// <param name="body">リクエストボディ</param>
        /// <returns>デシリアライズされたレスポンスデータ</returns>
        UniTask<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        /// DELETEリクエストを送信
        /// </summary>
        /// <param name="endpoint">APIエンドポイント</param>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> DeleteAsync(string endpoint);

        /// <summary>
        /// カスタムヘッダーを設定
        /// </summary>
        /// <param name="headers">ヘッダー名と値のペア</param>
        void SetCustomHeaders(Dictionary<string, string> headers);

        /// <summary>
        /// カスタムヘッダーをクリア
        /// </summary>
        void ClearCustomHeaders();

        /// <summary>
        /// ベースURLを設定
        /// </summary>
        /// <param name="baseUrl">ベースURL（例: "https://api.shaderop.com"）</param>
        void SetBaseUrl(string baseUrl);

        /// <summary>
        /// タイムアウト時間を設定（秒）
        /// </summary>
        /// <param name="timeoutSeconds">タイムアウト時間</param>
        void SetTimeout(int timeoutSeconds);
    }
}
