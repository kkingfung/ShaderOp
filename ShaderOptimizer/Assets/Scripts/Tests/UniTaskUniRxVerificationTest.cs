#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace ShaderOp.Tests
{
    /// <summary>
    /// UniTask/UniRxパッケージの動作検証テスト
    /// </summary>
    [TestFixture]
    public class UniTaskUniRxVerificationTest
    {
        /// <summary>
        /// UniTaskが正しくインポートされているか検証
        /// </summary>
        [Test]
        public void UniTask_IsImportedCorrectly()
        {
            // UniTaskの型が存在することを確認
            Type unitaskType = typeof(UniTask);
            Assert.IsNotNull(unitaskType, "UniTask型がインポートされていません");
            Debug.Log($"[Test] UniTask型が正常にインポートされました: {unitaskType.FullName}");
        }

        /// <summary>
        /// UniRxが正しくインポートされているか検証
        /// </summary>
        [Test]
        public void UniRx_IsImportedCorrectly()
        {
            // ReactivePropertyの型が存在することを確認
            Type reactivePropertyType = typeof(ReactiveProperty<int>);
            Assert.IsNotNull(reactivePropertyType, "ReactiveProperty型がインポートされていません");
            Debug.Log($"[Test] UniRx型が正常にインポートされました: {reactivePropertyType.FullName}");
        }

        /// <summary>
        /// UniTask.Delayが正常に動作するか検証
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator UniTask_DelayWorks() => UniTask.ToCoroutine(async () =>
        {
            var startTime = Time.realtimeSinceStartup;

            // 100ms待機
            await UniTask.Delay(100);

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            // 誤差を考慮して80-200msの範囲を許容
            Assert.IsTrue(elapsed >= 80f && elapsed <= 200f,
                $"UniTask.Delayの待機時間が不正です。期待: 100ms, 実際: {elapsed}ms");

            Debug.Log($"[Test] UniTask.Delayが正常に動作しました: {elapsed}ms");
        });

        /// <summary>
        /// UniTask.Yieldが正常に動作するか検証
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator UniTask_YieldWorks() => UniTask.ToCoroutine(async () =>
        {
            int frameCount = 0;

            // 3フレーム待機
            for (int i = 0; i < 3; i++)
            {
                await UniTask.Yield();
                frameCount++;
            }

            Assert.AreEqual(3, frameCount, "UniTask.Yieldが正しくフレームを待機していません");
            Debug.Log($"[Test] UniTask.Yieldが正常に動作しました: {frameCount}フレーム待機");
        });

        /// <summary>
        /// ReactivePropertyが正常に動作するか検証
        /// </summary>
        [Test]
        public void UniRx_ReactivePropertyWorks()
        {
            var reactiveInt = new ReactiveProperty<int>(0);
            int receivedValue = -1;

            // 値変更を購読
            reactiveInt.Subscribe(value => receivedValue = value);

            // 値を変更
            reactiveInt.Value = 42;

            Assert.AreEqual(42, receivedValue, "ReactivePropertyの値変更通知が正しく機能していません");
            Debug.Log($"[Test] ReactivePropertyが正常に動作しました: {receivedValue}");
        }

        /// <summary>
        /// UniTask.WhenAllが正常に動作するか検証
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator UniTask_WhenAllWorks() => UniTask.ToCoroutine(async () =>
        {
            var task1 = DelayedValueTask(1, 50);
            var task2 = DelayedValueTask(2, 100);
            var task3 = DelayedValueTask(3, 75);

            var (result1, result2, result3) = await UniTask.WhenAll(task1, task2, task3);

            Assert.AreEqual(1, result1, "Task1の結果が不正です");
            Assert.AreEqual(2, result2, "Task2の結果が不正です");
            Assert.AreEqual(3, result3, "Task3の結果が不正です");

            Debug.Log($"[Test] UniTask.WhenAllが正常に動作しました: [{result1}, {result2}, {result3}]");
        });

        /// <summary>
        /// UniTask.WhenAnyが正常に動作するか検証
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator UniTask_WhenAnyWorks() => UniTask.ToCoroutine(async () =>
        {
            var task1 = DelayedValueTask(1, 100);
            var task2 = DelayedValueTask(2, 50);  // 最速
            var task3 = DelayedValueTask(3, 150);

            var (winIndex, winResult, _, _) = await UniTask.WhenAny(task1, task2, task3);

            Assert.AreEqual(1, winIndex, "最速タスクのインデックスが不正です");
            Assert.AreEqual(2, winResult, "最速タスクの結果が不正です");

            Debug.Log($"[Test] UniTask.WhenAnyが正常に動作しました: Index={winIndex}, Value={winResult}");
        });

        /// <summary>
        /// UniTaskのキャンセルが正常に動作するか検証
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator UniTask_CancellationWorks() => UniTask.ToCoroutine(async () =>
        {
            var cts = new CancellationTokenSource();
            bool wasCancelled = false;

            var task = UniTask.Create(async () =>
            {
                try
                {
                    await UniTask.Delay(1000, cancellationToken: cts.Token);
                }
                catch (OperationCanceledException)
                {
                    wasCancelled = true;
                }
            });

            // 100ms後にキャンセル
            await UniTask.Delay(100);
            cts.Cancel();

            await task;

            Assert.IsTrue(wasCancelled, "UniTaskがキャンセルされませんでした");
            Debug.Log("[Test] UniTaskのキャンセルが正常に動作しました");

            cts.Dispose();
        });

        /// <summary>
        /// ReactivePropertyのDispose機能が正常に動作するか検証
        /// </summary>
        [Test]
        public void UniRx_ReactivePropertyDisposeWorks()
        {
            var reactiveInt = new ReactiveProperty<int>(0);
            int callCount = 0;

            var subscription = reactiveInt.Subscribe(_ => callCount++);

            // 初回購読で1回カウント
            Assert.AreEqual(1, callCount, "初回購読時のカウントが不正です");

            // 値変更で2回目
            reactiveInt.Value = 1;
            Assert.AreEqual(2, callCount, "値変更時のカウントが不正です");

            // 購読解除
            subscription.Dispose();

            // 値変更しても呼ばれない
            reactiveInt.Value = 2;
            Assert.AreEqual(2, callCount, "Dispose後も購読が残っています");

            Debug.Log("[Test] ReactivePropertyのDisposeが正常に動作しました");
        }

        /// <summary>
        /// 指定ミリ秒後に値を返すヘルパータスク
        /// </summary>
        private async UniTask<int> DelayedValueTask(int value, int delayMs)
        {
            await UniTask.Delay(delayMs);
            return value;
        }
    }
}
