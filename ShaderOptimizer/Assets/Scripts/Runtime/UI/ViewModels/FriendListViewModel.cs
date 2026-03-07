#nullable enable

using System;
using UniRx;
using Friend = ShaderOp.Online.Models.FriendData;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// フレンドリスト画面のViewModel
    /// </summary>
    public class FriendListViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        /// <summary>フレンドリスト</summary>
        public ReactiveCollection<Friend> Friends { get; } = new();

        /// <summary>フレンドリクエストリスト</summary>
        public ReactiveCollection<Friend> FriendRequests { get; } = new();

        /// <summary>検索テキスト</summary>
        public ReactiveProperty<string> SearchText { get; } = new("");

        /// <summary>フィルタリングされたフレンドリスト</summary>
        public ReactiveCollection<Friend> FilteredFriends { get; } = new();

        /// <summary>フレンドリクエスト数</summary>
        public ReactiveProperty<int> RequestCount { get; } = new(0);

        /// <summary>リクエストタブ表示中か</summary>
        public ReactiveProperty<bool> ShowingRequests { get; } = new(false);

        /// <summary>チャット開始イベント</summary>
        public event Action<Friend>? OnChatStarted;

        /// <summary>ゲーム招待イベント</summary>
        public event Action<Friend>? OnGameInvited;

        /// <summary>プロフィール表示イベント</summary>
        public event Action<Friend>? OnProfileRequested;

        /// <summary>フレンド追加イベント</summary>
        public event Action? OnAddFriendRequested;

        /// <summary>フレンドリクエスト承認イベント</summary>
        public event Action<Friend>? OnRequestAccepted;

        /// <summary>フレンドリクエスト拒否イベント</summary>
        public event Action<Friend>? OnRequestRejected;

        public FriendListViewModel()
        {
            // 検索テキスト変更を監視してフィルタリング
            SearchText.Subscribe(_ => FilterFriends()).AddTo(_disposables);

            // フレンドリスト変更を監視
            Friends.ObserveCountChanged().Subscribe(_ => FilterFriends()).AddTo(_disposables);

            // リクエスト数を監視
            FriendRequests.ObserveCountChanged().Subscribe(count => RequestCount.Value = count).AddTo(_disposables);

            // テストデータをロード
            LoadTestData();
        }

        /// <summary>
        /// テストデータをロード
        /// </summary>
        private void LoadTestData()
        {
            Friends.Add(new Friend
            {
                FriendPlayerId = "friend1",
                DisplayName = "Alice",
                Level = 45,
                IsOnline = true,
                StatusMessage = "Playing Hex Reversi"
            });

            Friends.Add(new Friend
            {
                FriendPlayerId = "friend2",
                DisplayName = "Bob",
                Level = 38,
                IsOnline = false,
                LastSeenAt = DateTime.Now.AddHours(-2).ToString("o"),
                StatusMessage = "Last seen 2 hours ago"
            });

            Friends.Add(new Friend
            {
                FriendPlayerId = "friend3",
                DisplayName = "Charlie",
                Level = 52,
                IsOnline = true,
                StatusMessage = "Online"
            });

            Friends.Add(new Friend
            {
                FriendPlayerId = "friend4",
                DisplayName = "Diana",
                Level = 30,
                IsOnline = false,
                LastSeenAt = DateTime.Now.AddDays(-1).ToString("o"),
                StatusMessage = "Last seen 1 day ago"
            });

            // フレンドリクエスト
            FriendRequests.Add(new Friend
            {
                FriendPlayerId = "request1",
                DisplayName = "Eve",
                Level = 28,
                IsOnline = true,
                StatusMessage = "Wants to be friends"
            });

            FriendRequests.Add(new Friend
            {
                FriendPlayerId = "request2",
                DisplayName = "Frank",
                Level = 33,
                IsOnline = false,
                StatusMessage = "Wants to be friends"
            });

            FilterFriends();
        }

        /// <summary>
        /// フレンドをフィルタリング
        /// </summary>
        private void FilterFriends()
        {
            FilteredFriends.Clear();

            foreach (var friend in Friends)
            {
                // 検索フィルター
                if (!string.IsNullOrWhiteSpace(SearchText.Value))
                {
                    string search = SearchText.Value.ToLower();
                    if (!friend.DisplayName.ToLower().Contains(search))
                    {
                        continue;
                    }
                }

                FilteredFriends.Add(friend);
            }

            UnityEngine.Debug.Log($"[FriendListViewModel] Displaying {FilteredFriends.Count} friends");
        }

        /// <summary>
        /// タブを切り替え（フレンド/リクエスト）
        /// </summary>
        public void ToggleRequestsTab()
        {
            ShowingRequests.Value = !ShowingRequests.Value;
            UnityEngine.Debug.Log($"[FriendListViewModel] Showing requests: {ShowingRequests.Value}");
        }

        /// <summary>
        /// チャットを開始
        /// </summary>
        public void StartChat(Friend friend)
        {
            UnityEngine.Debug.Log($"[FriendListViewModel] Starting chat with {friend.DisplayName}");
            OnChatStarted?.Invoke(friend);
        }

        /// <summary>
        /// ゲームに招待
        /// </summary>
        public void InviteToGame(Friend friend)
        {
            if (!friend.IsOnline)
            {
                UnityEngine.Debug.LogWarning($"[FriendListViewModel] Cannot invite {friend.DisplayName} - they are offline");
                return;
            }

            UnityEngine.Debug.Log($"[FriendListViewModel] Inviting {friend.DisplayName} to game");
            OnGameInvited?.Invoke(friend);
        }

        /// <summary>
        /// プロフィールを表示
        /// </summary>
        public void ShowProfile(Friend friend)
        {
            UnityEngine.Debug.Log($"[FriendListViewModel] Showing profile for {friend.DisplayName}");
            OnProfileRequested?.Invoke(friend);
        }

        /// <summary>
        /// フレンド追加ダイアログを表示
        /// </summary>
        public void ShowAddFriendDialog()
        {
            UnityEngine.Debug.Log("[FriendListViewModel] Add friend dialog requested");
            OnAddFriendRequested?.Invoke();
        }

        /// <summary>
        /// フレンドリクエストを承認
        /// </summary>
        public void AcceptRequest(Friend request)
        {
            FriendRequests.Remove(request);
            Friends.Add(request);

            UnityEngine.Debug.Log($"[FriendListViewModel] Accepted friend request from {request.DisplayName}");
            OnRequestAccepted?.Invoke(request);
        }

        /// <summary>
        /// フレンドリクエストを拒否
        /// </summary>
        public void RejectRequest(Friend request)
        {
            FriendRequests.Remove(request);

            UnityEngine.Debug.Log($"[FriendListViewModel] Rejected friend request from {request.DisplayName}");
            OnRequestRejected?.Invoke(request);
        }

        /// <summary>
        /// フレンドを削除
        /// </summary>
        public void RemoveFriend(Friend friend)
        {
            Friends.Remove(friend);
            UnityEngine.Debug.Log($"[FriendListViewModel] Removed friend {friend.DisplayName}");
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
