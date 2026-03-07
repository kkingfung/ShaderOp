#nullable enable

using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// アバターアイテムクラス
    /// </summary>
    [System.Serializable]
    public class AvatarItem
    {
        [SerializeField] private string _itemId = string.Empty;
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private AvatarItemCategory _category = AvatarItemCategory.Hair;
        [SerializeField] private int _priceCoins = 0;
        [SerializeField] private bool _isVIPOnly = false;
        [SerializeField] private bool _isOwned = false;

        /// <summary>アイテムID</summary>
        public string ItemId => _itemId;
        
        /// <summary>表示名</summary>
        public string DisplayName => _displayName;
        
        /// <summary>カテゴリ</summary>
        public AvatarItemCategory Category => _category;
        
        /// <summary>価格（コイン）</summary>
        public int PriceCoins => _priceCoins;
        
        /// <summary>VIP限定フラグ</summary>
        public bool IsVIPOnly => _isVIPOnly;
        
        /// <summary>所持フラグ</summary>
        public bool IsOwned
        {
            get => _isOwned;
            set => _isOwned = value;
        }

        public AvatarItem(string itemId, string displayName, AvatarItemCategory category, int priceCoins, bool isVIPOnly = false)
        {
            _itemId = itemId;
            _displayName = displayName;
            _category = category;
            _priceCoins = priceCoins;
            _isVIPOnly = isVIPOnly;
        }
    }
}
