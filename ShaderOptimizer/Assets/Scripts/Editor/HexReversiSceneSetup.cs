#nullable enable

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ShaderOp.Minigames.Games;

namespace ShaderOp.Editor
{
    /// <summary>
    /// HexReversiシーン自動セットアップツール
    /// </summary>
    public static class HexReversiSceneSetup
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Minigames/HexTile.prefab";
        private const string PIECE_PREFAB_PATH = "Assets/Prefabs/Minigames/Player1Piece.prefab";
        private const string TILE_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_HexTile_Interactive.mat";
        private const string PLAYER1_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_Player1Piece.mat";
        private const string PLAYER2_MATERIAL_PATH = "Assets/Materials/Minigames/MAT_Player2Piece.mat";

        [MenuItem("ShaderOp/Setup/HexReversi Complete Scene")]
        public static void SetupCompleteScene()
        {
            Debug.Log("[HexReversiSceneSetup] Starting complete scene setup...");

            // GameBootstrapが存在するか確認
            var bootstrap = Object.FindObjectOfType<ShaderOp.Core.GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning("[HexReversiSceneSetup] GameBootstrap not found. Creating one...");
                CreateGameBootstrap();
            }

            // メインカメラ設定
            SetupCamera();

            // ゲームコントローラー作成
            GameObject gameController = CreateGameController();

            // UI作成
            GameObject uiCanvas = CreateUI(gameController);

            Debug.Log("[HexReversiSceneSetup] Scene setup complete!");
            EditorUtility.DisplayDialog("HexReversi Setup", "Scene setup complete!\n\nNext steps:\n1. Press Play\n2. Click on tiles to place pieces\n3. Toggle 'Show Hints' to see valid moves", "OK");
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

            // 俯瞰視点（上から見下ろす）
            mainCamera.transform.position = new Vector3(0, 10, -5);
            mainCamera.transform.rotation = Quaternion.Euler(60, 0, 0);
            mainCamera.orthographic = false;
            mainCamera.fieldOfView = 60;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            Debug.Log("[HexReversiSceneSetup] Camera configured");
        }

        /// <summary>
        /// ゲームコントローラーを作成
        /// </summary>
        private static GameObject CreateGameController()
        {
            GameObject controllerObj = new GameObject("HexReversiController");
            HexReversiComplete controller = controllerObj.AddComponent<HexReversiComplete>();

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
                Debug.LogWarning($"[HexReversiSceneSetup] HexTile prefab not found at {PREFAB_PATH}");
            }

            if (piecePrefab != null)
            {
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_gamePiecePrefab").objectReferenceValue = piecePrefab;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[HexReversiSceneSetup] GamePiece prefab not found at {PIECE_PREFAB_PATH}");
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

            Undo.RegisterCreatedObjectUndo(controllerObj, "Create HexReversi Controller");

            Debug.Log("[HexReversiSceneSetup] Game controller created");
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

            // スコア表示
            GameObject player1Score = CreateTextElement(uiPanel, "Player1Score", "Player 1: 2", new Vector2(-200, 150), new Vector2(300, 60));
            GameObject player2Score = CreateTextElement(uiPanel, "Player2Score", "Player 2: 2", new Vector2(200, 150), new Vector2(300, 60));

            // ターン表示
            GameObject turnIndicator = CreateTextElement(uiPanel, "TurnIndicator", "Turn: Player 1", new Vector2(0, 80), new Vector2(400, 60));

            // ゲーム結果表示（初期非表示）
            GameObject gameResult = CreateTextElement(uiPanel, "GameResult", "Player 1 Wins!", new Vector2(0, 0), new Vector2(600, 100));
            gameResult.SetActive(false);
            TextMeshProUGUI gameResultText = gameResult.GetComponent<TextMeshProUGUI>();
            if (gameResultText != null)
            {
                gameResultText.fontSize = 48;
                gameResultText.fontStyle = FontStyles.Bold;
                gameResultText.color = Color.yellow;
            }

            // ヒントトグル
            GameObject hintToggle = CreateToggle(uiPanel, "ShowHintsToggle", "Show Hints", new Vector2(-200, -50));

            // リセットボタン
            GameObject resetButton = CreateButton(uiPanel, "ResetButton", "Reset", new Vector2(0, -150), new Vector2(200, 60));

            // メニューに戻るボタン
            GameObject backButton = CreateButton(uiPanel, "BackButton", "Back to Menu", new Vector2(0, -230), new Vector2(250, 60));

            // コントローラーにUI参照を設定
            HexReversiComplete? controller = gameController.GetComponent<HexReversiComplete>();
            if (controller != null)
            {
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_player1ScoreText").objectReferenceValue = player1Score.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_player2ScoreText").objectReferenceValue = player2Score.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_turnIndicatorText").objectReferenceValue = turnIndicator.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_gameResultText").objectReferenceValue = gameResult.GetComponent<TextMeshProUGUI>();
                serializedObject.FindProperty("_showHintsToggle").objectReferenceValue = hintToggle.GetComponent<Toggle>();
                serializedObject.FindProperty("_resetButton").objectReferenceValue = resetButton.GetComponent<Button>();
                serializedObject.FindProperty("_backToMenuButton").objectReferenceValue = backButton.GetComponent<Button>();
                serializedObject.ApplyModifiedProperties();
            }

            Debug.Log("[HexReversiSceneSetup] UI created");
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

            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = 36;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignmentOptions.Center;

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

            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = 28;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        /// <summary>
        /// トグルを作成
        /// </summary>
        private static GameObject CreateToggle(GameObject parent, string name, string labelText, Vector2 position)
        {
            GameObject toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = toggleObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(300, 40);

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = false;

            // Background
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(toggleObj.transform, false);

            RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.pivot = new Vector2(0, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(40, 40);

            Image bgImage = backgroundObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f);

            // Checkmark
            GameObject checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(backgroundObj.transform, false);

            RectTransform checkRect = checkmarkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;

            Image checkImage = checkmarkObj.AddComponent<Image>();
            checkImage.color = new Color(0.2f, 0.8f, 0.2f);

            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.anchoredPosition = new Vector2(50, 0);
            labelRect.sizeDelta = new Vector2(-50, 0);

            TextMeshProUGUI labelTextMesh = labelObj.AddComponent<TextMeshProUGUI>();
            labelTextMesh.text = labelText;
            labelTextMesh.fontSize = 28;
            labelTextMesh.color = Color.white;
            labelTextMesh.alignment = TextAlignmentOptions.Left;

            return toggleObj;
        }

        [MenuItem("ShaderOp/Setup/HexReversi Quick Test")]
        public static void QuickTest()
        {
            Debug.Log("[HexReversiSceneSetup] Running quick validation test...");

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
                Debug.Log("[HexReversiSceneSetup] All required assets found!");
                EditorUtility.DisplayDialog("Quick Test", "All required assets found!\n\nReady to setup scene.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Quick Test Failed", "Some required assets are missing. Check the Console for details.", "OK");
            }
        }
    }
}
