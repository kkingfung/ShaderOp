#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.UI.ViewModels;
using ShaderOp.Online.Models;
using ShaderOp.Core.Services;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using Friend = ShaderOp.Online.Models.FriendData;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// フレンドリスト画面のView
    /// </summary>
    /// <remarks>
    /// FriendList_Portrait.uxml と FriendListViewModel を使用
    /// フレンド一覧、リクエスト管理、招待機能を提供
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class FriendListView : MonoBehaviour
    {
        // ============================================
        // フィールド
        // ============================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;
        private FriendListViewModel? _viewModel;
        private readonly CompositeDisposable _disposables = new();

        // UI要素
        private Button? _backButton;
        private Button? _addFriendButton;
        private Button? _friendsTabButton;
        private Button? _requestsTabButton;
        private TextField? _searchField;
        private ScrollView? _friendListScrollView;
        private ScrollView? _requestListScrollView;
        private Label? _requestBadge;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _viewModel = new FriendListViewModel();
        }

        private void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[FriendListView] UIDocument または rootVisualElement が null です");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            QueryUIElements();
            RegisterEventHandlers();
            BindViewModel();

            Debug.Log("[FriendListView] UI初期化完了");
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

            _backButton = _root.Q<Button>("BackButton");
            _addFriendButton = _root.Q<Button>("AddFriendButton");
            _friendsTabButton = _root.Q<Button>("FriendsTab");
            _requestsTabButton = _root.Q<Button>("RequestsTab");
            _searchField = _root.Q<TextField>("SearchField");
            _friendListScrollView = _root.Q<ScrollView>("FriendList");
            _requestListScrollView = _root.Q<ScrollView>("RequestList");
            _requestBadge = _root.Q<Label>("RequestBadge");

            if (_backButton == null)
                Debug.LogWarning("[FriendListView] BackButton が見つかりません");
            if (_friendListScrollView == null)
                Debug.LogWarning("[FriendListView] FriendList が見つかりません");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked += OnBackClicked;

            if (_addFriendButton != null)
                _addFriendButton.clicked += OnAddFriendClicked;

            if (_friendsTabButton != null)
                _friendsTabButton.clicked += () => OnTabClicked(false);

            if (_requestsTabButton != null)
                _requestsTabButton.clicked += () => OnTabClicked(true);

            if (_searchField != null)
            {
                _searchField.RegisterValueChangedCallback(evt =>
                {
                    if (_viewModel != null)
                        _viewModel.SearchText.Value = evt.newValue;
                });
            }
        }

        private void UnregisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;

            if (_addFriendButton != null)
                _addFriendButton.clicked -= OnAddFriendClicked;
        }

        // ============================================
        // ViewModelバインディング
        // ============================================

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // フィルタリングされたフレンドリストの変更を監視
            _viewModel.FilteredFriends.ObserveCountChanged().Subscribe(_ =>
            {
                UpdateFriendList();
            }).AddTo(_disposables);

            // リクエストリストの変更を監視
            _viewModel.FriendRequests.ObserveCountChanged().Subscribe(_ =>
            {
                UpdateRequestList();
            }).AddTo(_disposables);

            // リクエスト数の変更を監視
            _viewModel.RequestCount.Subscribe(count =>
            {
                if (_requestBadge != null)
                {
                    _requestBadge.text = count.ToString();
                    _requestBadge.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }).AddTo(_disposables);

            // タブ表示状態の変更を監視
            _viewModel.ShowingRequests.Subscribe(showingRequests =>
            {
                UpdateTabVisibility(showingRequests);
            }).AddTo(_disposables);

            // ViewModelイベントを購読
            _viewModel.OnChatStarted += HandleChatStarted;
            _viewModel.OnGameInvited += HandleGameInvited;
            _viewModel.OnProfileRequested += HandleProfileRequested;
            _viewModel.OnAddFriendRequested += HandleAddFriendRequested;
            _viewModel.OnRequestAccepted += HandleRequestAccepted;
            _viewModel.OnRequestRejected += HandleRequestRejected;

            // 初期表示更新
            UpdateFriendList();
            UpdateRequestList();
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.OnChatStarted -= HandleChatStarted;
                _viewModel.OnGameInvited -= HandleGameInvited;
                _viewModel.OnProfileRequested -= HandleProfileRequested;
                _viewModel.OnAddFriendRequested -= HandleAddFriendRequested;
                _viewModel.OnRequestAccepted -= HandleRequestAccepted;
                _viewModel.OnRequestRejected -= HandleRequestRejected;
            }

            _disposables.Clear();
        }

        // ============================================
        // UI更新
        // ============================================

        private void UpdateFriendList()
        {
            if (_viewModel == null || _friendListScrollView == null) return;

            _friendListScrollView.Clear();

            foreach (var friend in _viewModel.FilteredFriends)
            {
                var friendElement = CreateFriendElement(friend);
                _friendListScrollView.Add(friendElement);
            }
        }

        private void UpdateRequestList()
        {
            if (_viewModel == null || _requestListScrollView == null) return;

            _requestListScrollView.Clear();

            foreach (var request in _viewModel.FriendRequests)
            {
                var requestElement = CreateRequestElement(request);
                _requestListScrollView.Add(requestElement);
            }
        }

        private VisualElement CreateFriendElement(Friend friend)
        {
            var container = new VisualElement();
            container.AddToClassList("friend-item");

            // 左側: アバターとステータス
            var leftSide = new VisualElement();
            leftSide.AddToClassList("friend-left");

            var avatar = new Label("👤");
            avatar.AddToClassList("friend-avatar");
            leftSide.Add(avatar);

            var onlineIndicator = new VisualElement();
            onlineIndicator.AddToClassList(friend.IsOnline ? "status-online" : "status-offline");
            leftSide.Add(onlineIndicator);

            container.Add(leftSide);

            // 中央: 名前とステータス
            var centerSide = new VisualElement();
            centerSide.AddToClassList("friend-center");

            var nameLabel = new Label($"{friend.DisplayName} (Lv.{friend.Level})");
            nameLabel.AddToClassList("friend-name");
            centerSide.Add(nameLabel);

            var statusLabel = new Label(friend.StatusMessage);
            statusLabel.AddToClassList("friend-status");
            centerSide.Add(statusLabel);

            container.Add(centerSide);

            // 右側: アクションボタン
            var rightSide = new VisualElement();
            rightSide.AddToClassList("friend-right");

            var chatButton = new Button(() => OnChatClicked(friend));
            chatButton.text = "💬";
            chatButton.AddToClassList("action-button");
            rightSide.Add(chatButton);

            var inviteButton = new Button(() => OnInviteClicked(friend));
            inviteButton.text = "🎮";
            inviteButton.SetEnabled(friend.IsOnline);
            inviteButton.AddToClassList("action-button");
            rightSide.Add(inviteButton);

            container.Add(rightSide);

            // クリックでプロフィール表示
            container.RegisterCallback<ClickEvent>(_ => OnProfileClicked(friend));

            return container;
        }

        private VisualElement CreateRequestElement(Friend request)
        {
            var container = new VisualElement();
            container.AddToClassList("request-item");

            var nameLabel = new Label($"{request.DisplayName} (Lv.{request.Level})");
            nameLabel.AddToClassList("request-name");
            container.Add(nameLabel);

            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("request-buttons");

            var acceptButton = new Button(() => OnAcceptClicked(request));
            acceptButton.text = "Accept";
            acceptButton.AddToClassList("accept-button");
            buttonRow.Add(acceptButton);

            var rejectButton = new Button(() => OnRejectClicked(request));
            rejectButton.text = "Reject";
            rejectButton.AddToClassList("reject-button");
            buttonRow.Add(rejectButton);

            container.Add(buttonRow);

            return container;
        }

        private void UpdateTabVisibility(bool showingRequests)
        {
            if (_friendListScrollView != null)
                _friendListScrollView.style.display = showingRequests ? DisplayStyle.None : DisplayStyle.Flex;

            if (_requestListScrollView != null)
                _requestListScrollView.style.display = showingRequests ? DisplayStyle.Flex : DisplayStyle.None;

            // タブボタンのアクティブ状態を更新
            _friendsTabButton?.RemoveFromClassList("tab-active");
            _requestsTabButton?.RemoveFromClassList("tab-active");

            if (showingRequests)
                _requestsTabButton?.AddToClassList("tab-active");
            else
                _friendsTabButton?.AddToClassList("tab-active");
        }

        // ============================================
        // イベントハンドラ
        // ============================================

        private void OnBackClicked()
        {
            Debug.Log("[FriendListView] Back button clicked");

            var sceneService = ServiceLocator.Instance.Get<ISceneLoaderService>();
            if (sceneService != null)
            {
                sceneService.LoadMainMenuAsync().Forget();
            }
        }

        private void OnAddFriendClicked()
        {
            Debug.Log("[FriendListView] Add friend button clicked");
            _viewModel?.ShowAddFriendDialog();
        }

        private void OnTabClicked(bool showRequests)
        {
            if (_viewModel != null)
                _viewModel.ShowingRequests.Value = showRequests;
        }

        private void OnChatClicked(Friend friend)
        {
            Debug.Log($"[FriendListView] Chat clicked for {friend.DisplayName}");
            _viewModel?.StartChat(friend);
        }

        private void OnInviteClicked(Friend friend)
        {
            Debug.Log($"[FriendListView] Invite clicked for {friend.DisplayName}");
            _viewModel?.InviteToGame(friend);
        }

        private void OnProfileClicked(Friend friend)
        {
            Debug.Log($"[FriendListView] Profile clicked for {friend.DisplayName}");
            _viewModel?.ShowProfile(friend);
        }

        private void OnAcceptClicked(Friend request)
        {
            Debug.Log($"[FriendListView] Accept clicked for {request.DisplayName}");
            _viewModel?.AcceptRequest(request);
        }

        private void OnRejectClicked(Friend request)
        {
            Debug.Log($"[FriendListView] Reject clicked for {request.DisplayName}");
            _viewModel?.RejectRequest(request);
        }

        // ============================================
        // ViewModelイベントハンドラ
        // ============================================

        private void HandleChatStarted(Friend friend)
        {
            Debug.Log($"[FriendListView] Starting chat with {friend.DisplayName}");
            // チャット画面を開く（未実装）
        }

        private void HandleGameInvited(Friend friend)
        {
            Debug.Log($"[FriendListView] Game invitation sent to {friend.DisplayName}");
            // 招待送信完了通知（未実装）
        }

        private void HandleProfileRequested(Friend friend)
        {
            Debug.Log($"[FriendListView] Showing profile for {friend.DisplayName}");
            // プロフィール画面を開く（未実装）
        }

        private void HandleAddFriendRequested()
        {
            Debug.Log("[FriendListView] Add friend dialog opened");
            // ダイアログ表示（未実装）
        }

        private void HandleRequestAccepted(Friend friend)
        {
            Debug.Log($"[FriendListView] Friend request accepted: {friend.DisplayName}");
            UpdateFriendList();
            UpdateRequestList();
        }

        private void HandleRequestRejected(Friend friend)
        {
            Debug.Log($"[FriendListView] Friend request rejected: {friend.DisplayName}");
            UpdateRequestList();
        }
    }
}
