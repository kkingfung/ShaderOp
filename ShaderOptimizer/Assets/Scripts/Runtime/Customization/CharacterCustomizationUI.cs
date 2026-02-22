#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShaderOp.Customization
{
    /// <summary>
    /// キャラクターカスタマイズUI制御
    /// </summary>
    /// <remarks>
    /// UIとCharacterCustomizerを接続
    /// ユーザー入力をカスタマイザーに伝達
    /// </remarks>
    public class CharacterCustomizationUI : MonoBehaviour
    {
        /// <summary>カスタマイザー参照</summary>
        [Header("Customizer")]
        [SerializeField] private CharacterCustomizer? _customizer;

        /// <summary>髪色スライダー（R, G, B）</summary>
        [Header("Hair Customization")]
        [SerializeField] private Slider? _hairColorRSlider;
        [SerializeField] private Slider? _hairColorGSlider;
        [SerializeField] private Slider? _hairColorBSlider;
        [SerializeField] private Image? _hairColorPreview;

        /// <summary>瞳色スライダー（R, G, B）</summary>
        [Header("Eye Customization")]
        [SerializeField] private Slider? _eyeColorRSlider;
        [SerializeField] private Slider? _eyeColorGSlider;
        [SerializeField] private Slider? _eyeColorBSlider;
        [SerializeField] private Image? _eyeColorPreview;

        /// <summary>肌色スライダー</summary>
        [Header("Skin Customization")]
        [SerializeField] private Slider? _skinToneSlider;
        [SerializeField] private TextMeshProUGUI? _skinToneText;

        /// <summary>トップスカラースライダー（R, G, B）</summary>
        [Header("Clothing Customization")]
        [SerializeField] private Slider? _topColorRSlider;
        [SerializeField] private Slider? _topColorGSlider;
        [SerializeField] private Slider? _topColorBSlider;
        [SerializeField] private Image? _topColorPreview;

        /// <summary>身長スライダー</summary>
        [Header("Body Customization")]
        [SerializeField] private Slider? _heightSlider;
        [SerializeField] private TextMeshProUGUI? _heightText;

        /// <summary>保存/リセットボタン</summary>
        [Header("Action Buttons")]
        [SerializeField] private Button? _saveButton;
        [SerializeField] private Button? _resetButton;

        private void Start()
        {
            if (_customizer == null)
            {
                _customizer = FindObjectOfType<CharacterCustomizer>();
            }

            SetupUIListeners();
            LoadInitialValues();
        }

        /// <summary>
        /// UIリスナーをセットアップ
        /// </summary>
        private void SetupUIListeners()
        {
            // 髪色
            if (_hairColorRSlider != null) _hairColorRSlider.onValueChanged.AddListener(_ => OnHairColorChanged());
            if (_hairColorGSlider != null) _hairColorGSlider.onValueChanged.AddListener(_ => OnHairColorChanged());
            if (_hairColorBSlider != null) _hairColorBSlider.onValueChanged.AddListener(_ => OnHairColorChanged());

            // 瞳色
            if (_eyeColorRSlider != null) _eyeColorRSlider.onValueChanged.AddListener(_ => OnEyeColorChanged());
            if (_eyeColorGSlider != null) _eyeColorGSlider.onValueChanged.AddListener(_ => OnEyeColorChanged());
            if (_eyeColorBSlider != null) _eyeColorBSlider.onValueChanged.AddListener(_ => OnEyeColorChanged());

            // 肌色
            if (_skinToneSlider != null) _skinToneSlider.onValueChanged.AddListener(OnSkinToneChanged);

            // トップスカラー
            if (_topColorRSlider != null) _topColorRSlider.onValueChanged.AddListener(_ => OnTopColorChanged());
            if (_topColorGSlider != null) _topColorGSlider.onValueChanged.AddListener(_ => OnTopColorChanged());
            if (_topColorBSlider != null) _topColorBSlider.onValueChanged.AddListener(_ => OnTopColorChanged());

            // 身長
            if (_heightSlider != null) _heightSlider.onValueChanged.AddListener(OnHeightChanged);

            // ボタン
            if (_saveButton != null) _saveButton.onClick.AddListener(OnSaveClicked);
            if (_resetButton != null) _resetButton.onClick.AddListener(OnResetClicked);
        }

        /// <summary>
        /// 初期値をロード
        /// </summary>
        private void LoadInitialValues()
        {
            if (_customizer == null) return;

            CharacterCustomizationData data = _customizer.GetCustomizationData();

            // 髪色
            if (_hairColorRSlider != null) _hairColorRSlider.value = data.HairColor.r;
            if (_hairColorGSlider != null) _hairColorGSlider.value = data.HairColor.g;
            if (_hairColorBSlider != null) _hairColorBSlider.value = data.HairColor.b;
            if (_hairColorPreview != null) _hairColorPreview.color = data.HairColor;

            // 瞳色
            if (_eyeColorRSlider != null) _eyeColorRSlider.value = data.EyeColor.r;
            if (_eyeColorGSlider != null) _eyeColorGSlider.value = data.EyeColor.g;
            if (_eyeColorBSlider != null) _eyeColorBSlider.value = data.EyeColor.b;
            if (_eyeColorPreview != null) _eyeColorPreview.color = data.EyeColor;

            // 肌色
            if (_skinToneSlider != null) _skinToneSlider.value = data.SkinToneId;
            if (_skinToneText != null) _skinToneText.text = $"Skin Tone: {data.SkinToneId}";

            // トップスカラー
            if (_topColorRSlider != null) _topColorRSlider.value = data.TopColor.r;
            if (_topColorGSlider != null) _topColorGSlider.value = data.TopColor.g;
            if (_topColorBSlider != null) _topColorBSlider.value = data.TopColor.b;
            if (_topColorPreview != null) _topColorPreview.color = data.TopColor;

            // 身長
            if (_heightSlider != null) _heightSlider.value = data.HeightScale;
            if (_heightText != null) _heightText.text = $"Height: {data.HeightScale:F2}";
        }

        /// <summary>
        /// 髪色変更時
        /// </summary>
        private void OnHairColorChanged()
        {
            if (_customizer == null || _hairColorRSlider == null || _hairColorGSlider == null || _hairColorBSlider == null)
                return;

            Color hairColor = new Color(_hairColorRSlider.value, _hairColorGSlider.value, _hairColorBSlider.value);
            _customizer.SetHairColor(hairColor);

            if (_hairColorPreview != null)
            {
                _hairColorPreview.color = hairColor;
            }
        }

        /// <summary>
        /// 瞳色変更時
        /// </summary>
        private void OnEyeColorChanged()
        {
            if (_customizer == null || _eyeColorRSlider == null || _eyeColorGSlider == null || _eyeColorBSlider == null)
                return;

            Color eyeColor = new Color(_eyeColorRSlider.value, _eyeColorGSlider.value, _eyeColorBSlider.value);
            _customizer.SetEyeColor(eyeColor);

            if (_eyeColorPreview != null)
            {
                _eyeColorPreview.color = eyeColor;
            }
        }

        /// <summary>
        /// 肌色変更時
        /// </summary>
        private void OnSkinToneChanged(float value)
        {
            if (_customizer == null) return;

            int skinToneId = Mathf.RoundToInt(value);
            _customizer.SetSkinTone(skinToneId);

            if (_skinToneText != null)
            {
                _skinToneText.text = $"Skin Tone: {skinToneId}";
            }
        }

        /// <summary>
        /// トップスカラー変更時
        /// </summary>
        private void OnTopColorChanged()
        {
            if (_customizer == null || _topColorRSlider == null || _topColorGSlider == null || _topColorBSlider == null)
                return;

            Color topColor = new Color(_topColorRSlider.value, _topColorGSlider.value, _topColorBSlider.value);
            _customizer.SetTopColor(topColor);

            if (_topColorPreview != null)
            {
                _topColorPreview.color = topColor;
            }
        }

        /// <summary>
        /// 身長変更時
        /// </summary>
        private void OnHeightChanged(float value)
        {
            if (_customizer == null) return;

            _customizer.SetHeightScale(value);

            if (_heightText != null)
            {
                _heightText.text = $"Height: {value:F2}";
            }
        }

        /// <summary>
        /// 保存ボタンクリック時
        /// </summary>
        private void OnSaveClicked()
        {
            if (_customizer != null)
            {
                _customizer.SaveCustomization();
                Debug.Log("[CharacterCustomizationUI] Customization saved!");
            }
        }

        /// <summary>
        /// リセットボタンクリック時
        /// </summary>
        private void OnResetClicked()
        {
            if (_customizer != null)
            {
                _customizer.ResetToDefault();
                LoadInitialValues();
                Debug.Log("[CharacterCustomizationUI] Customization reset to default!");
            }
        }

        private void OnDestroy()
        {
            // リスナー解除
            if (_hairColorRSlider != null) _hairColorRSlider.onValueChanged.RemoveAllListeners();
            if (_hairColorGSlider != null) _hairColorGSlider.onValueChanged.RemoveAllListeners();
            if (_hairColorBSlider != null) _hairColorBSlider.onValueChanged.RemoveAllListeners();

            if (_eyeColorRSlider != null) _eyeColorRSlider.onValueChanged.RemoveAllListeners();
            if (_eyeColorGSlider != null) _eyeColorGSlider.onValueChanged.RemoveAllListeners();
            if (_eyeColorBSlider != null) _eyeColorBSlider.onValueChanged.RemoveAllListeners();

            if (_skinToneSlider != null) _skinToneSlider.onValueChanged.RemoveAllListeners();

            if (_topColorRSlider != null) _topColorRSlider.onValueChanged.RemoveAllListeners();
            if (_topColorGSlider != null) _topColorGSlider.onValueChanged.RemoveAllListeners();
            if (_topColorBSlider != null) _topColorBSlider.onValueChanged.RemoveAllListeners();

            if (_heightSlider != null) _heightSlider.onValueChanged.RemoveAllListeners();

            if (_saveButton != null) _saveButton.onClick.RemoveAllListeners();
            if (_resetButton != null) _resetButton.onClick.RemoveAllListeners();
        }
    }
}
