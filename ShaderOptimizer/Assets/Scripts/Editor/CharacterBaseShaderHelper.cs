#nullable enable

using UnityEditor;
using UnityEngine;

namespace ShaderOp.Editor
{
    /// <summary>
    /// CharacterBase シェーダー用エディターヘルパー
    /// </summary>
    /// <remarks>
    /// テストマテリアルの作成とプリセット適用を提供
    /// </remarks>
    public static class CharacterBaseShaderHelper
    {
        private const string ShaderPath = "ShaderOp/Character/CharacterBase";
        private const string MaterialSavePath = "Assets/Materials/Character/";

        /// <summary>
        /// テストマテリアルを作成
        /// </summary>
        [MenuItem("ShaderOp/Create Material/Character Base Material")]
        public static void CreateTestMaterial()
        {
            // シェーダーを取得
            Shader? shader = Shader.Find(ShaderPath);
            if (shader == null)
            {
                Debug.LogError($"[CharacterBaseShaderHelper] Shader not found: {ShaderPath}");
                return;
            }

            // マテリアル作成
            Material material = new Material(shader);
            material.name = "CharacterBase_Test";

            // デフォルト設定を適用
            ApplyDefaultSettings(material);

            // アセットとして保存
            if (!AssetDatabase.IsValidFolder(MaterialSavePath))
            {
                string parentFolder = "Assets/Materials";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Materials");
                }
                AssetDatabase.CreateFolder(parentFolder, "Character");
            }

            string assetPath = $"{MaterialSavePath}CharacterBase_Test.mat";
            AssetDatabase.CreateAsset(material, assetPath);
            AssetDatabase.SaveAssets();

            // 作成したマテリアルを選択
            EditorGUIUtility.PingObject(material);
            Selection.activeObject = material;

            Debug.Log($"[CharacterBaseShaderHelper] Created test material: {assetPath}");
        }

        /// <summary>
        /// アニメ風プリセットを作成
        /// </summary>
        [MenuItem("ShaderOp/Create Material/Character Base (Anime Style)")]
        public static void CreateAnimeStyleMaterial()
        {
            Shader? shader = Shader.Find(ShaderPath);
            if (shader == null) return;

            Material material = new Material(shader);
            material.name = "CharacterBase_Anime";

            // アニメ風設定
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_ShadowColor", new Color(0.65f, 0.65f, 0.7f));
            material.SetFloat("_ShadowThreshold", 0.6f);
            material.SetFloat("_ShadowSmoothness", 0.02f);
            material.SetColor("_RimColor", new Color(1f, 0.95f, 0.9f));
            material.SetFloat("_RimPower", 4.0f);
            material.SetFloat("_RimIntensity", 0.7f);

            // カスタマイズカラー
            material.SetColor("_HairColor", new Color(0.2f, 0.1f, 0.05f));
            material.SetColor("_SkinColor", new Color(1.0f, 0.87f, 0.78f));
            material.SetColor("_EyeColor", new Color(0.3f, 0.5f, 1.0f));
            material.SetColor("_ClothColor", new Color(0.8f, 0.2f, 0.2f));

            SaveMaterial(material, "CharacterBase_Anime.mat");
        }

        /// <summary>
        /// ソフトトゥーンプリセットを作成
        /// </summary>
        [MenuItem("ShaderOp/Create Material/Character Base (Soft Toon)")]
        public static void CreateSoftToonMaterial()
        {
            Shader? shader = Shader.Find(ShaderPath);
            if (shader == null) return;

            Material material = new Material(shader);
            material.name = "CharacterBase_SoftToon";

            // ソフトトゥーン設定
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_ShadowColor", new Color(0.75f, 0.75f, 0.75f));
            material.SetFloat("_ShadowThreshold", 0.5f);
            material.SetFloat("_ShadowSmoothness", 0.1f);
            material.SetColor("_RimColor", Color.white);
            material.SetFloat("_RimPower", 2.0f);
            material.SetFloat("_RimIntensity", 0.3f);

            // カスタマイズカラー
            material.SetColor("_HairColor", new Color(0.4f, 0.25f, 0.15f));
            material.SetColor("_SkinColor", new Color(0.95f, 0.82f, 0.73f));
            material.SetColor("_EyeColor", new Color(0.2f, 0.6f, 0.3f));
            material.SetColor("_ClothColor", new Color(0.3f, 0.3f, 0.9f));

            SaveMaterial(material, "CharacterBase_SoftToon.mat");
        }

        /// <summary>
        /// デフォルト設定を適用
        /// </summary>
        private static void ApplyDefaultSettings(Material material)
        {
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_ShadowColor", new Color(0.7f, 0.7f, 0.7f));
            material.SetFloat("_ShadowThreshold", 0.5f);
            material.SetFloat("_ShadowSmoothness", 0.05f);
            material.SetColor("_RimColor", Color.white);
            material.SetFloat("_RimPower", 3.0f);
            material.SetFloat("_RimIntensity", 0.5f);

            // カスタマイズカラー
            material.SetColor("_HairColor", new Color(0.2f, 0.1f, 0.05f));
            material.SetColor("_SkinColor", new Color(1.0f, 0.87f, 0.78f));
            material.SetColor("_EyeColor", new Color(0.3f, 0.5f, 1.0f));
            material.SetColor("_ClothColor", new Color(0.8f, 0.2f, 0.2f));
        }

        /// <summary>
        /// マテリアルを保存
        /// </summary>
        private static void SaveMaterial(Material material, string fileName)
        {
            if (!AssetDatabase.IsValidFolder(MaterialSavePath))
            {
                string parentFolder = "Assets/Materials";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Materials");
                }
                AssetDatabase.CreateFolder(parentFolder, "Character");
            }

            string assetPath = MaterialSavePath + fileName;
            AssetDatabase.CreateAsset(material, assetPath);
            AssetDatabase.SaveAssets();

            EditorGUIUtility.PingObject(material);
            Selection.activeObject = material;

            Debug.Log($"[CharacterBaseShaderHelper] Created material: {assetPath}");
        }

        /// <summary>
        /// 選択中のマテリアルにプリセットを適用
        /// </summary>
        [MenuItem("ShaderOp/Apply Preset/Anime Style", true)]
        private static bool ValidateApplyAnimePreset()
        {
            return Selection.activeObject is Material;
        }

        [MenuItem("ShaderOp/Apply Preset/Anime Style")]
        private static void ApplyAnimePreset()
        {
            Material? material = Selection.activeObject as Material;
            if (material == null) return;

            Undo.RecordObject(material, "Apply Anime Preset");

            material.SetColor("_ShadowColor", new Color(0.65f, 0.65f, 0.7f));
            material.SetFloat("_ShadowThreshold", 0.6f);
            material.SetFloat("_ShadowSmoothness", 0.02f);
            material.SetFloat("_RimPower", 4.0f);
            material.SetFloat("_RimIntensity", 0.7f);

            EditorUtility.SetDirty(material);
            Debug.Log("[CharacterBaseShaderHelper] Applied Anime Style preset");
        }

        /// <summary>
        /// 選択中のマテリアルにソフトトゥーンプリセットを適用
        /// </summary>
        [MenuItem("ShaderOp/Apply Preset/Soft Toon", true)]
        private static bool ValidateApplySoftToonPreset()
        {
            return Selection.activeObject is Material;
        }

        [MenuItem("ShaderOp/Apply Preset/Soft Toon")]
        private static void ApplySoftToonPreset()
        {
            Material? material = Selection.activeObject as Material;
            if (material == null) return;

            Undo.RecordObject(material, "Apply Soft Toon Preset");

            material.SetFloat("_ShadowThreshold", 0.5f);
            material.SetFloat("_ShadowSmoothness", 0.1f);
            material.SetFloat("_RimPower", 2.0f);
            material.SetFloat("_RimIntensity", 0.3f);

            EditorUtility.SetDirty(material);
            Debug.Log("[CharacterBaseShaderHelper] Applied Soft Toon preset");
        }
    }
}
