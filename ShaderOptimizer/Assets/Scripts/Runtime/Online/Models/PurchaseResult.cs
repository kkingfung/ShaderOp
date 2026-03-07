#nullable enable

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// 購入結果クラス
    /// </summary>
    [System.Serializable]
    public class PurchaseResult
    {
        /// <summary>購入成功フラグ</summary>
        public bool Success { get; set; }
        
        /// <summary>エラーメッセージ（失敗時）</summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>購入後のコイン残高</summary>
        public int RemainingCoins { get; set; }
        
        /// <summary>購入アイテムID</summary>
        public string? PurchasedItemId { get; set; }
        
        /// <summary>トランザクションID</summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        public static PurchaseResult CreateSuccess(string itemId, int remainingCoins, string transactionId)
        {
            return new PurchaseResult
            {
                Success = true,
                PurchasedItemId = itemId,
                RemainingCoins = remainingCoins,
                TransactionId = transactionId
            };
        }

        /// <summary>
        /// 失敗結果を作成
        /// </summary>
        public static PurchaseResult CreateFailure(string errorMessage)
        {
            return new PurchaseResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
