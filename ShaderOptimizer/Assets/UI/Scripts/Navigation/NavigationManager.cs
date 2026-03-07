#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

namespace ShaderOp.UI.Navigation
{
    /// <summary>
    /// タブタイプ定義
    /// </summary>
    public enum TabType
    {
        Home,
        Games,
        Friends,
        Shop,
        Profile
    }

    /// <summary>
    /// ナビゲーション管理クラス
    /// タブベースのナビゲーションとモーダル管理を担当
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        [SerializeField] private UIDocument? _uiDocument;

        private VisualElement? _root;
        private VisualElement? _tabBar;
        private Dictionary<TabType, VisualElement> _tabContents = new();
        private Dictionary<TabType, Button> _tabButtons = new();
        private Stack<VisualElement> _modalStack = new();

        private TabType _currentTab = TabType.Home;

        /// <summary>現在アクティブなタブ</summary>
        public TabType CurrentTab => _currentTab;

        /// <summary>タブ変更イベント</summary>
        public event Action<TabType>? OnTabChanged;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("[NavigationManager] UIDocument is not assigned!");
                return;
            }

            _root = _uiDocument.rootVisualElement;
            InitializeTabBar();
            InitializeTabContents();

            // 初期タブを表示
            SwitchTab(TabType.Home);
        }

        /// <summary>
        /// タブバーを初期化
        /// </summary>
        private void InitializeTabBar()
        {
            _tabBar = _root?.Q<VisualElement>("TabBar");
            if (_tabBar == null)
            {
                Debug.LogWarning("[NavigationManager] TabBar not found in UXML");
                return;
            }

            // 各タブボタンを取得してイベント登録
            RegisterTabButton("HomeTab", TabType.Home);
            RegisterTabButton("GamesTab", TabType.Games);
            RegisterTabButton("FriendsTab", TabType.Friends);
            RegisterTabButton("ShopTab", TabType.Shop);
            RegisterTabButton("ProfileTab", TabType.Profile);
        }

        /// <summary>
        /// タブボタンを登録
        /// </summary>
        private void RegisterTabButton(string buttonName, TabType tabType)
        {
            var button = _tabBar?.Q<Button>(buttonName);
            if (button != null)
            {
                button.clicked += () => SwitchTab(tabType);
                _tabButtons[tabType] = button;
            }
            else
            {
                Debug.LogWarning($"[NavigationManager] Tab button '{buttonName}' not found");
            }
        }

        /// <summary>
        /// タブコンテンツを初期化
        /// </summary>
        private void InitializeTabContents()
        {
            RegisterTabContent("HomeContent", TabType.Home);
            RegisterTabContent("GamesContent", TabType.Games);
            RegisterTabContent("FriendsContent", TabType.Friends);
            RegisterTabContent("ShopContent", TabType.Shop);
            RegisterTabContent("ProfileContent", TabType.Profile);
        }

        /// <summary>
        /// タブコンテンツを登録
        /// </summary>
        private void RegisterTabContent(string contentName, TabType tabType)
        {
            var content = _root?.Q<VisualElement>(contentName);
            if (content != null)
            {
                _tabContents[tabType] = content;
                content.style.display = DisplayStyle.None;
            }
            else
            {
                Debug.LogWarning($"[NavigationManager] Tab content '{contentName}' not found");
            }
        }

        /// <summary>
        /// タブを切り替え
        /// </summary>
        public async void SwitchTab(TabType newTab)
        {
            if (_currentTab == newTab) return;

            Debug.Log($"[NavigationManager] Switching from {_currentTab} to {newTab}");

            // 現在のタブをフェードアウト
            if (_tabContents.TryGetValue(_currentTab, out var currentContent))
            {
                currentContent.RemoveFromClassList("tab-content-active");
                await UniTask.Delay(TimeSpan.FromMilliseconds(150));
                currentContent.style.display = DisplayStyle.None;
            }

            // 新しいタブをフェードイン
            _currentTab = newTab;
            if (_tabContents.TryGetValue(newTab, out var newContent))
            {
                newContent.style.display = DisplayStyle.Flex;
                await UniTask.Delay(TimeSpan.FromMilliseconds(50));
                newContent.AddToClassList("tab-content-active");
            }

            // タブボタンのアクティブ状態更新
            UpdateTabButtonStates();

            // イベント発火
            OnTabChanged?.Invoke(newTab);
        }

        /// <summary>
        /// タブボタンのアクティブ状態を更新
        /// </summary>
        private void UpdateTabButtonStates()
        {
            foreach (var kvp in _tabButtons)
            {
                if (kvp.Key == _currentTab)
                {
                    kvp.Value.AddToClassList("tab-button-active");
                }
                else
                {
                    kvp.Value.RemoveFromClassList("tab-button-active");
                }
            }
        }

        /// <summary>
        /// モーダルを表示
        /// </summary>
        public async void ShowModal(VisualElement modal)
        {
            if (modal == null) return;

            _root?.Add(modal);
            _modalStack.Push(modal);

            modal.style.display = DisplayStyle.Flex;
            modal.AddToClassList("modal-overlay");
            await UniTask.Delay(TimeSpan.FromMilliseconds(50));
            modal.AddToClassList("modal-active");

            Debug.Log($"[NavigationManager] Modal shown. Stack depth: {_modalStack.Count}");
        }

        /// <summary>
        /// 最前面のモーダルを閉じる
        /// </summary>
        public async void CloseModal()
        {
            if (_modalStack.Count == 0) return;

            var modal = _modalStack.Pop();
            modal.RemoveFromClassList("modal-active");

            await UniTask.Delay(TimeSpan.FromMilliseconds(200));

            modal.RemoveFromHierarchy();
            Debug.Log($"[NavigationManager] Modal closed. Stack depth: {_modalStack.Count}");
        }

        /// <summary>
        /// すべてのモーダルを閉じる
        /// </summary>
        public void CloseAllModals()
        {
            while (_modalStack.Count > 0)
            {
                var modal = _modalStack.Pop();
                modal.RemoveFromHierarchy();
            }

            Debug.Log("[NavigationManager] All modals closed");
        }

        /// <summary>
        /// 通知バッジを設定
        /// </summary>
        public void SetTabBadge(TabType tabType, int count)
        {
            if (!_tabButtons.TryGetValue(tabType, out var button)) return;

            var badge = button.Q<VisualElement>("Badge");
            if (badge == null) return;

            if (count > 0)
            {
                badge.RemoveFromClassList("d-none");
                var badgeLabel = badge.Q<Label>("BadgeCount");
                if (badgeLabel != null)
                {
                    badgeLabel.text = count > 99 ? "99+" : count.ToString();
                }
            }
            else
            {
                badge.AddToClassList("d-none");
            }
        }

        /// <summary>
        /// Androidバックボタン処理
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackButton();
            }
        }

        /// <summary>
        /// バックボタン処理ロジック
        /// </summary>
        private void HandleBackButton()
        {
            // モーダルが開いている場合は閉じる
            if (_modalStack.Count > 0)
            {
                CloseModal();
                return;
            }

            // ホームタブ以外の場合はホームに戻る
            if (_currentTab != TabType.Home)
            {
                SwitchTab(TabType.Home);
                return;
            }

            // ホームタブの場合は終了確認
            ShowQuitConfirmation();
        }

        /// <summary>
        /// 終了確認ダイアログを表示
        /// </summary>
        private void ShowQuitConfirmation()
        {
            Debug.Log("[NavigationManager] Quit confirmation requested");
            // TODO: 実装（モーダルダイアログ表示）
        }
    }
}
