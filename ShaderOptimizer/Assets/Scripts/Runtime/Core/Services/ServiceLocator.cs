#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// サービスロケーターパターン実装
    /// </summary>
    /// <remarks>
    /// ゲーム全体で使用するサービスの登録・取得を管理します。
    /// GameBootstrap.csで初期化時に各サービスを登録し、
    /// 各コンポーネントから取得して使用します。
    /// </remarks>
    public class ServiceLocator
    {
        /// <summary>シングルトンインスタンス</summary>
        private static ServiceLocator? _instance;
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }
                return _instance;
            }
        }

        /// <summary>登録されたサービスの辞書</summary>
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// プライベートコンストラクタ（シングルトンパターン）
        /// </summary>
        private ServiceLocator() { }

        /// <summary>
        /// サービスを登録
        /// </summary>
        /// <typeparam name="T">サービスインターフェース型</typeparam>
        /// <param name="service">サービスインスタンス</param>
        public void Register<T>(T service) where T : class
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Overwriting...");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"[ServiceLocator] Service {type.Name} registered successfully.");
            }
        }

        /// <summary>
        /// サービスを取得
        /// </summary>
        /// <typeparam name="T">サービスインターフェース型</typeparam>
        /// <returns>サービスインスタンス、存在しない場合はnull</returns>
        public T? Get<T>() where T : class
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out object? service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceLocator] Service {type.Name} not found! Make sure it's registered in GameBootstrap.");
            return null;
        }

        /// <summary>
        /// サービスが登録されているか確認
        /// </summary>
        /// <typeparam name="T">サービスインターフェース型</typeparam>
        /// <returns>登録されている場合true</returns>
        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// サービスの登録を解除
        /// </summary>
        /// <typeparam name="T">サービスインターフェース型</typeparam>
        public void Unregister<T>() where T : class
        {
            Type type = typeof(T);

            if (_services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Service {type.Name} unregistered.");
            }
            else
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} was not registered.");
            }
        }

        /// <summary>
        /// すべてのサービスをクリア
        /// </summary>
        public void Clear()
        {
            Debug.Log($"[ServiceLocator] Clearing {_services.Count} services...");
            _services.Clear();
        }

        /// <summary>
        /// テスト用：インスタンスをリセット
        /// </summary>
        internal static void ResetForTesting()
        {
            _instance?.Clear();
            _instance = null;
        }
    }
}
