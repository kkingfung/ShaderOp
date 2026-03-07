#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShaderOp.Shaders
{
    /// <summary>
    /// シェーダー最適化ヘルパークラス
    /// </summary>
    /// <remarks>
    /// ランタイムでのシェーダー最適化とキーワード管理を提供
    /// </remarks>
    public static class ShaderOptimizationHelper
    {
        /// <summary>グローバルシェーダーキーワードのキャッシュ</summary>
        private static readonly HashSet<string> _enabledGlobalKeywords = new HashSet<string>();

        /// <summary>
        /// グローバルシェーダーキーワードを有効化（キャッシュ付き）
        /// </summary>
        public static void EnableGlobalKeyword(string keyword)
        {
            if (_enabledGlobalKeywords.Add(keyword))
            {
                Shader.EnableKeyword(keyword);
                Debug.Log($"[ShaderOptimizationHelper] Enabled global keyword: {keyword}");
            }
        }

        /// <summary>
        /// グローバルシェーダーキーワードを無効化（キャッシュ付き）
        /// </summary>
        public static void DisableGlobalKeyword(string keyword)
        {
            if (_enabledGlobalKeywords.Remove(keyword))
            {
                Shader.DisableKeyword(keyword);
                Debug.Log($"[ShaderOptimizationHelper] Disabled global keyword: {keyword}");
            }
        }

        /// <summary>
        /// マテリアルのシェーダーキーワードを最適化
        /// </summary>
        public static void OptimizeMaterialKeywords(Material material)
        {
            if (material == null || material.shader == null)
                return;

            // 使用されていないキーワードを無効化
            string[] keywords = material.shaderKeywords;
            List<string> optimizedKeywords = new List<string>();

            foreach (string keyword in keywords)
            {
                // プロパティが実際に使用されているか確認
                if (IsKeywordUsed(material, keyword))
                {
                    optimizedKeywords.Add(keyword);
                }
                else
                {
                    Debug.Log($"[ShaderOptimizationHelper] Removing unused keyword: {keyword} from {material.name}");
                }
            }

            material.shaderKeywords = optimizedKeywords.ToArray();
        }

        /// <summary>
        /// キーワードが実際に使用されているか確認
        /// </summary>
        private static bool IsKeywordUsed(Material material, string keyword)
        {
            // 簡易実装: 常にtrueを返す
            // 実際のプロジェクトでは、シェーダープロパティの値をチェック
            return true;
        }

        /// <summary>
        /// マテリアルのプロパティをバッチ設定（最適化版）
        /// </summary>
        public static void SetMaterialPropertiesBatch(Material material, Dictionary<int, object> properties)
        {
            if (material == null)
                return;

            foreach (var kvp in properties)
            {
                int propertyId = kvp.Key;
                object value = kvp.Value;

                switch (value)
                {
                    case Color color:
                        material.SetColor(propertyId, color);
                        break;
                    case float floatValue:
                        material.SetFloat(propertyId, floatValue);
                        break;
                    case Vector4 vector:
                        material.SetVector(propertyId, vector);
                        break;
                    case Texture texture:
                        material.SetTexture(propertyId, texture);
                        break;
                }
            }
        }

        /// <summary>
        /// シェーダーのウォームアップ（初回ロード時のスタッター削減）
        /// </summary>
        public static void WarmupShader(Shader shader, params string[] keywords)
        {
            if (shader == null)
                return;

            // ShaderVariantCollectionを動的に生成
            ShaderVariantCollection collection = new ShaderVariantCollection();

            foreach (string keyword in keywords)
            {
                ShaderVariantCollection.ShaderVariant variant = new ShaderVariantCollection.ShaderVariant
                {
                    shader = shader,
                    passType = PassType.ForwardBase,
                    keywords = new[] { keyword }
                };

                collection.Add(variant);
            }

            collection.WarmUp();
            Debug.Log($"[ShaderOptimizationHelper] Warmed up shader: {shader.name} with {keywords.Length} variants");
        }

        /// <summary>
        /// すべてのマテリアルのシェーダーをウォームアップ
        /// </summary>
        public static void WarmupAllMaterials(Material[] materials)
        {
            if (materials == null || materials.Length == 0)
                return;

            HashSet<Shader> uniqueShaders = new HashSet<Shader>();

            foreach (Material mat in materials)
            {
                if (mat != null && mat.shader != null)
                {
                    uniqueShaders.Add(mat.shader);
                }
            }

            foreach (Shader shader in uniqueShaders)
            {
                WarmupShader(shader);
            }

            Debug.Log($"[ShaderOptimizationHelper] Warmed up {uniqueShaders.Count} unique shaders from {materials.Length} materials");
        }

        /// <summary>
        /// LOD（Level of Detail）に応じたシェーダーキーワード設定
        /// </summary>
        public static void SetLODKeywords(Material material, int lodLevel)
        {
            if (material == null)
                return;

            // LODレベルに応じてキーワードを切り替え
            switch (lodLevel)
            {
                case 0: // 最高品質
                    material.EnableKeyword("_DETAIL_MULX2");
                    material.EnableKeyword("_NORMALMAP");
                    material.EnableKeyword("_EMISSION");
                    break;

                case 1: // 中品質
                    material.DisableKeyword("_DETAIL_MULX2");
                    material.EnableKeyword("_NORMALMAP");
                    material.DisableKeyword("_EMISSION");
                    break;

                case 2: // 低品質
                    material.DisableKeyword("_DETAIL_MULX2");
                    material.DisableKeyword("_NORMALMAP");
                    material.DisableKeyword("_EMISSION");
                    break;
            }

            Debug.Log($"[ShaderOptimizationHelper] Set LOD {lodLevel} keywords for {material.name}");
        }

        /// <summary>
        /// モバイル向けシェーダー最適化
        /// </summary>
        public static void OptimizeForMobile(Material material)
        {
            if (material == null)
                return;

            // モバイルで不要な高品質機能を無効化
            material.DisableKeyword("_PARALLAXMAP");
            material.DisableKeyword("_DETAIL_MULX2");
            material.DisableKeyword("_TESSELLATION");

            // モバイル最適化フラグを有効化
            material.EnableKeyword("_MOBILE_OPTIMIZED");

            Debug.Log($"[ShaderOptimizationHelper] Optimized material for mobile: {material.name}");
        }

        /// <summary>
        /// シェーダープロパティIDのキャッシュ（GC削減）
        /// </summary>
        public static class PropertyIDs
        {
            // カラープロパティ
            public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
            public static readonly int Color = Shader.PropertyToID("_Color");
            public static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
            public static readonly int SpecColor = Shader.PropertyToID("_SpecColor");

            // テクスチャプロパティ
            public static readonly int MainTex = Shader.PropertyToID("_MainTex");
            public static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
            public static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
            public static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");

            // スカラープロパティ
            public static readonly int Metallic = Shader.PropertyToID("_Metallic");
            public static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
            public static readonly int BumpScale = Shader.PropertyToID("_BumpScale");
            public static readonly int OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");

            // カスタムプロパティ（ShaderOp固有）
            public static readonly int HairColor = Shader.PropertyToID("_HairColor");
            public static readonly int SkinColor = Shader.PropertyToID("_SkinColor");
            public static readonly int EyeColor = Shader.PropertyToID("_EyeColor");
            public static readonly int ClothColor = Shader.PropertyToID("_ClothColor");
        }

        /// <summary>
        /// マテリアルプロパティブロックのキャッシュプール
        /// </summary>
        private static readonly Stack<MaterialPropertyBlock> _propertyBlockPool = new Stack<MaterialPropertyBlock>();

        /// <summary>
        /// MaterialPropertyBlockをプールから取得（GC削減）
        /// </summary>
        public static MaterialPropertyBlock GetPropertyBlock()
        {
            if (_propertyBlockPool.Count > 0)
            {
                return _propertyBlockPool.Pop();
            }

            return new MaterialPropertyBlock();
        }

        /// <summary>
        /// MaterialPropertyBlockをプールに返却
        /// </summary>
        public static void ReleasePropertyBlock(MaterialPropertyBlock block)
        {
            if (block != null)
            {
                block.Clear();
                _propertyBlockPool.Push(block);
            }
        }

        /// <summary>
        /// グローバルシェーダープロパティの一括設定
        /// </summary>
        public static void SetGlobalPropertiesBatch(Dictionary<int, object> properties)
        {
            foreach (var kvp in properties)
            {
                int propertyId = kvp.Key;
                object value = kvp.Value;

                switch (value)
                {
                    case Color color:
                        Shader.SetGlobalColor(propertyId, color);
                        break;
                    case float floatValue:
                        Shader.SetGlobalFloat(propertyId, floatValue);
                        break;
                    case Vector4 vector:
                        Shader.SetGlobalVector(propertyId, vector);
                        break;
                    case Texture texture:
                        Shader.SetGlobalTexture(propertyId, texture);
                        break;
                }
            }
        }
    }
}
