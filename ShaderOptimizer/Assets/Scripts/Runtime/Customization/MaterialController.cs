#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Customization
{
    /// <summary>
    /// マテリアル制御クラス
    /// </summary>
    /// <remarks>
    /// キャラクターのマテリアルプロパティを一元管理
    /// カラーカスタマイズ、テクスチャスワップを担当
    /// </remarks>
    public class MaterialController : MonoBehaviour
    {
        /// <summary>制御対象のRenderer</summary>
        [SerializeField] private Renderer? _targetRenderer;

        /// <summary>マテリアルインスタンス（動的生成）</summary>
        private Material? _materialInstance;

        /// <summary>元のマテリアル（リセット用）</summary>
        private Material? _originalMaterial;

        /// <summary>シェーダープロパティキャッシュ</summary>
        private Dictionary<string, int> _propertyCache = new();

        // シェーダープロパティ名（Unity-Chan Toon Shader互換）
        private static class ShaderProperties
        {
            public static readonly string BaseColor = "_BaseColor";
            public static readonly string HairColor = "_HairColor";
            public static readonly string SkinColor = "_SkinColor";
            public static readonly string EyeColor = "_EyeColor";
            public static readonly string ClothColor = "_ClothColor";
            public static readonly string MainTex = "_MainTex";
            public static readonly string BumpMap = "_BumpMap";
            public static readonly string MetallicGlossMap = "_MetallicGlossMap";
            public static readonly string EmissionColor = "_EmissionColor";
        }

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }

            if (_targetRenderer != null)
            {
                _originalMaterial = _targetRenderer.sharedMaterial;
                CreateMaterialInstance();
                CacheShaderProperties();
            }
            else
            {
                Debug.LogError("[MaterialController] Renderer not found!");
            }
        }

        /// <summary>
        /// マテリアルインスタンスを作成
        /// </summary>
        private void CreateMaterialInstance()
        {
            if (_originalMaterial != null && _targetRenderer != null)
            {
                _materialInstance = new Material(_originalMaterial);
                _targetRenderer.material = _materialInstance;
            }
        }

        /// <summary>
        /// シェーダープロパティをキャッシュ
        /// </summary>
        private void CacheShaderProperties()
        {
            _propertyCache[ShaderProperties.BaseColor] = Shader.PropertyToID(ShaderProperties.BaseColor);
            _propertyCache[ShaderProperties.HairColor] = Shader.PropertyToID(ShaderProperties.HairColor);
            _propertyCache[ShaderProperties.SkinColor] = Shader.PropertyToID(ShaderProperties.SkinColor);
            _propertyCache[ShaderProperties.EyeColor] = Shader.PropertyToID(ShaderProperties.EyeColor);
            _propertyCache[ShaderProperties.ClothColor] = Shader.PropertyToID(ShaderProperties.ClothColor);
            _propertyCache[ShaderProperties.MainTex] = Shader.PropertyToID(ShaderProperties.MainTex);
            _propertyCache[ShaderProperties.BumpMap] = Shader.PropertyToID(ShaderProperties.BumpMap);
            _propertyCache[ShaderProperties.EmissionColor] = Shader.PropertyToID(ShaderProperties.EmissionColor);
        }

        /// <summary>
        /// 髪色を設定
        /// </summary>
        public void SetHairColor(Color color)
        {
            SetColor(ShaderProperties.HairColor, color);
        }

        /// <summary>
        /// 肌色を設定
        /// </summary>
        public void SetSkinColor(Color color)
        {
            SetColor(ShaderProperties.SkinColor, color);
        }

        /// <summary>
        /// 瞳色を設定
        /// </summary>
        public void SetEyeColor(Color color)
        {
            SetColor(ShaderProperties.EyeColor, color);
        }

        /// <summary>
        /// 衣装色を設定
        /// </summary>
        public void SetClothColor(Color color)
        {
            SetColor(ShaderProperties.ClothColor, color);
        }

        /// <summary>
        /// ベースカラーを設定
        /// </summary>
        public void SetBaseColor(Color color)
        {
            SetColor(ShaderProperties.BaseColor, color);
        }

        /// <summary>
        /// カラープロパティを設定（汎用）
        /// </summary>
        public void SetColor(string propertyName, Color color)
        {
            if (_materialInstance == null)
            {
                Debug.LogWarning("[MaterialController] Material instance is null!");
                return;
            }

            if (_propertyCache.TryGetValue(propertyName, out int propertyId))
            {
                _materialInstance.SetColor(propertyId, color);
            }
            else
            {
                _materialInstance.SetColor(propertyName, color);
            }
        }

        /// <summary>
        /// テクスチャを設定
        /// </summary>
        public void SetTexture(string propertyName, Texture? texture)
        {
            if (_materialInstance == null)
            {
                Debug.LogWarning("[MaterialController] Material instance is null!");
                return;
            }

            if (_propertyCache.TryGetValue(propertyName, out int propertyId))
            {
                _materialInstance.SetTexture(propertyId, texture);
            }
            else
            {
                _materialInstance.SetTexture(propertyName, texture);
            }
        }

        /// <summary>
        /// メインテクスチャを設定
        /// </summary>
        public void SetMainTexture(Texture? texture)
        {
            SetTexture(ShaderProperties.MainTex, texture);
        }

        /// <summary>
        /// ノーマルマップを設定
        /// </summary>
        public void SetNormalMap(Texture? texture)
        {
            SetTexture(ShaderProperties.BumpMap, texture);
        }

        /// <summary>
        /// Floatプロパティを設定
        /// </summary>
        public void SetFloat(string propertyName, float value)
        {
            if (_materialInstance == null)
            {
                Debug.LogWarning("[MaterialController] Material instance is null!");
                return;
            }

            _materialInstance.SetFloat(propertyName, value);
        }

        /// <summary>
        /// カラープロパティを取得
        /// </summary>
        public Color GetColor(string propertyName)
        {
            if (_materialInstance == null)
            {
                return Color.white;
            }

            if (_propertyCache.TryGetValue(propertyName, out int propertyId))
            {
                return _materialInstance.GetColor(propertyId);
            }

            return _materialInstance.GetColor(propertyName);
        }

        /// <summary>
        /// マテリアルをリセット
        /// </summary>
        public void ResetMaterial()
        {
            if (_targetRenderer != null && _originalMaterial != null)
            {
                if (_materialInstance != null)
                {
                    Destroy(_materialInstance);
                }

                CreateMaterialInstance();
            }
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                Destroy(_materialInstance);
            }
        }
    }
}
