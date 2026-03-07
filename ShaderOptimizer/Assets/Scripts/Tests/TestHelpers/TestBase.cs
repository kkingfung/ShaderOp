#nullable enable

using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace ShaderOp.Tests.Helpers
{
    /// <summary>
    /// テストベースクラス
    /// 共通のセットアップ・ティアダウン処理を提供
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// テスト中に作成されたGameObjectのリスト（自動クリーンアップ対象）
        /// </summary>
        protected List<GameObject> _testGameObjects = new List<GameObject>();

        /// <summary>
        /// 各テスト前に実行される基底セットアップ
        /// </summary>
        [SetUp]
        public virtual void BaseSetUp()
        {
            _testGameObjects = new List<GameObject>();
        }

        /// <summary>
        /// 各テスト後に実行される基底ティアダウン
        /// </summary>
        [TearDown]
        public virtual void BaseTearDown()
        {
            CleanupTestGameObjects();
        }

        /// <summary>
        /// テスト用GameObjectを作成（自動クリーンアップ対象）
        /// </summary>
        protected GameObject CreateGameObject(string name = "TestObject")
        {
            var obj = new GameObject(name);
            _testGameObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// テスト用GameObjectをコンポーネント付きで作成
        /// </summary>
        protected T CreateGameObjectWithComponent<T>(string name = "TestObject") where T : Component
        {
            var obj = CreateGameObject(name);
            return obj.AddComponent<T>();
        }

        /// <summary>
        /// 作成したGameObjectをすべて破棄
        /// </summary>
        protected void CleanupTestGameObjects()
        {
            foreach (var obj in _testGameObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            _testGameObjects.Clear();
        }

        /// <summary>
        /// 手動でGameObjectを登録（自動クリーンアップ対象に追加）
        /// </summary>
        protected void RegisterForCleanup(GameObject obj)
        {
            if (obj != null && !_testGameObjects.Contains(obj))
            {
                _testGameObjects.Add(obj);
            }
        }
    }
}
