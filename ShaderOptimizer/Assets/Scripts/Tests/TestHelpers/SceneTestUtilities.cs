#nullable enable

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace ShaderOp.Tests.Helpers
{
    /// <summary>
    /// シーンテスト用ユーティリティ
    /// テストシーンの作成、管理、クリーンアップ機能を提供
    /// </summary>
    public static class SceneTestUtilities
    {
        /// <summary>テストで作成されたシーンのリスト</summary>
        private static readonly List<string> _createdScenes = new List<string>();

        /// <summary>
        /// テスト用の空シーンを作成
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <returns>作成されたシーン</returns>
        public static Scene CreateTestScene(string sceneName)
        {
            Scene scene = SceneManager.CreateScene(sceneName);
            _createdScenes.Add(sceneName);
            return scene;
        }

        /// <summary>
        /// テスト用の空シーンを作成してアクティブに設定
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <returns>作成されたシーン</returns>
        public static Scene CreateAndSetActiveTestScene(string sceneName)
        {
            Scene scene = CreateTestScene(sceneName);
            SceneManager.SetActiveScene(scene);
            return scene;
        }

        /// <summary>
        /// テストシーンにGameObjectを追加
        /// </summary>
        /// <param name="scene">シーン</param>
        /// <param name="name">GameObjectの名前</param>
        /// <returns>作成されたGameObject</returns>
        public static GameObject AddGameObjectToScene(Scene scene, string name = "TestObject")
        {
            var obj = new GameObject(name);
            SceneManager.MoveGameObjectToScene(obj, scene);
            return obj;
        }

        /// <summary>
        /// 指定シーンをアンロード（非同期）
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        public static IEnumerator UnloadTestScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
                if (unloadOp != null)
                {
                    yield return unloadOp;
                }
            }
            _createdScenes.Remove(sceneName);
        }

        /// <summary>
        /// 作成したすべてのテストシーンをクリーンアップ
        /// </summary>
        public static IEnumerator CleanupAllTestScenes()
        {
            // コピーを作成してイテレート（削除中のリスト変更を避ける）
            var scenesToCleanup = new List<string>(_createdScenes);

            foreach (var sceneName in scenesToCleanup)
            {
                yield return UnloadTestScene(sceneName);
            }

            _createdScenes.Clear();
        }

        /// <summary>
        /// シーンが存在するかチェック（ビルド設定に含まれているか）
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <returns>存在する場合true</returns>
        public static bool SceneExistsInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameFromPath == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// シーンがロード済みかチェック
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <returns>ロード済みの場合true</returns>
        public static bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }

        /// <summary>
        /// アクティブシーンの名前を取得
        /// </summary>
        /// <returns>アクティブシーン名</returns>
        public static string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// ロード済みのすべてのシーン名を取得
        /// </summary>
        /// <returns>シーン名のリスト</returns>
        public static List<string> GetLoadedSceneNames()
        {
            var sceneNames = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                sceneNames.Add(SceneManager.GetSceneAt(i).name);
            }
            return sceneNames;
        }

        /// <summary>
        /// シーンロードが完了するまで待機（タイムアウトあり）
        /// </summary>
        /// <param name="sceneName">待機するシーン名</param>
        /// <param name="timeoutSeconds">タイムアウト秒数</param>
        public static IEnumerator WaitForSceneLoad(string sceneName, float timeoutSeconds = 10f)
        {
            float elapsed = 0f;
            while (!IsSceneLoaded(sceneName) && elapsed < timeoutSeconds)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (!IsSceneLoaded(sceneName))
            {
                Assert.Fail($"シーン '{sceneName}' のロードがタイムアウトしました（{timeoutSeconds}秒）");
            }
        }

        /// <summary>
        /// シーンアンロードが完了するまで待機（タイムアウトあり）
        /// </summary>
        /// <param name="sceneName">待機するシーン名</param>
        /// <param name="timeoutSeconds">タイムアウト秒数</param>
        public static IEnumerator WaitForSceneUnload(string sceneName, float timeoutSeconds = 10f)
        {
            float elapsed = 0f;
            while (IsSceneLoaded(sceneName) && elapsed < timeoutSeconds)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (IsSceneLoaded(sceneName))
            {
                Assert.Fail($"シーン '{sceneName}' のアンロードがタイムアウトしました（{timeoutSeconds}秒）");
            }
        }

        /// <summary>
        /// テストで使用可能な最初のビルド設定シーンを取得
        /// </summary>
        /// <returns>シーン名（存在しない場合はnull）</returns>
        public static string? GetFirstAvailableTestScene()
        {
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(0);
                return System.IO.Path.GetFileNameWithoutExtension(scenePath);
            }
            return null;
        }

        /// <summary>
        /// 実行時に作成されたテストシーンの数を取得
        /// </summary>
        /// <returns>テストシーン数</returns>
        public static int GetCreatedTestSceneCount()
        {
            return _createdScenes.Count;
        }
    }
}
