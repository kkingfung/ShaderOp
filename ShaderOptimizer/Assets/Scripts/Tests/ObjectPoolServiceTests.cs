#nullable enable

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using ShaderOp.Core.Services;

namespace ShaderOp.Tests
{
    /// <summary>
    /// ObjectPoolServiceの単体テスト
    /// </summary>
    public class ObjectPoolServiceTests
    {
        private GameObject? _testGameObject;
        private ObjectPoolService? _poolService;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObject作成
            _testGameObject = new GameObject("PoolServiceTestObject");
            _poolService = _testGameObject.AddComponent<ObjectPoolService>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
        }

        #region RegisterPool Tests

        [Test]
        public void RegisterPool_正常にプールを登録できる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();

            // Act
            _poolService!.RegisterPool(prefab, 5, 50);

            // Assert
            Assert.IsTrue(_poolService.IsRegistered<TestComponent>());

            // Cleanup
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void RegisterPool_NullPrefabで例外をスロー()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                _poolService!.RegisterPool<TestComponent>(null!);
            });
        }

        [Test]
        public void RegisterPool_コンポーネントがないPrefabで例外をスロー()
        {
            // Arrange
            var invalidPrefab = new GameObject("InvalidPrefab");

            // Act & Assert
            Assert.Throws<System.ArgumentException>(() =>
            {
                _poolService!.RegisterPool(invalidPrefab.GetComponent<TestComponent>());
            });

            // Cleanup
            Object.DestroyImmediate(invalidPrefab);
        }

        [Test]
        public void RegisterPool_同じ型を2回登録しても警告のみ()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();

            // Act
            _poolService!.RegisterPool(prefab, 5, 50);
            _poolService.RegisterPool(prefab, 10, 100); // 2回目

            // Assert
            Assert.IsTrue(_poolService.IsRegistered<TestComponent>());
            LogAssert.Expect(LogType.Warning, $"[ObjectPoolService] {typeof(TestComponent).Name}のプールは既に登録されています");

            // Cleanup
            Object.DestroyImmediate(prefab.gameObject);
        }

        #endregion

        #region Get/Return Tests

        [Test]
        public void Get_プールからオブジェクトを取得できる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);

            // Act
            var obj = _poolService.Get<TestComponent>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.gameObject.activeSelf);

            // Cleanup
            _poolService.Return(obj);
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void Get_位置と回転を指定してオブジェクトを取得できる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);
            var position = new Vector3(1, 2, 3);
            var rotation = Quaternion.Euler(45, 90, 135);

            // Act
            var obj = _poolService.Get<TestComponent>(position, rotation);

            // Assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(position, obj.transform.position);
            Assert.AreEqual(rotation, obj.transform.rotation);

            // Cleanup
            _poolService.Return(obj);
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void Get_未登録のプールで例外をスロー()
        {
            // Act & Assert
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                _poolService!.Get<TestComponent>();
            });
        }

        [Test]
        public void Return_プールにオブジェクトを返却できる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);
            var obj = _poolService.Get<TestComponent>();

            // Act
            _poolService.Return(obj);

            // Assert
            Assert.IsFalse(obj.gameObject.activeSelf);
            var stats = _poolService.GetStatistics<TestComponent>();
            Assert.AreEqual(0, stats.ActiveCount);
            Assert.AreEqual(1, stats.InactiveCount);

            // Cleanup
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void Return_Nullオブジェクトで警告のみ()
        {
            // Act
            _poolService!.Return<TestComponent>(null!);

            // Assert
            LogAssert.Expect(LogType.Warning, "[ObjectPoolService] nullオブジェクトを返却しようとしました");
        }

        [UnityTest]
        public IEnumerator GetAsync_非同期でオブジェクトを取得できる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);

            // Act
            var task = _poolService.GetAsync<TestComponent>();
            yield return new WaitUntil(() => task.Status == Cysharp.Threading.Tasks.UniTaskStatus.Succeeded);

            // Assert
            Assert.IsNotNull(task.GetAwaiter().GetResult());
            Assert.IsTrue(task.GetAwaiter().GetResult().gameObject.activeSelf);

            // Cleanup
            _poolService.Return(task.GetAwaiter().GetResult());
            Object.DestroyImmediate(prefab.gameObject);
        }

        #endregion

        #region Prewarm Tests

        [Test]
        public void Prewarm_指定数のオブジェクトを事前生成できる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);

            // Act
            _poolService.Prewarm<TestComponent>(10);

            // Assert
            var stats = _poolService.GetStatistics<TestComponent>();
            Assert.AreEqual(10, stats.InactiveCount);
            Assert.AreEqual(0, stats.ActiveCount);

            // Cleanup
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void Prewarm_未登録のプールで警告のみ()
        {
            // Act
            _poolService!.Prewarm<TestComponent>(5);

            // Assert
            LogAssert.Expect(LogType.Warning, $"[ObjectPoolService] {typeof(TestComponent).Name}のプールが登録されていません");
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_特定のプールをクリアできる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);
            _poolService.Prewarm<TestComponent>(5);

            // Act
            _poolService.Clear<TestComponent>();

            // Assert
            Assert.IsFalse(_poolService.IsRegistered<TestComponent>());
            var stats = _poolService.GetStatistics<TestComponent>();
            Assert.AreEqual(0, stats.TotalCount);

            // Cleanup
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void ClearAll_すべてのプールをクリアできる()
        {
            // Arrange
            var prefab1 = CreateTestPrefab<TestComponent>();
            var prefab2 = CreateTestPrefab<TestComponent2>();
            _poolService!.RegisterPool(prefab1, 5, 50);
            _poolService.RegisterPool(prefab2, 5, 50);
            _poolService.Prewarm<TestComponent>(5);
            _poolService.Prewarm<TestComponent2>(5);

            // Act
            _poolService.ClearAll();

            // Assert
            Assert.IsFalse(_poolService.IsRegistered<TestComponent>());
            Assert.IsFalse(_poolService.IsRegistered<TestComponent2>());

            // Cleanup
            Object.DestroyImmediate(prefab1.gameObject);
            Object.DestroyImmediate(prefab2.gameObject);
        }

        #endregion

        #region Statistics Tests

        [Test]
        public void GetStatistics_正確な統計情報を返す()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);
            _poolService.Prewarm<TestComponent>(10);
            var obj1 = _poolService.Get<TestComponent>();
            var obj2 = _poolService.Get<TestComponent>();

            // Act
            var stats = _poolService.GetStatistics<TestComponent>();

            // Assert
            Assert.AreEqual(2, stats.ActiveCount, "アクティブ数が一致しません");
            Assert.AreEqual(8, stats.InactiveCount, "非アクティブ数が一致しません");
            Assert.AreEqual(10, stats.TotalCount, "合計数が一致しません");

            // Cleanup
            _poolService.Return(obj1);
            _poolService.Return(obj2);
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void GetStatistics_未登録のプールでゼロを返す()
        {
            // Act
            var stats = _poolService!.GetStatistics<TestComponent>();

            // Assert
            Assert.AreEqual(0, stats.ActiveCount);
            Assert.AreEqual(0, stats.InactiveCount);
            Assert.AreEqual(0, stats.TotalCount);
        }

        #endregion

        #region IsRegistered Tests

        [Test]
        public void IsRegistered_登録済みプールでTrueを返す()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);

            // Act & Assert
            Assert.IsTrue(_poolService.IsRegistered<TestComponent>());

            // Cleanup
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void IsRegistered_未登録プールでFalseを返す()
        {
            // Act & Assert
            Assert.IsFalse(_poolService!.IsRegistered<TestComponent>());
        }

        #endregion

        #region IPoolable Tests

        [Test]
        public void Get_IPoolableコールバックが呼ばれる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestPoolableComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);

            // Act
            var obj = _poolService.Get<TestPoolableComponent>();

            // Assert
            Assert.IsTrue(obj.OnGetFromPoolCalled);
            Assert.IsFalse(obj.OnReturnToPoolCalled);

            // Cleanup
            _poolService.Return(obj);
            Object.DestroyImmediate(prefab.gameObject);
        }

        [Test]
        public void Return_IPoolableコールバックが呼ばれる()
        {
            // Arrange
            var prefab = CreateTestPrefab<TestPoolableComponent>();
            _poolService!.RegisterPool(prefab, 5, 50);
            var obj = _poolService.Get<TestPoolableComponent>();

            // Act
            _poolService.Return(obj);

            // Assert
            Assert.IsTrue(obj.OnReturnToPoolCalled);

            // Cleanup
            Object.DestroyImmediate(prefab.gameObject);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// テスト用Prefabを作成
        /// </summary>
        private T CreateTestPrefab<T>() where T : Component
        {
            var obj = new GameObject($"{typeof(T).Name}_Prefab");
            var component = obj.AddComponent<T>();
            return component;
        }

        #endregion

        #region Test Components

        /// <summary>
        /// テスト用コンポーネント1
        /// </summary>
        private class TestComponent : MonoBehaviour
        {
        }

        /// <summary>
        /// テスト用コンポーネント2
        /// </summary>
        private class TestComponent2 : MonoBehaviour
        {
        }

        /// <summary>
        /// IPoolableテスト用コンポーネント
        /// </summary>
        private class TestPoolableComponent : MonoBehaviour, IPoolable
        {
            public bool OnGetFromPoolCalled { get; private set; }
            public bool OnReturnToPoolCalled { get; private set; }

            public void OnGetFromPool()
            {
                OnGetFromPoolCalled = true;
                OnReturnToPoolCalled = false;
            }

            public void OnReturnToPool()
            {
                OnReturnToPoolCalled = true;
            }
        }

        #endregion
    }
}
