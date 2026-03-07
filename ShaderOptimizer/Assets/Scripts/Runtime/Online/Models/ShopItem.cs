#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// ショップアイテムデータ
    /// </summary>
    /// <remarks>
    /// ショップで販売されるアイテムの情報を保持します。
    /// </remarks>
    [Serializable]
    public class ShopItem
    {
        /// <summary>アイテムID</summary>
        [SerializeField] private string _itemId = string.Empty;
        public string ItemId
        {
            get => _itemId;
            set => _itemId = value;
        }

        /// <summary>アイテム名</summary>
        [SerializeField] private string _itemName = string.Empty;
        public string ItemName
        {
            get => _itemName;
            set => _itemName = value;
        }

        /// <summary>アイテム説明</summary>
        [SerializeField] private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => _description = value;
        }

        /// <summary>アイテムカテゴリ</summary>
        [SerializeField] private ItemCategory _category = ItemCategory.Clothing;
        public ItemCategory Category
        {
            get => _category;
            set => _category = value;
        }

        /// <summary>価格（コイン）</summary>
        [SerializeField] private int _priceCoins = 0;
        public int PriceCoins
        {
            get => _priceCoins;
            set => _priceCoins = value;
        }

        /// <summary>VIP限定アイテムか</summary>
        [SerializeField] private bool _isVIPOnly = false;
        public bool IsVIPOnly
        {
            get => _isVIPOnly;
            set => _isVIPOnly = value;
        }

        /// <summary>期間限定アイテムか</summary>
        [SerializeField] private bool _isLimitedTime = false;
        public bool IsLimitedTime
        {
            get => _isLimitedTime;
            set => _isLimitedTime = value;
        }

        /// <summary>販売終了日時</summary>
        [SerializeField] private string _availableUntil = string.Empty;
        public string AvailableUntil
        {
            get => _availableUntil;
            set => _availableUntil = value;
        }

        /// <summary>アイテムアイコンパス</summary>
        [SerializeField] private string _iconPath = string.Empty;
        public string IconPath
        {
            get => _iconPath;
            set => _iconPath = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ShopItem() { }

        /// <summary>
        /// アイテム作成用コンストラクタ
        /// </summary>
        public ShopItem(string itemId, string itemName, ItemCategory category, int priceCoins)
        {
            _itemId = itemId;
            _itemName = itemName;
            _category = category;
            _priceCoins = priceCoins;
            _isVIPOnly = false;
            _isLimitedTime = false;
        }

        /// <summary>
        /// 購入可能か確認
        /// </summary>
        public bool CanPurchase(int playerCoins, bool isVIP)
        {
            if (_isVIPOnly && !isVIP) return false;
            if (playerCoins < _priceCoins) return false;
            if (_isLimitedTime && !string.IsNullOrEmpty(_availableUntil))
            {
                try
                {
                    DateTime endDate = DateTime.Parse(_availableUntil);
                    if (DateTime.UtcNow > endDate) return false;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// アイテムカテゴリ列挙型
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>髪型</summary>
        Hair = 0,

        /// <summary>服装</summary>
        Clothing = 1,

        /// <summary>アクセサリー</summary>
        Accessory = 2,

        /// <summary>エフェクト</summary>
        Effect = 3,

        /// <summary>その他</summary>
        Other = 4
    }
}
