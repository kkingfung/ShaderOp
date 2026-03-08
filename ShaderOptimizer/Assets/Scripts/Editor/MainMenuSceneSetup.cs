#nullable enable

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShaderOp.Editor
{
    /// <summary>
    /// MainMenuシーンの自動セットアップツール
    /// </summary>
    /// <remarks>
    /// ワンクリックでMainMenuシーンを完全に構成します。
    /// メニュー: ShaderOp → Setup → MainMenu Scene
    /// </remarks>
    public static class MainMenuSceneSetup
    {
        private const string MENU_PATH = "ShaderOp/Setup/MainMenu Scene";
        private const int MENU_PRIORITY = 150;

        private const string UXML_PATH = "Assets/UI/MainMenu.uxml";
        private const string USS_PATH = "Assets/UI/MainMenu.uss";
        private const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";

        // ========================================
        // メニュー項目
        // ========================================

        /// <summary>
        /// MainMenuシーンを自動セットアップ
        /// </summary>
        [MenuItem(MENU_PATH, priority = MENU_PRIORITY)]
        public static void SetupMainMenuScene()
        {
            Debug.Log("[MainMenuSceneSetup] セットアップ開始");

            // 1. アセット存在確認
            if (!ValidateAssets())
            {
                Debug.LogError("[MainMenuSceneSetup] 必要なアセットが見つかりません。セットアップを中止します。");
                return;
            }

            // 2. シーンをロードまたは作成
            if (!LoadOrCreateScene())
            {
                Debug.LogError("[MainMenuSceneSetup] シーンのロードに失敗しました。");
                return;
            }

            // 3. GameBootstrapを作成/設定
            SetupGameBootstrap();

            // 4. カメラを設定
            SetupCamera();

            // 5. UIを作成
            SetupUI();

            // 6. シーンを保存
            SaveScene();

            Debug.Log("[MainMenuSceneSetup] ✅ セットアップ完了！");
            EditorUtility.DisplayDialog(
                "MainMenu Scene Setup",
                "MainMenuシーンのセットアップが完了しました！\n\nPlayボタンを押してテストしてください。",
                "OK"
            );
        }

        // ========================================
        // バリデーション
        // ========================================

        /// <summary>
        /// 必要なアセットが存在するか確認
        /// </summary>
        private static bool ValidateAssets()
        {
            bool allValid = true;

            // UXML確認
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            if (uxml == null)
            {
                Debug.LogError($"[MainMenuSceneSetup] UXMLファイルが見つかりません: {UXML_PATH}");
                allValid = false;
            }
            else
            {
                Debug.Log($"[MainMenuSceneSetup] ✅ UXML: {UXML_PATH}");
            }

            // USS確認
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            if (uss == null)
            {
                Debug.LogError($"[MainMenuSceneSetup] USSファイルが見つかりません: {USS_PATH}");
                allValid = false;
            }
            else
            {
                Debug.Log($"[MainMenuSceneSetup] ✅ USS: {USS_PATH}");
            }

            return allValid;
        }

        // ========================================
        // シーンセットアップ
        // ========================================

        /// <summary>
        /// シーンをロードまたは作成
        /// </summary>
        private static bool LoadOrCreateScene()
        {
            // 既存シーンをロード
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(SCENE_PATH, UnityEditor.SceneManagement.OpenSceneMode.Single);
            if (scene.IsValid())
            {
                Debug.Log($"[MainMenuSceneSetup] シーンをロード: {SCENE_PATH}");
                return true;
            }

            // シーンが存在しない場合は作成
            Debug.LogWarning($"[MainMenuSceneSetup] シーンが見つかりません。新規作成します: {SCENE_PATH}");
            var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single
            );

            if (newScene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, SCENE_PATH);
                Debug.Log($"[MainMenuSceneSetup] 新規シーンを作成: {SCENE_PATH}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// GameBootstrapを作成/設定
        /// </summary>
        private static void SetupGameBootstrap()
        {
            // 既存のGameBootstrapを検索
            var existing = GameObject.Find("GameBootstrap");
            if (existing != null)
            {
                Debug.Log("[MainMenuSceneSetup] 既存のGameBootstrapが見つかりました。スキップします。");
                return;
            }

            // GameBootstrapを作成
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.tag = "GameController";

            Debug.Log("[MainMenuSceneSetup] ✅ GameBootstrapを作成");
        }

        /// <summary>
        /// カメラを設定
        /// </summary>
        private static void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("[MainMenuSceneSetup] Main Cameraが見つかりません");
                return;
            }

            // 縦画面向け設定
            camera.transform.position = new Vector3(0, 0, -10);
            camera.transform.rotation = Quaternion.identity;
            camera.orthographic = false;
            camera.fieldOfView = 60f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);

            Debug.Log("[MainMenuSceneSetup] ✅ カメラを設定");
        }

        /// <summary>
        /// UIを作成
        /// </summary>
        private static void SetupUI()
        {
            // 既存のUIを検索
            var existingUI = GameObject.Find("MainMenuUI");
            if (existingUI != null)
            {
                Debug.Log("[MainMenuSceneSetup] 既存のMainMenuUIが見つかりました。削除して再作成します。");
                GameObject.DestroyImmediate(existingUI);
            }

            // UIルートオブジェクトを作成
            var uiRoot = new GameObject("MainMenuUI");
            uiRoot.tag = "Respawn"; // UI用のタグ

            // UIDocumentコンポーネントを追加
            var uiDocument = uiRoot.AddComponent<UIDocument>();

            // UXMLを設定
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
                Debug.Log("[MainMenuSceneSetup] ✅ UXMLを設定");
            }
            else
            {
                Debug.LogError("[MainMenuSceneSetup] UXMLのロードに失敗");
            }

            // MainMenuControllerを追加
            var controller = uiRoot.AddComponent<ShaderOp.Core.MainMenuController>();
            Debug.Log("[MainMenuSceneSetup] ✅ MainMenuControllerを追加");

            // Panel Settings (オプション)
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/PanelSettings.asset");
            if (panelSettings != null)
            {
                uiDocument.panelSettings = panelSettings;
                Debug.Log("[MainMenuSceneSetup] ✅ PanelSettingsを設定");
            }
            else
            {
                Debug.LogWarning("[MainMenuSceneSetup] PanelSettingsが見つかりません (オプション)");
            }
        }

        /// <summary>
        /// シーンを保存
        /// </summary>
        private static void SaveScene()
        {
            var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);
            Debug.Log($"[MainMenuSceneSetup] ✅ シーンを保存: {currentScene.path}");
        }

        // ========================================
        // クイックテスト
        // ========================================

        /// <summary>
        /// MainMenuアセットの存在確認のみ実行
        /// </summary>
        [MenuItem("ShaderOp/Validate/MainMenu Assets", priority = 251)]
        public static void QuickValidateAssets()
        {
            Debug.Log("[MainMenuSceneSetup] クイックバリデーション開始");

            int found = 0;
            int total = 2;

            // UXML確認
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            if (uxml != null)
            {
                Debug.Log($"✅ UXML: {UXML_PATH}");
                found++;
            }
            else
            {
                Debug.LogError($"❌ UXML not found: {UXML_PATH}");
            }

            // USS確認
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            if (uss != null)
            {
                Debug.Log($"✅ USS: {USS_PATH}");
                found++;
            }
            else
            {
                Debug.LogError($"❌ USS not found: {USS_PATH}");
            }

            // 結果表示
            Debug.Log($"[MainMenuSceneSetup] バリデーション結果: {found}/{total} アセット発見");

            if (found == total)
            {
                EditorUtility.DisplayDialog(
                    "MainMenu Assets Validation",
                    $"✅ すべてのアセットが見つかりました！\n\n{found}/{total} assets found",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "MainMenu Assets Validation",
                    $"❌ 一部のアセットが見つかりません\n\n{found}/{total} assets found\n\nConsoleログを確認してください。",
                    "OK"
                );
            }
        }
    }
}
#endif
