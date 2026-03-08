#nullable enable

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace ShaderOp.Editor
{
    /// <summary>
    /// TicTacToeHex シーンセットアップユーティリティ
    /// </summary>
    /// <remarks>
    /// TicTacToeHex垂直スライスのシーンを自動構築します
    /// - 3x3ヘックスグリッド
    /// - 縦画面UI（ターン表示、リセットボタン）
    /// - シェーダー統合
    /// </remarks>
    public static class TicTacToeHexSceneSetup
    {
        private const string MATERIALS_PATH = "Assets/Materials/Minigames";
        private const string PREFABS_PATH = "Assets/Prefabs/Minigames";
        private const string SPRITES_PATH = "Assets/Sprites/Minigames";

        /// <summary>
        /// TicTacToeHexシーンをセットアップ
        /// </summary>
        [MenuItem("ShaderOp/Setup/TicTacToeHex Vertical Slice")]
        public static void SetupTicTacToeHexScene()
        {
            Debug.Log("[TicTacToeHexSceneSetup] シーンセットアップを開始します...");

            // 必要なディレクトリを作成
            EnsureDirectoriesExist();

            // スプライトを作成
            CreateHexagonSprites();

            // UIキャンバスを作成
            GameObject? uiCanvas = CreatePortraitUICanvas();

            // ゲームコントローラーを作成
            GameObject? gameController = CreateGameController(uiCanvas);

            // 選択状態にする
            if (gameController != null)
            {
                Selection.activeGameObject = gameController;
            }

            // シーンを保存
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("[TicTacToeHexSceneSetup] シーンセットアップが完了しました！");
        }

        /// <summary>
        /// 必要なディレクトリを作成
        /// </summary>
        private static void EnsureDirectoriesExist()
        {
            if (!AssetDatabase.IsValidFolder(SPRITES_PATH))
            {
                string parentFolder = Path.GetDirectoryName(SPRITES_PATH)?.Replace('\\', '/') ?? "Assets/Sprites";
                string folderName = Path.GetFileName(SPRITES_PATH);

                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Sprites");
                    parentFolder = "Assets/Sprites";
                }

                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }

        /// <summary>
        /// ヘックス型スプライトを作成
        /// </summary>
        private static void CreateHexagonSprites()
        {
            // ヘックスタイル用スプライト
            string hexTilePath = $"{SPRITES_PATH}/HexTile.png";
            if (!File.Exists(hexTilePath))
            {
                Texture2D hexTex = CreateHexagonTexture(128, Color.white);
                SaveTextureAsPNG(hexTex, hexTilePath);
                Debug.Log($"[TicTacToeHexSceneSetup] ヘックスタイルスプライトを作成: {hexTilePath}");
            }

            // プレイヤー1駒スプライト（円）
            string player1Path = $"{SPRITES_PATH}/Player1Piece.png";
            if (!File.Exists(player1Path))
            {
                Texture2D player1Tex = CreateCircleTexture(64, Color.white);
                SaveTextureAsPNG(player1Tex, player1Path);
                Debug.Log($"[TicTacToeHexSceneSetup] プレイヤー1駒スプライトを作成: {player1Path}");
            }

            // プレイヤー2駒スプライト（円）
            string player2Path = $"{SPRITES_PATH}/Player2Piece.png";
            if (!File.Exists(player2Path))
            {
                Texture2D player2Tex = CreateCircleTexture(64, Color.white);
                SaveTextureAsPNG(player2Tex, player2Path);
                Debug.Log($"[TicTacToeHexSceneSetup] プレイヤー2駒スプライトを作成: {player2Path}");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// ヘックス型のテクスチャを生成
        /// </summary>
        private static Texture2D CreateHexagonTexture(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            float centerX = size / 2f;
            float centerY = size / 2f;
            float radius = size / 2f - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;

                    // ヘックス判定（簡易版: 円で代用）
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < radius)
                    {
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 円型のテクスチャを生成
        /// </summary>
        private static Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            float centerX = size / 2f;
            float centerY = size / 2f;
            float radius = size / 2f - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < radius)
                    {
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// テクスチャをPNGとして保存
        /// </summary>
        private static void SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
        }

        /// <summary>
        /// 縦画面向けUIキャンバスを作成
        /// </summary>
        private static GameObject CreatePortraitUICanvas()
        {
            // 既存のCanvasを検索
            Canvas? existingCanvas = Object.FindObjectOfType<Canvas>();
            if (existingCanvas != null)
            {
                Debug.Log("[TicTacToeHexSceneSetup] 既存のCanvasを使用します");
                return existingCanvas.gameObject;
            }

            // 新規Canvas作成
            GameObject canvasObj = new GameObject("UI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // 縦画面
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // UIパネル（画面下部40%）
            GameObject panelObj = new GameObject("GamePanel");
            panelObj.transform.SetParent(canvasObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0.4f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // ターン表示テキスト
            GameObject turnTextObj = new GameObject("TurnIndicatorText");
            turnTextObj.transform.SetParent(panelObj.transform, false);

            RectTransform turnTextRect = turnTextObj.AddComponent<RectTransform>();
            turnTextRect.anchorMin = new Vector2(0.1f, 0.6f);
            turnTextRect.anchorMax = new Vector2(0.9f, 0.9f);
            turnTextRect.offsetMin = Vector2.zero;
            turnTextRect.offsetMax = Vector2.zero;

            Text turnText = turnTextObj.AddComponent<Text>();
            turnText.text = "プレイヤー1のターン";
            turnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            turnText.fontSize = 48;
            turnText.alignment = TextAnchor.MiddleCenter;
            turnText.color = Color.white;

            // ゲーム状態テキスト
            GameObject statusTextObj = new GameObject("GameStatusText");
            statusTextObj.transform.SetParent(panelObj.transform, false);

            RectTransform statusTextRect = statusTextObj.AddComponent<RectTransform>();
            statusTextRect.anchorMin = new Vector2(0.1f, 0.4f);
            statusTextRect.anchorMax = new Vector2(0.9f, 0.6f);
            statusTextRect.offsetMin = Vector2.zero;
            statusTextRect.offsetMax = Vector2.zero;

            Text statusText = statusTextObj.AddComponent<Text>();
            statusText.text = "";
            statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.fontSize = 36;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.yellow;

            // リセットボタン
            GameObject buttonObj = new GameObject("ResetButton");
            buttonObj.transform.SetParent(panelObj.transform, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.3f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.7f, 0.35f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.6f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);

            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;

            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "リセット";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 32;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            Debug.Log("[TicTacToeHexSceneSetup] 縦画面UIキャンバスを作成しました");

            return canvasObj;
        }

        /// <summary>
        /// ゲームコントローラーを作成
        /// </summary>
        private static GameObject CreateGameController(GameObject? uiCanvas)
        {
            GameObject controllerObj = new GameObject("TicTacToeHexGame");
            var controller = controllerObj.AddComponent<ShaderOp.Minigames.Games.TicTacToeHexVerticalSlice>();

            // ボード親オブジェクト作成
            GameObject boardParent = new GameObject("BoardParent");
            boardParent.transform.SetParent(controllerObj.transform);
            boardParent.transform.localPosition = Vector3.zero;

            // プレハブ参照をロード
            GameObject? tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_PATH}/HexTile.prefab");
            GameObject? player1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_PATH}/Player1Piece.prefab");
            GameObject? player2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_PATH}/Player2Piece.prefab");

            // マテリアル参照をロード
            Material? hexIdleMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/MAT_HexTile_Idle.mat");
            Material? hexHoverMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/MAT_HexTile_Hover.mat");
            Material? hexSelectedMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/MAT_HexTile_Selected.mat");
            Material? player1Mat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/MAT_Player1Piece.mat");
            Material? player2Mat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/MAT_Player2Piece.mat");

            // SerializedObjectでフィールドを設定
            SerializedObject serializedController = new SerializedObject(controller);

            serializedController.FindProperty("_boardParent").objectReferenceValue = boardParent.transform;
            serializedController.FindProperty("_tilePrefab").objectReferenceValue = tilePrefab;
            serializedController.FindProperty("_player1PiecePrefab").objectReferenceValue = player1Prefab;
            serializedController.FindProperty("_player2PiecePrefab").objectReferenceValue = player2Prefab;

            serializedController.FindProperty("_hexTileIdleMaterial").objectReferenceValue = hexIdleMat;
            serializedController.FindProperty("_hexTileHoverMaterial").objectReferenceValue = hexHoverMat;
            serializedController.FindProperty("_hexTileSelectedMaterial").objectReferenceValue = hexSelectedMat;
            serializedController.FindProperty("_player1PieceMaterial").objectReferenceValue = player1Mat;
            serializedController.FindProperty("_player2PieceMaterial").objectReferenceValue = player2Mat;

            // UI参照
            if (uiCanvas != null)
            {
                serializedController.FindProperty("_uiCanvas").objectReferenceValue = uiCanvas.GetComponent<Canvas>();

                Transform panelTransform = uiCanvas.transform.Find("GamePanel");
                if (panelTransform != null)
                {
                    serializedController.FindProperty("_uiPanel").objectReferenceValue = panelTransform.gameObject;
                    serializedController.FindProperty("_turnIndicatorText").objectReferenceValue =
                        panelTransform.Find("TurnIndicatorText")?.GetComponent<Text>();
                    serializedController.FindProperty("_gameStatusText").objectReferenceValue =
                        panelTransform.Find("GameStatusText")?.GetComponent<Text>();
                    serializedController.FindProperty("_resetButton").objectReferenceValue =
                        panelTransform.Find("ResetButton")?.GetComponent<Button>();
                }
            }

            serializedController.ApplyModifiedProperties();

            Debug.Log("[TicTacToeHexSceneSetup] ゲームコントローラーを作成しました");

            return controllerObj;
        }
    }
}
