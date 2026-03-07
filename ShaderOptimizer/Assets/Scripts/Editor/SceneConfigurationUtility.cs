#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShaderOp.Minigames.Games;

namespace ShaderOp.Editor
{
    /// <summary>
    /// シーン設定ユーティリティ
    /// </summary>
    /// <remarks>
    /// シーン内のコンポーネントにPrefabやGameObject参照を自動的に割り当てるツール
    /// </remarks>
    public static class SceneConfigurationUtility
    {
        private const string MENU_ROOT = "ShaderOp/Setup/Configure/";
        private const string PREFABS_PATH = "Assets/Prefabs/Minigames/";

        #region Auto-Configuration

        [MenuItem(MENU_ROOT + "Configure All Minigame Scenes")]
        public static void ConfigureAllMinigameScenes()
        {
            ConfigureTicTacToeHexScene();
            ConfigureHexReversiScene();
            ConfigureHexCheckersScene();
            ConfigureHexChessScene();

            Debug.Log("[SceneConfigurationUtility] All minigame scenes configured successfully!");
        }

        [MenuItem(MENU_ROOT + "Configure TicTacToeHex Scene")]
        public static void ConfigureTicTacToeHexScene()
        {
            ConfigureGameScene(
                "Assets/Scenes/TicTacToeHex.unity",
                "TicTacToeHexGame/View",
                typeof(TicTacToeHexView)
            );
        }

        [MenuItem(MENU_ROOT + "Configure HexReversi Scene")]
        public static void ConfigureHexReversiScene()
        {
            ConfigureGameScene(
                "Assets/Scenes/HexReversi.unity",
                "HexReversiGame/View",
                typeof(HexReversiView)
            );
        }

        [MenuItem(MENU_ROOT + "Configure HexCheckers Scene")]
        public static void ConfigureHexCheckersScene()
        {
            ConfigureGameScene(
                "Assets/Scenes/HexCheckers.unity",
                "HexCheckersGame/View",
                typeof(HexCheckersView)
            );
        }

        [MenuItem(MENU_ROOT + "Configure HexChess Scene")]
        public static void ConfigureHexChessScene()
        {
            ConfigureGameScene(
                "Assets/Scenes/HexChess.unity",
                "HexChessGame/View",
                typeof(HexChessView)
            );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ゲームシーンを設定
        /// </summary>
        private static void ConfigureGameScene(string scenePath, string viewPath, System.Type viewType)
        {
            // シーンを開く
            var scene = EditorSceneManager.OpenScene(scenePath);

            // HexTile Prefabをロード
            GameObject? tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "HexTile.prefab");
            if (tilePrefab == null)
            {
                Debug.LogError($"[SceneConfigurationUtility] HexTile prefab not found at {PREFABS_PATH}HexTile.prefab");
                return;
            }

            // Player Piece Prefabsをロードしてスプライトを取得
            GameObject? player1PiecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "Player1Piece.prefab");
            GameObject? player2PiecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "Player2Piece.prefab");

            Sprite? player1Sprite = null;
            Sprite? player2Sprite = null;

            if (player1PiecePrefab != null)
            {
                SpriteRenderer? renderer = player1PiecePrefab.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    player1Sprite = renderer.sprite;
                }
            }

            if (player2PiecePrefab != null)
            {
                SpriteRenderer? renderer = player2PiecePrefab.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    player2Sprite = renderer.sprite;
                }
            }

            // View GameObjectを検索（シーンのルートから検索）
            GameObject? viewObject = FindGameObjectInScene(scene, "View");
            if (viewObject == null)
            {
                Debug.LogError($"[SceneConfigurationUtility] View GameObject not found in scene: {scene.name}");
                return;
            }

            // Viewコンポーネントを取得
            var viewComponent = viewObject.GetComponent(viewType);
            if (viewComponent == null)
            {
                Debug.LogError($"[SceneConfigurationUtility] {viewType.Name} component not found on {viewObject.name}");
                return;
            }

            // BoardParentを検索
            Transform? boardParent = viewObject.transform.Find("BoardParent");
            if (boardParent == null)
            {
                Debug.LogError($"[SceneConfigurationUtility] BoardParent not found under {viewObject.name}");
                return;
            }

            // SerializedObjectを使ってプロパティを設定
            SerializedObject serializedView = new SerializedObject(viewComponent);

            // _tilePrefab を設定
            SerializedProperty tilePrefabProp = serializedView.FindProperty("_tilePrefab");
            if (tilePrefabProp != null)
            {
                tilePrefabProp.objectReferenceValue = tilePrefab;
            }
            else
            {
                Debug.LogWarning($"[SceneConfigurationUtility] _tilePrefab property not found on {viewType.Name}");
            }

            // _boardParent を設定
            SerializedProperty boardParentProp = serializedView.FindProperty("_boardParent");
            if (boardParentProp != null)
            {
                boardParentProp.objectReferenceValue = boardParent;
            }
            else
            {
                Debug.LogWarning($"[SceneConfigurationUtility] _boardParent property not found on {viewType.Name}");
            }

            // _player1Sprite を設定
            SerializedProperty player1SpriteProp = serializedView.FindProperty("_player1Sprite");
            if (player1SpriteProp != null && player1Sprite != null)
            {
                player1SpriteProp.objectReferenceValue = player1Sprite;
            }

            // _player2Sprite を設定
            SerializedProperty player2SpriteProp = serializedView.FindProperty("_player2Sprite");
            if (player2SpriteProp != null && player2Sprite != null)
            {
                player2SpriteProp.objectReferenceValue = player2Sprite;
            }

            // 変更を適用
            serializedView.ApplyModifiedProperties();

            // シーンを保存
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[SceneConfigurationUtility] {scene.name} configured successfully!");
        }

        /// <summary>
        /// シーン内からGameObjectを名前で検索
        /// </summary>
        private static GameObject? FindGameObjectInScene(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                // ルートオブジェクト自体をチェック
                if (rootObject.name == name)
                    return rootObject;

                // 子オブジェクトを再帰的に検索
                GameObject? found = FindInChildren(rootObject.transform, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Transform階層を再帰的に検索
        /// </summary>
        private static GameObject? FindInChildren(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child.gameObject;

                GameObject? found = FindInChildren(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        #endregion

        #region Validation

        [MenuItem(MENU_ROOT + "Validate All Scene Configurations")]
        public static void ValidateAllSceneConfigurations()
        {
            bool allValid = true;

            allValid &= ValidateSceneConfiguration("Assets/Scenes/TicTacToeHex.unity", "TicTacToeHexGame/View", typeof(TicTacToeHexView));
            allValid &= ValidateSceneConfiguration("Assets/Scenes/HexReversi.unity", "HexReversiGame/View", typeof(HexReversiView));
            allValid &= ValidateSceneConfiguration("Assets/Scenes/HexCheckers.unity", "HexCheckersGame/View", typeof(HexCheckersView));
            allValid &= ValidateSceneConfiguration("Assets/Scenes/HexChess.unity", "HexChessGame/View", typeof(HexChessView));

            if (allValid)
            {
                Debug.Log("[SceneConfigurationUtility] ✅ All scene configurations are valid!");
            }
            else
            {
                Debug.LogWarning("[SceneConfigurationUtility] ⚠️ Some scenes have configuration issues. Run 'Configure All Minigame Scenes' to fix.");
            }
        }

        /// <summary>
        /// シーン設定を検証
        /// </summary>
        private static bool ValidateSceneConfiguration(string scenePath, string viewPath, System.Type viewType)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            bool isValid = true;

            GameObject? viewObject = FindGameObjectInScene(scene, "View");
            if (viewObject == null)
            {
                Debug.LogWarning($"[SceneConfigurationUtility] {scene.name}: View GameObject not found");
                EditorSceneManager.CloseScene(scene, true);
                return false;
            }

            var viewComponent = viewObject.GetComponent(viewType);
            if (viewComponent == null)
            {
                Debug.LogWarning($"[SceneConfigurationUtility] {scene.name}: {viewType.Name} component not found");
                EditorSceneManager.CloseScene(scene, true);
                return false;
            }

            SerializedObject serializedView = new SerializedObject(viewComponent);

            // _tilePrefab をチェック
            SerializedProperty tilePrefabProp = serializedView.FindProperty("_tilePrefab");
            if (tilePrefabProp == null || tilePrefabProp.objectReferenceValue == null)
            {
                Debug.LogWarning($"[SceneConfigurationUtility] {scene.name}: _tilePrefab is not assigned");
                isValid = false;
            }

            // _boardParent をチェック
            SerializedProperty boardParentProp = serializedView.FindProperty("_boardParent");
            if (boardParentProp == null || boardParentProp.objectReferenceValue == null)
            {
                Debug.LogWarning($"[SceneConfigurationUtility] {scene.name}: _boardParent is not assigned");
                isValid = false;
            }

            // _player1Sprite をチェック
            SerializedProperty player1SpriteProp = serializedView.FindProperty("_player1Sprite");
            if (player1SpriteProp == null || player1SpriteProp.objectReferenceValue == null)
            {
                Debug.LogWarning($"[SceneConfigurationUtility] {scene.name}: _player1Sprite is not assigned");
                isValid = false;
            }

            // _player2Sprite をチェック
            SerializedProperty player2SpriteProp = serializedView.FindProperty("_player2Sprite");
            if (player2SpriteProp == null || player2SpriteProp.objectReferenceValue == null)
            {
                Debug.LogWarning($"[SceneConfigurationUtility] {scene.name}: _player2Sprite is not assigned");
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log($"[SceneConfigurationUtility] {scene.name}: ✅ Configuration valid");
            }

            EditorSceneManager.CloseScene(scene, true);
            return isValid;
        }

        #endregion
    }
}
