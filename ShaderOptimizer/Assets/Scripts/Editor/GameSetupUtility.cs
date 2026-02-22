#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShaderOp.Core;
using ShaderOp.Customization;
using ShaderOp.Minigames.Games;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Editor
{
    /// <summary>
    /// ゲームセットアップユーティリティ
    /// </summary>
    /// <remarks>
    /// Unityエディタメニューから実行可能なセットアップツール
    /// シーン、プレハブ、基本GameObjectの自動生成機能を提供
    /// </remarks>
    public static class GameSetupUtility
    {
        private const string MENU_ROOT = "ShaderOp/Setup/";
        private const string SCENES_PATH = "Assets/Scenes/";
        private const string PREFABS_PATH = "Assets/Prefabs/";

        #region Scene Setup

        [MenuItem(MENU_ROOT + "Create All Scenes")]
        public static void CreateAllScenes()
        {
            CreateMainMenuScene();
            CreateTicTacToeHexScene();
            CreateRoomDecorationScene();

            Debug.Log("[GameSetupUtility] All scenes created successfully!");
        }

        [MenuItem(MENU_ROOT + "Scenes/Create MainMenu Scene")]
        public static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // GameManagerを追加
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();

            // MainMenuUIを追加
            GameObject mainMenuObj = new GameObject("MainMenuUI");
            mainMenuObj.AddComponent<MainMenuUI>();

            // EventSystemを追加（UI用）
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // シーンを保存
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, SCENES_PATH + "MainMenu.unity");
            Debug.Log("[GameSetupUtility] MainMenu scene created!");
        }

        [MenuItem(MENU_ROOT + "Scenes/Create TicTacToeHex Scene")]
        public static void CreateTicTacToeHexScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // ゲームマネージャー
            GameObject gameObj = new GameObject("TicTacToeHexGame");

            // Model
            var modelObj = new GameObject("Model");
            modelObj.transform.SetParent(gameObj.transform);
            var model = modelObj.AddComponent<TicTacToeHexModel>();

            // View
            var viewObj = new GameObject("View");
            viewObj.transform.SetParent(gameObj.transform);
            var view = viewObj.AddComponent<TicTacToeHexView>();

            // ボードの親オブジェクト
            var boardParent = new GameObject("BoardParent");
            boardParent.transform.SetParent(viewObj.transform);

            // Controller
            var controllerObj = new GameObject("Controller");
            controllerObj.transform.SetParent(gameObj.transform);
            var controller = controllerObj.AddComponent<TicTacToeHexController>();

            // EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // シーンを保存
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, SCENES_PATH + "TicTacToeHex.unity");
            Debug.Log("[GameSetupUtility] TicTacToeHex scene created!");
        }

        [MenuItem(MENU_ROOT + "Scenes/Create RoomDecoration Scene")]
        public static void CreateRoomDecorationScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Room
            GameObject roomObj = new GameObject("Room");

            // Floor
            GameObject floorObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorObj.name = "Floor";
            floorObj.transform.SetParent(roomObj.transform);
            floorObj.transform.localScale = new Vector3(5, 1, 5);

            // Wall (簡易的に4つの壁を作成)
            CreateWall(roomObj.transform, "WallNorth", new Vector3(0, 2.5f, 25), new Vector3(50, 5, 1));
            CreateWall(roomObj.transform, "WallSouth", new Vector3(0, 2.5f, -25), new Vector3(50, 5, 1));
            CreateWall(roomObj.transform, "WallEast", new Vector3(25, 2.5f, 0), new Vector3(1, 5, 50));
            CreateWall(roomObj.transform, "WallWest", new Vector3(-25, 2.5f, 0), new Vector3(1, 5, 50));

            // RoomDecorator
            GameObject decoratorObj = new GameObject("RoomDecorator");
            var decorator = decoratorObj.AddComponent<RoomDecorator>();

            // FurnitureContainer
            GameObject furnitureContainer = new GameObject("FurnitureContainer");
            furnitureContainer.transform.SetParent(roomObj.transform);

            // シーンを保存
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, SCENES_PATH + "RoomDecoration.unity");
            Debug.Log("[GameSetupUtility] RoomDecoration scene created!");
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
        }

        #endregion

        #region Prefab Setup

        [MenuItem(MENU_ROOT + "Prefabs/Create HexTile Prefab")]
        public static void CreateHexTilePrefab()
        {
            // HexTileプレハブを作成
            GameObject tilePrefab = new GameObject("HexTile");

            // SpriteRendererを追加
            SpriteRenderer renderer = tilePrefab.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateHexagonSprite();
            renderer.sortingOrder = 0;

            // Colliderを追加（クリック検出用）
            PolygonCollider2D collider = tilePrefab.AddComponent<PolygonCollider2D>();

            // HexTileVisualizerを追加
            tilePrefab.AddComponent<HexTileVisualizer>();

            // Prefabsフォルダを確認・作成
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Minigames"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Minigames");
            }

            // プレハブとして保存
            string prefabPath = PREFABS_PATH + "Minigames/HexTile.prefab";
            PrefabUtility.SaveAsPrefabAsset(tilePrefab, prefabPath);
            Object.DestroyImmediate(tilePrefab);

            Debug.Log($"[GameSetupUtility] HexTile prefab created at {prefabPath}");
        }

        [MenuItem(MENU_ROOT + "Prefabs/Create All Prefabs")]
        public static void CreateAllPrefabs()
        {
            CreateHexTilePrefab();
            Debug.Log("[GameSetupUtility] All prefabs created successfully!");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 六角形スプライトを作成（プレースホルダー）
        /// </summary>
        private static Sprite CreateHexagonSprite()
        {
            // 簡易的な六角形テクスチャを作成
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f);
        }

        #endregion

        #region Validation

        [MenuItem(MENU_ROOT + "Validate Project Setup")]
        public static void ValidateProjectSetup()
        {
            bool allValid = true;

            // シーンの存在確認
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENES_PATH + "MainMenu.unity"))
            {
                Debug.LogWarning("[GameSetupUtility] MainMenu scene not found!");
                allValid = false;
            }

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENES_PATH + "MainCustomization.unity"))
            {
                Debug.LogWarning("[GameSetupUtility] MainCustomization scene not found!");
                allValid = false;
            }

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENES_PATH + "TicTacToeHex.unity"))
            {
                Debug.LogWarning("[GameSetupUtility] TicTacToeHex scene not found!");
                allValid = false;
            }

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENES_PATH + "RoomDecoration.unity"))
            {
                Debug.LogWarning("[GameSetupUtility] RoomDecoration scene not found!");
                allValid = false;
            }

            // プレハブの存在確認
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "Minigames/HexTile.prefab"))
            {
                Debug.LogWarning("[GameSetupUtility] HexTile prefab not found!");
                allValid = false;
            }

            if (allValid)
            {
                Debug.Log("[GameSetupUtility] ✅ Project setup is valid!");
            }
            else
            {
                Debug.LogWarning("[GameSetupUtility] ⚠️ Some project files are missing. Run setup commands from ShaderOp/Setup menu.");
            }
        }

        #endregion
    }
}
