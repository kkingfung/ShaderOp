#nullable enable

using UnityEngine;
using UnityEditor;
using System.Text;
using ShaderOp.Minigames.Games;
using ShaderOp.Shaders;

namespace ShaderOp.Editor
{
    /// <summary>
    /// HexReversiシーン検証ツール
    /// </summary>
    public static class HexReversiValidator
    {
        [MenuItem("ShaderOp/Validate/HexReversi Scene")]
        public static void ValidateScene()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== HexReversi Scene Validation Report ===\n");

            int errorCount = 0;
            int warningCount = 0;

            // 1. GameBootstrap確認
            report.AppendLine("1. GameBootstrap Check:");
            var bootstrap = Object.FindObjectOfType<ShaderOp.Core.GameBootstrap>();
            if (bootstrap == null)
            {
                report.AppendLine("   [ERROR] GameBootstrap not found in scene");
                errorCount++;
            }
            else
            {
                report.AppendLine("   [OK] GameBootstrap found");
            }

            // 2. HexReversiComplete確認
            report.AppendLine("\n2. HexReversiComplete Controller Check:");
            var controller = Object.FindObjectOfType<HexReversiComplete>();
            if (controller == null)
            {
                report.AppendLine("   [ERROR] HexReversiComplete controller not found");
                errorCount++;
            }
            else
            {
                report.AppendLine("   [OK] HexReversiComplete controller found");

                // Prefab参照確認
                var serializedObject = new SerializedObject(controller);

                if (serializedObject.FindProperty("_hexTilePrefab").objectReferenceValue == null)
                {
                    report.AppendLine("   [WARNING] HexTile prefab not assigned");
                    warningCount++;
                }
                else
                {
                    report.AppendLine("   [OK] HexTile prefab assigned");
                }

                if (serializedObject.FindProperty("_gamePiecePrefab").objectReferenceValue == null)
                {
                    report.AppendLine("   [WARNING] GamePiece prefab not assigned");
                    warningCount++;
                }
                else
                {
                    report.AppendLine("   [OK] GamePiece prefab assigned");
                }

                // マテリアル参照確認
                if (serializedObject.FindProperty("_hexTileMaterial").objectReferenceValue == null)
                {
                    report.AppendLine("   [WARNING] HexTile material not assigned");
                    warningCount++;
                }
                else
                {
                    report.AppendLine("   [OK] HexTile material assigned");
                }

                // UI参照確認
                if (serializedObject.FindProperty("_player1ScoreText").objectReferenceValue == null)
                {
                    report.AppendLine("   [WARNING] Player1 score text not assigned");
                    warningCount++;
                }
                else
                {
                    report.AppendLine("   [OK] Player1 score text assigned");
                }

                if (serializedObject.FindProperty("_resetButton").objectReferenceValue == null)
                {
                    report.AppendLine("   [WARNING] Reset button not assigned");
                    warningCount++;
                }
                else
                {
                    report.AppendLine("   [OK] Reset button assigned");
                }
            }

            // 3. Camera確認
            report.AppendLine("\n3. Camera Check:");
            Camera? mainCamera = Camera.main;
            if (mainCamera == null)
            {
                report.AppendLine("   [ERROR] Main Camera not found");
                errorCount++;
            }
            else
            {
                report.AppendLine("   [OK] Main Camera found");
                report.AppendLine($"      Position: {mainCamera.transform.position}");
                report.AppendLine($"      Rotation: {mainCamera.transform.rotation.eulerAngles}");
                report.AppendLine($"      FOV: {mainCamera.fieldOfView}");
            }

            // 4. Canvas確認
            report.AppendLine("\n4. UI Canvas Check:");
            var canvas = Object.FindObjectOfType<UnityEngine.Canvas>();
            if (canvas == null)
            {
                report.AppendLine("   [ERROR] Canvas not found");
                errorCount++;
            }
            else
            {
                report.AppendLine("   [OK] Canvas found");

                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    report.AppendLine($"      Reference Resolution: {scaler.referenceResolution}");

                    // 縦画面チェック（9:16推奨）
                    if (scaler.referenceResolution.x > scaler.referenceResolution.y)
                    {
                        report.AppendLine("   [WARNING] Resolution appears to be landscape (should be portrait)");
                        warningCount++;
                    }
                    else
                    {
                        report.AppendLine("   [OK] Portrait orientation detected");
                    }
                }
            }

            // 5. Shader確認
            report.AppendLine("\n5. Shader Assets Check:");
            ValidateShaderAsset(report, "Assets/Shaders/ShaderGraphs/Minigames/SG_HexTile_Interactive.shadergraph", ref errorCount, ref warningCount);
            ValidateShaderAsset(report, "Assets/Shaders/ShaderGraphs/Minigames/SG_GamePiece_2D.shadergraph", ref errorCount, ref warningCount);

            // 6. Material確認
            report.AppendLine("\n6. Material Assets Check:");
            ValidateMaterialAsset(report, "Assets/Materials/Minigames/MAT_HexTile_Interactive.mat", ref errorCount, ref warningCount);
            ValidateMaterialAsset(report, "Assets/Materials/Minigames/MAT_Player1Piece.mat", ref errorCount, ref warningCount);
            ValidateMaterialAsset(report, "Assets/Materials/Minigames/MAT_Player2Piece.mat", ref errorCount, ref warningCount);

            // 7. Prefab確認
            report.AppendLine("\n7. Prefab Assets Check:");
            ValidatePrefabAsset(report, "Assets/Prefabs/Minigames/HexTile.prefab", ref errorCount, ref warningCount);
            ValidatePrefabAsset(report, "Assets/Prefabs/Minigames/Player1Piece.prefab", ref errorCount, ref warningCount);
            ValidatePrefabAsset(report, "Assets/Prefabs/Minigames/Player2Piece.prefab", ref errorCount, ref warningCount);

            // 8. パフォーマンスチェック
            report.AppendLine("\n8. Performance Recommendations:");
            report.AppendLine("   - Target: 60 FPS on mobile");
            report.AppendLine("   - Expected tile count: 37 (radius 3 hexagon)");
            report.AppendLine("   - Expected draw calls: <10 (with GPU Instancing)");
            report.AppendLine("   - Use Profiler to verify performance");

            // サマリー
            report.AppendLine("\n=== Validation Summary ===");
            report.AppendLine($"Errors: {errorCount}");
            report.AppendLine($"Warnings: {warningCount}");

            if (errorCount == 0 && warningCount == 0)
            {
                report.AppendLine("\n[SUCCESS] Scene is properly configured!");
            }
            else if (errorCount == 0)
            {
                report.AppendLine("\n[OK] Scene is functional but has some warnings");
            }
            else
            {
                report.AppendLine("\n[FAILED] Scene has errors that need to be fixed");
            }

            // レポート出力
            Debug.Log(report.ToString());

            // ダイアログ表示
            string dialogTitle = errorCount == 0 ? "Validation Passed" : "Validation Failed";
            string dialogMessage = $"Errors: {errorCount}\nWarnings: {warningCount}\n\nCheck Console for detailed report.";
            EditorUtility.DisplayDialog(dialogTitle, dialogMessage, "OK");
        }

        /// <summary>
        /// シェーダーアセットを検証
        /// </summary>
        private static void ValidateShaderAsset(StringBuilder report, string path, ref int errorCount, ref int warningCount)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null)
            {
                report.AppendLine($"   [WARNING] Shader not found: {path}");
                warningCount++;
            }
            else
            {
                report.AppendLine($"   [OK] {System.IO.Path.GetFileName(path)}");
            }
        }

        /// <summary>
        /// マテリアルアセットを検証
        /// </summary>
        private static void ValidateMaterialAsset(StringBuilder report, string path, ref int errorCount, ref int warningCount)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                report.AppendLine($"   [WARNING] Material not found: {path}");
                warningCount++;
            }
            else
            {
                report.AppendLine($"   [OK] {System.IO.Path.GetFileName(path)}");

                // GPU Instancingチェック
                if (material.enableInstancing)
                {
                    report.AppendLine("      GPU Instancing: Enabled");
                }
                else
                {
                    report.AppendLine("      [INFO] GPU Instancing: Disabled (consider enabling for performance)");
                }
            }
        }

        /// <summary>
        /// Prefabアセットを検証
        /// </summary>
        private static void ValidatePrefabAsset(StringBuilder report, string path, ref int errorCount, ref int warningCount)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                report.AppendLine($"   [WARNING] Prefab not found: {path}");
                warningCount++;
            }
            else
            {
                report.AppendLine($"   [OK] {System.IO.Path.GetFileName(path)}");

                // コンポーネントチェック
                if (path.Contains("HexTile"))
                {
                    var renderer = prefab.GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        report.AppendLine("      [WARNING] Renderer component missing");
                        warningCount++;
                    }

                    var collider = prefab.GetComponent<Collider>();
                    if (collider == null)
                    {
                        report.AppendLine("      [INFO] Collider component missing (will be added at runtime)");
                    }
                }
            }
        }

        [MenuItem("ShaderOp/Validate/Check Shader Integration")]
        public static void ValidateShaderIntegration()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== Shader Integration Check ===\n");

            int issueCount = 0;

            // ランタイムシーン内のシェーダーコントローラーを確認
            var tileShaders = Object.FindObjectsOfType<HexTileShaderController>();
            var pieceAnimators = Object.FindObjectsOfType<GamePieceShaderAnimator>();

            report.AppendLine($"HexTileShaderController instances: {tileShaders.Length}");
            report.AppendLine($"GamePieceShaderAnimator instances: {pieceAnimators.Length}");

            if (tileShaders.Length == 0)
            {
                report.AppendLine("[INFO] No tile shader controllers found (normal if game hasn't started)");
            }

            if (pieceAnimators.Length == 0)
            {
                report.AppendLine("[INFO] No piece animators found (normal if game hasn't started)");
            }

            // マテリアルのシェーダープロパティ確認
            report.AppendLine("\nMaterial Shader Properties:");

            Material? tileMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Minigames/MAT_HexTile_Interactive.mat");
            if (tileMat != null)
            {
                report.AppendLine($"  HexTile Material:");
                report.AppendLine($"    Shader: {tileMat.shader.name}");

                if (tileMat.HasProperty("_State"))
                    report.AppendLine("    [OK] _State property found");
                else
                {
                    report.AppendLine("    [ERROR] _State property missing");
                    issueCount++;
                }

                if (tileMat.HasProperty("_GlowIntensity"))
                    report.AppendLine("    [OK] _GlowIntensity property found");
                else
                {
                    report.AppendLine("    [ERROR] _GlowIntensity property missing");
                    issueCount++;
                }
            }

            Material? pieceMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Minigames/MAT_Player1Piece.mat");
            if (pieceMat != null)
            {
                report.AppendLine($"\n  GamePiece Material:");
                report.AppendLine($"    Shader: {pieceMat.shader.name}");

                if (pieceMat.HasProperty("_PlayerColor"))
                    report.AppendLine("    [OK] _PlayerColor property found");
                else
                {
                    report.AppendLine("    [ERROR] _PlayerColor property missing");
                    issueCount++;
                }

                if (pieceMat.HasProperty("_Fade"))
                    report.AppendLine("    [OK] _Fade property found");
                else
                {
                    report.AppendLine("    [ERROR] _Fade property missing");
                    issueCount++;
                }
            }

            report.AppendLine($"\n=== Total Issues: {issueCount} ===");

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Shader Integration Check", $"Issues found: {issueCount}\n\nCheck Console for details.", "OK");
        }

        [MenuItem("ShaderOp/Validate/Performance Profile")]
        public static void PerformanceProfile()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== Performance Profile ===\n");

            // 理論的な計算
            int expectedTiles = 37; // 半径3の六角形
            int expectedPieces = 4; // 初期配置
            int expectedDrawCalls = 2; // タイル + 駒（GPU Instancingあり）

            report.AppendLine("Expected Performance:");
            report.AppendLine($"  Tiles: {expectedTiles}");
            report.AppendLine($"  Initial Pieces: {expectedPieces}");
            report.AppendLine($"  Max Pieces: {expectedTiles}");
            report.AppendLine($"  Draw Calls (with GPU Instancing): ~{expectedDrawCalls}");
            report.AppendLine($"  Draw Calls (without GPU Instancing): ~{expectedTiles + expectedTiles}");
            report.AppendLine($"  Target FPS: 60");
            report.AppendLine($"  Target Platform: Mobile");

            report.AppendLine("\nOptimization Tips:");
            report.AppendLine("  1. Enable GPU Instancing on all materials");
            report.AppendLine("  2. Use object pooling for game pieces");
            report.AppendLine("  3. Batch UI draw calls");
            report.AppendLine("  4. Use async animations with UniTask");
            report.AppendLine("  5. Profile with Unity Profiler during gameplay");

            report.AppendLine("\nTo profile actual performance:");
            report.AppendLine("  1. Enter Play Mode");
            report.AppendLine("  2. Open Window > Analysis > Profiler");
            report.AppendLine("  3. Focus on CPU and Rendering modules");
            report.AppendLine("  4. Check Stats window for draw calls");

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Performance Profile", "Performance profile generated.\n\nCheck Console for details.", "OK");
        }
    }
}
