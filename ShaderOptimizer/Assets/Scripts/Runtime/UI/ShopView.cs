#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.UI.ViewModels;
using ShaderOp.Online.Models;
using ShaderOp.Core.Services;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// ショップ画面のView
    /// </summary>
    /// <remarks>
    /// Shop_Portrait.uxml と ShopViewModel を使用
    /// アイテム一覧表示、カテゴリフィルタ、購入処理を管理
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class ShopView : MonoBehaviour
    {
        // ============================================
        // フィールド
        // ============================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;
        private ShopViewModel? _viewModel;
        private readonly CompositeDisposable _disposables = new();

        // UI要素
        private Button? _backButton;
        private Label? _coinsLabel;
        private Label? _gemsLabel;
        private TextField? _searchField;
        private ScrollView? _itemListScrollView;
        private ScrollView? _featuredScrollView;

        // カテゴリボタン
        private Button? _allCategoryButton;
        private Button? _hairCategoryButton;
        private Button? _outfitCategoryButton;
        private Button? _accessoryCategoryButton;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _viewModel = new ShopViewModel();
        }

        private void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[ShopView] UIDocument または rootVisualElement が null です");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            QueryUIElements();
            RegisterEventHandlers();
            BindViewModel();

            Debug.Log("[ShopView] UI初期化完了");
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
            _coinsLabel = _root.Q<Label>("CoinsLabel");
            _gemsLabel = _root.Q<Label>("GemsLabel");
            _searchField = _root.Q<TextField>("SearchField");
            _itemListScrollView = _root.Q<ScrollView>("ItemList");
            _featuredScrollView = _root.Q<ScrollView>("FeaturedItems");

            // カテゴリボタン
            _allCategoryButton = _root.Q<Button>("AllCategoryButton");
            _hairCategoryButton = _root.Q<Button>("HairCategoryButton");
            _outfitCategoryButton = _root.Q<Button>("OutfitCategoryButton");
            _accessoryCategoryButton = _root.Q<Button>("AccessoryCategoryButton");

            if (_backButton == null)
                Debug.LogWarning("[ShopView] BackButton が見つかりません");
            if (_itemListScrollView == null)
                Debug.LogWarning("[ShopView] ItemList が見つかりません");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked += OnBackClicked;

            if (_searchField != null)
            {
                _searchField.RegisterValueChangedCallback(evt =>
                {
                    if (_viewModel != null)
                        _viewModel.SearchText.Value = evt.newValue;
                });
            }

            // カテゴリボタン
            if (_allCategoryButton != null)
                _allCategoryButton.clicked += () => OnCategoryClicked("All");
            if (_hairCategoryButton != null)
                _hairCategoryButton.clicked += () => OnCategoryClicked("Hair");
            if (_outfitCategoryButton != null)
                _outfitCategoryButton.clicked += () => OnCategoryClicked("Outfit");
            if (_accessoryCategoryButton != null)
                _accessoryCategoryButton.clicked += () => OnCategoryClicked("Accessory");
        }

        private void UnregisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;
        }

        // ============================================
        // ViewModelバインディング
        // ============================================

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // コイン数の変更を監視
            _viewModel.Coins.Subscribe(coins =>
            {
                if (_coinsLabel != null)
                    _coinsLabel.text = $"🪙 {coins}";
            }).AddTo(_disposables);

            // ジェム数の変更を監視
            _viewModel.Gems.Subscribe(gems =>
            {
                if (_gemsLabel != null)
                    _gemsLabel.text = $"💎 {gems}";
            }).AddTo(_disposables);

            // 表示アイテムリストの変更を監視
            _viewModel.DisplayedItems.ObserveCountChanged().Subscribe(_ =>
            {
                UpdateItemList();
            }).AddTo(_disposables);

            // 注目アイテムリストの変更を監視
            _viewModel.FeaturedItems.ObserveCountChanged().Subscribe(_ =>
            {
                UpdateFeaturedItems();
            }).AddTo(_disposables);

            // カテゴリ変更を監視
            _viewModel.SelectedCategory.Subscribe(category =>
            {
                UpdateCategoryButtons(category);
            }).AddTo(_disposables);

            // ViewModelイベントを購読
            _viewModel.OnItemPurchased += HandleItemPurchased;
            _viewModel.OnItemDetailRequested += HandleItemDetailRequested;

            // 初期表示更新
            UpdateItemList();
            UpdateFeaturedItems();
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.OnItemPurchased -= HandleItemPurchased;
                _viewModel.OnItemDetailRequested -= HandleItemDetailRequested;
            }

            _disposables.Clear();
        }

        // ============================================
        // UI更新
        // ============================================

        private void UpdateItemList()
        {
            if (_viewModel == null || _itemListScrollView == null) return;

            _itemListScrollView.Clear();

            foreach (var item in _viewModel.DisplayedItems)
            {
                var itemElement = CreateItemElement(item);
                _itemListScrollView.Add(itemElement);
            }
        }

        private void UpdateFeaturedItems()
        {
            if (_viewModel == null || _featuredScrollView == null) return;

            _featuredScrollView.Clear();

            foreach (var item in _viewModel.FeaturedItems)
            {
                var itemElement = CreateFeaturedItemElement(item);
                _featuredScrollView.Add(itemElement);
            }
        }

        private VisualElement CreateItemElement(ShopItem item)
        {
            var container = new VisualElement();
            container.AddToClassList("shop-item");

            // アイテム名
            var nameLabel = new Label(item.ItemName);
            nameLabel.AddToClassList("item-name");
            container.Add(nameLabel);

            // 説明
            var descLabel = new Label(item.Description);
            descLabel.AddToClassList("item-description");
            container.Add(descLabel);

            // 価格と購入ボタン
            var bottomRow = new VisualElement();
            bottomRow.AddToClassList("item-bottom-row");

            var priceLabel = new Label($"🪙 {item.PriceCoins}");
            priceLabel.AddToClassList("item-price");
            bottomRow.Add(priceLabel);

            var buyButton = new Button(() => OnBuyClicked(item));
            bool isPurchased = _viewModel != null && _viewModel.IsPurchased(item.ItemId);
            buyButton.text = isPurchased ? "Owned" : "Buy";
            buyButton.SetEnabled(!isPurchased);
            buyButton.AddToClassList("buy-button");
            bottomRow.Add(buyButton);

            container.Add(bottomRow);

            // クリックで詳細表示
            container.RegisterCallback<ClickEvent>(_ => OnItemClicked(item));

            return container;
        }

        private VisualElement CreateFeaturedItemElement(ShopItem item)
        {
            var container = new VisualElement();
            container.AddToClassList("featured-item");

            var nameLabel = new Label(item.ItemName);
            nameLabel.AddToClassList("featured-item-name");
            container.Add(nameLabel);

            var priceLabel = new Label($"🪙 {item.PriceCoins}");
            priceLabel.AddToClassList("featured-item-price");
            container.Add(priceLabel);

            container.RegisterCallback<ClickEvent>(_ => OnItemClicked(item));

            return container;
        }

        private void UpdateCategoryButtons(string category)
        {
            // すべてのカテゴリボタンからアクティブクラスを削除
            _allCategoryButton?.RemoveFromClassList("category-active");
            _hairCategoryButton?.RemoveFromClassList("category-active");
            _outfitCategoryButton?.RemoveFromClassList("category-active");
            _accessoryCategoryButton?.RemoveFromClassList("category-active");

            // 選択されたカテゴリにアクティブクラスを追加
            switch (category)
            {
                case "All":
                    _allCategoryButton?.AddToClassList("category-active");
                    break;
                case "Hair":
                    _hairCategoryButton?.AddToClassList("category-active");
                    break;
                case "Outfit":
                    _outfitCategoryButton?.AddToClassList("category-active");
                    break;
                case "Accessory":
                    _accessoryCategoryButton?.AddToClassList("category-active");
                    break;
            }
        }

        // ============================================
        // イベントハンドラ
        // ============================================

        private void OnBackClicked()
        {
            Debug.Log("[ShopView] Back button clicked");

            var sceneService = ServiceLocator.Instance.Get<ISceneLoaderService>();
            if (sceneService != null)
            {
                sceneService.LoadMainMenuAsync().Forget();
            }
        }

        private void OnCategoryClicked(string category)
        {
            Debug.Log($"[ShopView] Category clicked: {category}");
            _viewModel?.ChangeCategory(category);
        }

        private void OnItemClicked(ShopItem item)
        {
            Debug.Log($"[ShopView] Item clicked: {item.ItemName}");
            _viewModel?.ShowItemDetail(item);
        }

        private void OnBuyClicked(ShopItem item)
        {
            Debug.Log($"[ShopView] Buy button clicked for: {item.ItemName}");
            _viewModel?.PurchaseItem(item);
        }

        // ============================================
        // ViewModelイベントハンドラ
        // ============================================

        private void HandleItemPurchased(ShopItem item)
        {
            Debug.Log($"[ShopView] Item purchased: {item.ItemName}");
            // 購入完了アニメーション/通知表示（未実装）
            UpdateItemList(); // リスト更新
        }

        private void HandleItemDetailRequested(ShopItem item)
        {
            Debug.Log($"[ShopView] Showing details for: {item.ItemName}");
            // 詳細ダイアログ表示（未実装）
        }
    }
}
