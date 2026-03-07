#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ShaderOp.Core.Services.Online
{
    /// <summary>
    /// HTTPクライアントサービス実装
    /// </summary>
    /// <remarks>
    /// UnityWebRequestを使用してHTTPリクエストを送信します。
    /// Firebase IDトークンを自動的にAuthorizationヘッダーに追加します。
    /// </remarks>
    public class HttpClientService : IHttpClientService
    {
        // ============================================
        // フィールド
        // ============================================

        private readonly IFirebaseAuthService _authService;
        private string _baseUrl = "https://api.shaderop.com";
        private int _timeoutSeconds = 30;
        private Dictionary<string, string> _customHeaders = new();

        // ============================================
        // コンストラクタ
        // ============================================

        public HttpClientService(IFirebaseAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        // ============================================
        // 設定メソッド
        // ============================================

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
            Debug.Log($"[HttpClientService] Base URL set to: {_baseUrl}");
        }

        public void SetTimeout(int timeoutSeconds)
        {
            _timeoutSeconds = timeoutSeconds;
            Debug.Log($"[HttpClientService] Timeout set to: {_timeoutSeconds}s");
        }

        public void SetCustomHeaders(Dictionary<string, string> headers)
        {
            _customHeaders = new Dictionary<string, string>(headers);
            Debug.Log($"[HttpClientService] Custom headers set: {headers.Count} headers");
        }

        public void ClearCustomHeaders()
        {
            _customHeaders.Clear();
            Debug.Log("[HttpClientService] Custom headers cleared");
        }

        // ============================================
        // HTTPリクエストメソッド
        // ============================================

        /// <summary>
        /// GETリクエストを送信
        /// </summary>
        public async UniTask<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null) where T : class
        {
            string url = BuildUrl(endpoint, queryParams);

            Debug.Log($"[HttpClientService] GET {url}");

            using var request = UnityWebRequest.Get(url);
            await SendRequestAsync(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                return DeserializeResponse<T>(request.downloadHandler.text);
            }

            LogError("GET", url, request);
            return null;
        }

        /// <summary>
        /// POSTリクエストを送信
        /// </summary>
        public async UniTask<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
            where TRequest : class
            where TResponse : class
        {
            string url = BuildUrl(endpoint);
            string jsonBody = SerializeRequest(body);

            Debug.Log($"[HttpClientService] POST {url}");

            using var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await SendRequestAsync(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                return DeserializeResponse<TResponse>(request.downloadHandler.text);
            }

            LogError("POST", url, request);
            return null;
        }

        /// <summary>
        /// PUTリクエストを送信
        /// </summary>
        public async UniTask<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body)
            where TRequest : class
            where TResponse : class
        {
            string url = BuildUrl(endpoint);
            string jsonBody = SerializeRequest(body);

            Debug.Log($"[HttpClientService] PUT {url}");

            using var request = new UnityWebRequest(url, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await SendRequestAsync(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                return DeserializeResponse<TResponse>(request.downloadHandler.text);
            }

            LogError("PUT", url, request);
            return null;
        }

        /// <summary>
        /// DELETEリクエストを送信
        /// </summary>
        public async UniTask<bool> DeleteAsync(string endpoint)
        {
            string url = BuildUrl(endpoint);

            Debug.Log($"[HttpClientService] DELETE {url}");

            using var request = UnityWebRequest.Delete(url);
            await SendRequestAsync(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[HttpClientService] DELETE successful: {url}");
                return true;
            }

            LogError("DELETE", url, request);
            return false;
        }

        // ============================================
        // ヘルパーメソッド
        // ============================================

        /// <summary>
        /// リクエストを送信し、共通のヘッダーを設定
        /// </summary>
        private async UniTask SendRequestAsync(UnityWebRequest request)
        {
            // Firebase IDトークンを取得してAuthorizationヘッダーに設定
            if (_authService.IsAuthenticated)
            {
                var idToken = await _authService.GetIdTokenAsync(forceRefresh: false);
                if (!string.IsNullOrEmpty(idToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                }
            }

            // カスタムヘッダーを設定
            foreach (var header in _customHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            // タイムアウトを設定
            request.timeout = _timeoutSeconds;

            // リクエスト送信
            await request.SendWebRequest();
        }

        /// <summary>
        /// URLを構築
        /// </summary>
        private string BuildUrl(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            string url = _baseUrl + endpoint;

            if (queryParams != null && queryParams.Count > 0)
            {
                var queryString = new StringBuilder();
                queryString.Append("?");

                foreach (var param in queryParams)
                {
                    if (queryString.Length > 1)
                        queryString.Append("&");

                    queryString.Append(UnityWebRequest.EscapeURL(param.Key));
                    queryString.Append("=");
                    queryString.Append(UnityWebRequest.EscapeURL(param.Value));
                }

                url += queryString.ToString();
            }

            return url;
        }

        /// <summary>
        /// リクエストボディをシリアライズ
        /// </summary>
        private string SerializeRequest<T>(T obj) where T : class
        {
            return JsonUtility.ToJson(obj);
        }

        /// <summary>
        /// レスポンスをデシリアライズ
        /// </summary>
        private T? DeserializeResponse<T>(string json) where T : class
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HttpClientService] Failed to deserialize response: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// エラーログを出力
        /// </summary>
        private void LogError(string method, string url, UnityWebRequest request)
        {
            Debug.LogError($"[HttpClientService] {method} request failed");
            Debug.LogError($"  URL: {url}");
            Debug.LogError($"  Result: {request.result}");
            Debug.LogError($"  Error: {request.error}");
            Debug.LogError($"  Response Code: {request.responseCode}");

            if (!string.IsNullOrEmpty(request.downloadHandler?.text))
            {
                Debug.LogError($"  Response: {request.downloadHandler.text}");
            }
        }
    }
}
