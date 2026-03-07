#nullable enable

using UnityEngine;

namespace ShaderOp.Shaders
{
    /// <summary>
    /// 2Dアバタースプライトのシェーダー制御
    /// SG_Avatar_2D.shadergraph と連携
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Avatar2DShaderController : MonoBehaviour
    {
        private Material? _avatarMaterial;
        private SpriteRenderer? _spriteRenderer;

        [Header("デフォルトカラー")]
        [SerializeField] private Color _defaultHairColor = new Color(0.3f, 0.2f, 0.1f); // 茶色
        [SerializeField] private Color _defaultClothingColor = new Color(0.8f, 0.2f, 0.2f); // 赤

        [Header("リムライト設定")]
        [SerializeField, Range(0.5f, 5f)] private float _rimPower = 2.0f;
        [SerializeField, Range(0f, 1f)] private float _rimIntensity = 0.3f;
        [SerializeField] private Color _rimColor = Color.white;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null && _spriteRenderer.sharedMaterial != null)
            {
                // マテリアルをインスタンス化
                _avatarMaterial = _spriteRenderer.material;

                // デフォルト設定を適用
                ApplyDefaultSettings();
            }
            else
            {
                Debug.LogError($"[Avatar2DShaderController] SpriteRenderer または Material が見つかりません: {gameObject.name}");
            }
        }

        /// <summary>
        /// デフォルト設定を適用
        /// </summary>
        private void ApplyDefaultSettings()
        {
            if (_avatarMaterial == null) return;

            SetHairColor(_defaultHairColor);
            SetClothingColor(_defaultClothingColor);
            SetRimLight(_rimColor, _rimPower, _rimIntensity);
        }

        /// <summary>
        /// 髪の色を設定
        /// </summary>
        /// <param name="color">髪の色</param>
        public void SetHairColor(Color color)
        {
            if (_avatarMaterial == null) return;

            _avatarMaterial.SetColor("_HairColor", color);
        }

        /// <summary>
        /// 服の色を設定
        /// </summary>
        /// <param name="color">服の色</param>
        public void SetClothingColor(Color color)
        {
            if (_avatarMaterial == null) return;

            _avatarMaterial.SetColor("_ClothingColor", color);
        }

        /// <summary>
        /// 髪と服の色を同時に設定（カスタマイズ用）
        /// </summary>
        /// <param name="hairColor">髪の色</param>
        /// <param name="clothingColor">服の色</param>
        public void SetCustomColors(Color hairColor, Color clothingColor)
        {
            SetHairColor(hairColor);
            SetClothingColor(clothingColor);
        }

        /// <summary>
        /// リムライト設定
        /// </summary>
        /// <param name="color">リムライトの色</param>
        /// <param name="power">リムライトの鋭さ（0.5〜5）</param>
        /// <param name="intensity">リムライトの強度（0〜1）</param>
        public void SetRimLight(Color color, float power, float intensity)
        {
            if (_avatarMaterial == null) return;

            _avatarMaterial.SetColor("_RimColor", color);
            _avatarMaterial.SetFloat("_RimPower", Mathf.Clamp(power, 0.5f, 5f));
            _avatarMaterial.SetFloat("_RimIntensity", Mathf.Clamp01(intensity));
        }

        /// <summary>
        /// アルファクリッピング閾値を設定
        /// </summary>
        /// <param name="cutoff">アルファ閾値（0〜1）</param>
        public void SetAlphaCutoff(float cutoff)
        {
            if (_avatarMaterial == null) return;

            _avatarMaterial.SetFloat("_AlphaCutoff", Mathf.Clamp01(cutoff));
        }

        /// <summary>
        /// カラーマスクテクスチャを設定
        /// </summary>
        /// <param name="maskTexture">カラーマスクテクスチャ</param>
        public void SetColorMask(Texture2D maskTexture)
        {
            if (_avatarMaterial == null) return;

            if (maskTexture != null)
            {
                _avatarMaterial.SetTexture("_ColorMask", maskTexture);
            }
            else
            {
                Debug.LogWarning($"[Avatar2DShaderController] カラーマスクテクスチャが null です: {gameObject.name}");
            }
        }

        /// <summary>
        /// スプライトテクスチャを設定
        /// </summary>
        /// <param name="sprite">アバタースプライト</param>
        public void SetSprite(Sprite sprite)
        {
            if (_spriteRenderer == null) return;

            if (sprite != null)
            {
                _spriteRenderer.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[Avatar2DShaderController] スプライトが null です: {gameObject.name}");
            }
        }

        void OnDestroy()
        {
            // マテリアルインスタンスを破棄
            if (_avatarMaterial != null)
            {
                Destroy(_avatarMaterial);
                _avatarMaterial = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でのテスト用メソッド
        /// </summary>
        [ContextMenu("Test: Set Hair Color (Blonde)")]
        private void TestBlondeHair()
        {
            SetHairColor(new Color(1f, 0.9f, 0.5f));
        }

        [ContextMenu("Test: Set Hair Color (Black)")]
        private void TestBlackHair()
        {
            SetHairColor(new Color(0.1f, 0.1f, 0.1f));
        }

        [ContextMenu("Test: Set Clothing Color (Blue)")]
        private void TestBlueClothing()
        {
            SetClothingColor(Color.blue);
        }

        [ContextMenu("Test: Set Clothing Color (Green)")]
        private void TestGreenClothing()
        {
            SetClothingColor(Color.green);
        }

        [ContextMenu("Test: Random Customization")]
        private void TestRandomCustomization()
        {
            Color randomHair = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.3f, 1f);
            Color randomClothing = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            SetCustomColors(randomHair, randomClothing);
        }

        [ContextMenu("Test: Toggle Rim Light")]
        private void TestToggleRimLight()
        {
            float currentIntensity = _avatarMaterial?.GetFloat("_RimIntensity") ?? 0f;
            SetRimLight(_rimColor, _rimPower, currentIntensity > 0.1f ? 0f : _rimIntensity);
        }
#endif
    }
}
