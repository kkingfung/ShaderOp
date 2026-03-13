#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// オブジェクトプールサービス実装
    /// </summary>
    /// <remarks>
    /// Unity標準のUnityEngine.Pool.ObjectPool<T>を使用して型別にプールを管理します。
    /// IPoolableインターフェースをサポートし、取得/返却時のライフサイクルフックを提供します。
    /// ServiceLocatorで管理され、シングルトンとして動作します。
    /// </remarks>
    public class ObjectPoolService : MonoBehaviour, IObjectPoolService
    {
        // プールルート（非アクティブオブジェクトの親）
        private Transform? _poolRoot;

        // 型ごとのプール（ComponentのGameObjectを管理）
        private readonly Dictionary<Type, IObjectPool<GameObject>> _pools = new();

        // 型ごとのPrefab参照
        private readonly Dictionary<Type, GameObject> _prefabs = new();

        // プール設定情報
        private readonly Dictionary<Type, PoolConfig> _configs = new();

        // アクティブなオブジェクトの追跡（統計用）
        private readonly Dictionary<Type, HashSet<GameObject>> _activeObjects = new();

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Awake()
        {
            // プールルート作成
            GameObject poolRootObj = new GameObject("PoolRoot");
            poolRootObj.transform.SetParent(transform);
            _poolRoot = poolRootObj.transform;

            Debug.Log("[ObjectPoolService] 初期化完了");
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        private void OnDestroy()
        {
            ClearAll();
            Debug.Log("[ObjectPoolService] すべてのプールをクリアしました");
        }

        /// <summary>
        /// プールを登録
        /// </summary>
        public void RegisterPool<T>(T prefab, int defaultCapacity = 10, int maxSize = 100) where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefabがnullです");
            }

            Type type = typeof(T);

            if (_pools.ContainsKey(type))
            {
                Debug.LogWarning($"[ObjectPoolService] {type.Name}のプールは既に登録されています");
                return;
            }

            // Prefabが指定された型のコンポーネントを持っているか確認
            if (prefab.GetComponent<T>() == null)
            {
                throw new ArgumentException($"Prefabに{type.Name}コンポーネントがアタッチされていません", nameof(prefab));
            }

            // Prefabを保存
            _prefabs[type] = prefab.gameObject;

            // 設定を保存
            _configs[type] = new PoolConfig
            {
                DefaultCapacity = defaultCapacity,
                MaxSize = maxSize
            };

            // アクティブオブジェクト追跡用セット作成
            _activeObjects[type] = new HashSet<GameObject>();

            // ObjectPoolを作成
            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreatePooledObject<T>(),
                actionOnGet: OnGetFromPool<T>,
                actionOnRelease: OnReturnToPool<T>,
                actionOnDestroy: obj => UnityEngine.Object.Destroy(obj),
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );

            _pools[type] = pool;

            Debug.Log($"[ObjectPoolService] {type.Name}のプールを登録しました (容量: {defaultCapacity}, 最大: {maxSize})");
        }

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        public T Get<T>() where T : Component
        {
            Type type = typeof(T);

            if (!_pools.TryGetValue(type, out var pool))
            {
                throw new InvalidOperationException($"{type.Name}のプールが登録されていません。RegisterPool<{type.Name}>()を先に呼び出してください。");
            }

            GameObject obj = pool.Get();
            T? component = obj.GetComponent<T>();

            if (component == null)
            {
                Debug.LogError($"[ObjectPoolService] プールから取得したオブジェクトに{type.Name}がアタッチされていません");
                throw new InvalidOperationException($"プールオブジェクトに{type.Name}コンポーネントがありません");
            }

            return component;
        }

        /// <summary>
        /// プールからオブジェクトを取得（位置・回転指定）
        /// </summary>
        public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            T component = Get<T>();
            component.transform.position = position;
            component.transform.rotation = rotation;
            return component;
        }

        /// <summary>
        /// プールからオブジェクトを非同期で取得
        /// </summary>
        /// <remarks>
        /// 現在は同期的に動作しますが、将来的にAddressablesからの動的ロードに対応可能
        /// </remarks>
        public async UniTask<T> GetAsync<T>() where T : Component
        {
            // 現在は同期的に取得
            T result = Get<T>();

            // フレーム待機してAsync動作をシミュレート
            await UniTask.Yield();

            return result;
        }

        /// <summary>
        /// プールにオブジェクトを返却
        /// </summary>
        public void Return<T>(T obj) where T : Component
        {
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPoolService] nullオブジェクトを返却しようとしました");
                return;
            }

            Type type = typeof(T);

            if (!_pools.TryGetValue(type, out var pool))
            {
                Debug.LogWarning($"[ObjectPoolService] {type.Name}のプールが登録されていません。オブジェクトを破棄します。");
                UnityEngine.Object.Destroy(obj.gameObject);
                return;
            }

            pool.Release(obj.gameObject);
        }

        /// <summary>
        /// プレウォーム（事前にオブジェクトを生成してプールに格納）
        /// </summary>
        public void Prewarm<T>(int count) where T : Component
        {
            Type type = typeof(T);

            if (!_pools.TryGetValue(type, out var pool))
            {
                Debug.LogWarning($"[ObjectPoolService] {type.Name}のプールが登録されていません");
                return;
            }

            List<GameObject> tempList = new List<GameObject>(count);

            // 一時的に取得
            for (int i = 0; i < count; i++)
            {
                tempList.Add(pool.Get());
            }

            // すぐに返却
            foreach (var obj in tempList)
            {
                pool.Release(obj);
            }

            Debug.Log($"[ObjectPoolService] {type.Name}を{count}個プレウォームしました");
        }

        /// <summary>
        /// 特定のプールをクリア
        /// </summary>
        public void Clear<T>() where T : Component
        {
            Type type = typeof(T);

            if (!_pools.TryGetValue(type, out var pool))
            {
                Debug.LogWarning($"[ObjectPoolService] {type.Name}のプールが登録されていません");
                return;
            }

            // アクティブなオブジェクトを先に返却
            if (_activeObjects.TryGetValue(type, out var activeSet))
            {
                List<GameObject> activeList = new List<GameObject>(activeSet);
                foreach (var obj in activeList)
                {
                    if (obj != null)
                    {
                        pool.Release(obj);
                    }
                }
                activeSet.Clear();
            }

            // プールをクリア
            pool.Clear();

            // 辞書から削除
            _pools.Remove(type);
            _prefabs.Remove(type);
            _configs.Remove(type);
            _activeObjects.Remove(type);

            Debug.Log($"[ObjectPoolService] {type.Name}のプールをクリアしました");
        }

        /// <summary>
        /// すべてのプールをクリア
        /// </summary>
        public void ClearAll()
        {
            // すべてのアクティブオブジェクトを返却
            foreach (var kvp in _activeObjects)
            {
                if (_pools.TryGetValue(kvp.Key, out var pool))
                {
                    foreach (var obj in kvp.Value)
                    {
                        if (obj != null)
                        {
                            pool.Release(obj);
                        }
                    }
                }
                kvp.Value.Clear();
            }

            // すべてのプールをクリア
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }

            _pools.Clear();
            _prefabs.Clear();
            _configs.Clear();
            _activeObjects.Clear();

            Debug.Log("[ObjectPoolService] すべてのプールをクリアしました");
        }

        /// <summary>
        /// プールの統計情報を取得
        /// </summary>
        public PoolStatistics GetStatistics<T>() where T : Component
        {
            Type type = typeof(T);

            if (!_pools.ContainsKey(type))
            {
                return new PoolStatistics(0, 0, 0);
            }

            int activeCount = _activeObjects.TryGetValue(type, out var activeSet) ? activeSet.Count : 0;

            // プール内のオブジェクト数はCountFromメソッドで取得
            var pool = (ObjectPool<GameObject>)_pools[type];
            int inactiveCount = pool.CountInactive;
            int totalCount = activeCount + inactiveCount;

            return new PoolStatistics(activeCount, inactiveCount, totalCount);
        }

        /// <summary>
        /// プールが登録されているか確認
        /// </summary>
        public bool IsRegistered<T>() where T : Component
        {
            return _pools.ContainsKey(typeof(T));
        }

        #region Private Helper Methods

        /// <summary>
        /// プールオブジェクトを生成
        /// </summary>
        private GameObject CreatePooledObject<T>() where T : Component
        {
            Type type = typeof(T);

            if (!_prefabs.TryGetValue(type, out var prefab))
            {
                throw new InvalidOperationException($"{type.Name}のPrefabが見つかりません");
            }

            GameObject obj = UnityEngine.Object.Instantiate(prefab, _poolRoot);
            obj.name = $"{prefab.name} (Pooled)";
            obj.SetActive(false);

            return obj;
        }

        /// <summary>
        /// プールからオブジェクトを取得した時の処理
        /// </summary>
        private void OnGetFromPool<T>(GameObject obj) where T : Component
        {
            Type type = typeof(T);

            // アクティブリストに追加
            if (_activeObjects.TryGetValue(type, out var activeSet))
            {
                activeSet.Add(obj);
            }

            // IPoolableコールバック
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnGetFromPool();

            // オブジェクトをアクティブ化
            obj.SetActive(true);
        }

        /// <summary>
        /// プールにオブジェクトを返却した時の処理
        /// </summary>
        private void OnReturnToPool<T>(GameObject obj) where T : Component
        {
            Type type = typeof(T);

            // アクティブリストから削除
            if (_activeObjects.TryGetValue(type, out var activeSet))
            {
                activeSet.Remove(obj);
            }

            // IPoolableコールバック
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnReturnToPool();

            // オブジェクトを非アクティブ化
            obj.SetActive(false);

            // プールルートの子に設定
            obj.transform.SetParent(_poolRoot);
        }

        #endregion

        /// <summary>
        /// プール設定情報（内部使用）
        /// </summary>
        private class PoolConfig
        {
            public int DefaultCapacity { get; set; }
            public int MaxSize { get; set; }
        }
    }
}
