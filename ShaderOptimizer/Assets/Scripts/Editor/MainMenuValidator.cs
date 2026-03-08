#nullable enable

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShaderOp.Editor
{
    /// <summary>
    /// MainMenuシーンのバリデーションツール
    /// </summary>
    /// <remarks>
    /// MainMenuシーンの構成と設定を検証します。
    /// メニュー: ShaderOp → Validate → MainMenu Scene
    /// </remarks>
    public static class MainMenuValidator
    {
        private const string MENU_PATH = "ShaderOp/Validate/MainMenu Scene";
        private const int MENU_PRIORITY = 250;

        // ========================================
        // メニュー項目
        // ========================================

        /// <summary>
        /// MainMenuシーンを検証
        /// </summary>
        [MenuItem(MENU_PATH, priority = MENU_PRIORITY)]
        public static void ValidateMainMenuScene()
        {
            Debug.Log("========================================");
            Debug.Log("[MainMenuValidator] MainMenuシーン検証開始");
            Debug.Log("========================================");

            int totalChecks = 0;
            int passedChecks = 0;

            // 1. GameBootstrap確認
            if (ValidateGameBootstrap())
            {
                passedChecks++;
            }
            totalChecks++;

            // 2. Camera確認
            if (ValidateCamera())
            {
                passedChecks++;
            }
            totalChecks++;

            // 3. UI確認
            if (ValidateUI())
            {
                passedChecks++;
            }
            totalChecks++;

            // 4. Controller確認
            if (ValidateController())
            {
                passedChecks++;
            }
            totalChecks++;

            // 5. Assets確認
            if (ValidateAssets())
            {
                passedChecks++;
            }
            totalChecks++;

            // 結果表示
            Debug.Log("========================================");
            Debug.Log($"[MainMenuValidator] 検証完了: {passedChecks}/{totalChecks} チェック合格");
            Debug.Log("========================================");

            float percentage = (float)passedChecks / totalChecks * 100f;
            string result = percentage == 100f ? "✅ 完璧です！" : $"⚠️ {totalChecks - passedChecks} 件の問題があります";

            EditorUtility.DisplayDialog(
                "MainMenu Scene Validation",
                $"{result}\n\n合格: {passedChecks}/{totalChecks} ({percentage:F0}%)\n\n詳細はConsoleログを確認してください。",
                "OK"
            );
        }

        // ========================================
        // 個別検証メソッド
        // ========================================

        /// <summary>
        /// GameBootstrapを検証
        /// </summary>
        private static bool ValidateGameBootstrap()
        {
            Debug.Log("\n[1] GameBootstrap検証");

            var bootstrap = GameObject.Find("GameBootstrap");
            if (bootstrap == null)
            {
                Debug.LogError("❌ GameBootstrapが見つかりません");
                Debug.LogError("   → ShaderOp → Setup → MainMenu Scene を実行してください");
                return false;
            }

            Debug.Log("✅ GameBootstrapが見つかりました");

            // タグ確認
            if (bootstrap.tag != "GameController")
            {
                Debug.LogWarning($"⚠️ GameBootstrapのタグが推奨値と異なります: {bootstrap.tag} (推奨: GameController)");
            }

            return true;
        }

        /// <summary>
        /// カメラを検証
        /// </summary>
        private static bool ValidateCamera()
        {
            Debug.Log("\n[2] Camera検証");

            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("❌ Main Cameraが見つかりません");
                return false;
            }

            Debug.Log("✅ Main Cameraが見つかりました");

            // カメラ設定確認
            if (camera.clearFlags != CameraClearFlags.SolidColor)
            {
                Debug.LogWarning($"⚠️ ClearFlagsが推奨値と異なります: {camera.clearFlags} (推奨: SolidColor)");
            }

            if (camera.backgroundColor != new Color(0.1f, 0.1f, 0.15f, 1f))
            {
                Debug.LogWarning($"⚠️ BackgroundColorが推奨値と異なります: {camera.backgroundColor}");
            }

            return true;
        }

        /// <summary>
        /// UIを検証
        /// </summary>
        private static bool ValidateUI()
        {
            Debug.Log("\n[3] UI検証");

            var uiRoot = GameObject.Find("MainMenuUI");
            if (uiRoot == null)
            {
                Debug.LogError("❌ MainMenuUIが見つかりません");
                Debug.LogError("   → ShaderOp → Setup → MainMenu Scene を実行してください");
                return false;
            }

            Debug.Log("✅ MainMenuUIが見つかりました");

            // UIDocumentコンポーネント確認
            var uiDocument = uiRoot.GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("❌ UIDocumentコンポーネントが見つかりません");
                return false;
            }

            Debug.Log("✅ UIDocumentコンポーネントが見つかりました");

            // VisualTreeAsset確認
            if (uiDocument.visualTreeAsset == null)
            {
                Debug.LogError("❌ VisualTreeAssetが設定されていません");
                return false;
            }

            Debug.Log($"✅ VisualTreeAsset: {uiDocument.visualTreeAsset.name}");

            return true;
        }

        /// <summary>
        /// Controllerを検証
        /// </summary>
        private static bool ValidateController()
        {
            Debug.Log("\n[4] Controller検証");

            var uiRoot = GameObject.Find("MainMenuUI");
            if (uiRoot == null)
            {
                Debug.LogError("❌ MainMenuUIが見つかりません (Controller検証スキップ)");
                return false;
            }

            // MainMenuControllerコンポーネント確認
            var controller = uiRoot.GetComponent<ShaderOp.UI.MainMenuController>();
            if (controller == null)
            {
                Debug.LogError("❌ MainMenuControllerコンポーネントが見つかりません");
                Debug.LogError("   → MainMenuUIにMainMenuControllerを追加してください");
                return false;
            }

            Debug.Log("✅ MainMenuControllerコンポーネントが見つかりました");

            return true;
        }

        /// <summary>
        /// Assetsを検証
        /// </summary>
        private static bool ValidateAssets()
        {
            Debug.Log("\n[5] Assets検証");

            bool allValid = true;

            // UXML確認
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/MainMenu.uxml");
            if (uxml == null)
            {
                Debug.LogError("❌ MainMenu.uxmlが見つかりません");
                allValid = false;
            }
            else
            {
                Debug.Log("✅ MainMenu.uxmlが見つかりました");
            }

            // USS確認
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/MainMenu.uss");
            if (uss == null)
            {
                Debug.LogError("❌ MainMenu.ussが見つかりません");
                allValid = false;
            }
            else
            {
                Debug.Log("✅ MainMenu.ussが見つかりました");
            }

            return allValid;
        }

        // ========================================
        // UI要素検証
        // ========================================

        /// <summary>
        /// UI要素の詳細検証
        /// </summary>
        [MenuItem("ShaderOp/Validate/MainMenu UI Elements", priority = 252)]
        public static void ValidateUIElements()
        {
            Debug.Log("========================================");
            Debug.Log("[MainMenuValidator] UI要素詳細検証開始");
            Debug.Log("========================================");

            var uiRoot = GameObject.Find("MainMenuUI");
            if (uiRoot == null)
            {
                Debug.LogError("❌ MainMenuUIが見つかりません");
                EditorUtility.DisplayDialog("UI Elements Validation", "MainMenuUIが見つかりません", "OK");
                return;
            }

            var uiDocument = uiRoot.GetComponent<UIDocument>();
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("❌ UIDocumentまたはrootVisualElementが見つかりません");
                EditorUtility.DisplayDialog("UI Elements Validation", "UIDocumentが正しく設定されていません", "OK");
                return;
            }

            var root = uiDocument.rootVisualElement;

            int totalElements = 0;
            int foundElements = 0;

            // ボタン要素を検証
            string[] buttonNames = new string[]
            {
                "PlayTicTacToeBtn",
                "PlayHexReversiBtn",
                "PlayHexCheckersBtn",
                "PlayHexChessBtn",
                "RoomDecorationBtn",
                "CharacterCustomizationBtn",
                "SettingsBtn",
                "QuitBtn"
            };

            Debug.Log("\n[ボタン要素検証]");
            foreach (var name in buttonNames)
            {
                totalElements++;
                var btn = root.Q<Button>(name);
                if (btn != null)
                {
                    Debug.Log($"✅ {name}");
                    foundElements++;
                }
                else
                {
                    Debug.LogError($"❌ {name} が見つかりません");
                }
            }

            // ラベル要素を検証
            string[] labelNames = new string[]
            {
                "GameTitle",
                "Subtitle",
                "VersionLabel"
            };

            Debug.Log("\n[ラベル要素検証]");
            foreach (var name in labelNames)
            {
                totalElements++;
                var label = root.Q<Label>(name);
                if (label != null)
                {
                    Debug.Log($"✅ {name}: \"{label.text}\"");
                    foundElements++;
                }
                else
                {
                    Debug.LogError($"❌ {name} が見つかりません");
                }
            }

            // 結果表示
            Debug.Log("========================================");
            Debug.Log($"[MainMenuValidator] UI要素検証完了: {foundElements}/{totalElements} 要素発見");
            Debug.Log("========================================");

            float percentage = (float)foundElements / totalElements * 100f;
            EditorUtility.DisplayDialog(
                "UI Elements Validation",
                $"発見: {foundElements}/{totalElements} ({percentage:F0}%)\n\n詳細はConsoleログを確認してください。",
                "OK"
            );
        }
    }
}
#endif
