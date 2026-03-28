#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShaderOp.Core;

namespace ShaderOp.Editor
{
    /// <summary>
    /// MainMenuシーンビルダー (UI Toolkit版)
    /// </summary>
    /// <remarks>
    /// UI Toolkit を使用した Portrait モバイル最適化メニューを自動構築
    /// </remarks>
    public static class MainMenuSceneBuilder
    {
        private const string MenuPath = "ShaderOp/Setup/Build MainMenu Scene (Complete Portal)";
        private const int MenuPriority = 100;

        [MenuItem(MenuPath, false, MenuPriority)]
        public static void BuildMainMenuScene()
        {
            Debug.Log("[MainMenuSceneBuilder] Building MainMenu scene...");

            // 1. MainMenu シーンを開く
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

            if (!scene.IsValid())
            {
                Debug.LogError("[MainMenuSceneBuilder] Failed to open MainMenu scene!");
                return;
            }

            // 2. 既存の Canvas を削除（UI Toolkit に移行）
            CleanupLegacyUI();

            // 3. UI Document GameObject を作成/取得
            GameObject uiDocumentGO = SetupUIDocument();

            if (uiDocumentGO == null)
            {
                Debug.LogError("[MainMenuSceneBuilder] Failed to setup UI Document!");
                return;
            }

            // 4. MainMenuController を追加/取得
            SetupMainMenuController(uiDocumentGO);

            // 5. GameBootstrap が存在するか確認
            EnsureGameBootstrap();

            // 6. EventSystem を確保（UI Toolkit でも必要な場合がある）
            EnsureEventSystem();

            // 7. シーンを保存
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[MainMenuSceneBuilder] MainMenu scene setup complete!");
            EditorUtility.DisplayDialog(
                "MainMenu Scene Setup Complete",
                "MainMenu scene has been successfully configured with:\n\n" +
                "✓ UI Toolkit Document\n" +
                "✓ MainMenuController\n" +
                "✓ GameBootstrap (ServiceLocator)\n" +
                "✓ Portrait Mobile Layout (9:16)\n" +
                "✓ Safe Area Support\n\n" +
                "Press Play to test!",
                "OK"
            );
        }

        /// <summary>
        /// レガシー UI (Canvas) を削除
        /// </summary>
        private static void CleanupLegacyUI()
        {
            // Canvas がある場合は削除
            var canvas = Object.FindFirstObjectByType<UnityEngine.Canvas>();
            if (canvas != null)
            {
                Debug.Log("[MainMenuSceneBuilder] Removing legacy Canvas...");
                Object.DestroyImmediate(canvas.gameObject);
            }

            // MainMenuUI (旧レガシー版) がある場合は削除
            var legacyUI = Object.FindFirstObjectByType<MainMenuUI>();
            if (legacyUI != null)
            {
                Debug.Log("[MainMenuSceneBuilder] Removing legacy MainMenuUI...");
                Object.DestroyImmediate(legacyUI.gameObject);
            }
        }

        /// <summary>
        /// UI Document GameObject をセットアップ
        /// </summary>
        private static GameObject SetupUIDocument()
        {
            // 既存の UI Document を検索
            var existingUIDoc = Object.FindFirstObjectByType<UIDocument>();

            if (existingUIDoc != null)
            {
                Debug.Log("[MainMenuSceneBuilder] Using existing UIDocument");
                return existingUIDoc.gameObject;
            }

            // 新規作成
            Debug.Log("[MainMenuSceneBuilder] Creating new MainMenuUI GameObject");
            GameObject uiDocGO = new GameObject("MainMenuUI");

            // UIDocument コンポーネントを追加
            var uiDocument = uiDocGO.AddComponent<UIDocument>();

            // UXML アセットを読み込んで割り当て
            string uxmlPath = "Assets/UI/MainMenu.uxml";
            var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (uxmlAsset != null)
            {
                uiDocument.visualTreeAsset = uxmlAsset;
                Debug.Log($"[MainMenuSceneBuilder] Assigned UXML: {uxmlPath}");
            }
            else
            {
                Debug.LogWarning($"[MainMenuSceneBuilder] UXML not found at: {uxmlPath}");
            }

            // PanelSettings を設定（デフォルト設定を使用）
            var panelSettings = Resources.FindObjectsOfTypeAll<PanelSettings>();
            if (panelSettings.Length > 0)
            {
                uiDocument.panelSettings = panelSettings[0];
                Debug.Log("[MainMenuSceneBuilder] Assigned PanelSettings");
            }
            else
            {
                Debug.LogWarning("[MainMenuSceneBuilder] No PanelSettings found. Please assign manually.");
            }

            return uiDocGO;
        }

        /// <summary>
        /// MainMenuController をセットアップ
        /// </summary>
        private static void SetupMainMenuController(GameObject uiDocumentGO)
        {
            // 既存のコントローラーを確認
            var existingController = uiDocumentGO.GetComponent<MainMenuController>();

            if (existingController != null)
            {
                Debug.Log("[MainMenuSceneBuilder] MainMenuController already exists");
                return;
            }

            // 新規追加
            uiDocumentGO.AddComponent<MainMenuController>();
            Debug.Log("[MainMenuSceneBuilder] Added MainMenuController component");
        }

        /// <summary>
        /// GameBootstrap が存在するか確認
        /// </summary>
        private static void EnsureGameBootstrap()
        {
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();

            if (bootstrap != null)
            {
                Debug.Log("[MainMenuSceneBuilder] GameBootstrap found");
                return;
            }

            // GameBootstrap が存在しない場合は作成
            Debug.Log("[MainMenuSceneBuilder] Creating GameBootstrap...");
            GameObject bootstrapGO = new GameObject("GameBootstrap");
            bootstrapGO.AddComponent<GameBootstrap>();

            Debug.Log("[MainMenuSceneBuilder] GameBootstrap created");
        }

        /// <summary>
        /// EventSystemを確認・作成
        /// </summary>
        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[MainMenuSceneBuilder] Created EventSystem");
            }
            else
            {
                Debug.Log("[MainMenuSceneBuilder] EventSystem already exists");
            }
        }

        /// <summary>
        /// メニューが有効かどうか
        /// </summary>
        [MenuItem(MenuPath, true)]
        public static bool ValidateBuildMainMenuScene()
        {
            // MainMenu シーンが存在する場合のみ有効化
            return System.IO.File.Exists("Assets/Scenes/MainMenu.unity");
        }
    }
}
