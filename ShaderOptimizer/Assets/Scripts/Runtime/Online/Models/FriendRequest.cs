#nullable enable

using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// フレンドリクエストクラス
    /// </summary>
    [System.Serializable]
    public class FriendRequest
    {
        [SerializeField] private string _requestId = string.Empty;
        [SerializeField] private string _fromPlayerId = string.Empty;
        [SerializeField] private string _toPlayerId = string.Empty;
        [SerializeField] private string _fromPlayerName = string.Empty;
        [SerializeField] private RequestStatus _status = RequestStatus.Pending;
        [SerializeField] private string _sentAt = string.Empty;

        /// <summary>リクエストID</summary>
        public string RequestId => _requestId;
        
        /// <summary>送信者プレイヤーID</summary>
        public string FromPlayerId => _fromPlayerId;
        
        /// <summary>受信者プレイヤーID</summary>
        public string ToPlayerId => _toPlayerId;
        
        /// <summary>送信者プレイヤー名</summary>
        public string FromPlayerName => _fromPlayerName;
        
        /// <summary>リクエストステータス</summary>
        public RequestStatus Status
        {
            get => _status;
            set => _status = value;
        }
        
        /// <summary>送信日時（ISO 8601形式）</summary>
        public string SentAt => _sentAt;

        public FriendRequest(string requestId, string fromPlayerId, string toPlayerId, string fromPlayerName)
        {
            _requestId = requestId;
            _fromPlayerId = fromPlayerId;
            _toPlayerId = toPlayerId;
            _fromPlayerName = fromPlayerName;
            _sentAt = System.DateTime.UtcNow.ToString("o");
        }
    }

    /// <summary>
    /// リクエストステータス列挙型
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>保留中</summary>
        Pending = 0,
        
        /// <summary>承認済み</summary>
        Accepted = 1,
        
        /// <summary>拒否済み</summary>
        Rejected = 2
    }
}
