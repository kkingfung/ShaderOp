#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.UI.ViewModels;
using ShaderOp.Online.Models;
using UniRx;
using System;
using System.Linq;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// チャットオーバーレイのView
    /// </summary>
    /// <remarks>
    /// ChatOverlay_Portrait.uxml と ChatOverlayViewModel を使用
    /// チャットメッセージの送受信、表示/非表示の切り替えを管理
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class ChatOverlayView : MonoBehaviour
    {
        // ============================================
        // フィールド
        // ============================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;
        private ChatOverlayViewModel? _viewModel;
        private readonly CompositeDisposable _disposables = new();

        // UI要素
        private VisualElement? _chatContainer;
        private Button? _toggleButton;
        private Button? _sendButton;
        private TextField? _messageInput;
        private ScrollView? _messageListScrollView;
        private Label? _unreadBadge;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _viewModel = new ChatOverlayViewModel();
        }

        private void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[ChatOverlayView] UIDocument または rootVisualElement が null です");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            QueryUIElements();
            RegisterEventHandlers();
            BindViewModel();

            Debug.Log("[ChatOverlayView] UI初期化完了");
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
            UnbindViewModel();
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            _disposables.Dispose();
        }

        // ============================================
        // UI要素取得
        // ============================================

        private void QueryUIElements()
        {
            if (_root == null) return;

            _chatContainer = _root.Q<VisualElement>("ChatContainer");
            _toggleButton = _root.Q<Button>("ToggleButton");
            _sendButton = _root.Q<Button>("SendButton");
            _messageInput = _root.Q<TextField>("MessageInput");
            _messageListScrollView = _root.Q<ScrollView>("MessageList");
            _unreadBadge = _root.Q<Label>("UnreadBadge");

            if (_chatContainer == null)
                Debug.LogWarning("[ChatOverlayView] ChatContainer が見つかりません");
            if (_toggleButton == null)
                Debug.LogWarning("[ChatOverlayView] ToggleButton が見つかりません");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            if (_toggleButton != null)
                _toggleButton.clicked += OnToggleClicked;

            if (_sendButton != null)
                _sendButton.clicked += OnSendClicked;

            if (_messageInput != null)
            {
                _messageInput.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        OnSendClicked();
                    }
                });
            }
        }

        private void UnregisterEventHandlers()
        {
            if (_toggleButton != null)
                _toggleButton.clicked -= OnToggleClicked;

            if (_sendButton != null)
                _sendButton.clicked -= OnSendClicked;
        }

        // ============================================
        // ViewModelバインディング
        // ============================================

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // 展開状態の変更を監視
            _viewModel.IsExpanded.Subscribe(isExpanded =>
            {
                UpdateChatVisibility(isExpanded);
            }).AddTo(_disposables);

            // 未読数の変更を監視
            _viewModel.UnreadCount.Subscribe(count =>
            {
                if (_unreadBadge != null)
                {
                    _unreadBadge.text = count.ToString();
                    _unreadBadge.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }).AddTo(_disposables);

            // 送信可能状態の変更を監視
            _viewModel.CanSend.Subscribe(canSend =>
            {
                if (_sendButton != null)
                    _sendButton.SetEnabled(canSend);
            }).AddTo(_disposables);

            // 入力メッセージの変更を監視
            _viewModel.InputMessage.Subscribe(msg =>
            {
                if (_messageInput != null && _messageInput.value != msg)
                    _messageInput.value = msg;
            }).AddTo(_disposables);

            // メッセージリストの変更を監視
            _viewModel.Messages.ObserveCountChanged().Subscribe(_ =>
            {
                UpdateMessageList();
            }).AddTo(_disposables);

            // ViewModelイベントを購読
            _viewModel.OnExpandToggled += HandleExpandToggled;
            _viewModel.OnMessageSent += HandleMessageSent;

            // 初期表示更新
            UpdateChatVisibility(_viewModel.IsExpanded.Value);
            UpdateMessageList();
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.OnExpandToggled -= HandleExpandToggled;
                _viewModel.OnMessageSent -= HandleMessageSent;
            }

            _disposables.Clear();
        }

        // ============================================
        // UI更新
        // ============================================

        private void UpdateChatVisibility(bool isExpanded)
        {
            if (_chatContainer == null) return;

            if (isExpanded)
            {
                _chatContainer.RemoveFromClassList("chat-collapsed");
                _chatContainer.AddToClassList("chat-expanded");
            }
            else
            {
                _chatContainer.RemoveFromClassList("chat-expanded");
                _chatContainer.AddToClassList("chat-collapsed");
            }

            Debug.Log($"[ChatOverlayView] Chat visibility: {isExpanded}");
        }

        private void UpdateMessageList()
        {
            if (_viewModel == null || _messageListScrollView == null) return;

            _messageListScrollView.Clear();

            foreach (var message in _viewModel.Messages)
            {
                var messageElement = CreateMessageElement(message);
                _messageListScrollView.Add(messageElement);
            }

            // 最下部にスクロール
            _messageListScrollView.ScrollTo(_messageListScrollView.contentContainer.Children().Last());
        }

        private VisualElement CreateMessageElement(ChatMessage message)
        {
            var container = new VisualElement();
            container.AddToClassList("chat-message");

            // 自分のメッセージかどうかを判定（SenderId が "you" の場合）
            bool isOwnMessage = message.SenderId == "you";
            if (isOwnMessage)
            {
                container.AddToClassList("chat-message-own");
            }
            else
            {
                container.AddToClassList("chat-message-other");
            }

            // 送信者名
            var senderLabel = new Label(message.SenderName);
            senderLabel.AddToClassList("chat-sender");
            container.Add(senderLabel);

            // メッセージ内容
            var messageLabel = new Label(message.Content);
            messageLabel.AddToClassList("chat-text");
            container.Add(messageLabel);

            // タイムスタンプ (string型なので GetTimeString() を使用)
            var timeLabel = new Label(message.GetTimeString());
            timeLabel.AddToClassList("chat-timestamp");
            container.Add(timeLabel);

            return container;
        }

        // ============================================
        // イベントハンドラ
        // ============================================

        private void OnToggleClicked()
        {
            Debug.Log("[ChatOverlayView] Toggle button clicked");
            _viewModel?.ToggleExpand();
        }

        private void OnSendClicked()
        {
            if (_viewModel == null || _messageInput == null) return;

            // ViewModelの入力メッセージを更新
            _viewModel.InputMessage.Value = _messageInput.value;

            // メッセージ送信
            _viewModel.SendMessage();

            // 入力フィールドをクリア
            _messageInput.value = "";
            _messageInput.Focus();

            Debug.Log("[ChatOverlayView] Message sent");
        }

        // ============================================
        // ViewModelイベントハンドラ
        // ============================================

        private void HandleExpandToggled(bool isExpanded)
        {
            Debug.Log($"[ChatOverlayView] Expand toggled: {isExpanded}");
        }

        private void HandleMessageSent(string message)
        {
            Debug.Log($"[ChatOverlayView] Message sent via event: {message}");
            // 送信音を再生するなど（未実装）
        }
    }
}
