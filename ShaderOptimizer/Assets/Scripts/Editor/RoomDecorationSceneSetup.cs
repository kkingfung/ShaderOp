#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using ShaderOp.Core;
using ShaderOp.Customization;

namespace ShaderOp.Editor
{
    /// <summary>
    /// 部屋デコレーションシーン自動セットアップツール
    /// </summary>
    /// <remarks>
    /// メニュー: ShaderOp → Setup → RoomDecoration Cloth Showcase
    ///
    /// セットアップ内容:
    /// - 3D部屋ジオメトリ作成（床、壁）
    /// - 布シェーダーデモオブジェクト配置（カーテン、ラグ、クッション、テーブルクロス）
    /// - マテリアル自動割り当て（Cotton, Silk, Denim, Leather）
    /// - ライティング設定
    /// - OrbitCameraController セットアップ
    /// - UI Toolkit パネル設定
    /// </remarks>
    public static class RoomDecorationSceneSetup
    {
        private const string MENU_PATH = "ShaderOp/Setup/RoomDecoration Cloth Showcase";

        [MenuItem(MENU_PATH, false, 200)]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog(
                "RoomDecoration Scene Setup",
                "現在のシーンに部屋デコレーション用のオブジェクトを自動生成します。\n\n" +
                "以下が作成されます:\n" +
                "- 3D部屋（床、壁、天井）\n" +
                "- 布シェーダーデモオブジェクト（カーテン、ラグ、クッション、テーブルクロス）\n" +
                "- ライティング\n" +
                "- OrbitCameraController\n" +
                "- UI Toolkit パネル\n\n" +
                "続行しますか?",
                "Yes",
                "Cancel"))
            {
                return;
            }

            Debug.Log("[RoomDecorationSceneSetup] セットアップ開始...");

            // 1. ルートオブジェクト作成
            GameObject roomRoot = CreateRoomRoot();

            // 2. 3D部屋ジオメトリ作成
            CreateRoomGeometry(roomRoot);

            // 3. 布シェーダーデモオブジェクト作成
            GameObject curtainLeft, curtainRight, rug, cushion1, cushion2, cushion3, tableCloth;
            CreateClothObjects(roomRoot, out curtainLeft, out curtainRight, out rug, out cushion1, out cushion2, out cushion3, out tableCloth);

            // 4. ライティング設定
            CreateLighting();

            // 5. カメラ設定
            GameObject cameraObj = SetupCamera(roomRoot);

            // 6. UI Toolkit設定
            GameObject uiObj = SetupUIToolkit(curtainLeft, curtainRight, rug, cushion1, cushion2, cushion3, tableCloth, cameraObj);

            // 7. マテリアル読み込みと割り当て
            AssignMaterials(curtainLeft, curtainRight, rug, cushion1, cushion2, cushion3, tableCloth, uiObj);

            // 8. シーンを保存
            EditorUtility.SetDirty(SceneManager.GetActiveScene().GetRootGameObjects()[0]);
            AssetDatabase.SaveAssets();

            Debug.Log("[RoomDecorationSceneSetup] セットアップ完了!");
            EditorUtility.DisplayDialog("Success", "部屋デコレーションシーンのセットアップが完了しました!", "OK");
        }

        // ============================================
        // ルートオブジェクト作成
        // ============================================

        private static GameObject CreateRoomRoot()
        {
            GameObject root = GameObject.Find("Room");
            if (root == null)
            {
                root = new GameObject("Room");
            }

            root.transform.position = Vector3.zero;
            return root;
        }

        // ============================================
        // 3D部屋ジオメトリ作成
        // ============================================

        private static void CreateRoomGeometry(GameObject parent)
        {
            // Floor (50x50 Plane)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(parent.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(5f, 1f, 5f); // 50x50 units

            // Walls (Cubes stretched thin)
            CreateWall(parent, "WallNorth", new Vector3(0, 2.5f, 25f), new Vector3(50f, 5f, 1f));
            CreateWall(parent, "WallSouth", new Vector3(0, 2.5f, -25f), new Vector3(50f, 5f, 1f));
            CreateWall(parent, "WallEast", new Vector3(25f, 2.5f, 0), new Vector3(1f, 5f, 50f));
            CreateWall(parent, "WallWest", new Vector3(-25f, 2.5f, 0), new Vector3(1f, 5f, 50f));

            Debug.Log("[RoomDecorationSceneSetup] 部屋ジオメトリ作成完了");
        }

        private static void CreateWall(GameObject parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
        }

        // ============================================
        // 布シェーダーデモオブジェクト作成
        // ============================================

        private static void CreateClothObjects(
            GameObject parent,
            out GameObject curtainLeft,
            out GameObject curtainRight,
            out GameObject rug,
            out GameObject cushion1,
            out GameObject cushion2,
            out GameObject cushion3,
            out GameObject tableCloth)
        {
            // Curtains (Vertical Quads on North Wall)
            curtainLeft = CreateQuad("CurtainLeft", parent, new Vector3(-10f, 2.5f, 24f), new Vector3(3f, 4f, 0.1f));
            curtainRight = CreateQuad("CurtainRight", parent, new Vector3(10f, 2.5f, 24f), new Vector3(3f, 4f, 0.1f));

            // Rug (Horizontal Plane on Floor)
            rug = GameObject.CreatePrimitive(PrimitiveType.Plane);
            rug.name = "Rug";
            rug.transform.SetParent(parent.transform);
            rug.transform.localPosition = new Vector3(0, 0.05f, 5f);
            rug.transform.localScale = new Vector3(1.5f, 1f, 2f); // 15x20 units

            // Cushions (Small Cubes)
            cushion1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cushion1.name = "Cushion1";
            cushion1.transform.SetParent(parent.transform);
            cushion1.transform.localPosition = new Vector3(-3f, 0.5f, 5f);
            cushion1.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

            cushion2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cushion2.name = "Cushion2";
            cushion2.transform.SetParent(parent.transform);
            cushion2.transform.localPosition = new Vector3(0f, 0.5f, 5f);
            cushion2.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

            cushion3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cushion3.name = "Cushion3";
            cushion3.transform.SetParent(parent.transform);
            cushion3.transform.localPosition = new Vector3(3f, 0.5f, 5f);
            cushion3.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

            // Table Cloth (Plane draped over cube)
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.SetParent(parent.transform);
            table.transform.localPosition = new Vector3(0, 1f, -5f);
            table.transform.localScale = new Vector3(4f, 2f, 3f);

            tableCloth = GameObject.CreatePrimitive(PrimitiveType.Plane);
            tableCloth.name = "TableCloth";
            tableCloth.transform.SetParent(parent.transform);
            tableCloth.transform.localPosition = new Vector3(0, 2.05f, -5f);
            tableCloth.transform.localScale = new Vector3(0.5f, 1f, 0.4f);
            tableCloth.transform.localRotation = Quaternion.Euler(0, 0, 0);

            Debug.Log("[RoomDecorationSceneSetup] 布シェーダーデモオブジェクト作成完了");
        }

        private static GameObject CreateQuad(string name, GameObject parent, Vector3 position, Vector3 scale)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent.transform);
            quad.transform.localPosition = position;
            quad.transform.localScale = scale;
            return quad;
        }

        // ============================================
        // ライティング設定
        // ============================================

        private static void CreateLighting()
        {
            // Directional Light (Main)
            GameObject mainLight = GameObject.Find("Directional Light");
            if (mainLight == null)
            {
                mainLight = new GameObject("Directional Light");
                Light light = mainLight.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0);
            Light mainLightComponent = mainLight.GetComponent<Light>();
            if (mainLightComponent != null)
            {
                mainLightComponent.intensity = 1.2f;
                mainLightComponent.color = new Color(1f, 0.95f, 0.9f);
            }

            // Point Light 1 (Ambiance)
            CreatePointLight("PointLight1", new Vector3(-10f, 3f, 10f), 0.8f, new Color(1f, 0.9f, 0.8f), 15f);

            // Point Light 2 (Ambiance)
            CreatePointLight("PointLight2", new Vector3(10f, 3f, -10f), 0.7f, new Color(0.9f, 0.95f, 1f), 12f);

            Debug.Log("[RoomDecorationSceneSetup] ライティング設定完了");
        }

        private static void CreatePointLight(string name, Vector3 position, float intensity, Color color, float range)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.position = position;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.intensity = intensity;
            light.color = color;
            light.range = range;
        }

        // ============================================
        // カメラ設定
        // ============================================

        private static GameObject SetupCamera(GameObject roomRoot)
        {
            // Main Cameraを探す
            GameObject cameraObj = GameObject.Find("Main Camera");
            if (cameraObj == null)
            {
                cameraObj = new GameObject("Main Camera");
                cameraObj.tag = "MainCamera";
                cameraObj.AddComponent<Camera>();
                cameraObj.AddComponent<AudioListener>();
            }

            // カメラ位置設定
            cameraObj.transform.position = new Vector3(0, 3f, -15f);
            cameraObj.transform.rotation = Quaternion.Euler(10f, 0, 0);

            // OrbitCameraController追加
            OrbitCameraController? orbitController = cameraObj.GetComponent<OrbitCameraController>();
            if (orbitController == null)
            {
                orbitController = cameraObj.AddComponent<OrbitCameraController>();
            }

            // ターゲットを部屋の中心に設定
            GameObject target = new GameObject("CameraTarget");
            target.transform.SetParent(roomRoot.transform);
            target.transform.localPosition = new Vector3(0, 2f, 0);

            // SerializedObjectでプライベートフィールドを設定
            SerializedObject so = new SerializedObject(orbitController);
            so.FindProperty("_target").objectReferenceValue = target.transform;
            so.FindProperty("_initialDistance").floatValue = 15f;
            so.FindProperty("_minDistance").floatValue = 5f;
            so.FindProperty("_maxDistance").floatValue = 30f;
            so.FindProperty("_autoRotate").boolValue = true;
            so.FindProperty("_autoRotateSpeed").floatValue = 5f;
            so.ApplyModifiedProperties();

            Debug.Log("[RoomDecorationSceneSetup] カメラ設定完了");
            return cameraObj;
        }

        // ============================================
        // UI Toolkit設定
        // ============================================

        private static GameObject SetupUIToolkit(
            GameObject curtainLeft,
            GameObject curtainRight,
            GameObject rug,
            GameObject cushion1,
            GameObject cushion2,
            GameObject cushion3,
            GameObject tableCloth,
            GameObject cameraObj)
        {
            // UI GameObject作成
            GameObject uiObj = new GameObject("RoomDecorationUI");
            UIDocument uiDocument = uiObj.AddComponent<UIDocument>();

            // UXMLアセットを読み込み
            VisualTreeAsset? uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/RoomDecoration.uxml");
            if (uxmlAsset != null)
            {
                uiDocument.visualTreeAsset = uxmlAsset;
            }
            else
            {
                Debug.LogWarning("[RoomDecorationSceneSetup] RoomDecoration.uxml が見つかりません");
            }

            // RoomDecoratorController追加
            RoomDecoratorController controller = uiObj.AddComponent<RoomDecoratorController>();

            // SerializedObjectでフィールドを設定
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_curtainLeft").objectReferenceValue = curtainLeft;
            so.FindProperty("_curtainRight").objectReferenceValue = curtainRight;
            so.FindProperty("_rug").objectReferenceValue = rug;
            so.FindProperty("_cushion1").objectReferenceValue = cushion1;
            so.FindProperty("_cushion2").objectReferenceValue = cushion2;
            so.FindProperty("_cushion3").objectReferenceValue = cushion3;
            so.FindProperty("_tableCloth").objectReferenceValue = tableCloth;
            so.FindProperty("_orbitCamera").objectReferenceValue = cameraObj.GetComponent<OrbitCameraController>();
            so.ApplyModifiedProperties();

            Debug.Log("[RoomDecorationSceneSetup] UI Toolkit設定完了");
            return uiObj;
        }

        // ============================================
        // マテリアル割り当て
        // ============================================

        private static void AssignMaterials(
            GameObject curtainLeft,
            GameObject curtainRight,
            GameObject rug,
            GameObject cushion1,
            GameObject cushion2,
            GameObject cushion3,
            GameObject tableCloth,
            GameObject uiObj)
        {
            // マテリアルを読み込み
            Material? cottonMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Cotton_New.mat");
            Material? silkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Silk_New.mat");
            Material? denimMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Denim_New.mat");
            Material? leatherMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cloth/MAT_Leather_New.mat");

            // マテリアル割り当て
            if (cottonMat != null && silkMat != null && denimMat != null && leatherMat != null)
            {
                // Curtains (Silk)
                AssignMaterial(curtainLeft, silkMat);
                AssignMaterial(curtainRight, silkMat);

                // Rug (Cotton)
                AssignMaterial(rug, cottonMat);

                // Cushions (Denim)
                AssignMaterial(cushion1, denimMat);
                AssignMaterial(cushion2, denimMat);
                AssignMaterial(cushion3, denimMat);

                // Table Cloth (Leather)
                AssignMaterial(tableCloth, leatherMat);

                // RoomDecoratorController にマテリアル参照を設定
                RoomDecoratorController controller = uiObj.GetComponent<RoomDecoratorController>();
                if (controller != null)
                {
                    SerializedObject so = new SerializedObject(controller);
                    so.FindProperty("_cottonMaterial").objectReferenceValue = cottonMat;
                    so.FindProperty("_silkMaterial").objectReferenceValue = silkMat;
                    so.FindProperty("_denimMaterial").objectReferenceValue = denimMat;
                    so.FindProperty("_leatherMaterial").objectReferenceValue = leatherMat;
                    so.ApplyModifiedProperties();
                }

                Debug.Log("[RoomDecorationSceneSetup] マテリアル割り当て完了");
            }
            else
            {
                Debug.LogWarning("[RoomDecorationSceneSetup] 一部のマテリアルが見つかりません");
            }
        }

        private static void AssignMaterial(GameObject obj, Material material)
        {
            Renderer? renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }
    }
}
