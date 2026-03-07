#nullable enable

using System;
using System.Collections.Generic;
using UniRx;
using ShopItem = ShaderOp.Online.Models.ShopItem;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// ショップ画面のViewModel
    /// </summary>
    public class ShopViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        /// <summary>すべてのアイテム</summary>
        private readonly List<ShopItem> _allItems = new();

        /// <summary>購入済みアイテムID</summary>
        private readonly HashSet<string> _purchasedItemIds = new();

        /// <summary>表示中のアイテムリスト</summary>
        public ReactiveCollection<ShopItem> DisplayedItems { get; } = new();

        /// <summary>注目アイテムリスト</summary>
        public ReactiveCollection<ShopItem> FeaturedItems { get; } = new();

        /// <summary>選択中のカテゴリ</summary>
        public ReactiveProperty<string> SelectedCategory { get; } = new("All");

        /// <summary>所持コイン</summary>
        public ReactiveProperty<int> Coins { get; } = new(1000);

        /// <summary>所持ジェム</summary>
        public ReactiveProperty<int> Gems { get; } = new(50);

        /// <summary>検索テキスト</summary>
        public ReactiveProperty<string> SearchText { get; } = new("");

        /// <summary>カテゴリ変更イベント</summary>
        public event Action<string>? OnCategoryChanged;

        /// <summary>アイテム購入イベント</summary>
        public event Action<ShopItem>? OnItemPurchased;

        /// <summary>アイテム詳細表示イベント</summary>
        public event Action<ShopItem>? OnItemDetailRequested;

        public ShopViewModel()
        {
            // カテゴリ変更を監視してフィルタリング
            SelectedCategory.Subscribe(category => FilterItems()).AddTo(_disposables);

            // 検索テキスト変更を監視
            SearchText.Subscribe(_ => FilterItems()).AddTo(_disposables);

            // テストアイテムをロード
            LoadTestItems();
            FilterItems();
        }

        /// <summary>
        /// テストアイテムをロード
        /// </summary>
        private void LoadTestItems()
        {
            _allItems.Add(new ShopItem
            {
                ItemId = "hair_001",
                ItemName = "Cool Hairstyle",
                Description = "A stylish new hairstyle",
                PriceCoins = 100,
                Category = ShaderOp.Online.Models.ItemCategory.Hair,
                IsLimitedTime = false
            });

            _allItems.Add(new ShopItem
            {
                ItemId = "outfit_001",
                ItemName = "Epic Outfit",
                Description = "Look amazing with this outfit",
                PriceCoins = 250,
                Category = ShaderOp.Online.Models.ItemCategory.Clothing,
                IsLimitedTime = false
            });

            _allItems.Add(new ShopItem
            {
                ItemId = "accessory_001",
                ItemName = "Fancy Hat",
                Description = "A very fancy hat",
                PriceCoins = 50,
                Category = ShaderOp.Online.Models.ItemCategory.Accessory,
                IsLimitedTime = false
            });

            _allItems.Add(new ShopItem
            {
                ItemId = "hair_002",
                ItemName = "Ponytail",
                Description = "Classic ponytail hairstyle",
                PriceCoins = 80,
                Category = ShaderOp.Online.Models.ItemCategory.Hair,
                IsLimitedTime = false
            });

            _allItems.Add(new ShopItem
            {
                ItemId = "outfit_002",
                ItemName = "Casual Wear",
                Description = "Comfortable casual outfit",
                PriceCoins = 150,
                Category = ShaderOp.Online.Models.ItemCategory.Clothing,
                IsVIPOnly = true
            });

            // 注目アイテムを抽出 (VIP限定アイテムを注目として扱う)
            foreach (var item in _allItems)
            {
                if (item.IsVIPOnly)
                {
                    FeaturedItems.Add(item);
                }
            }
        }

        /// <summary>
        /// アイテムをフィルタリング
        /// </summary>
        private void FilterItems()
        {
            DisplayedItems.Clear();

            foreach (var item in _allItems)
            {
                // カテゴリフィルター
                if (SelectedCategory.Value != "All" && item.Category.ToString() != SelectedCategory.Value)
                {
                    continue;
                }

                // 検索フィルター
                if (!string.IsNullOrWhiteSpace(SearchText.Value))
                {
                    string search = SearchText.Value.ToLower();
                    if (!item.ItemName.ToLower().Contains(search) &&
                        !item.Description.ToLower().Contains(search))
                    {
                        continue;
                    }
                }

                DisplayedItems.Add(item);
            }

            UnityEngine.Debug.Log($"[ShopViewModel] Displaying {DisplayedItems.Count} items in category '{SelectedCategory.Value}'");
        }

        /// <summary>
        /// カテゴリを変更
        /// </summary>
        public void ChangeCategory(string category)
        {
            if (SelectedCategory.Value == category) return;

            SelectedCategory.Value = category;
            UnityEngine.Debug.Log($"[ShopViewModel] Category changed to {category}");
            OnCategoryChanged?.Invoke(category);
        }

        /// <summary>
        /// アイテムを購入
        /// </summary>
        public void PurchaseItem(ShopItem item)
        {
            if (_purchasedItemIds.Contains(item.ItemId))
            {
                UnityEngine.Debug.LogWarning($"[ShopViewModel] Item '{item.ItemName}' already purchased");
                return;
            }

            if (Coins.Value < item.PriceCoins)
            {
                UnityEngine.Debug.LogWarning($"[ShopViewModel] Not enough coins to purchase '{item.ItemName}'");
                return;
            }

            Coins.Value -= item.PriceCoins;
            _purchasedItemIds.Add(item.ItemId);

            UnityEngine.Debug.Log($"[ShopViewModel] Purchased '{item.ItemName}' for {item.PriceCoins} coins");
            OnItemPurchased?.Invoke(item);
        }

        /// <summary>
        /// アイテム詳細を表示
        /// </summary>
        public void ShowItemDetail(ShopItem item)
        {
            UnityEngine.Debug.Log($"[ShopViewModel] Showing details for '{item.ItemName}'");
            OnItemDetailRequested?.Invoke(item);
        }

        /// <summary>
        /// アイテムが購入済みか確認
        /// </summary>
        public bool IsPurchased(string itemId)
        {
            return _purchasedItemIds.Contains(itemId);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
