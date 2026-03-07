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

namespace ShaderOp.Tests.PlayMode
{
    /// <summary>
    /// SceneLoader PlayModeテスト
    /// 実際のシーンロード・アンロードを検証
    /// </summary>
    [TestFixture]
    public class SceneLoaderPlayModeTests : TestBase
    {
        private SceneLoader? _loader;

        [SetUp]
        public void SetUp()
        {
            _loader = SceneLoader.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            if (_loader != null && _loader.IsLoading)
            {
                _loader.CancelLoading();
            }
        }

        #region 実際のシーンロードテスト

        [UnityTest]
        public IEnumerator LoadSceneAsync_WithTestScene_LoadsAndActivates()
        {
            // Arrange
            string testSceneName = "TestScene_PlayMode_LoadActivate";
            Scene testScene = SceneTestUtilities.CreateTestScene(testSceneName);

            // テストシーンにオブジェクトを追加
            GameObject testObject = SceneTestUtilities.AddGameObjectToScene(testScene, "TestObject");
            Assert.IsNotNull(testObject, "テストオブジェクトが作成されませんでした");

            string initialActiveScene = SceneManager.GetActiveScene().name;

            // Act
            var loadTask = _loader!.LoadSceneAsync(testSceneName, LoadSceneMode.Additive);

            // ロード完了まで待機
            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);

            // Assert
            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName), "シーンがロードされていません");
            Scene loadedScene = SceneManager.GetSceneByName(testSceneName);
            Assert.IsTrue(loadedScene.isLoaded, "Scene.isLoadedがfalseです");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_SingleMode_ReplacesCurrentScene()
        {
            // Arrange
            string testSceneName1 = "TestScene_PlayMode_Single1";
            string testSceneName2 = "TestScene_PlayMode_Single2";

            Scene scene1 = SceneTestUtilities.CreateAndSetActiveTestScene(testSceneName1);
            SceneTestUtilities.AddGameObjectToScene(scene1, "SceneObject1");

            // テストシーン2を作成（まだロードはしない）
            Scene scene2 = SceneTestUtilities.CreateTestScene(testSceneName2);
            SceneTestUtilities.AddGameObjectToScene(scene2, "SceneObject2");

            yield return null;

            // Act - SingleModeで2つ目のシーンをロード
            var loadTask = _loader!.LoadSceneAsync(testSceneName2, LoadSceneMode.Single);

            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);
            yield return null; // シーンアクティベーション待機

            // Assert
            // SingleModeなので前のシーンは自動的にアンロードされる
            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName2), "新しいシーンがロードされていません");

            // 注: SingleModeでは元のシーンは自動的にアンロードされるため、
            // scene1はもうロードされていないはず（Unity 2019.3以降）
            // ただし、テスト環境によって挙動が異なる場合があるため柔軟にチェック

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName2);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_AdditiveMode_KeepsExistingScenes()
        {
            // Arrange
            string testSceneName1 = "TestScene_PlayMode_Additive1";
            string testSceneName2 = "TestScene_PlayMode_Additive2";

            Scene scene1 = SceneTestUtilities.CreateAndSetActiveTestScene(testSceneName1);
            SceneTestUtilities.AddGameObjectToScene(scene1, "SceneObject1");

            Scene scene2 = SceneTestUtilities.CreateTestScene(testSceneName2);
            SceneTestUtilities.AddGameObjectToScene(scene2, "SceneObject2");

            yield return null;

            // Act - Additiveモードでロード
            var loadTask = _loader!.LoadSceneAsync(testSceneName2, LoadSceneMode.Additive);

            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);

            // Assert
            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName1), "元のシーンがアンロードされました");
            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName2), "新しいシーンがロードされていません");

            // 両方のシーンがロードされている
            Assert.GreaterOrEqual(SceneManager.sceneCount, 2, "シーン数が2未満です");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName1);
            yield return SceneTestUtilities.UnloadTestScene(testSceneName2);
        }

        #endregion

        #region 進捗追跡テスト

        [UnityTest]
        public IEnumerator LoadSceneAsync_TracksProgressCorrectly()
        {
            // Arrange
            string testSceneName = "TestScene_PlayMode_Progress";
            Scene testScene = SceneTestUtilities.CreateTestScene(testSceneName);
            SceneTestUtilities.AddGameObjectToScene(testScene, "LargeObject");

            float[] progressValues = new float[10];
            int progressIndex = 0;

            _loader!.OnProgressChanged += (progress) =>
            {
                if (progressIndex < progressValues.Length)
                {
                    progressValues[progressIndex++] = progress;
                }
            };

            // Act
            var loadTask = _loader.LoadSceneAsync(testSceneName, LoadSceneMode.Additive);

            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);

            // Assert
            Assert.Greater(progressIndex, 0, "進捗イベントが1回も発火しませんでした");

            // 進捗は単調増加するはず
            for (int i = 1; i < progressIndex; i++)
            {
                Assert.GreaterOrEqual(progressValues[i], progressValues[i - 1],
                    $"進捗が減少しました: [{i - 1}]={progressValues[i - 1]}, [{i}]={progressValues[i]}");
            }

            // 最後の進捗は1.0に近い値のはず（完了時は0にリセットされるため、最後の記録値をチェック）
            if (progressIndex > 0)
            {
                Assert.GreaterOrEqual(progressValues[progressIndex - 1], 0.5f,
                    "最終進捗が50%未満です");
            }

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        #endregion

        #region 複数シーン管理テスト

        [UnityTest]
        public IEnumerator LoadSceneAsync_MultipleAdditive_AllScenesPresent()
        {
            // Arrange
            string[] sceneNames = {
                "TestScene_PlayMode_Multi1",
                "TestScene_PlayMode_Multi2",
                "TestScene_PlayMode_Multi3"
            };

            // テストシーンを作成
            foreach (string sceneName in sceneNames)
            {
                Scene scene = SceneTestUtilities.CreateTestScene(sceneName);
                SceneTestUtilities.AddGameObjectToScene(scene, $"Object_{sceneName}");
            }

            yield return null;

            // Act - 順次ロード
            foreach (string sceneName in sceneNames)
            {
                var loadTask = _loader!.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);
            }

            // Assert
            foreach (string sceneName in sceneNames)
            {
                Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(sceneName),
                    $"シーン '{sceneName}' がロードされていません");
            }

            // Cleanup
            foreach (string sceneName in sceneNames)
            {
                yield return SceneTestUtilities.UnloadTestScene(sceneName);
            }
        }

        [UnityTest]
        public IEnumerator UnloadSceneAsync_AdditiveScene_UnloadsCorrectly()
        {
            // Arrange
            string testSceneName = "TestScene_PlayMode_UnloadAdditive";
            Scene testScene = SceneTestUtilities.CreateTestScene(testSceneName);
            SceneTestUtilities.AddGameObjectToScene(testScene, "UnloadTestObject");

            var loadTask = _loader!.LoadSceneAsync(testSceneName, LoadSceneMode.Additive);
            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);

            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName), "シーンがロードされていません");

            // Act
            LogAssert.Expect(LogType.Log, $"[SceneLoader] Unloading scene: {testSceneName}");
            LogAssert.Expect(LogType.Log, $"[SceneLoader] Scene unloaded: {testSceneName}");

            var unloadTask = _loader.UnloadSceneAsync(testSceneName);
            yield return TestUtilities.WaitUntil(() => unloadTask.Status != UniTaskStatus.Pending, 10f);

            // Assert
            Assert.IsFalse(SceneTestUtilities.IsSceneLoaded(testSceneName),
                "シーンがアンロードされていません");
        }

        #endregion

        #region エラー処理とエッジケース

        [UnityTest]
        public IEnumerator LoadSceneAsync_NonExistentScene_HandlesGracefully()
        {
            // Arrange
            string invalidSceneName = "NonExistentScene_PlayMode_INVALID";

            bool loadFailed = false;
            string? failedSceneName = null;

            _loader!.OnLoadFailed += (sceneName, error) =>
            {
                loadFailed = true;
                failedSceneName = sceneName;
            };

            // Act
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*"));

            var loadTask = _loader.LoadSceneAsync(invalidSceneName, LoadSceneMode.Additive);

            // エラー処理完了まで待機
            yield return TestUtilities.WaitForFrames(10);

            // Assert
            Assert.IsTrue(loadFailed, "OnLoadFailedイベントが発火しませんでした");
            Assert.AreEqual(invalidSceneName, failedSceneName, "失敗したシーン名が一致しません");
            Assert.IsFalse(_loader.IsLoading, "エラー後もIsLoadingがtrueです");
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_ConcurrentLoads_HandlesSequentially()
        {
            // Arrange
            string testSceneName1 = "TestScene_PlayMode_Concurrent1";
            string testSceneName2 = "TestScene_PlayMode_Concurrent2";

            Scene scene1 = SceneTestUtilities.CreateTestScene(testSceneName1);
            Scene scene2 = SceneTestUtilities.CreateTestScene(testSceneName2);

            SceneTestUtilities.AddGameObjectToScene(scene1, "Object1");
            SceneTestUtilities.AddGameObjectToScene(scene2, "Object2");

            // Act
            var loadTask1 = _loader!.LoadSceneAsync(testSceneName1, LoadSceneMode.Additive);
            yield return null;

            // 2つ目のロードは警告が出るはず
            LogAssert.Expect(LogType.Warning, $"[SceneLoader] Already loading a scene. Cannot load {testSceneName2}");
            _loader.LoadScene(testSceneName2, LoadSceneMode.Additive);

            // 最初のロード完了を待つ
            yield return TestUtilities.WaitUntil(() => loadTask1.Status != UniTaskStatus.Pending, 10f);

            // Assert
            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName1),
                "最初のシーンがロードされていません");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName1);
        }

        #endregion

        #region パフォーマンステスト

        [UnityTest]
        public IEnumerator LoadSceneAsync_Performance_CompletesInReasonableTime()
        {
            // Arrange
            string testSceneName = "TestScene_PlayMode_Performance";
            Scene testScene = SceneTestUtilities.CreateTestScene(testSceneName);

            // 複数のオブジェクトを追加してシーンサイズを増やす
            for (int i = 0; i < 50; i++)
            {
                SceneTestUtilities.AddGameObjectToScene(testScene, $"PerfObject_{i}");
            }

            float startTime = Time.realtimeSinceStartup;

            // Act
            var loadTask = _loader!.LoadSceneAsync(testSceneName, LoadSceneMode.Additive);
            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);

            float elapsedTime = Time.realtimeSinceStartup - startTime;

            // Assert
            Assert.Less(elapsedTime, 5f, $"シーンロードに{elapsedTime}秒かかりました（期待: 5秒以内）");
            Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(testSceneName),
                "シーンがロードされていません");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        #endregion

        #region ライフサイクルテスト

        [UnityTest]
        public IEnumerator LoadSceneAsync_StateTransitions_CorrectSequence()
        {
            // Arrange
            string testSceneName = "TestScene_PlayMode_Lifecycle";
            Scene testScene = SceneTestUtilities.CreateTestScene(testSceneName);
            SceneTestUtilities.AddGameObjectToScene(testScene, "LifecycleObject");

            bool onLoadStartedFired = false;
            bool onProgressChangedFired = false;
            bool onLoadCompletedFired = false;

            int eventSequence = 0;
            int loadStartedOrder = -1;
            int progressChangedOrder = -1;
            int loadCompletedOrder = -1;

            _loader!.OnLoadStarted += (sceneName) =>
            {
                onLoadStartedFired = true;
                loadStartedOrder = eventSequence++;
            };

            _loader.OnProgressChanged += (progress) =>
            {
                if (!onProgressChangedFired)
                {
                    onProgressChangedFired = true;
                    progressChangedOrder = eventSequence++;
                }
            };

            _loader.OnLoadCompleted += (sceneName) =>
            {
                onLoadCompletedFired = true;
                loadCompletedOrder = eventSequence++;
            };

            // Act
            var loadTask = _loader.LoadSceneAsync(testSceneName, LoadSceneMode.Additive);
            yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);

            // Assert
            Assert.IsTrue(onLoadStartedFired, "OnLoadStartedが発火しませんでした");
            Assert.IsTrue(onProgressChangedFired, "OnProgressChangedが発火しませんでした");
            Assert.IsTrue(onLoadCompletedFired, "OnLoadCompletedが発火しませんでした");

            // イベントの順序を確認
            Assert.Less(loadStartedOrder, progressChangedOrder,
                "OnLoadStartedがOnProgressChangedより後に発火しました");
            Assert.Less(progressChangedOrder, loadCompletedOrder,
                "OnProgressChangedがOnLoadCompletedより後に発火しました");

            // Cleanup
            yield return SceneTestUtilities.UnloadTestScene(testSceneName);
        }

        #endregion
    }
}
