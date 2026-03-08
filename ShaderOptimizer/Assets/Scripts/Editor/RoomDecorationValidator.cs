#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ShaderOp.Customization;
using ShaderOp.Core;

namespace ShaderOp.Editor
{
    /// <summary>
    /// 部屋デコレーションシーン検証ツール
    /// </summary>
    /// <remarks>
    /// メニュー: ShaderOp → Validate → RoomDecoration Scene
    ///
    /// 検証項目:
    /// - 必須オブジェクトの存在確認
    /// - マテリアルの割り当て確認
    /// - ライティング設定確認
    /// - UI Toolkit設定確認
    /// - パフォーマンスチェック（推定）
    /// </remarks>
    public static class RoomDecorationValidator
    {
        private const string MENU_PATH = "ShaderOp/Validate/RoomDecoration Scene";

        [MenuItem(MENU_PATH, false, 300)]
        public static void ValidateScene()
        {
            Debug.Log("[RoomDecorationValidator] シーン検証開始...");

            int errorCount = 0;
            int warningCount = 0;

            // 1. 必須オブジェクトの存在確認
            errorCount += ValidateRequiredObjects();

            // 2. マテリアル確認
            warningCount += ValidateMaterials();

            // 3. ライティング確認
            warningCount += ValidateLighting();

            // 4. UI Toolkit確認
            errorCount += ValidateUIToolkit();

            // 5. カメラ確認
            errorCount += ValidateCamera();

            // 6. パフォーマンスチェック
            warningCount += ValidatePerformance();

            // 結果表示
            string result = $"検証完了!\n\nエラー: {errorCount}\n警告: {warningCount}";

            if (errorCount == 0 && warningCount == 0)
            {
                result = "検証完了!\n\nすべてのチェックに合格しました。";
                EditorUtility.DisplayDialog("Validation Success", result, "OK");
            }
            else
            {
                result += "\n\n詳細はConsoleログを確認してください。";
                EditorUtility.DisplayDialog("Validation Complete", result, "OK");
            }

            Debug.Log($"[RoomDecorationValidator] 検証完了: エラー {errorCount}, 警告 {warningCount}");
        }

        // ============================================
        // 必須オブジェクト確認
        // ============================================

        private static int ValidateRequiredObjects()
        {
            int errors = 0;

            string[] requiredObjects = new string[]
            {
                "Room",
                "Floor",
                "WallNorth",
                "WallSouth",
                "WallEast",
                "WallWest",
                "CurtainLeft",
                "CurtainRight",
                "Rug",
                "Cushion1",
                "Cushion2",
                "Cushion3",
                "TableCloth"
            };

            foreach (string objName in requiredObjects)
            {
                GameObject? obj = GameObject.Find(objName);
                if (obj == null)
                {
                    Debug.LogError($"[RoomDecorationValidator] 必須オブジェクトが見つかりません: {objName}");
                    errors++;
                }
            }

            if (errors == 0)
            {
                Debug.Log("[RoomDecorationValidator] ✓ 必須オブジェクトチェック合格");
            }

            return errors;
        }

        // ============================================
        // マテリアル確認
        // ============================================

        private static int ValidateMaterials()
        {
            int warnings = 0;

            // マテリアルが存在するか確認
            Material? cottonMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Cotton_New.mat");
            Material? silkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Silk_New.mat");
            Material? denimMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Denim_New.mat");
            Material? leatherMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Leather_New.mat");

            if (cottonMat == null)
            {
                Debug.LogWarning("[RoomDecorationValidator] MAT_Cotton_New.mat が見つかりません");
                warnings++;
            }

            if (silkMat == null)
            {
                Debug.LogWarning("[RoomDecorationValidator] MAT_Silk_New.mat が見つかりません");
                warnings++;
            }

            if (denimMat == null)
            {
                Debug.LogWarning("[RoomDecorationValidator] MAT_Denim_New.mat が見つかりません");
                warnings++;
            }

            if (leatherMat == null)
            {
                Debug.LogWarning("[RoomDecorationValidator] MAT_Leather_New.mat が見つかりません");
                warnings++;
            }

            // オブジェクトにマテリアルが割り当てられているか確認
            ValidateObjectMaterial("CurtainLeft", ref warnings);
            ValidateObjectMaterial("CurtainRight", ref warnings);
            ValidateObjectMaterial("Rug", ref warnings);
            ValidateObjectMaterial("Cushion1", ref warnings);
            ValidateObjectMaterial("Cushion2", ref warnings);
            ValidateObjectMaterial("Cushion3", ref warnings);
            ValidateObjectMaterial("TableCloth", ref warnings);

            if (warnings == 0)
            {
                Debug.Log("[RoomDecorationValidator] ✓ マテリアルチェック合格");
            }

            return warnings;
        }

        private static void ValidateObjectMaterial(string objectName, ref int warnings)
        {
            GameObject? obj = GameObject.Find(objectName);
            if (obj != null)
            {
                Renderer? renderer = obj.GetComponent<Renderer>();
                if (renderer == null || renderer.sharedMaterial == null)
                {
                    Debug.LogWarning($"[RoomDecorationValidator] {objectName} にマテリアルが割り当てられていません");
                    warnings++;
                }
            }
        }

        // ============================================
        // ライティング確認
        // ============================================

        private static int ValidateLighting()
        {
            int warnings = 0;

            // Directional Light確認
            Light[] lights = Object.FindObjectsOfType<Light>();
            bool hasDirectionalLight = false;
            int pointLightCount = 0;

            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    hasDirectionalLight = true;
                }
                else if (light.type == LightType.Point)
                {
                    pointLightCount++;
                }
            }

            if (!hasDirectionalLight)
            {
                Debug.LogWarning("[RoomDecorationValidator] Directional Light が見つかりません");
                warnings++;
            }

            if (pointLightCount < 2)
            {
                Debug.LogWarning("[RoomDecorationValidator] Point Light が推奨数（2つ以上）より少ないです");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("[RoomDecorationValidator] ✓ ライティングチェック合格");
            }

            return warnings;
        }

        // ============================================
        // UI Toolkit確認
        // ============================================

        private static int ValidateUIToolkit()
        {
            int errors = 0;

            // RoomDecoratorController確認
            RoomDecoratorController? controller = Object.FindObjectOfType<RoomDecoratorController>();
            if (controller == null)
            {
                Debug.LogError("[RoomDecorationValidator] RoomDecoratorController が見つかりません");
                errors++;
                return errors;
            }

            // UIDocument確認
            UIDocument? uiDocument = controller.GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[RoomDecorationValidator] UIDocument が見つかりません");
                errors++;
            }
            else if (uiDocument.visualTreeAsset == null)
            {
                Debug.LogError("[RoomDecorationValidator] UIDocument の VisualTreeAsset が設定されていません");
                errors++;
            }

            if (errors == 0)
            {
                Debug.Log("[RoomDecorationValidator] ✓ UI Toolkit チェック合格");
            }

            return errors;
        }

        // ============================================
        // カメラ確認
        // ============================================

        private static int ValidateCamera()
        {
            int errors = 0;

            Camera? mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[RoomDecorationValidator] Main Camera が見つかりません");
                errors++;
                return errors;
            }

            // OrbitCameraController確認
            OrbitCameraController? orbitController = mainCamera.GetComponent<OrbitCameraController>();
            if (orbitController == null)
            {
                Debug.LogError("[RoomDecorationValidator] OrbitCameraController が見つかりません");
                errors++;
            }

            if (errors == 0)
            {
                Debug.Log("[RoomDecorationValidator] ✓ カメラチェック合格");
            }

            return errors;
        }

        // ============================================
        // パフォーマンスチェック
        // ============================================

        private static int ValidatePerformance()
        {
            int warnings = 0;

            // ポリゴン数チェック（簡易）
            MeshFilter[] meshFilters = Object.FindObjectsOfType<MeshFilter>();
            int totalTriangles = 0;

            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    totalTriangles += meshFilter.sharedMesh.triangles.Length / 3;
                }
            }

            Debug.Log($"[RoomDecorationValidator] 総ポリゴン数: {totalTriangles}");

            if (totalTriangles > 50000)
            {
                Debug.LogWarning($"[RoomDecorationValidator] ポリゴン数が多すぎます（{totalTriangles} tris）。モバイルでのパフォーマンスに影響する可能性があります");
                warnings++;
            }

            // ライト数チェック
            Light[] lights = Object.FindObjectsOfType<Light>();
            if (lights.Length > 5)
            {
                Debug.LogWarning($"[RoomDecorationValidator] ライト数が多すぎます（{lights.Length}）。推奨は3-5個です");
                warnings++;
            }

            // マテリアル数チェック
            Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
            int materialCount = 0;
            foreach (Renderer renderer in renderers)
            {
                materialCount += renderer.sharedMaterials.Length;
            }

            Debug.Log($"[RoomDecorationValidator] マテリアル数: {materialCount}");

            if (materialCount > 20)
            {
                Debug.LogWarning($"[RoomDecorationValidator] マテリアル数が多すぎます（{materialCount}）。ドローコールが増加する可能性があります");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("[RoomDecorationValidator] ✓ パフォーマンスチェック合格");
            }

            return warnings;
        }
    }
}
