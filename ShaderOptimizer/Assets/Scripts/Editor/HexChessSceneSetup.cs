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
    /// HexChessシーン自動セットアップツール
    /// </summary>
    public static class HexChessSceneSetup
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Minigames/HexTile.prefab";
        private const string PIECE_PREFAB_PATH = "Assets/Prefabs/Minigames/Player1Piece.prefab";
        private const string TILE_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_HexTile_Interactive.mat";
        private const string PLAYER1_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_Player1Piece.mat";
        private const string PLAYER2_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_Player2Piece.mat";

        [MenuItem("ShaderOp/Setup/HexChess Complete Scene")]
        public static void SetupCompleteScene()
        {
            Debug.Log("[HexChessSceneSetup] Starting complete scene setup...");

            // GameBootstrapが存在するか確認
            var bootstrap = Object.FindObjectOfType<ShaderOp.Core.GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning("[HexChessSceneSetup] GameBootstrap not found. Creating one...");
                CreateGameBootstrap();
            }

            // メインカメラ設定
            SetupCamera();

            // ゲームコントローラー作成
            GameObject gameController = CreateGameController();

            // UI作成
            GameObject uiCanvas = CreateUI(gameController);

            Debug.Log("[HexChessSceneSetup] Scene setup complete!");
            EditorUtility.DisplayDialog("HexChess Setup", "Scene setup complete!\n\nNext steps:\n1. Press Play\n2. Select a piece to see valid moves\n3. Move pieces according to chess rules\n4. Checkmate your opponent!", "OK");
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

            // 俯瞰視点（11x11グリッド用 - より広い視野）
            mainCamera.transform.position = new Vector3(0, 15, -8);
            mainCamera.transform.rotation = Quaternion.Euler(60, 0, 0);
            mainCamera.orthographic = false;
            mainCamera.fieldOfView = 55;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            Debug.Log("[HexChessSceneSetup] Camera configured");
        }

        /// <summary>
        /// ゲームコントローラーを作成
        /// </summary>
        private static GameObject CreateGameController()
        {
            GameObject controllerObj = new GameObject("HexChessController");
            HexChessController controller = controllerObj.AddComponent<HexChessController>();

            // Prefab参照を設定
            GameObject? hexTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            GameObject? piecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PIECE_PREFAB_PATH);
            Material? tileMaterial = AssetDatabase.LoadAssetAtPath<Material>(TILE_MATERIAL_PATH);
            Material? player1Material = AssetDatabase.LoadAssetAtPath<Material>(PLAYER1_MATERIAL_PATH);
            Material? player2Material = AssetDatabase.LoadAssetAtPath<Material>(PLAYER2_MATERIAL_PATH);

            if (hexTilePrefab != null)
            {
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_hexTilePrefab").objectReferenceValue = hexTilePrefab;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[HexChessSceneSetup] HexTile prefab not found at {PREFAB_PATH}");
            }

            if (piecePrefab != null)
            {
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_gamePiecePrefab").objectReferenceValue = piecePrefab;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[HexChessSceneSetup] GamePiece prefab not found at {PIECE_PREFAB_PATH}");
            }

            if (tileMaterial != null)
            {
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_hexTileMaterial").objectReferenceValue = tileMaterial;
                serializedObject.ApplyModifiedProperties();
            }

            if (player1Material != null)
            {
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_player1PieceMaterial").objectReferenceValue = player1Material;
                serializedObject.ApplyModifiedProperties();
            }

            if (player2Material != null)
            {
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_player2PieceMaterial").objectReferenceValue = player2Material;
                serializedObject.ApplyModifiedProperties();
            }

            Undo.RegisterCreatedObjectUndo(controllerObj, "Create HexChess Controller");

            Debug.Log("[HexChessSceneSetup] Game controller created");
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

            // Player名表示
            GameObject player1Name = CreateTextElement(uiPanel, "Player1Name", "Player 1 (White)", new Vector2(-200, 150), new Vector2(300, 60));
            GameObject player2Name = CreateTextElement(uiPanel, "Player2Name", "Player 2 (Black)", new Vector2(200, 150), new Vector2(300, 60));

            // ターン表示
            GameObject turnIndicator = CreateTextElement(uiPanel, "TurnIndicator", "Turn: Player 1", new Vector2(0, 80), new Vector2(400, 60));

            // チェック状態表示
            GameObject checkIndicator = CreateTextElement(uiPanel, "CheckIndicator", "", new Vector2(0, 10), new Vector2(600, 60));
#if TEXTMESHPRO_PRESENT
            TextMeshProUGUI checkText = checkIndicator.GetComponent<TextMeshProUGUI>();
            if (checkText != null)
            {
                checkText.fontSize = 40;
                checkText.color = Color.red;
            }
#endif

            // ゲーム結果表示（初期非表示）
            GameObject gameResult = CreateTextElement(uiPanel, "GameResult", "Checkmate! Player 1 Wins!", new Vector2(0, -60), new Vector2(600, 100));
            gameResult.SetActive(false);
#if TEXTMESHPRO_PRESENT
            TextMeshProUGUI gameResultText = gameResult.GetComponent<TextMeshProUGUI>();
            if (gameResultText != null)
            {
                gameResultText.fontSize = 44;
                gameResultText.fontStyle = FontStyles.Bold;
                gameResultText.color = Color.yellow;
            }
#endif

            // リセットボタン
            GameObject resetButton = CreateButton(uiPanel, "ResetButton", "Reset", new Vector2(-100, -180), new Vector2(180, 60));

            // メニューに戻るボタン
            GameObject backButton = CreateButton(uiPanel, "BackButton", "Back to Menu", new Vector2(100, -180), new Vector2(200, 60));

            // コントローラーにUI参照を設定
            HexChessController? controller = gameController.GetComponent<HexChessController>();
            if (controller != null)
            {
                var serializedObject = new SerializedObject(controller);
#if TEXTMESHPRO_PRESENT
                serializedObject.FindProperty("_player1NameText").objectReferenceValue = player1Name.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_player2NameText").objectReferenceValue = player2Name.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_turnIndicatorText").objectReferenceValue = turnIndicator.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_checkIndicatorText").objectReferenceValue = checkIndicator.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_gameResultText").objectReferenceValue = gameResult.GetComponent<TextMeshProUGUI>();
#endif
                serializedObject.FindProperty("_resetButton").objectReferenceValue = resetButton.GetComponent<Button>();
                serializedObject.FindProperty("_backToMenuButton").objectReferenceValue = backButton.GetComponent<Button>();
                serializedObject.ApplyModifiedProperties();
            }

            Debug.Log("[HexChessSceneSetup] UI created");
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

        [MenuItem("ShaderOp/Setup/HexChess Quick Test")]
        public static void QuickTest()
        {
            Debug.Log("[HexChessSceneSetup] Running quick validation test...");

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
                Debug.Log("[HexChessSceneSetup] All required assets found!");
                EditorUtility.DisplayDialog("Quick Test", "All required assets found!\n\nReady to setup scene.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Quick Test Failed", "Some required assets are missing. Check the Console for details.", "OK");
            }
        }
    }
}
