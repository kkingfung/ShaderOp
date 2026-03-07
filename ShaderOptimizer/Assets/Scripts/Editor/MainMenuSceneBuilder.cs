#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using ShaderOp.Core;

namespace ShaderOp.Editor
{
    /// <summary>
    /// MainMenuシーンビルダー
    /// </summary>
    /// <remarks>
    /// MainMenuシーンのUI構築を自動化
    /// </remarks>
    public static class MainMenuSceneBuilder
    {
        [MenuItem("ShaderOp/Setup/Build MainMenu Scene UI")]
        public static void BuildMainMenuUI()
        {
            // 現在のシーンがMainMenuか確認
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "MainMenu")
            {
                Debug.LogWarning("[MainMenuSceneBuilder] Please load MainMenu scene first!");
                return;
            }

            // Canvasを作成
            GameObject canvasObj = CreateCanvas();

            // メインメニューパネルを作成
            GameObject mainPanel = CreateMainPanel(canvasObj);

            // タイトルテキストを作成
            CreateTitleText(mainPanel);

            // ボタン群を作成
            CreateMenuButtons(mainPanel);

            // ミニゲームボタン群を作成
            CreateMinigameButtons(mainPanel);

            // MainMenuUIコンポーネントを設定
            SetupMainMenuUI(canvasObj);

            // EventSystemを確認・作成
            EnsureEventSystem();

            // GameManagerを確認・作成
            EnsureGameManager();

            // シーンを保存
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[MainMenuSceneBuilder] MainMenu UI built successfully!");
        }

        /// <summary>
        /// Canvasを作成
        /// </summary>
        private static GameObject CreateCanvas()
        {
            // 既存のCanvasを探す
            Canvas? existingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                Debug.Log("[MainMenuSceneBuilder] Using existing Canvas");
                return existingCanvas.gameObject;
            }

            // 新規Canvas作成
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[MainMenuSceneBuilder] Created new Canvas");
            return canvasObj;
        }

        /// <summary>
        /// メインパネルを作成
        /// </summary>
        private static GameObject CreateMainPanel(GameObject parent)
        {
            GameObject panel = new GameObject("MainPanel");
            panel.transform.SetParent(parent.transform, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            return panel;
        }

        /// <summary>
        /// タイトルテキストを作成
        /// </summary>
        private static void CreateTitleText(GameObject parent)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent.transform, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.8f);
            rt.anchorMax = new Vector2(0.5f, 0.8f);
            rt.sizeDelta = new Vector2(800, 150);
            rt.anchoredPosition = Vector2.zero;

            Text text = titleObj.AddComponent<Text>();
            text.text = "ShaderOp";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 80;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            // アウトライン効果
            Outline outline = titleObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.2f, 0.8f);
            outline.effectDistance = new Vector2(3, -3);
        }

        /// <summary>
        /// メニューボタン群を作成
        /// </summary>
        private static void CreateMenuButtons(GameObject parent)
        {
            // ボタンコンテナ
            GameObject container = new GameObject("MenuButtons");
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 300);
            rt.anchoredPosition = new Vector2(-250, 0);

            // VerticalLayoutGroup
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // ボタン作成
            CreateButton(container, "CustomizationButton", "Character Customization");
            CreateButton(container, "RoomDecorationButton", "Room Decoration");
            CreateButton(container, "QuitButton", "Quit", new Color(0.8f, 0.2f, 0.2f));
        }

        /// <summary>
        /// ミニゲームボタン群を作成
        /// </summary>
        private static void CreateMinigameButtons(GameObject parent)
        {
            // ボタンコンテナ
            GameObject container = new GameObject("MinigameButtons");
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 300);
            rt.anchoredPosition = new Vector2(250, 0);

            // VerticalLayoutGroup
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // ミニゲームタイトル
            GameObject titleObj = new GameObject("MinigamesTitle");
            titleObj.transform.SetParent(container.transform, false);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "Mini Games";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 24;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 1f, 0.5f);

            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(0, 40);

            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 40;

            // ミニゲームボタン作成
            CreateButton(container, "TicTacToeButton", "Tic-Tac-Toe Hex", new Color(0.2f, 0.6f, 0.8f));
            CreateButton(container, "ReversiButton", "Hex Reversi", new Color(0.2f, 0.6f, 0.8f));
            CreateButton(container, "CheckersButton", "Hex Checkers", new Color(0.2f, 0.6f, 0.8f));
            CreateButton(container, "ChessButton", "Hex Chess", new Color(0.2f, 0.6f, 0.8f));
        }

        /// <summary>
        /// ボタンを作成
        /// </summary>
        private static GameObject CreateButton(GameObject parent, string name, string label, Color? buttonColor = null)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 50);

            // LayoutElement
            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50;

            // Button
            Button button = buttonObj.AddComponent<Button>();

            // Image
            Image image = buttonObj.AddComponent<Image>();
            image.color = buttonColor ?? new Color(0.3f, 0.3f, 0.6f);

            // Navigation設定（キーボードナビゲーション用）
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Automatic;
            button.navigation = nav;

            // テキスト
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return buttonObj;
        }

        /// <summary>
        /// MainMenuUIコンポーネントを設定
        /// </summary>
        private static void SetupMainMenuUI(GameObject canvasObj)
        {
            // 既存のMainMenuUIを探す
            MainMenuUI? existingUI = Object.FindFirstObjectByType<MainMenuUI>();
            GameObject uiObj;

            if (existingUI != null)
            {
                uiObj = existingUI.gameObject;
                Debug.Log("[MainMenuSceneBuilder] Using existing MainMenuUI");
            }
            else
            {
                uiObj = new GameObject("MainMenuUI");
                uiObj.AddComponent<MainMenuUI>();
                Debug.Log("[MainMenuSceneBuilder] Created new MainMenuUI");
            }

            MainMenuUI ui = uiObj.GetComponent<MainMenuUI>();

            // SerializedObjectを使ってprivateフィールドを設定
            SerializedObject so = new SerializedObject(ui);

            // ボタンを探して割り当て
            AssignButton(so, "_customizationButton", "CustomizationButton");
            AssignButton(so, "_roomDecorationButton", "RoomDecorationButton");
            AssignButton(so, "_quitButton", "QuitButton");
            AssignButton(so, "_ticTacToeButton", "TicTacToeButton");
            AssignButton(so, "_reversiButton", "ReversiButton");
            AssignButton(so, "_checkersButton", "CheckersButton");
            AssignButton(so, "_chessButton", "ChessButton");

            so.ApplyModifiedProperties();

            Debug.Log("[MainMenuSceneBuilder] MainMenuUI configured!");
        }

        /// <summary>
        /// ボタンを割り当て
        /// </summary>
        private static void AssignButton(SerializedObject so, string fieldName, string buttonName)
        {
            Button? button = GameObject.Find(buttonName)?.GetComponent<Button>();
            if (button != null)
            {
                SerializedProperty prop = so.FindProperty(fieldName);
                if (prop != null)
                {
                    prop.objectReferenceValue = button;
                    Debug.Log($"[MainMenuSceneBuilder] Assigned {buttonName} to {fieldName}");
                }
            }
            else
            {
                Debug.LogWarning($"[MainMenuSceneBuilder] Button '{buttonName}' not found!");
            }
        }

        /// <summary>
        /// EventSystemを確認・作成
        /// </summary>
        private static void EnsureEventSystem()
        {
            UnityEngine.EventSystems.EventSystem? eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
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
        /// GameManagerを確認・作成
        /// </summary>
        private static void EnsureGameManager()
        {
            GameManager? gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
                Debug.Log("[MainMenuSceneBuilder] Created GameManager");
            }
            else
            {
                Debug.Log("[MainMenuSceneBuilder] GameManager already exists");
            }
        }
    }
}
