#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// チャットメッセージデータ
    /// </summary>
    /// <remarks>
    /// ゲーム内チャットのメッセージ情報を保持します。
    /// Photon RPCでメッセージを送信する際に使用します。
    /// </remarks>
    [Serializable]
    public class ChatMessage
    {
        /// <summary>メッセージID</summary>
        [SerializeField] private string _messageId = string.Empty;
        public string MessageId
        {
            get => _messageId;
            set => _messageId = value;
        }

        /// <summary>送信者のプレイヤーID</summary>
        [SerializeField] private string _senderId = string.Empty;
        public string SenderId
        {
            get => _senderId;
            set => _senderId = value;
        }

        /// <summary>送信者の表示名</summary>
        [SerializeField] private string _senderName = string.Empty;
        public string SenderName
        {
            get => _senderName;
            set => _senderName = value;
        }

        /// <summary>メッセージ内容</summary>
        [SerializeField] private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => _content = value;
        }

        /// <summary>メッセージタイプ</summary>
        [SerializeField] private MessageType _messageType = MessageType.Text;
        public MessageType MessageType
        {
            get => _messageType;
            set => _messageType = value;
        }

        /// <summary>送信日時</summary>
        [SerializeField] private string _timestamp = string.Empty;
        public string Timestamp
        {
            get => _timestamp;
            set => _timestamp = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ChatMessage() { }

        /// <summary>
        /// メッセージ作成用コンストラクタ
        /// </summary>
        public ChatMessage(string senderId, string senderName, string content, MessageType messageType = MessageType.Text)
        {
            _messageId = Guid.NewGuid().ToString();
            _senderId = senderId;
            _senderName = senderName;
            _content = content;
            _messageType = messageType;
            _timestamp = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// 時刻を短縮形式で取得（HH:mm）
        /// </summary>
        public string GetTimeString()
        {
            try
            {
                DateTime time = DateTime.Parse(_timestamp);
                return time.ToLocalTime().ToString("HH:mm");
            }
            catch
            {
                return "00:00";
            }
        }

        /// <summary>
        /// システムメッセージを作成
        /// </summary>
        public static ChatMessage CreateSystemMessage(string content)
        {
            return new ChatMessage("SYSTEM", "System", content, MessageType.System);
        }
    }

    /// <summary>
    /// メッセージタイプ列挙型
    /// </summary>
    public enum MessageType
    {
        /// <summary>通常のテキストメッセージ</summary>
        Text = 0,

        /// <summary>スタンプ</summary>
        Stamp = 1,

        /// <summary>システムメッセージ</summary>
        System = 2
    }
}
