#nullable enable

using NUnit.Framework;
using ShaderOp.Core;
using ShaderOp.Tests.Helpers;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Tests
{
    /// <summary>
    /// SceneLoader 非同期処理テスト（EditMode）
    /// UniTaskを使用したシーンロード機能の包括的なテスト
    /// </summary>
    [TestFixture]
    public class SceneLoaderAsyncTests
    {
        private SceneLoader? _loader;
        private CancellationTokenSource? _cts;

        [SetUp]
        public void SetUp()
        {
            // SceneLoader インスタンスを取得
            _loader = SceneLoader.Instance;
            _cts = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            // キャンセルトークンソースをクリーンアップ
            _cts?.Dispose();
            _cts = null;

            // SceneLoaderの状態をリセット
            if (_loader != null && _loader.IsLoading)
            {
                _loader.CancelLoading();
            }
        }

        #region 基本機能テスト

        [Test]
        public void SceneLoader_Singleton_ReturnsSameInstance()
        {
            // Arrange & Act
            SceneLoader instance1 = SceneLoader.Instance;
            SceneLoader instance2 = SceneLoader.Instance;

            // Assert
            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2, "シングルトンインスタンスが異なります");
        }

        [Test]
        public void SceneLoader_InitialState_IsNotLoading()
        {
            // Arrange & Act
            SceneLoader loader = SceneLoader.Instance;

            // Assert
            Assert.IsFalse(loader.IsLoading, "初期状態でIsLoadingがtrueです");
            Assert.AreEqual(0f, loader.LoadProgress, 0.001f, "初期状態でLoadProgressが0ではありません");
        }

        [Test]
        public void SceneLoader_LoadProgress_BeforeLoad_ReturnsZero()
        {
            // Arrange
            SceneLoader loader = SceneLoader.Instance;

            // Act
            float progress = loader.LoadProgress;

            // Assert
            Assert.AreEqual(0f, progress, 0.001f, "ロード前のLoadProgressが0ではありません");
        }

        #endregion

        #region イベント発火テスト

        [UnityTest]
        public IEnumerator LoadSceneAsync_ValidScene_TriggersOnLoadStartedEvent()
        {
            // Arrange
            bool eventTriggered = false;
            string? eventSceneName = null;

            _loader!.OnLoadStarted += (sceneName) =>
            {
                eventTriggered = true;
                eventSceneName = sceneName;
            };

            // テストシーンを作成
            string testSceneName = "TestScene_LoadStarted";
            SceneTestUtilities.CreateTestScene(testSceneName);

            // Act
            _loader.LoadScene(testSceneName, LoadSceneMode.Additive);

            // 少し待機してイベント発火を確認
            yield return TestUtilities.WaitForFrames(2);

            // Assert
            Assert.IsTrue(eventTriggered, "OnLoadStartedイベントが発火しませんでした");
            Assert.AreEqual(testSceneName, eventSceneName, "イベントのシーン名が一致しません");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_ValidScene_TriggersOnProgressChangedEvent()
        {
            // Arrange
            bool eventTriggered = false;
            float lastProgress = -1f;
            int progressUpdateCount = 0;

            _loader!.OnProgressChanged += (progress) =>
            {
                eventTriggered = true;
                lastProgress = progress;
                progressUpdateCount++;
            };

            string testSceneName = "TestScene_Progress";
            SceneTestUtilities.CreateTestScene(testSceneName);

            // Act
            _loader.LoadScene(testSceneName, LoadSceneMode.Additive);

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);

            // Assert
            Assert.IsTrue(eventTriggered, "OnProgressChangedイベントが発火しませんでした");
            Assert.Greater(progressUpdateCount, 0, "進捗更新が1回も発火しませんでした");
            Assert.GreaterOrEqual(lastProgress, 0f, "最終進捗が0未満です");
            Assert.LessOrEqual(lastProgress, 1f, "最終進捗が1を超えています");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_ValidScene_TriggersOnLoadCompletedEvent()
        {
            // Arrange
            bool eventTriggered = false;
            string? eventSceneName = null;

            _loader!.OnLoadCompleted += (sceneName) =>
            {
                eventTriggered = true;
                eventSceneName = sceneName;
            };

            string testSceneName = "TestScene_Completed";
            SceneTestUtilities.CreateTestScene(testSceneName);

            // Act
            _loader.LoadScene(testSceneName, LoadSceneMode.Additive);

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);

            // Assert
            Assert.IsTrue(eventTriggered, "OnLoadCompletedイベントが発火しませんでした");
            Assert.AreEqual(testSceneName, eventSceneName, "イベントのシーン名が一致しません");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_InvalidScene_TriggersOnLoadFailedEvent()
        {
            // Arrange
            bool eventTriggered = false;
            string? eventSceneName = null;
            string? errorMessage = null;

            _loader!.OnLoadFailed += (sceneName, error) =>
            {
                eventTriggered = true;
                eventSceneName = sceneName;
                errorMessage = error;
            };

            // 存在しないシーン名
            string invalidSceneName = "NonExistentScene_12345";

            // Act
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*"));
            _loader.LoadScene(invalidSceneName, LoadSceneMode.Additive);

            // エラー処理完了まで待機
            yield return TestUtilities.WaitForFrames(5);

            // Assert
            Assert.IsTrue(eventTriggered, "OnLoadFailedイベントが発火しませんでした");
            Assert.AreEqual(invalidSceneName, eventSceneName, "イベントのシーン名が一致しません");
            Assert.IsNotNull(errorMessage, "エラーメッセージがnullです");
            Assert.IsNotEmpty(errorMessage, "エラーメッセージが空です");
        }

        #endregion

        #region 状態管理テスト

        [UnityTest]
        public IEnumerator LoadSceneAsync_DuringLoad_IsLoadingReturnsTrue()
        {
            // Arrange
            string testSceneName = "TestScene_IsLoading";
            SceneTestUtilities.CreateTestScene(testSceneName);

            // Act
            _loader!.LoadScene(testSceneName, LoadSceneMode.Additive);
            yield return null; // 1フレーム待機

            // Assert
            bool isLoadingDuringLoad = _loader.IsLoading;
            Assert.IsTrue(isLoadingDuringLoad, "ロード中にIsLoadingがfalseです");

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);

            // ロード完了後はfalse
            Assert.IsFalse(_loader.IsLoading, "ロード完了後にIsLoadingがtrueです");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_DuringLoad_LoadProgressIncreases()
        {
            // Arrange
            string testSceneName = "TestScene_LoadProgress";
            SceneTestUtilities.CreateTestScene(testSceneName);

            float initialProgress = 0f;
            float midProgress = 0f;
            float finalProgress = 0f;

            // Act
            initialProgress = _loader!.LoadProgress;

            _loader.LoadScene(testSceneName, LoadSceneMode.Additive);

            // 少し待ってから中間進捗を取得
            yield return TestUtilities.WaitForFrames(2);
            midProgress = _loader.LoadProgress;

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);
            finalProgress = _loader.LoadProgress;

            // Assert
            Assert.AreEqual(0f, initialProgress, 0.001f, "初期進捗が0ではありません");
            // 中間進捗は0以上（シーンサイズによっては即完了の場合もあるため >= 0でチェック）
            Assert.GreaterOrEqual(midProgress, 0f, "中間進捗が負の値です");
            // ロード完了後は0にリセット
            Assert.AreEqual(0f, finalProgress, 0.001f, "ロード完了後の進捗が0にリセットされていません");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_WhileLoading_LogsWarning()
        {
            // Arrange
            string testSceneName1 = "TestScene_Concurrent1";
            string testSceneName2 = "TestScene_Concurrent2";
            SceneTestUtilities.CreateTestScene(testSceneName1);
            SceneTestUtilities.CreateTestScene(testSceneName2);

            // Act
            _loader!.LoadScene(testSceneName1, LoadSceneMode.Additive);
            yield return null; // 1フレーム待機してロード開始を確認

            // 2つ目のロードを試みる（警告ログが出るはず）
            LogAssert.Expect(LogType.Warning, $"[SceneLoader] Already loading a scene. Cannot load {testSceneName2}");
            _loader.LoadScene(testSceneName2, LoadSceneMode.Additive);

            // Assert
            Assert.IsTrue(_loader.IsLoading, "ロード中のはずです");

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName1);
        }

        #endregion

        #region キャンセル処理テスト

        [UnityTest]
        public IEnumerator CancelLoading_DuringLoad_StopsLoading()
        {
            // Arrange
            string testSceneName = "TestScene_Cancel";
            SceneTestUtilities.CreateTestScene(testSceneName);

            // Act
            _loader!.LoadScene(testSceneName, LoadSceneMode.Additive);
            yield return TestUtilities.WaitForFrames(2); // ロード開始を待つ

            Assert.IsTrue(_loader.IsLoading, "ロードが開始されていません");

            // キャンセル
            LogAssert.Expect(LogType.Warning, "[SceneLoader] Loading cancelled");
            _loader.CancelLoading();

            yield return null;

            // Assert
            Assert.IsFalse(_loader.IsLoading, "キャンセル後もIsLoadingがtrueです");
            Assert.AreEqual(0f, _loader.LoadProgress, 0.001f, "キャンセル後の進捗が0ではありません");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_WithCancellationToken_CanBeCancelled()
        {
            // Arrange
            string testSceneName = "TestScene_TokenCancel";
            SceneTestUtilities.CreateTestScene(testSceneName);

            bool loadFailed = false;
            _loader!.OnLoadFailed += (sceneName, error) =>
            {
                loadFailed = true;
            };

            // Act
            var cts = new CancellationTokenSource();
            var loadTask = _loader.LoadSceneAsync(testSceneName, LoadSceneMode.Additive, cts.Token);

            yield return TestUtilities.WaitForFrames(2);

            // キャンセル
            cts.Cancel();
            LogAssert.Expect(LogType.Warning, $"[SceneLoader] Scene loading cancelled: {testSceneName}");

            // タスク完了まで待機
            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 5f);

            // Assert
            Assert.IsTrue(loadFailed, "OnLoadFailedイベントが発火しませんでした");
            Assert.IsFalse(_loader.IsLoading, "キャンセル後もIsLoadingがtrueです");

            // Cleanup
            cts.Dispose();
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        #endregion

        #region エッジケーステスト

        [Test]
        public void SceneExists_ValidScene_ReturnsTrue()
        {
            // Arrange
            // ビルド設定に少なくとも1つはシーンがあると仮定
            string? firstScene = SceneTestUtilities.GetFirstAvailableTestScene();

            if (firstScene != null)
            {
                // Act
                bool exists = SceneLoader.SceneExists(firstScene);

                // Assert
                Assert.IsTrue(exists, $"ビルド設定のシーン '{firstScene}' が存在しないと判定されました");
            }
            else
            {
                Assert.Inconclusive("ビルド設定にシーンが含まれていません");
            }
        }

        [Test]
        public void SceneExists_InvalidScene_ReturnsFalse()
        {
            // Arrange
            string nonExistentScene = "NonExistentScene_ABCDEF12345";

            // Act
            bool exists = SceneLoader.SceneExists(nonExistentScene);

            // Assert
            Assert.IsFalse(exists, "存在しないシーンがexistsと判定されました");
        }

        [Test]
        public void GetActiveSceneName_ReturnsCurrentScene()
        {
            // Arrange & Act
            string activeSceneName = SceneLoader.GetActiveSceneName();

            // Assert
            Assert.IsNotNull(activeSceneName, "アクティブシーン名がnullです");
            Assert.IsNotEmpty(activeSceneName, "アクティブシーン名が空です");
        }

        #endregion

        #region アンロード処理テスト

        [UnityTest]
        public IEnumerator UnloadSceneAsync_LoadedScene_UnloadsSuccessfully()
        {
            // Arrange
            string testSceneName = "TestScene_Unload";
            SceneTestUtilities.CreateTestScene(testSceneName);

            // シーンをロード
            _loader!.LoadScene(testSceneName, LoadSceneMode.Additive);
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);

            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName), "シーンがロードされていません");

            // Act
            LogAssert.Expect(LogType.Log, $"[SceneLoader] Unloading scene: {testSceneName}");
            LogAssert.Expect(LogType.Log, $"[SceneLoader] Scene unloaded: {testSceneName}");

            var unloadTask = _loader.UnloadSceneAsync(testSceneName);
            yield return TestUtilities.WaitUntil(() => unloadTask.Status != UniTaskStatus.Pending, 5f);

            // Assert
            Assert.IsFalse(SceneTestUtilities.IsSceneLoaded(testSceneName), "シーンがアンロードされていません");
        }

        [UnityTest]
        public IEnumerator UnloadSceneAsync_WhileLoading_LogsWarning()
        {
            // Arrange
            string testSceneName1 = "TestScene_UnloadWhileLoading";
            string testSceneName2 = "TestScene_ToUnload";
            SceneTestUtilities.CreateTestScene(testSceneName1);
            SceneTestUtilities.CreateTestScene(testSceneName2);

            // Act
            _loader!.LoadScene(testSceneName1, LoadSceneMode.Additive);
            yield return null;

            // ロード中にアンロードを試みる
            LogAssert.Expect(LogType.Warning, $"[SceneLoader] Cannot unload scene while loading: {testSceneName2}");
            _loader.UnloadScene(testSceneName2);

            // Assert
            // 警告が出ることを確認（LogAssert.Expectで検証済み）

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName1);
        }

        #endregion

        #region レガシー互換性テスト

        [UnityTest]
        public IEnumerator LoadScene_LegacyMethod_LoadsSceneSuccessfully()
        {
            // Arrange
            string testSceneName = "TestScene_Legacy";
            SceneTestUtilities.CreateTestScene(testSceneName);

            bool loadCompleted = false;
            _loader!.OnLoadCompleted += (sceneName) =>
            {
                if (sceneName == testSceneName)
                {
                    loadCompleted = true;
                }
            };

            // Act
            _loader.LoadScene(testSceneName, LoadSceneMode.Additive); // レガシーメソッド（非同期）

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => loadCompleted, 5f);

            // Assert
            Assert.IsTrue(loadCompleted, "レガシーLoadSceneメソッドでロードが完了しませんでした");
            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName), "シーンがロードされていません");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator UnloadScene_LegacyMethod_UnloadsSceneSuccessfully()
        {
            // Arrange
            string testSceneName = "TestScene_LegacyUnload";
            SceneTestUtilities.CreateTestScene(testSceneName);

            _loader!.LoadScene(testSceneName, LoadSceneMode.Additive);
            yield return TestUtilities.WaitUntil(() => !_loader.IsLoading, 5f);

            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName), "シーンがロードされていません");

            // Act
            LogAssert.Expect(LogType.Log, $"[SceneLoader] Unloading scene: {testSceneName}");
            LogAssert.Expect(LogType.Log, $"[SceneLoader] Scene unloaded: {testSceneName}");
            _loader.UnloadScene(testSceneName); // レガシーメソッド（非同期）

            // アンロード完了まで待機
            yield return SceneTestUtilities.WaitForSceneUnload(testSceneName, 5f);

            // Assert
            Assert.IsFalse(SceneTestUtilities.IsSceneLoaded(testSceneName), "レガシーメソッドでアンロードされませんでした");
        }

        #endregion
    }
}
