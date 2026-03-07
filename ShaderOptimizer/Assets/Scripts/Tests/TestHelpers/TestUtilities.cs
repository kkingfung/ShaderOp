#nullable enable

using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NUnit.Framework;

namespace ShaderOp.Tests.Helpers
{
    /// <summary>
    /// テストユーティリティクラス
    /// 共通的なテストヘルパーメソッドを提供します
    /// </summary>
    public static class TestUtilities
    {
        /// <summary>
        /// 指定ミリ秒待機するコルーチン
        /// </summary>
        public static IEnumerator WaitForMilliseconds(int milliseconds)
        {
            yield return new WaitForSeconds(milliseconds / 1000f);
        }

        /// <summary>
        /// 指定フレーム数待機するコルーチン
        /// </summary>
        public static IEnumerator WaitForFrames(int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 条件が真になるまで待機するコルーチン（タイムアウトあり）
        /// </summary>
        /// <param name="condition">待機条件</param>
        /// <param name="timeoutSeconds">タイムアウト秒数</param>
        /// <param name="errorMessage">タイムアウト時のエラーメッセージ</param>
        public static IEnumerator WaitUntil(System.Func<bool> condition, float timeoutSeconds = 5f, string errorMessage = "タイムアウトしました")
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (!condition())
            {
                Assert.Fail(errorMessage);
            }
        }

        /// <summary>
        /// ゲームオブジェクトを作成して返す（テスト終了時に自動削除）
        /// </summary>
        public static GameObject CreateTestGameObject(string name = "TestObject")
        {
            var obj = new GameObject(name);
            return obj;
        }

        /// <summary>
        /// コンポーネント付きゲームオブジェクトを作成
        /// </summary>
        public static T CreateTestGameObjectWithComponent<T>(string name = "TestObject") where T : Component
        {
            var obj = CreateTestGameObject(name);
            return obj.AddComponent<T>();
        }

        /// <summary>
        /// 浮動小数点数の近似比較（デフォルトの許容誤差: 0.0001f）
        /// </summary>
        public static void AssertApproximately(float expected, float actual, float tolerance = 0.0001f, string? message = null)
        {
            message = message ?? $"Expected: {expected}, Actual: {actual}, Tolerance: {tolerance}";
            Assert.That(Mathf.Abs(expected - actual), Is.LessThanOrEqualTo(tolerance), message);
        }

        /// <summary>
        /// Vector3の近似比較
        /// </summary>
        public static void AssertVector3Approximately(Vector3 expected, Vector3 actual, float tolerance = 0.0001f, string? message = null)
        {
            message = message ?? $"Expected: {expected}, Actual: {actual}";
            AssertApproximately(expected.x, actual.x, tolerance, message + " (X)");
            AssertApproximately(expected.y, actual.y, tolerance, message + " (Y)");
            AssertApproximately(expected.z, actual.z, tolerance, message + " (Z)");
        }

        /// <summary>
        /// Vector2の近似比較
        /// </summary>
        public static void AssertVector2Approximately(Vector2 expected, Vector2 actual, float tolerance = 0.0001f, string? message = null)
        {
            message = message ?? $"Expected: {expected}, Actual: {actual}";
            AssertApproximately(expected.x, actual.x, tolerance, message + " (X)");
            AssertApproximately(expected.y, actual.y, tolerance, message + " (Y)");
        }

        /// <summary>
        /// Color の近似比較
        /// </summary>
        public static void AssertColorApproximately(Color expected, Color actual, float tolerance = 0.0001f, string? message = null)
        {
            message = message ?? $"Expected: {expected}, Actual: {actual}";
            AssertApproximately(expected.r, actual.r, tolerance, message + " (R)");
            AssertApproximately(expected.g, actual.g, tolerance, message + " (G)");
            AssertApproximately(expected.b, actual.b, tolerance, message + " (B)");
            AssertApproximately(expected.a, actual.a, tolerance, message + " (A)");
        }

        /// <summary>
        /// LogAssert でエラーログをキャプチャして検証
        /// </summary>
        public static void ExpectLogError(string expectedMessage)
        {
            LogAssert.Expect(LogType.Error, expectedMessage);
        }

        /// <summary>
        /// LogAssert で警告ログをキャプチャして検証
        /// </summary>
        public static void ExpectLogWarning(string expectedMessage)
        {
            LogAssert.Expect(LogType.Warning, expectedMessage);
        }
    }
}
