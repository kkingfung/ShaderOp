#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ShaderOp.Core
{
    /// <summary>
    /// Addressablesアセット読み込みマネージャー
    /// </summary>
    /// <remarks>
    /// UniTaskを使用したゼロアロケーション非同期読み込み
    /// アセットのライフサイクル管理とキャッシュ機能を提供
    /// </remarks>
    public class AssetLoader : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static AssetLoader? _instance;
        public static AssetLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AssetLoader");
                    _instance = go.AddComponent<AssetLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>読み込み済みアセットハンドルのキャッシュ</summary>
        private readonly Dictionary<string, AsyncOperationHandle> _loadedHandles = new();

        /// <summary>読み込み進捗イベント</summary>
        public event Action<string, float>? OnLoadProgress;

        /// <summary>読み込み完了イベント</summary>
        public event Action<string>? OnLoadCompleted;

        /// <summary>読み込み失敗イベント</summary>
        public event Action<string, string>? OnLoadFailed;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// アセットを非同期で読み込み（ジェネリック版）
        /// </summary>
        public async UniTask<T?> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            try
            {
                Debug.Log($"[AssetLoader] Loading asset: {address}");

                // 既に読み込み済みの場合はキャッシュから返す
                if (_loadedHandles.TryGetValue(address, out var existingHandle))
                {
                    if (existingHandle.IsValid() && existingHandle.IsDone)
                    {
                        Debug.Log($"[AssetLoader] Returning cached asset: {address}");
                        return existingHandle.Result as T;
                    }
                }

                // 非同期読み込み開始
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);

                // 進捗を監視
                while (!handle.IsDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    OnLoadProgress?.Invoke(address, handle.PercentComplete);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                // 完了確認
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception($"Failed to load asset: {address}");
                }

                T result = handle.Result;

                // ハンドルをキャッシュ
                _loadedHandles[address] = handle;

                OnLoadCompleted?.Invoke(address);
                Debug.Log($"[AssetLoader] Asset loaded successfully: {address}");

                return result;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[AssetLoader] Asset loading cancelled: {address}");
                OnLoadFailed?.Invoke(address, "Loading cancelled");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetLoader] Failed to load asset '{address}': {ex.Message}");
                OnLoadFailed?.Invoke(address, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// GameObjectをインスタンス化（Addressables版）
        /// </summary>
        public async UniTask<GameObject?> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform? parent = null, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.Log($"[AssetLoader] Instantiating GameObject: {address}");

                // 非同期インスタンス化
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, position, rotation, parent);

                // 進捗を監視
                while (!handle.IsDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    OnLoadProgress?.Invoke(address, handle.PercentComplete);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                // 完了確認
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception($"Failed to instantiate GameObject: {address}");
                }

                GameObject result = handle.Result;

                OnLoadCompleted?.Invoke(address);
                Debug.Log($"[AssetLoader] GameObject instantiated successfully: {address}");

                return result;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[AssetLoader] GameObject instantiation cancelled: {address}");
                OnLoadFailed?.Invoke(address, "Instantiation cancelled");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetLoader] Failed to instantiate GameObject '{address}': {ex.Message}");
                OnLoadFailed?.Invoke(address, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 複数のアセットを並列で読み込み
        /// </summary>
        public async UniTask<List<T>> LoadAssetsAsync<T>(List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            List<T> results = new List<T>(addresses.Count);

            // 並列読み込み（UniTask.WhenAll使用）
            var tasks = new List<UniTask<T?>>(addresses.Count);
            foreach (string address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, cancellationToken));
            }

            T?[] loadedAssets = await UniTask.WhenAll(tasks);

            // nullでないものだけ結果に追加
            foreach (var asset in loadedAssets)
            {
                if (asset != null)
                {
                    results.Add(asset);
                }
            }

            return results;
        }

        /// <summary>
        /// アセットを解放
        /// </summary>
        public void ReleaseAsset(string address)
        {
            if (_loadedHandles.TryGetValue(address, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    Debug.Log($"[AssetLoader] Released asset: {address}");
                }

                _loadedHandles.Remove(address);
            }
            else
            {
                Debug.LogWarning($"[AssetLoader] Asset not found in cache: {address}");
            }
        }

        /// <summary>
        /// GameObjectインスタンスを解放
        /// </summary>
        public bool ReleaseInstance(GameObject instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("[AssetLoader] Cannot release null instance");
                return false;
            }

            bool result = Addressables.ReleaseInstance(instance);
            if (result)
            {
                Debug.Log($"[AssetLoader] Released instance: {instance.name}");
            }
            else
            {
                Debug.LogWarning($"[AssetLoader] Failed to release instance: {instance.name}");
            }

            return result;
        }

        /// <summary>
        /// すべての読み込み済みアセットを解放
        /// </summary>
        public void ReleaseAllAssets()
        {
            foreach (var kvp in _loadedHandles)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }

            _loadedHandles.Clear();
            Debug.Log("[AssetLoader] Released all assets");
        }

        /// <summary>
        /// アセットが読み込み済みかチェック
        /// </summary>
        public bool IsAssetLoaded(string address)
        {
            if (_loadedHandles.TryGetValue(address, out var handle))
            {
                return handle.IsValid() && handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded;
            }
            return false;
        }

        /// <summary>
        /// アセットのダウンロードサイズを取得
        /// </summary>
        public async UniTask<long> GetDownloadSizeAsync(string address, CancellationToken cancellationToken = default)
        {
            try
            {
                AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(address);

                while (!handle.IsDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    return 0;
                }

                long size = handle.Result;
                Addressables.Release(handle);
                return size;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetLoader] Failed to get download size for '{address}': {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// アセットをダウンロード（プリロード）
        /// </summary>
        public async UniTask<bool> DownloadDependenciesAsync(string address, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.Log($"[AssetLoader] Downloading dependencies for: {address}");

                AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(address);

                // 進捗を監視
                while (!handle.IsDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    OnLoadProgress?.Invoke(address, handle.PercentComplete);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                await handle.ToUniTask(cancellationToken: cancellationToken);
                Addressables.Release(handle);

                Debug.Log($"[AssetLoader] Dependencies downloaded: {address}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetLoader] Failed to download dependencies for '{address}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 読み込み済みアセット情報をログ出力
        /// </summary>
        public void LogLoadedAssets()
        {
            Debug.Log($"[AssetLoader] Loaded assets count: {_loadedHandles.Count}");
            foreach (var kvp in _loadedHandles)
            {
                string status = kvp.Value.IsValid() ? kvp.Value.Status.ToString() : "Invalid";
                Debug.Log($"  - {kvp.Key}: {status}");
            }
        }

        private void OnDestroy()
        {
            ReleaseAllAssets();
        }
    }
}
