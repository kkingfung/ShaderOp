#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShaderOp.Core;
using ShaderOp.Customization;

namespace ShaderOp.Editor
{
    /// <summary>
    /// MainCustomizationシーンビルダー
    /// </summary>
    /// <remarks>
    /// キャラクターカスタマイズシーンの自動構築
    /// </remarks>
    public static class MainCustomizationSceneBuilder
    {
        [MenuItem("ShaderOp/Setup/Build MainCustomization Scene")]
        public static void BuildMainCustomizationScene()
        {
            // 現在のシーンがMainCustomizationか確認
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "MainCustomization")
            {
                Debug.LogWarning("[MainCustomizationSceneBuilder] Please load MainCustomization scene first!");
                return;
            }

            // カメラセットアップ
            SetupCamera();

            // ライティングセットアップ
            SetupLighting();

            // プレースホルダーキャラクター作成
            CreatePlaceholderCharacter();

            // OrbitCameraController追加
            AddOrbitCameraController();

            // CharacterCustomizer追加
            AddCharacterCustomizer();

            // CharacterCustomizationUI追加
            AddCharacterCustomizationUI();

            // シーンを保存
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[MainCustomizationSceneBuilder] MainCustomization scene built successfully!");
        }

        /// <summary>
        /// カメラをセットアップ
        /// </summary>
        private static void SetupCamera()
        {
            Camera? mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            // カメラ位置設定（キャラクターを見やすい位置）
            mainCamera.transform.position = new Vector3(0, 1.5f, -3f);
            mainCamera.transform.rotation = Quaternion.Euler(10, 0, 0);

            // カメラ設定
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            mainCamera.fieldOfView = 45f;
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 100f;

            Debug.Log("[MainCustomizationSceneBuilder] Camera configured");
        }

        /// <summary>
        /// ライティングをセットアップ
        /// </summary>
        private static void SetupLighting()
        {
            // Directional Light確認
            Light? directionalLight = Object.FindFirstObjectByType<Light>();
            if (directionalLight == null || directionalLight.type != LightType.Directional)
            {
                GameObject lightObj = new GameObject("Directional Light");
                directionalLight = lightObj.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
            }

            // ライト設定（トゥーンシェーダーに適した設定）
            directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            directionalLight.color = new Color(1f, 0.98f, 0.95f); // 温かみのある白
            directionalLight.intensity = 1.2f;
            directionalLight.shadows = LightShadows.Soft;

            // 環境光設定
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);

            Debug.Log("[MainCustomizationSceneBuilder] Lighting configured");
        }

        /// <summary>
        /// プレースホルダーキャラクターを作成
        /// </summary>
        private static GameObject CreatePlaceholderCharacter()
        {
            // 既存のCharacterModelを探す
            GameObject? existing = GameObject.Find("CharacterModel");
            if (existing != null)
            {
                Debug.Log("[MainCustomizationSceneBuilder] Using existing CharacterModel");
                return existing;
            }

            // プレースホルダーキャラクター作成
            GameObject characterObj = new GameObject("CharacterModel");
            characterObj.transform.position = Vector3.zero;
            characterObj.transform.rotation = Quaternion.identity;

            // ボディパーツ作成（Cubeでプレースホルダー）
            CreateBodyPart(characterObj, "Head", new Vector3(0, 1.7f, 0), new Vector3(0.3f, 0.3f, 0.3f));
            CreateBodyPart(characterObj, "Body", new Vector3(0, 1.0f, 0), new Vector3(0.5f, 0.7f, 0.3f));
            CreateBodyPart(characterObj, "LeftArm", new Vector3(-0.4f, 1.1f, 0), new Vector3(0.15f, 0.6f, 0.15f));
            CreateBodyPart(characterObj, "RightArm", new Vector3(0.4f, 1.1f, 0), new Vector3(0.15f, 0.6f, 0.15f));
            CreateBodyPart(characterObj, "LeftLeg", new Vector3(-0.15f, 0.4f, 0), new Vector3(0.18f, 0.8f, 0.18f));
            CreateBodyPart(characterObj, "RightLeg", new Vector3(0.15f, 0.4f, 0), new Vector3(0.18f, 0.8f, 0.18f));

            Debug.Log("[MainCustomizationSceneBuilder] Created placeholder character");
            return characterObj;
        }

        /// <summary>
        /// ボディパーツを作成
        /// </summary>
        private static void CreateBodyPart(GameObject parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;

            // MeshRendererにデフォルトマテリアル設定
            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // 後でCharacterBase shaderを使ったマテリアルに差し替え可能
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.8f, 0.7f, 0.6f); // 肌色っぽい色
                renderer.sharedMaterial = mat;
            }
        }

        /// <summary>
        /// OrbitCameraControllerを追加
        /// </summary>
        private static void AddOrbitCameraController()
        {
            Camera? mainCamera = Camera.main;
            if (mainCamera == null) return;

            OrbitCameraController? controller = mainCamera.GetComponent<OrbitCameraController>();
            if (controller == null)
            {
                controller = mainCamera.gameObject.AddComponent<OrbitCameraController>();
                Debug.Log("[MainCustomizationSceneBuilder] Added OrbitCameraController");
            }

            // ターゲットをCharacterModelに設定
            GameObject? characterModel = GameObject.Find("CharacterModel");
            if (characterModel != null)
            {
                // SerializedObjectでprivateフィールドを設定
                SerializedObject so = new SerializedObject(controller);
                SerializedProperty targetProp = so.FindProperty("_target");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = characterModel.transform;
                    so.ApplyModifiedProperties();
                    Debug.Log("[MainCustomizationSceneBuilder] Set camera target to CharacterModel");
                }
            }
        }

        /// <summary>
        /// CharacterCustomizerを追加
        /// </summary>
        private static void AddCharacterCustomizer()
        {
            GameObject? characterModel = GameObject.Find("CharacterModel");
            if (characterModel == null) return;

            CharacterCustomizer? customizer = characterModel.GetComponent<CharacterCustomizer>();
            if (customizer == null)
            {
                customizer = characterModel.AddComponent<CharacterCustomizer>();
                Debug.Log("[MainCustomizationSceneBuilder] Added CharacterCustomizer");
            }

            // CharacterModelへの参照を設定
            SerializedObject so = new SerializedObject(customizer);
            SerializedProperty modelProp = so.FindProperty("_characterModel");
            if (modelProp != null)
            {
                modelProp.objectReferenceValue = characterModel;
                so.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// CharacterCustomizationUIを追加
        /// </summary>
        private static void AddCharacterCustomizationUI()
        {
            // UIはCanvas上に作成する必要があるため、
            // ここではGameObjectのみ作成
            GameObject? uiObj = GameObject.Find("CharacterCustomizationUI");
            if (uiObj == null)
            {
                uiObj = new GameObject("CharacterCustomizationUI");
                uiObj.AddComponent<CharacterCustomizationUI>();
                Debug.Log("[MainCustomizationSceneBuilder] Created CharacterCustomizationUI GameObject");
            }

            // CharacterCustomizerへの参照を設定
            CharacterCustomizationUI? ui = uiObj.GetComponent<CharacterCustomizationUI>();
            GameObject? characterModel = GameObject.Find("CharacterModel");

            if (ui != null && characterModel != null)
            {
                CharacterCustomizer? customizer = characterModel.GetComponent<CharacterCustomizer>();
                if (customizer != null)
                {
                    SerializedObject so = new SerializedObject(ui);
                    SerializedProperty customizerProp = so.FindProperty("_customizer");
                    if (customizerProp != null)
                    {
                        customizerProp.objectReferenceValue = customizer;
                        so.ApplyModifiedProperties();
                        Debug.Log("[MainCustomizationSceneBuilder] Linked UI to Customizer");
                    }
                }
            }

            Debug.Log("[MainCustomizationSceneBuilder] Note: UI sliders/buttons need to be created manually or via UI Toolkit");
        }
    }
}
