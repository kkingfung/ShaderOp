#nullable enable

using System;
using UniRx;
using ChatMessage = ShaderOp.Online.Models.ChatMessage;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// チャットオーバーレイのViewModel
    /// </summary>
    public class ChatOverlayViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        /// <summary>チャットが展開されているか</summary>
        public ReactiveProperty<bool> IsExpanded { get; } = new(false);

        /// <summary>メッセージリスト</summary>
        public ReactiveCollection<ChatMessage> Messages { get; } = new();

        /// <summary>入力中のメッセージ</summary>
        public ReactiveProperty<string> InputMessage { get; } = new("");

        /// <summary>未読メッセージ数</summary>
        public ReactiveProperty<int> UnreadCount { get; } = new(0);

        /// <summary>送信可能かどうか</summary>
        public ReactiveProperty<bool> CanSend { get; } = new(false);

        /// <summary>チャット展開/折りたたみイベント</summary>
        public event Action<bool>? OnExpandToggled;

        /// <summary>メッセージ送信イベント</summary>
        public event Action<string>? OnMessageSent;

        public ChatOverlayViewModel()
        {
            // 入力メッセージの変更を監視して送信可能状態を更新
            InputMessage.Subscribe(msg => CanSend.Value = !string.IsNullOrWhiteSpace(msg)).AddTo(_disposables);

            // メッセージ追加時に未読数をリセット（展開中の場合）
            Messages.ObserveCountChanged().Subscribe(_ =>
            {
                if (IsExpanded.Value)
                {
                    UnreadCount.Value = 0;
                }
            }).AddTo(_disposables);

            // テストメッセージを追加
            InitializeTestMessages();
        }

        /// <summary>
        /// テストメッセージを初期化
        /// </summary>
        private void InitializeTestMessages()
        {
            Messages.Add(new ChatMessage
            {
                SenderId = "friend1",
                SenderName = "Friend1",
                Content = "Hey! Want to play a game?",
                Timestamp = DateTime.Now.AddMinutes(-5).ToString("o")
            });

            Messages.Add(new ChatMessage
            {
                SenderId = "you",
                SenderName = "You",
                Content = "Sure! Let's play Hex Reversi",
                Timestamp = DateTime.Now.AddMinutes(-4).ToString("o")
            });

            Messages.Add(new ChatMessage
            {
                SenderId = "friend1",
                SenderName = "Friend1",
                Content = "Great! I'll create a lobby",
                Timestamp = DateTime.Now.AddMinutes(-3).ToString("o")
            });

            UnreadCount.Value = 3;
        }

        /// <summary>
        /// チャットの展開/折りたたみを切り替え
        /// </summary>
        public void ToggleExpand()
        {
            IsExpanded.Value = !IsExpanded.Value;

            if (IsExpanded.Value)
            {
                // 展開時に未読をクリア
                UnreadCount.Value = 0;
            }

            UnityEngine.Debug.Log($"[ChatOverlayViewModel] Chat expanded: {IsExpanded.Value}");
            OnExpandToggled?.Invoke(IsExpanded.Value);
        }

        /// <summary>
        /// チャットを展開
        /// </summary>
        public void Expand()
        {
            if (IsExpanded.Value) return;
            ToggleExpand();
        }

        /// <summary>
        /// チャットを折りたたむ
        /// </summary>
        public void Collapse()
        {
            if (!IsExpanded.Value) return;
            ToggleExpand();
        }

        /// <summary>
        /// メッセージを送信
        /// </summary>
        public void SendMessage()
        {
            if (!CanSend.Value) return;

            string message = InputMessage.Value.Trim();
            if (string.IsNullOrEmpty(message)) return;

            Messages.Add(new ChatMessage
            {
                SenderId = "you",
                SenderName = "You",
                Content = message,
                Timestamp = DateTime.Now.ToString("o")
            });

            UnityEngine.Debug.Log($"[ChatOverlayViewModel] Message sent: {message}");
            OnMessageSent?.Invoke(message);

            // 入力をクリア
            InputMessage.Value = "";
        }

        /// <summary>
        /// 新しいメッセージを受信
        /// </summary>
        public void ReceiveMessage(string senderName, string message)
        {
            Messages.Add(new ChatMessage
            {
                SenderId = senderName.ToLower(),
                SenderName = senderName,
                Content = message,
                Timestamp = DateTime.Now.ToString("o")
            });

            // 折りたたみ中の場合は未読数を増やす
            if (!IsExpanded.Value)
            {
                UnreadCount.Value++;
            }

            UnityEngine.Debug.Log($"[ChatOverlayViewModel] Message received from {senderName}: {message}");
        }

        /// <summary>
        /// チャット履歴をクリア
        /// </summary>
        public void ClearHistory()
        {
            Messages.Clear();
            UnreadCount.Value = 0;
            UnityEngine.Debug.Log("[ChatOverlayViewModel] Chat history cleared");
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
