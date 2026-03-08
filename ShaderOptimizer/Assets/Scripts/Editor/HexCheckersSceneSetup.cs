#nullable enable

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
#if TEXTMESHPRO_PRESENT
using TMPro;
#endif
using ShaderOp.Minigames.Games;

namespace ShaderOp.Editor
{
    /// <summary>
    /// HexCheckersシーン自動セットアップツール
    /// </summary>
    public static class HexCheckersSceneSetup
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Minigames/HexTile.prefab";
        private const string PIECE_PREFAB_PATH = "Assets/Prefabs/Minigames/Player1Piece.prefab";
        private const string TILE_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_HexTile_Interactive.mat";
        private const string PLAYER1_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_Player1Piece.mat";
        private const string PLAYER2_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_Player2Piece.mat";

        [MenuItem("ShaderOp/Setup/HexCheckers Complete Scene")]
        public static void SetupCompleteScene()
        {
            Debug.Log("[HexCheckersSceneSetup] Starting complete scene setup...");

            // GameBootstrapが存在するか確認
            var bootstrap = Object.FindObjectOfType<ShaderOp.Core.GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning("[HexCheckersSceneSetup] GameBootstrap not found. Creating one...");
                CreateGameBootstrap();
            }

            // メインカメラ設定
            SetupCamera();

            // ゲームコントローラー作成
            GameObject gameController = CreateGameController();

            // UI作成
            GameObject uiCanvas = CreateUI(gameController);

            Debug.Log("[HexCheckersSceneSetup] Scene setup complete!");
            EditorUtility.DisplayDialog("HexCheckers Setup", "Scene setup complete!\n\nNext steps:\n1. Press Play\n2. Click on tiles to select pieces\n3. Valid moves will be highlighted\n4. Jump over opponent pieces to capture them", "OK");
        }

        /// <summary>
        /// GameBootstrapを作成
        /// </summary>
        private static void CreateGameBootstrap()
        {
            GameObject bootstrapObj = new GameObject("GameBootstrap");
            bootstrapObj.AddComponent<ShaderOp.Core.GameBootstrap>();
            Undo.RegisterCreatedObjectUndo(bootstrapObj, "Create GameBootstrap");
        }

        /// <summary>
        /// カメラを設定（縦画面・俯瞰視点）
        /// </summary>
        private static void SetupCamera()
        {
            Camera? mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(cameraObj, "Create Main Camera");
            }

            // 俯瞰視点（8x8グリッド用）
            mainCamera.transform.position = new Vector3(0, 12, -6);
            mainCamera.transform.rotation = Quaternion.Euler(60, 0, 0);
            mainCamera.orthographic = false;
            mainCamera.fieldOfView = 50;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            Debug.Log("[HexCheckersSceneSetup] Camera configured");
        }

        /// <summary>
        /// ゲームコントローラーとビューを作成
        /// </summary>
        private static GameObject CreateGameController()
        {
            // Controller GameObject作成
            GameObject controllerObj = new GameObject("HexCheckersController");
            HexCheckersController controller = controllerObj.AddComponent<HexCheckersController>();

            // View GameObject作成
            GameObject viewObj = new GameObject("HexCheckersView");
            viewObj.transform.SetParent(controllerObj.transform);
            HexCheckersView view = viewObj.AddComponent<HexCheckersView>();

            // BoardParent作成
            GameObject boardParent = new GameObject("Board");
            boardParent.transform.SetParent(viewObj.transform);

            // Prefabをロード
            GameObject? hexTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);

            if (hexTilePrefab == null)
            {
                Debug.LogWarning($"[HexCheckersSceneSetup] HexTile prefab not found at {PREFAB_PATH}");
            }

            // ViewにPrefab参照を設定
            var viewSerializedObject = new SerializedObject(view);
            viewSerializedObject.FindProperty("_tilePrefab").objectReferenceValue = hexTilePrefab;
            viewSerializedObject.FindProperty("_boardParent").objectReferenceValue = boardParent.transform;
            viewSerializedObject.ApplyModifiedProperties();

            // ControllerにView参照を設定
            var controllerSerializedObject = new SerializedObject(controller);
            controllerSerializedObject.FindProperty("_view").objectReferenceValue = view;
            controllerSerializedObject.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(controllerObj, "Create HexCheckers Controller");

            Debug.Log("[HexCheckersSceneSetup] Game controller and view created");
            return controllerObj;
        }

        /// <summary>
        /// UIを作成（縦画面レイアウト）
        /// </summary>
        private static GameObject CreateUI(GameObject gameController)
        {
            // Canvas作成
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // 縦画面 9:16
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create UI Canvas");

            // UIパネル（画面下部40%）
            GameObject uiPanel = CreateUIPanel(canvasObj);

            // Player駒数表示
            GameObject player1Count = CreateTextElement(uiPanel, "Player1Count", "Player 1: 12", new Vector2(-200, 150), new Vector2(300, 60));
            GameObject player2Count = CreateTextElement(uiPanel, "Player2Count", "Player 2: 12", new Vector2(200, 150), new Vector2(300, 60));

            // ターン表示
            GameObject turnIndicator = CreateTextElement(uiPanel, "TurnIndicator", "Turn: Player 1", new Vector2(0, 80), new Vector2(400, 60));

            // 状態メッセージ表示
            GameObject statusMessage = CreateTextElement(uiPanel, "StatusMessage", "Select a piece", new Vector2(0, 10), new Vector2(600, 60));

            // ゲーム結果表示（初期非表示）
            GameObject gameResult = CreateTextElement(uiPanel, "GameResult", "Player 1 Wins!", new Vector2(0, -60), new Vector2(600, 100));
            gameResult.SetActive(false);
#if TEXTMESHPRO_PRESENT
            TextMeshProUGUI gameResultText = gameResult.GetComponent<TextMeshProUGUI>();
            if (gameResultText != null)
            {
                gameResultText.fontSize = 48;
                gameResultText.fontStyle = FontStyles.Bold;
                gameResultText.color = Color.yellow;
            }
#endif

            // リセットボタン
            GameObject resetButton = CreateButton(uiPanel, "ResetButton", "Reset", new Vector2(-100, -180), new Vector2(180, 60));

            // メニューに戻るボタン
            GameObject backButton = CreateButton(uiPanel, "BackButton", "Back to Menu", new Vector2(100, -180), new Vector2(200, 60));

            // コントローラーにUI参照を設定
            HexCheckersController? controller = gameController.GetComponent<HexCheckersController>();
            if (controller != null)
            {
                var serializedObject = new SerializedObject(controller);
#if TEXTMESHPRO_PRESENT
                serializedObject.FindProperty("_player1ScoreText").objectReferenceValue = player1Count.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_player2ScoreText").objectReferenceValue = player2Count.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_currentPlayerText").objectReferenceValue = turnIndicator.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_statusText").objectReferenceValue = statusMessage.GetComponent<TextMeshProUGUI>();
#endif
                serializedObject.FindProperty("_resetButton").objectReferenceValue = resetButton.GetComponent<Button>();
                serializedObject.ApplyModifiedProperties();
            }

            Debug.Log("[HexCheckersSceneSetup] UI created");
            return canvasObj;
        }

        /// <summary>
        /// UIパネルを作成
        /// </summary>
        private static GameObject CreateUIPanel(GameObject parent)
        {
            GameObject panelObj = new GameObject("UIPanel");
            panelObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0.4f); // 下部40%
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image image = panelObj.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            return panelObj;
        }

        /// <summary>
        /// テキスト要素を作成
        /// </summary>
        private static GameObject CreateTextElement(GameObject parent, string name, string text, Vector2 position, Vector2 size)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

#if TEXTMESHPRO_PRESENT
            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = 36;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignmentOptions.Center;
#endif

            return textObj;
        }

        /// <summary>
        /// ボタンを作成
        /// </summary>
        private static GameObject CreateButton(GameObject parent, string name, string text, Vector2 position, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.5f, 0.8f);

            Button button = buttonObj.AddComponent<Button>();

            // ボタンテキスト
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

#if TEXTMESHPRO_PRESENT
            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = 28;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignmentOptions.Center;
#endif

            return buttonObj;
        }

        [MenuItem("ShaderOp/Setup/HexCheckers Quick Test")]
        public static void QuickTest()
        {
            Debug.Log("[HexCheckersSceneSetup] Running quick validation test...");

            // Prefabの存在確認
            bool allAssetsFound = true;

            if (!AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH))
            {
                Debug.LogError($"HexTile prefab not found: {PREFAB_PATH}");
                allAssetsFound = false;
            }

            if (!AssetDatabase.LoadAssetAtPath<GameObject>(PIECE_PREFAB_PATH))
            {
                Debug.LogError($"GamePiece prefab not found: {PIECE_PREFAB_PATH}");
                allAssetsFound = false;
            }

            if (!AssetDatabase.LoadAssetAtPath<Material>(TILE_MATERIAL_PATH))
            {
                Debug.LogError($"Tile material not found: {TILE_MATERIAL_PATH}");
                allAssetsFound = false;
            }

            if (allAssetsFound)
            {
                Debug.Log("[HexCheckersSceneSetup] All required assets found!");
                EditorUtility.DisplayDialog("Quick Test", "All required assets found!\n\nReady to setup scene.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Quick Test Failed", "Some required assets are missing. Check the Console for details.", "OK");
            }
        }
    }
}
