#nullable enable

using System;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Core.Services.Online
{
    /// <summary>
    /// セーブデータサービスインターフェース
    /// </summary>
    /// <remarks>
    /// プレイヤーデータのローカル保存・ロードを管理します。
    /// 将来的にはクラウド同期機能も追加予定です。
    /// </remarks>
    public interface ISaveDataService
    {
        /// <summary>セーブデータが存在するか</summary>
        bool HasSaveData { get; }

        /// <summary>現在のプレイヤーID</summary>
        string? CurrentPlayerId { get; }

        /// <summary>
        /// セーブデータを保存
        /// </summary>
        /// <typeparam name="T">データ型</typeparam>
        /// <param name="key">保存キー</param>
        /// <param name="data">保存するデータ</param>
        UniTask<bool> SaveAsync<T>(string key, T data) where T : class;

        /// <summary>
        /// セーブデータを読み込み
        /// </summary>
        /// <typeparam name="T">データ型</typeparam>
        /// <param name="key">保存キー</param>
        /// <returns>読み込んだデータ、存在しない場合はnull</returns>
        UniTask<T?> LoadAsync<T>(string key) where T : class;

        /// <summary>
        /// セーブデータを削除
        /// </summary>
        /// <param name="key">保存キー</param>
        UniTask<bool> DeleteAsync(string key);

        /// <summary>
        /// セーブデータが存在するか確認
        /// </summary>
        /// <param name="key">保存キー</param>
        /// <returns>存在する場合true</returns>
        bool Exists(string key);

        /// <summary>
        /// すべてのセーブデータをクリア
        /// </summary>
        UniTask ClearAllAsync();
    }
}
