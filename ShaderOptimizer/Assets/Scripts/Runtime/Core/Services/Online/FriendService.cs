#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ShaderOp.Online.Models;
using UnityEngine;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// フレンド管理サービス実装
    /// </summary>
    /// <remarks>
    /// REST APIでフレンドリストを管理し、Photonでオンライン状態を監視します。
    /// </remarks>
    public class FriendService : IFriendService
    {
        // TODO: REST APIエンドポイント
        private const string API_BASE_URL = "https://api.shaderop.com";
        private const string API_FRIENDS = "/v1/friends";
        private const string API_FRIEND_REQUESTS = "/v1/friends/requests";
        private const string API_SEARCH_USERS = "/v1/users/search";

        // イベント
        public event Action<string, bool>? OnFriendOnlineStatusChanged;
        public event Action<FriendRequest>? OnFriendRequestReceived;

        // ローカルキャッシュ
        private List<FriendData> _friendsCache = new();
        private List<FriendRequest> _pendingRequestsCache = new();

        /// <summary>
        /// フレンドリストを取得します
        /// </summary>
        public async UniTask<List<FriendData>> GetFriendsAsync()
        {
            try
            {
                Debug.Log("[FriendService] フレンドリスト取得中...");

                // TODO: REST API呼び出し
                // var response = await RestClient.Get<FriendListResponse>($"{API_BASE_URL}{API_FRIENDS}");
                // _friendsCache = response.Friends;

                // モック実装
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                _friendsCache = GenerateMockFriends();

                Debug.Log($"[FriendService] フレンドリスト取得完了: {_friendsCache.Count}人");
                return _friendsCache;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendService] フレンドリスト取得エラー: {ex.Message}");
                return new List<FriendData>();
            }
        }

        /// <summary>
        /// フレンドリクエストを送信します
        /// </summary>
        public async UniTask<bool> SendFriendRequestAsync(string targetUserId)
        {
            try
            {
                Debug.Log($"[FriendService] フレンドリクエスト送信: {targetUserId}");

                // TODO: REST API呼び出し
                // var response = await RestClient.Post($"{API_BASE_URL}{API_FRIEND_REQUESTS}", new { targetUserId });

                // モック実装
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                Debug.Log("[FriendService] フレンドリクエスト送信完了");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendService] フレンドリクエスト送信エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// フレンドリクエストを承認します
        /// </summary>
        public async UniTask<bool> AcceptFriendRequestAsync(string requestId)
        {
            try
            {
                Debug.Log($"[FriendService] フレンドリクエスト承認: {requestId}");

                // TODO: REST API呼び出し
                // await RestClient.Put($"{API_BASE_URL}{API_FRIEND_REQUESTS}/{requestId}/accept");

                // モック実装
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                // キャッシュから削除
                _pendingRequestsCache.RemoveAll(r => r.RequestId == requestId);

                Debug.Log("[FriendService] フレンドリクエスト承認完了");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendService] フレンドリクエスト承認エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// フレンドリクエストを拒否します
        /// </summary>
        public async UniTask<bool> RejectFriendRequestAsync(string requestId)
        {
            try
            {
                Debug.Log($"[FriendService] フレンドリクエスト拒否: {requestId}");

                // TODO: REST API呼び出し
                // await RestClient.Put($"{API_BASE_URL}{API_FRIEND_REQUESTS}/{requestId}/reject");

                // モック実装
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                // キャッシュから削除
                _pendingRequestsCache.RemoveAll(r => r.RequestId == requestId);

                Debug.Log("[FriendService] フレンドリクエスト拒否完了");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendService] フレンドリクエスト拒否エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// フレンドを削除します
        /// </summary>
        public async UniTask<bool> RemoveFriendAsync(string friendUserId)
        {
            try
            {
                Debug.Log($"[FriendService] フレンド削除: {friendUserId}");

                // TODO: REST API呼び出し
                // await RestClient.Delete($"{API_BASE_URL}{API_FRIENDS}/{friendUserId}");

                // モック実装
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                // キャッシュから削除
                _friendsCache.RemoveAll(f => f.FriendPlayerId == friendUserId);

                Debug.Log("[FriendService] フレンド削除完了");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendService] フレンド削除エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ユーザーを検索します
        /// </summary>
        public async UniTask<List<PlayerProfile>> SearchUsersAsync(string query)
        {
            try
            {
                Debug.Log($"[FriendService] ユーザー検索: {query}");

                // TODO: REST API呼び出し
                // var response = await RestClient.Get<UserSearchResponse>($"{API_BASE_URL}{API_SEARCH_USERS}?q={query}");
                // return response.Users;

                // モック実装
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                var mockResults = GenerateMockSearchResults(query);

                Debug.Log($"[FriendService] ユーザー検索完了: {mockResults.Count}件");
                return mockResults;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendService] ユーザー検索エラー: {ex.Message}");
                return new List<PlayerProfile>();
            }
        }

        /// <summary>
        /// 受信したフレンドリクエストリストを取得します
        /// </summary>
        public async UniTask<List<FriendRequest>> GetPendingRequestsAsync()
        {
            try
            {
                Debug.Log("[FriendService] フレンドリクエストリスト取得中...");

                // TODO: REST API呼び出し
                // var response = await RestClient.Get<FriendRequestListResponse>($"{API_BASE_URL}{API_FRIEND_REQUESTS}");
                // _pendingRequestsCache = response.Requests;

                // モック実装
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                _pendingRequestsCache = GenerateMockRequests();

                Debug.Log($"[FriendService] フレンドリクエストリスト取得完了: {_pendingRequestsCache.Count}件");
                return _pendingRequestsCache;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendService] フレンドリクエストリスト取得エラー: {ex.Message}");
                return new List<FriendRequest>();
            }
        }

        // ===== モックデータ生成 =====

        private List<FriendData> GenerateMockFriends()
        {
            return new List<FriendData>
            {
                new() { FriendPlayerId = "user_001", DisplayName = "Alice", Level = 15, IsOnline = true },
                new() { FriendPlayerId = "user_002", DisplayName = "Bob", Level = 10, IsOnline = false },
                new() { FriendPlayerId = "user_003", DisplayName = "Charlie", Level = 20, IsOnline = true }
            };
        }

        private List<FriendRequest> GenerateMockRequests()
        {
            return new List<FriendRequest>
            {
                new("req_001", "user_004", "current_user", "Diana")
            };
        }

        private List<PlayerProfile> GenerateMockSearchResults(string query)
        {
            return new List<PlayerProfile>
            {
                new() { PlayerId = "user_005", DisplayName = $"User_{query}_1", Level = 5 },
                new() { PlayerId = "user_006", DisplayName = $"User_{query}_2", Level = 8 }
            };
        }
    }
}
