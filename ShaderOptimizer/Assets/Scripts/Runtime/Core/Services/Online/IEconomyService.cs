#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ShaderOp.Online.Models;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// 経済システムサービス（ショップ・通貨・VIP管理）
    /// </summary>
    /// <remarks>
    /// ゲーム内通貨、ショップアイテム、VIPサブスクリプションを管理します。
    /// </remarks>
    public interface IEconomyService
    {
        /// <summary>所持コイン数</summary>
        int Coins { get; }

        /// <summary>所持ジェム数</summary>
        int Gems { get; }

        /// <summary>VIPステータス</summary>
        bool IsVipActive { get; }

        /// <summary>VIP有効期限</summary>
        DateTime? VipExpiryDate { get; }

        /// <summary>
        /// ショップアイテムリストを取得します（REST API経由）
        /// </summary>
        /// <param name="category">カテゴリフィルター（nullの場合すべて）</param>
        /// <returns>ショップアイテムリスト</returns>
        UniTask<List<ShopItem>> GetShopItemsAsync(ShopCategory? category = null);

        /// <summary>
        /// アイテムを購入します
        /// </summary>
        /// <param name="itemId">アイテムID</param>
        /// <param name="currencyType">支払い通貨タイプ</param>
        /// <returns>購入結果</returns>
        UniTask<PurchaseResult> PurchaseItemAsync(string itemId, CurrencyType currencyType);

        /// <summary>
        /// VIPサブスクリプションを購入します
        /// </summary>
        /// <returns>成功時true</returns>
        UniTask<bool> SubscribeVipAsync();

        /// <summary>
        /// コインを追加します（報酬など）
        /// </summary>
        /// <param name="amount">追加量</param>
        void AddCoins(int amount);

        /// <summary>
        /// ジェムを追加します（報酬など）
        /// </summary>
        /// <param name="amount">追加量</param>
        void AddGems(int amount);

        /// <summary>
        /// 通貨残高をサーバーから同期します
        /// </summary>
        UniTask SyncCurrencyAsync();

        /// <summary>通貨が変更されたときに発火</summary>
        event Action<CurrencyType, int>? OnCurrencyChanged;

        /// <summary>VIPステータスが変更されたときに発火</summary>
        event Action<bool>? OnVipStatusChanged;

        /// <summary>アイテム購入完了時に発火</summary>
        event Action<PurchaseResult>? OnPurchaseCompleted;
    }
}
