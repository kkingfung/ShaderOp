#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// フレンドデータ
    /// </summary>
    /// <remarks>
    /// フレンドリストに表示する情報を保持します。
    /// </remarks>
    [Serializable]
    public class FriendData
    {
        /// <summary>フレンドのプレイヤーID</summary>
        [SerializeField] private string _friendPlayerId = string.Empty;
        public string FriendPlayerId
        {
            get => _friendPlayerId;
            set => _friendPlayerId = value;
        }

        /// <summary>フレンドの表示名</summary>
        [SerializeField] private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        /// <summary>フレンドのレベル</summary>
        [SerializeField] private int _level = 1;
        public int Level
        {
            get => _level;
            set => _level = value;
        }

        /// <summary>オンライン状態</summary>
        [SerializeField] private bool _isOnline = false;
        public bool IsOnline
        {
            get => _isOnline;
            set => _isOnline = value;
        }

        /// <summary>現在のステータスメッセージ</summary>
        [SerializeField] private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => _statusMessage = value;
        }

        /// <summary>最終ログイン日時</summary>
        [SerializeField] private string _lastSeenAt = string.Empty;
        public string LastSeenAt
        {
            get => _lastSeenAt;
            set => _lastSeenAt = value;
        }

        /// <summary>フレンド登録日時</summary>
        [SerializeField] private string _friendsSince = string.Empty;
        public string FriendsSince
        {
            get => _friendsSince;
            set => _friendsSince = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public FriendData() { }

        /// <summary>
        /// フレンド作成用コンストラクタ
        /// </summary>
        public FriendData(string friendPlayerId, string displayName, int level, bool isOnline)
        {
            _friendPlayerId = friendPlayerId;
            _displayName = displayName;
            _level = level;
            _isOnline = isOnline;
            _statusMessage = isOnline ? "Online" : "Offline";
            _lastSeenAt = DateTime.UtcNow.ToString("o");
            _friendsSince = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// 最終ログインからの経過時間を取得
        /// </summary>
        public string GetLastSeenText()
        {
            if (_isOnline) return "Online";
            if (string.IsNullOrEmpty(_lastSeenAt)) return "Unknown";

            try
            {
                DateTime lastSeen = DateTime.Parse(_lastSeenAt);
                TimeSpan elapsed = DateTime.UtcNow - lastSeen;

                if (elapsed.TotalMinutes < 60)
                    return $"{(int)elapsed.TotalMinutes}m ago";
                if (elapsed.TotalHours < 24)
                    return $"{(int)elapsed.TotalHours}h ago";
                if (elapsed.TotalDays < 7)
                    return $"{(int)elapsed.TotalDays}d ago";
                return $"{(int)(elapsed.TotalDays / 7)}w ago";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
