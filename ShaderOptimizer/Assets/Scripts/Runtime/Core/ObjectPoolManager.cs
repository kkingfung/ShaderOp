#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ShaderOp.Core
{
    /// <summary>
    /// オブジェクトプール管理クラス
    /// </summary>
    /// <remarks>
    /// Unity Object Poolを使用してGameObjectの生成/破棄コストを削減
    /// プレウォーム機能とプール統計機能を提供
    /// </remarks>
    public class ObjectPoolManager : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static ObjectPoolManager? _instance;
        public static ObjectPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ObjectPoolManager");
                    _instance = go.AddComponent<ObjectPoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>プールのディクショナリ（Prefab名をキーとする）</summary>
        private readonly Dictionary<string, IObjectPool<GameObject>> _pools = new();

        /// <summary>プール設定のディクショナリ</summary>
        private readonly Dictionary<string, PoolConfig> _configs = new();

        /// <summary>プールされているオブジェクトの親Transform</summary>
        private Transform? _poolRoot;

        /// <summary>
        /// プール設定
        /// </summary>
        [Serializable]
        public class PoolConfig
        {
            /// <summary>Prefab</summary>
            public GameObject Prefab;

            /// <summary>デフォルト容量</summary>
            public int DefaultCapacity = 10;

            /// <summary>最大サイズ</summary>
            public int MaxSize = 100;

            /// <summary>コレクションチェック有効化（デバッグ用）</summary>
            public bool CollectionCheck = false;

            /// <summary>プレウォーム数</summary>
            public int PrewarmCount = 0;

            public PoolConfig(GameObject prefab, int defaultCapacity = 10, int maxSize = 100, int prewarmCount = 0)
            {
                Prefab = prefab;
                DefaultCapacity = defaultCapacity;
                MaxSize = maxSize;
                PrewarmCount = prewarmCount;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                // プールルートを作成
                GameObject poolRootObj = new GameObject("PoolRoot");
                poolRootObj.transform.SetParent(transform);
                _poolRoot = poolRootObj.transform;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// プールを登録（初回のみ）
        /// </summary>
        public void RegisterPool(string poolName, PoolConfig config)
        {
            if (_pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool already exists: {poolName}");
                return;
            }

            _configs[poolName] = config;

            // ObjectPoolを作成
            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreatePooledObject(poolName, config.Prefab),
                actionOnGet: (obj) => OnGetFromPool(obj),
                actionOnRelease: (obj) => OnReleaseToPool(obj),
                actionOnDestroy: (obj) => OnDestroyPoolObject(obj),
                collectionCheck: config.CollectionCheck,
                defaultCapacity: config.DefaultCapacity,
                maxSize: config.MaxSize
            );

            _pools[poolName] = pool;

            // プレウォーム
            if (config.PrewarmCount > 0)
            {
                PrewarmPool(poolName, config.PrewarmCount);
            }

            Debug.Log($"[ObjectPoolManager] Registered pool: {poolName} (Capacity: {config.DefaultCapacity}, Max: {config.MaxSize})");
        }

        /// <summary>
        /// プレウォーム（事前にオブジェクトを生成してプールに戻す）
        /// </summary>
        private void PrewarmPool(string poolName, int count)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"[ObjectPoolManager] Pool not found: {poolName}");
                return;
            }

            List<GameObject> tempList = new List<GameObject>(count);

            // 一時的に取得
            for (int i = 0; i < count; i++)
            {
                tempList.Add(pool.Get());
            }

            // すべて戻す
            foreach (GameObject obj in tempList)
            {
                pool.Release(obj);
            }

            Debug.Log($"[ObjectPoolManager] Prewarmed pool: {poolName} with {count} objects");
        }

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        public GameObject Get(string poolName)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"[ObjectPoolManager] Pool not found: {poolName}. Did you forget to register it?");
                return new GameObject($"[ERROR] {poolName}");
            }

            return pool.Get();
        }

        /// <summary>
        /// プールからオブジェクトを取得（位置・回転指定）
        /// </summary>
        public GameObject Get(string poolName, Vector3 position, Quaternion rotation)
        {
            GameObject obj = Get(poolName);
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        /// <summary>
        /// プールにオブジェクトを返却
        /// </summary>
        public void Release(string poolName, GameObject obj)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"[ObjectPoolManager] Pool not found: {poolName}");
                Destroy(obj);
                return;
            }

            pool.Release(obj);
        }

        /// <summary>
        /// オブジェクト生成時の処理
        /// </summary>
        private GameObject CreatePooledObject(string poolName, GameObject prefab)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = $"{poolName}_Pooled";

            // IPoolableインターフェースがあれば初期化
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnCreated();

            return obj;
        }

        /// <summary>
        /// プールから取得時の処理
        /// </summary>
        private void OnGetFromPool(GameObject obj)
        {
            obj.SetActive(true);

            // IPoolableインターフェースがあれば通知
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnGetFromPool();
        }

        /// <summary>
        /// プールに返却時の処理
        /// </summary>
        private void OnReleaseToPool(GameObject obj)
        {
            obj.SetActive(false);

            // IPoolableインターフェースがあれば通知
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnReleaseToPool();

            // プールルート配下に移動
            if (_poolRoot != null)
            {
                obj.transform.SetParent(_poolRoot);
            }
        }

        /// <summary>
        /// プールオブジェクト破棄時の処理
        /// </summary>
        private void OnDestroyPoolObject(GameObject obj)
        {
            // IPoolableインターフェースがあれば通知
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnDestroyed();

            Destroy(obj);
        }

        /// <summary>
        /// 特定のプールをクリア
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (_pools.TryGetValue(poolName, out var pool))
            {
                pool.Clear();
                Debug.Log($"[ObjectPoolManager] Cleared pool: {poolName}");
            }
        }

        /// <summary>
        /// すべてのプールをクリア
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }

            Debug.Log("[ObjectPoolManager] Cleared all pools");
        }

        /// <summary>
        /// プールの統計情報を取得
        /// </summary>
        public void LogPoolStatistics()
        {
            foreach (var kvp in _pools)
            {
                string poolName = kvp.Key;
                var pool = kvp.Value as ObjectPool<GameObject>;

                if (pool != null)
                {
                    Debug.Log($"[ObjectPoolManager] Pool '{poolName}': Active={pool.CountActive}, Inactive={pool.CountInactive}, All={pool.CountAll}");
                }
            }
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }

    /// <summary>
    /// プール可能なオブジェクトのインターフェース
    /// </summary>
    /// <remarks>
    /// このインターフェースを実装することで、プールのライフサイクルイベントを受け取れる
    /// </remarks>
    public interface IPoolable
    {
        /// <summary>プールで生成された時に呼ばれる（初回のみ）</summary>
        void OnCreated();

        /// <summary>プールから取得された時に呼ばれる</summary>
        void OnGetFromPool();

        /// <summary>プールに返却された時に呼ばれる</summary>
        void OnReleaseToPool();

        /// <summary>プールから破棄される時に呼ばれる</summary>
        void OnDestroyed();
    }
}
