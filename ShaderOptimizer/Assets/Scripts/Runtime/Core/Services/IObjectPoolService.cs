#nullable enable

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// オブジェクトプールサービスインターフェース
    /// </summary>
    /// <remarks>
    /// GameObjectの生成/破棄コストを削減するためのオブジェクトプール機能を提供します。
    /// ミニゲームのHexTileやGamePieceなど、頻繁に生成/削除されるオブジェクトに使用します。
    /// ServiceLocatorパターンで管理され、各コンポーネントから取得して使用します。
    /// </remarks>
    public interface IObjectPoolService
    {
        /// <summary>
        /// プールを登録
        /// </summary>
        /// <typeparam name="T">プール対象のコンポーネント型</typeparam>
        /// <param name="prefab">Prefab（T型コンポーネントが付いている必要がある）</param>
        /// <param name="defaultCapacity">デフォルト容量</param>
        /// <param name="maxSize">最大サイズ</param>
        void RegisterPool<T>(T prefab, int defaultCapacity = 10, int maxSize = 100) where T : Component;

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        /// <typeparam name="T">取得するコンポーネント型</typeparam>
        /// <returns>プールされたオブジェクトのコンポーネント</returns>
        T Get<T>() where T : Component;

        /// <summary>
        /// プールからオブジェクトを取得（位置・回転指定）
        /// </summary>
        /// <typeparam name="T">取得するコンポーネント型</typeparam>
        /// <param name="position">配置位置</param>
        /// <param name="rotation">回転</param>
        /// <returns>プールされたオブジェクトのコンポーネント</returns>
        T Get<T>(Vector3 position, Quaternion rotation) where T : Component;

        /// <summary>
        /// プールからオブジェクトを非同期で取得（将来の拡張用）
        /// </summary>
        /// <typeparam name="T">取得するコンポーネント型</typeparam>
        /// <returns>プールされたオブジェクトのコンポーネント</returns>
        /// <remarks>
        /// 現在は同期的に動作しますが、将来的にAddressablesからの動的ロードに対応可能
        /// </remarks>
        UniTask<T> GetAsync<T>() where T : Component;

        /// <summary>
        /// プールにオブジェクトを返却
        /// </summary>
        /// <typeparam name="T">返却するコンポーネント型</typeparam>
        /// <param name="obj">返却するオブジェクトのコンポーネント</param>
        void Return<T>(T obj) where T : Component;

        /// <summary>
        /// プレウォーム（事前にオブジェクトを生成してプールに格納）
        /// </summary>
        /// <typeparam name="T">プレウォーム対象のコンポーネント型</typeparam>
        /// <param name="count">事前生成数</param>
        void Prewarm<T>(int count) where T : Component;

        /// <summary>
        /// 特定のプールをクリア
        /// </summary>
        /// <typeparam name="T">クリア対象のコンポーネント型</typeparam>
        void Clear<T>() where T : Component;

        /// <summary>
        /// すべてのプールをクリア
        /// </summary>
        void ClearAll();

        /// <summary>
        /// プールの統計情報を取得（デバッグ用）
        /// </summary>
        /// <typeparam name="T">統計対象のコンポーネント型</typeparam>
        /// <returns>統計情報（アクティブ数、非アクティブ数、合計数）</returns>
        PoolStatistics GetStatistics<T>() where T : Component;

        /// <summary>
        /// プールが登録されているか確認
        /// </summary>
        /// <typeparam name="T">確認対象のコンポーネント型</typeparam>
        /// <returns>登録されている場合true</returns>
        bool IsRegistered<T>() where T : Component;
    }

    /// <summary>
    /// プール統計情報
    /// </summary>
    public struct PoolStatistics
    {
        /// <summary>アクティブなオブジェクト数</summary>
        public int ActiveCount;

        /// <summary>プールに格納されている非アクティブなオブジェクト数</summary>
        public int InactiveCount;

        /// <summary>合計オブジェクト数</summary>
        public int TotalCount;

        public PoolStatistics(int active, int inactive, int total)
        {
            ActiveCount = active;
            InactiveCount = inactive;
            TotalCount = total;
        }

        public override string ToString()
        {
            return $"Active: {ActiveCount}, Inactive: {InactiveCount}, Total: {TotalCount}";
        }
    }
}
