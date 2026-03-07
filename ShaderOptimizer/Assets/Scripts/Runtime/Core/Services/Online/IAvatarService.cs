#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ShaderOp.Online.Models;
using UnityEngine;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// アバター管理サービス
    /// </summary>
    /// <remarks>
    /// アバターのカスタマイズ、アイテムのアンロック、
    /// 3Dプレビュー生成を管理します。
    /// </remarks>
    public interface IAvatarService
    {
        /// <summary>現在のアバターデータ</summary>
        AvatarData? CurrentAvatar { get; }

        /// <summary>
        /// 所持しているアイテムリストを取得します
        /// </summary>
        /// <param name="category">カテゴリフィルター（nullの場合すべて）</param>
        /// <returns>所持アイテムリスト</returns>
        UniTask<List<AvatarItem>> GetOwnedItemsAsync(AvatarItemCategory? category = null);

        /// <summary>
        /// 利用可能なすべてのアイテムを取得します
        /// </summary>
        /// <param name="category">カテゴリフィルター（nullの場合すべて）</param>
        /// <returns>アイテムリスト</returns>
        UniTask<List<AvatarItem>> GetAvailableItemsAsync(AvatarItemCategory? category = null);

        /// <summary>
        /// アバターパーツを変更します（ローカルのみ）
        /// </summary>
        /// <param name="category">カテゴリ</param>
        /// <param name="itemId">アイテムID</param>
        void ChangeAvatarPart(AvatarItemCategory category, string itemId);

        /// <summary>
        /// アバターの髪の色を変更します
        /// </summary>
        /// <param name="colorHex">16進数カラーコード</param>
        void ChangeHairColor(string colorHex);

        /// <summary>
        /// アバターの目の色を変更します
        /// </summary>
        /// <param name="colorHex">16進数カラーコード</param>
        void ChangeEyeColor(string colorHex);

        /// <summary>
        /// アバターの肌の色を変更します
        /// </summary>
        /// <param name="skinToneId">肌色ID</param>
        void ChangeSkinTone(int skinToneId);

        /// <summary>
        /// 現在のアバターをサーバーに保存します
        /// </summary>
        /// <returns>成功時true</returns>
        UniTask<bool> SaveAvatarAsync();

        /// <summary>
        /// アイテムをアンロックします
        /// </summary>
        /// <param name="itemId">アイテムID</param>
        /// <returns>成功時true</returns>
        UniTask<bool> UnlockItemAsync(string itemId);

        /// <summary>
        /// 3Dアバタープレビューを生成します
        /// </summary>
        /// <param name="avatarData">アバターデータ</param>
        /// <returns>生成されたGameObject</returns>
        UniTask<GameObject?> GenerateAvatarPreviewAsync(AvatarData avatarData);

        /// <summary>
        /// 他のプレイヤーのアバターを読み込みます
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>アバターデータ</returns>
        UniTask<AvatarData?> LoadPlayerAvatarAsync(string userId);

        /// <summary>アバターが変更されたときに発火</summary>
        event Action<AvatarData>? OnAvatarChanged;

        /// <summary>アイテムがアンロックされたときに発火</summary>
        event Action<string>? OnItemUnlocked;
    }
}
