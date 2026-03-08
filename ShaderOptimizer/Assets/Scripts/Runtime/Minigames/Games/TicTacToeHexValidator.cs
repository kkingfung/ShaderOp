#nullable enable

using UnityEngine;
using ShaderOp.Minigames.HexGrid;
using ShaderOp.Shaders;
using System.Collections.Generic;
using System.Text;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// TicTacToeHex 垂直スライス検証ツール
    /// </summary>
    /// <remarks>
    /// Play Mode実行時にシステムが正しくセットアップされているか検証します
    /// Consoleに詳細なレポートを出力
    /// </remarks>
    public class TicTacToeHexValidator : MonoBehaviour
    {
        [Header("検証設定")]
        [SerializeField] private bool _validateOnStart = true;
        [SerializeField] private bool _logDetailedReport = true;
        [SerializeField] private bool _validateEveryFrame = false;

        private TicTacToeHexVerticalSlice? _gameController;
        private int _frameCount = 0;
        private float _lastFPS = 0f;
        private List<string> _errors = new List<string>();
        private List<string> _warnings = new List<string>();
        private List<string> _successes = new List<string>();

        private void Start()
        {
            if (_validateOnStart)
            {
                ValidateSystem();
            }
        }

        private void Update()
        {
            if (_validateEveryFrame)
            {
                _frameCount++;
                if (_frameCount % 60 == 0) // 1秒ごと
                {
                    _lastFPS = 1.0f / Time.deltaTime;
                    ValidatePerformance();
                }
            }
        }

        /// <summary>
        /// システム全体を検証
        /// </summary>
        [ContextMenu("Validate System")]
        public void ValidateSystem()
        {
            _errors.Clear();
            _warnings.Clear();
            _successes.Clear();

            Debug.Log("=== TicTacToeHex Vertical Slice 検証開始 ===");

            // 1. ゲームコントローラー検証
            ValidateGameController();

            // 2. プレハブ参照検証
            ValidatePrefabReferences();

            // 3. マテリアル参照検証
            ValidateMaterialReferences();

            // 4. UI参照検証
            ValidateUIReferences();

            // 5. シーン構成検証
            ValidateSceneSetup();

            // 6. シェーダー検証
            ValidateShaders();

            // 7. レポート出力
            PrintValidationReport();
        }

        /// <summary>
        /// ゲームコントローラー検証
        /// </summary>
        private void ValidateGameController()
        {
            _gameController = FindObjectOfType<TicTacToeHexVerticalSlice>();

            if (_gameController == null)
            {
                _errors.Add("❌ TicTacToeHexVerticalSlice コンポーネントが見つかりません");
                return;
            }

            _successes.Add("✅ TicTacToeHexVerticalSlice コンポーネント検出");
        }

        /// <summary>
        /// プレハブ参照検証
        /// </summary>
        private void ValidatePrefabReferences()
        {
            if (_gameController == null) return;

            // SerializedObjectで非公開フィールドにアクセス
            var serializedController = new UnityEditor.SerializedObject(_gameController);

            // Tile Prefab
            var tilePrefabProp = serializedController.FindProperty("_tilePrefab");
            if (tilePrefabProp.objectReferenceValue == null)
            {
                _errors.Add("❌ Tile Prefabが設定されていません");
            }
            else
            {
                _successes.Add("✅ Tile Prefab: " + tilePrefabProp.objectReferenceValue.name);
            }

            // Player1 Piece Prefab
            var player1PrefabProp = serializedController.FindProperty("_player1PiecePrefab");
            if (player1PrefabProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  Player1 Piece Prefabが設定されていません");
            }
            else
            {
                _successes.Add("✅ Player1 Piece Prefab: " + player1PrefabProp.objectReferenceValue.name);
            }

            // Player2 Piece Prefab
            var player2PrefabProp = serializedController.FindProperty("_player2PiecePrefab");
            if (player2PrefabProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  Player2 Piece Prefabが設定されていません");
            }
            else
            {
                _successes.Add("✅ Player2 Piece Prefab: " + player2PrefabProp.objectReferenceValue.name);
            }
        }

        /// <summary>
        /// マテリアル参照検証
        /// </summary>
        private void ValidateMaterialReferences()
        {
            if (_gameController == null) return;

            var serializedController = new UnityEditor.SerializedObject(_gameController);

            // HexTile Idle Material
            var hexIdleProp = serializedController.FindProperty("_hexTileIdleMaterial");
            if (hexIdleProp.objectReferenceValue == null)
            {
                _errors.Add("❌ HexTile Idle Materialが設定されていません");
            }
            else
            {
                Material mat = hexIdleProp.objectReferenceValue as Material;
                if (mat != null && mat.shader != null)
                {
                    _successes.Add($"✅ HexTile Idle Material: {mat.name} (Shader: {mat.shader.name})");
                }
            }

            // HexTile Hover Material
            var hexHoverProp = serializedController.FindProperty("_hexTileHoverMaterial");
            if (hexHoverProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  HexTile Hover Materialが設定されていません");
            }
            else
            {
                _successes.Add("✅ HexTile Hover Material: " + hexHoverProp.objectReferenceValue.name);
            }

            // HexTile Selected Material
            var hexSelectedProp = serializedController.FindProperty("_hexTileSelectedMaterial");
            if (hexSelectedProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  HexTile Selected Materialが設定されていません");
            }
            else
            {
                _successes.Add("✅ HexTile Selected Material: " + hexSelectedProp.objectReferenceValue.name);
            }

            // Player Materials
            var player1MatProp = serializedController.FindProperty("_player1PieceMaterial");
            var player2MatProp = serializedController.FindProperty("_player2PieceMaterial");

            if (player1MatProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  Player1 Piece Materialが設定されていません");
            }
            else
            {
                _successes.Add("✅ Player1 Piece Material: " + player1MatProp.objectReferenceValue.name);
            }

            if (player2MatProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  Player2 Piece Materialが設定されていません");
            }
            else
            {
                _successes.Add("✅ Player2 Piece Material: " + player2MatProp.objectReferenceValue.name);
            }
        }

        /// <summary>
        /// UI参照検証
        /// </summary>
        private void ValidateUIReferences()
        {
            if (_gameController == null) return;

            var serializedController = new UnityEditor.SerializedObject(_gameController);

            // Canvas
            var canvasProp = serializedController.FindProperty("_uiCanvas");
            if (canvasProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  UI Canvasが設定されていません（UI表示なし）");
            }
            else
            {
                _successes.Add("✅ UI Canvas設定済み");
            }

            // Turn Indicator Text
            var turnTextProp = serializedController.FindProperty("_turnIndicatorText");
            if (turnTextProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  Turn Indicator Textが設定されていません");
            }
            else
            {
                _successes.Add("✅ Turn Indicator Text設定済み");
            }

            // Game Status Text
            var statusTextProp = serializedController.FindProperty("_gameStatusText");
            if (statusTextProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  Game Status Textが設定されていません");
            }
            else
            {
                _successes.Add("✅ Game Status Text設定済み");
            }

            // Reset Button
            var resetButtonProp = serializedController.FindProperty("_resetButton");
            if (resetButtonProp.objectReferenceValue == null)
            {
                _warnings.Add("⚠️  Reset Buttonが設定されていません");
            }
            else
            {
                _successes.Add("✅ Reset Button設定済み");
            }
        }

        /// <summary>
        /// シーン構成検証
        /// </summary>
        private void ValidateSceneSetup()
        {
            // Main Camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                _errors.Add("❌ Main Cameraが見つかりません");
            }
            else
            {
                _successes.Add($"✅ Main Camera検出 (Orthographic: {mainCamera.orthographic}, Size: {mainCamera.orthographicSize})");

                if (!mainCamera.orthographic)
                {
                    _warnings.Add("⚠️  Cameraが透視投影です。Orthographicを推奨");
                }

                if (mainCamera.orthographicSize < 4f || mainCamera.orthographicSize > 6f)
                {
                    _warnings.Add($"⚠️  Camera Size推奨値: 5 (現在: {mainCamera.orthographicSize})");
                }
            }

            // EventSystem
            UnityEngine.EventSystems.EventSystem? eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                _errors.Add("❌ EventSystemが見つかりません（クリック検出不可）");
            }
            else
            {
                _successes.Add("✅ EventSystem検出");
            }

            // Canvas
            Canvas? canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                _warnings.Add("⚠️  Canvasが見つかりません（UI表示なし）");
            }
            else
            {
                _successes.Add($"✅ Canvas検出 (Render Mode: {canvas.renderMode})");
            }
        }

        /// <summary>
        /// シェーダー検証
        /// </summary>
        private void ValidateShaders()
        {
            // HexTileShaderController
            HexTileShaderController[] tileControllers = FindObjectsOfType<HexTileShaderController>();
            if (tileControllers.Length > 0)
            {
                _successes.Add($"✅ HexTileShaderController検出: {tileControllers.Length}個");
            }
            else
            {
                _warnings.Add("⚠️  HexTileShaderControllerが見つかりません（Play後に生成される可能性あり）");
            }

            // GamePieceShaderAnimator
            GamePieceShaderAnimator[] pieceAnimators = FindObjectsOfType<GamePieceShaderAnimator>();
            if (pieceAnimators.Length > 0)
            {
                _successes.Add($"✅ GamePieceShaderAnimator検出: {pieceAnimators.Length}個");
            }
            else
            {
                _warnings.Add("⚠️  GamePieceShaderAnimatorが見つかりません（駒配置後に生成）");
            }
        }

        /// <summary>
        /// パフォーマンス検証
        /// </summary>
        private void ValidatePerformance()
        {
            if (_lastFPS < 30f)
            {
                Debug.LogWarning($"⚠️  FPS低下: {_lastFPS:F1} fps");
            }
            else if (_lastFPS >= 60f)
            {
                Debug.Log($"✅ パフォーマンス良好: {_lastFPS:F1} fps");
            }
        }

        /// <summary>
        /// 検証レポート出力
        /// </summary>
        private void PrintValidationReport()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("\n========================================");
            report.AppendLine("  TicTacToeHex 検証レポート");
            report.AppendLine("========================================\n");

            // エラー
            if (_errors.Count > 0)
            {
                report.AppendLine("【エラー】");
                foreach (var error in _errors)
                {
                    report.AppendLine($"  {error}");
                }
                report.AppendLine();
            }

            // 警告
            if (_warnings.Count > 0)
            {
                report.AppendLine("【警告】");
                foreach (var warning in _warnings)
                {
                    report.AppendLine($"  {warning}");
                }
                report.AppendLine();
            }

            // 成功
            if (_logDetailedReport && _successes.Count > 0)
            {
                report.AppendLine("【正常】");
                foreach (var success in _successes)
                {
                    report.AppendLine($"  {success}");
                }
                report.AppendLine();
            }

            // サマリー
            report.AppendLine("【サマリー】");
            report.AppendLine($"  ✅ 成功: {_successes.Count}");
            report.AppendLine($"  ⚠️  警告: {_warnings.Count}");
            report.AppendLine($"  ❌ エラー: {_errors.Count}");
            report.AppendLine();

            if (_errors.Count == 0)
            {
                report.AppendLine("🎉 検証完了！ゲームプレイ可能です");
            }
            else
            {
                report.AppendLine("⚠️  エラーを修正してください");
            }

            report.AppendLine("========================================\n");

            Debug.Log(report.ToString());
        }

        /// <summary>
        /// エディタ上でのテスト用
        /// </summary>
        [ContextMenu("Test: Show Stats")]
        private void ShowStats()
        {
            Debug.Log($"FPS: {1.0f / Time.deltaTime:F1}");
            Debug.Log($"Batches: {UnityEngine.Rendering.DebugManager.instance}");
            Debug.Log($"Active GameObjects: {FindObjectsOfType<GameObject>().Length}");
        }
    }
}
