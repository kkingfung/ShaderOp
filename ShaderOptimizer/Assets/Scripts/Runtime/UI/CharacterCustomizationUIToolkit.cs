#nullable enable

using System;
using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.Customization;
using ShaderOp.Core;

namespace ShaderOp.UI
{
    /// <summary>
    /// キャラクターカスタマイズUI (UI Toolkit版)
    /// </summary>
    /// <remarks>
    /// 最新のUI Toolkitを使用した、アクセシブルでレスポンシブなUI実装
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class CharacterCustomizationUIToolkit : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterCustomizer? _customizer;
        [SerializeField] private UIDocument? _uiDocument;

        // UI Elements
        private VisualElement? _root;

        // Hair Controls
        private Slider? _hairColorR;
        private Slider? _hairColorG;
        private Slider? _hairColorB;
        private VisualElement? _hairColorPreview;
        private Label? _hairColorRValue;
        private Label? _hairColorGValue;
        private Label? _hairColorBValue;

        // Eye Controls
        private Slider? _eyeColorR;
        private Slider? _eyeColorG;
        private Slider? _eyeColorB;
        private VisualElement? _eyeColorPreview;
        private Label? _eyeColorRValue;
        private Label? _eyeColorGValue;
        private Label? _eyeColorBValue;

        // Skin Controls
        private Slider? _skinTone;
        private Label? _skinToneValue;

        // Clothing Controls
        private Slider? _clothingColorR;
        private Slider? _clothingColorG;
        private Slider? _clothingColorB;
        private VisualElement? _clothingColorPreview;
        private Label? _clothingColorRValue;
        private Label? _clothingColorGValue;
        private Label? _clothingColorBValue;

        // Body Controls
        private Slider? _height;
        private Label? _heightValue;

        // Camera Controls
        private Button? _resetCamera;
        private Toggle? _autoRotate;

        // Action Buttons
        private Button? _backButton;
        private Button? _resetButton;
        private Button? _saveButton;

        // Preset Buttons
        private Button? _hairPreset1;
        private Button? _hairPreset2;
        private Button? _hairPreset3;
        private Button? _hairPreset4;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }
        }

        private void OnEnable()
        {
            if (_uiDocument == null) return;

            _root = _uiDocument.rootVisualElement;
            if (_root == null) return;

            QueryUIElements();
            RegisterEventHandlers();
            LoadInitialValues();
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
        }

        /// <summary>
        /// UI要素をクエリ
        /// </summary>
        private void QueryUIElements()
        {
            if (_root == null) return;

            // Hair
            _hairColorR = _root.Q<Slider>("HairColorR");
            _hairColorG = _root.Q<Slider>("HairColorG");
            _hairColorB = _root.Q<Slider>("HairColorB");
            _hairColorPreview = _root.Q<VisualElement>("HairColorPreview");
            _hairColorRValue = _root.Q<Label>("HairColorRValue");
            _hairColorGValue = _root.Q<Label>("HairColorGValue");
            _hairColorBValue = _root.Q<Label>("HairColorBValue");

            // Eyes
            _eyeColorR = _root.Q<Slider>("EyeColorR");
            _eyeColorG = _root.Q<Slider>("EyeColorG");
            _eyeColorB = _root.Q<Slider>("EyeColorB");
            _eyeColorPreview = _root.Q<VisualElement>("EyeColorPreview");
            _eyeColorRValue = _root.Q<Label>("EyeColorRValue");
            _eyeColorGValue = _root.Q<Label>("EyeColorGValue");
            _eyeColorBValue = _root.Q<Label>("EyeColorBValue");

            // Skin
            _skinTone = _root.Q<Slider>("SkinTone");
            _skinToneValue = _root.Q<Label>("SkinToneValue");

            // Clothing
            _clothingColorR = _root.Q<Slider>("ClothingColorR");
            _clothingColorG = _root.Q<Slider>("ClothingColorG");
            _clothingColorB = _root.Q<Slider>("ClothingColorB");
            _clothingColorPreview = _root.Q<VisualElement>("ClothingColorPreview");
            _clothingColorRValue = _root.Q<Label>("ClothingColorRValue");
            _clothingColorGValue = _root.Q<Label>("ClothingColorGValue");
            _clothingColorBValue = _root.Q<Label>("ClothingColorBValue");

            // Body
            _height = _root.Q<Slider>("Height");
            _heightValue = _root.Q<Label>("HeightValue");

            // Camera
            _resetCamera = _root.Q<Button>("ResetCamera");
            _autoRotate = _root.Q<Toggle>("AutoRotate");

            // Action Buttons
            _backButton = _root.Q<Button>("BackButton");
            _resetButton = _root.Q<Button>("ResetButton");
            _saveButton = _root.Q<Button>("SaveButton");

            // Preset Buttons
            _hairPreset1 = _root.Q<Button>("HairPreset1");
            _hairPreset2 = _root.Q<Button>("HairPreset2");
            _hairPreset3 = _root.Q<Button>("HairPreset3");
            _hairPreset4 = _root.Q<Button>("HairPreset4");
        }

        /// <summary>
        /// イベントハンドラー登録
        /// </summary>
        private void RegisterEventHandlers()
        {
            // Hair sliders
            if (_hairColorR != null) _hairColorR.RegisterValueChangedCallback(evt => OnHairColorChanged());
            if (_hairColorG != null) _hairColorG.RegisterValueChangedCallback(evt => OnHairColorChanged());
            if (_hairColorB != null) _hairColorB.RegisterValueChangedCallback(evt => OnHairColorChanged());

            // Eye sliders
            if (_eyeColorR != null) _eyeColorR.RegisterValueChangedCallback(evt => OnEyeColorChanged());
            if (_eyeColorG != null) _eyeColorG.RegisterValueChangedCallback(evt => OnEyeColorChanged());
            if (_eyeColorB != null) _eyeColorB.RegisterValueChangedCallback(evt => OnEyeColorChanged());

            // Skin slider
            if (_skinTone != null) _skinTone.RegisterValueChangedCallback(OnSkinToneChanged);

            // Clothing sliders
            if (_clothingColorR != null) _clothingColorR.RegisterValueChangedCallback(evt => OnClothingColorChanged());
            if (_clothingColorG != null) _clothingColorG.RegisterValueChangedCallback(evt => OnClothingColorChanged());
            if (_clothingColorB != null) _clothingColorB.RegisterValueChangedCallback(evt => OnClothingColorChanged());

            // Height slider
            if (_height != null) _height.RegisterValueChangedCallback(OnHeightChanged);

            // Camera buttons
            if (_resetCamera != null) _resetCamera.clicked += OnResetCameraClicked;
            if (_autoRotate != null) _autoRotate.RegisterValueChangedCallback(OnAutoRotateChanged);

            // Action buttons
            if (_backButton != null) _backButton.clicked += OnBackClicked;
            if (_resetButton != null) _resetButton.clicked += OnResetClicked;
            if (_saveButton != null) _saveButton.clicked += OnSaveClicked;

            // Preset buttons
            if (_hairPreset1 != null) _hairPreset1.clicked += () => ApplyHairPreset(new Color(0.1f, 0.1f, 0.1f)); // Black
            if (_hairPreset2 != null) _hairPreset2.clicked += () => ApplyHairPreset(new Color(0.9f, 0.8f, 0.5f)); // Blonde
            if (_hairPreset3 != null) _hairPreset3.clicked += () => ApplyHairPreset(new Color(0.3f, 0.15f, 0.05f)); // Brown
            if (_hairPreset4 != null) _hairPreset4.clicked += () => ApplyHairPreset(new Color(0.6f, 0.2f, 0.1f)); // Red
        }

        /// <summary>
        /// イベントハンドラー解除
        /// </summary>
        private void UnregisterEventHandlers()
        {
            if (_hairColorR != null) _hairColorR.UnregisterValueChangedCallback(evt => OnHairColorChanged());
            if (_hairColorG != null) _hairColorG.UnregisterValueChangedCallback(evt => OnHairColorChanged());
            if (_hairColorB != null) _hairColorB.UnregisterValueChangedCallback(evt => OnHairColorChanged());

            if (_eyeColorR != null) _eyeColorR.UnregisterValueChangedCallback(evt => OnEyeColorChanged());
            if (_eyeColorG != null) _eyeColorG.UnregisterValueChangedCallback(evt => OnEyeColorChanged());
            if (_eyeColorB != null) _eyeColorB.UnregisterValueChangedCallback(evt => OnEyeColorChanged());

            if (_skinTone != null) _skinTone.UnregisterValueChangedCallback(OnSkinToneChanged);

            if (_clothingColorR != null) _clothingColorR.UnregisterValueChangedCallback(evt => OnClothingColorChanged());
            if (_clothingColorG != null) _clothingColorG.UnregisterValueChangedCallback(evt => OnClothingColorChanged());
            if (_clothingColorB != null) _clothingColorB.UnregisterValueChangedCallback(evt => OnClothingColorChanged());

            if (_height != null) _height.UnregisterValueChangedCallback(OnHeightChanged);

            if (_resetCamera != null) _resetCamera.clicked -= OnResetCameraClicked;
            if (_autoRotate != null) _autoRotate.UnregisterValueChangedCallback(OnAutoRotateChanged);

            if (_backButton != null) _backButton.clicked -= OnBackClicked;
            if (_resetButton != null) _resetButton.clicked -= OnResetClicked;
            if (_saveButton != null) _saveButton.clicked -= OnSaveClicked;
        }

        /// <summary>
        /// 初期値をロード
        /// </summary>
        private void LoadInitialValues()
        {
            if (_customizer == null) return;

            CharacterCustomizationData data = _customizer.GetCustomizationData();

            // Hair
            SetSliderValue(_hairColorR, data.HairColor.r, _hairColorRValue);
            SetSliderValue(_hairColorG, data.HairColor.g, _hairColorGValue);
            SetSliderValue(_hairColorB, data.HairColor.b, _hairColorBValue);
            UpdateColorPreview(_hairColorPreview, data.HairColor);

            // Eyes
            SetSliderValue(_eyeColorR, data.EyeColor.r, _eyeColorRValue);
            SetSliderValue(_eyeColorG, data.EyeColor.g, _eyeColorGValue);
            SetSliderValue(_eyeColorB, data.EyeColor.b, _eyeColorBValue);
            UpdateColorPreview(_eyeColorPreview, data.EyeColor);

            // Skin
            SetSliderValue(_skinTone, data.SkinToneId, _skinToneValue);

            // Clothing
            SetSliderValue(_clothingColorR, data.TopColor.r, _clothingColorRValue);
            SetSliderValue(_clothingColorG, data.TopColor.g, _clothingColorGValue);
            SetSliderValue(_clothingColorB, data.TopColor.b, _clothingColorBValue);
            UpdateColorPreview(_clothingColorPreview, data.TopColor);

            // Height
            SetSliderValue(_height, data.HeightScale, _heightValue);
        }

        /// <summary>
        /// スライダー値設定ヘルパー
        /// </summary>
        private void SetSliderValue(Slider? slider, float value, Label? label)
        {
            if (slider != null)
            {
                slider.value = value;
            }

            if (label != null)
            {
                label.text = value.ToString("F2");
            }
        }

        /// <summary>
        /// カラープレビュー更新ヘルパー
        /// </summary>
        private void UpdateColorPreview(VisualElement? preview, Color color)
        {
            if (preview != null)
            {
                preview.style.backgroundColor = new StyleColor(color);
            }
        }

        /// <summary>
        /// 髪色変更時
        /// </summary>
        private void OnHairColorChanged()
        {
            if (_customizer == null || _hairColorR == null || _hairColorG == null || _hairColorB == null)
                return;

            Color hairColor = new Color(_hairColorR.value, _hairColorG.value, _hairColorB.value);
            _customizer.SetHairColor(hairColor);

            UpdateColorPreview(_hairColorPreview, hairColor);

            if (_hairColorRValue != null) _hairColorRValue.text = _hairColorR.value.ToString("F2");
            if (_hairColorGValue != null) _hairColorGValue.text = _hairColorG.value.ToString("F2");
            if (_hairColorBValue != null) _hairColorBValue.text = _hairColorB.value.ToString("F2");
        }

        /// <summary>
        /// 瞳色変更時
        /// </summary>
        private void OnEyeColorChanged()
        {
            if (_customizer == null || _eyeColorR == null || _eyeColorG == null || _eyeColorB == null)
                return;

            Color eyeColor = new Color(_eyeColorR.value, _eyeColorG.value, _eyeColorB.value);
            _customizer.SetEyeColor(eyeColor);

            UpdateColorPreview(_eyeColorPreview, eyeColor);

            if (_eyeColorRValue != null) _eyeColorRValue.text = _eyeColorR.value.ToString("F2");
            if (_eyeColorGValue != null) _eyeColorGValue.text = _eyeColorG.value.ToString("F2");
            if (_eyeColorBValue != null) _eyeColorBValue.text = _eyeColorB.value.ToString("F2");
        }

        /// <summary>
        /// 肌色変更時
        /// </summary>
        private void OnSkinToneChanged(ChangeEvent<float> evt)
        {
            if (_customizer == null) return;

            int skinToneId = Mathf.RoundToInt(evt.newValue);
            _customizer.SetSkinTone(skinToneId);

            if (_skinToneValue != null)
            {
                _skinToneValue.text = skinToneId.ToString();
            }
        }

        /// <summary>
        /// 衣装色変更時
        /// </summary>
        private void OnClothingColorChanged()
        {
            if (_customizer == null || _clothingColorR == null || _clothingColorG == null || _clothingColorB == null)
                return;

            Color clothingColor = new Color(_clothingColorR.value, _clothingColorG.value, _clothingColorB.value);
            _customizer.SetTopColor(clothingColor);

            UpdateColorPreview(_clothingColorPreview, clothingColor);

            if (_clothingColorRValue != null) _clothingColorRValue.text = _clothingColorR.value.ToString("F2");
            if (_clothingColorGValue != null) _clothingColorGValue.text = _clothingColorG.value.ToString("F2");
            if (_clothingColorBValue != null) _clothingColorBValue.text = _clothingColorB.value.ToString("F2");
        }

        /// <summary>
        /// 身長変更時
        /// </summary>
        private void OnHeightChanged(ChangeEvent<float> evt)
        {
            if (_customizer == null) return;

            _customizer.SetHeightScale(evt.newValue);

            if (_heightValue != null)
            {
                _heightValue.text = evt.newValue.ToString("F2");
            }
        }

        /// <summary>
        /// カメラリセットボタンクリック時
        /// </summary>
        private void OnResetCameraClicked()
        {
            OrbitCameraController? camera = FindFirstObjectByType<OrbitCameraController>();
            if (camera != null)
            {
                // カメラをリセット（メソッドがあれば呼び出す）
                Debug.Log("[CharacterCustomizationUIToolkit] Camera reset requested");
            }
        }

        /// <summary>
        /// 自動回転トグル時
        /// </summary>
        private void OnAutoRotateChanged(ChangeEvent<bool> evt)
        {
            OrbitCameraController? camera = FindFirstObjectByType<OrbitCameraController>();
            if (camera != null)
            {
                // カメラの自動回転を設定（メソッドがあれば呼び出す）
                Debug.Log($"[CharacterCustomizationUIToolkit] Auto rotate: {evt.newValue}");
            }
        }

        /// <summary>
        /// 戻るボタンクリック時
        /// </summary>
        private void OnBackClicked()
        {
            Debug.Log("[CharacterCustomizationUIToolkit] Back to menu");
            GameManager.Instance.LoadMainMenu();
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
                Debug.Log("[CharacterCustomizationUIToolkit] Reset to default");
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
                Debug.Log("[CharacterCustomizationUIToolkit] Character saved!");

                // 保存成功フィードバック（アニメーションやサウンド）
                ShowSaveSuccessMessage();
            }
        }

        /// <summary>
        /// 髪色プリセット適用
        /// </summary>
        private void ApplyHairPreset(Color color)
        {
            if (_hairColorR != null) _hairColorR.value = color.r;
            if (_hairColorG != null) _hairColorG.value = color.g;
            if (_hairColorB != null) _hairColorB.value = color.b;

            OnHairColorChanged();
        }

        /// <summary>
        /// 保存成功メッセージ表示
        /// </summary>
        private void ShowSaveSuccessMessage()
        {
            // TODO: トーストメッセージやアニメーション表示
            Debug.Log("[CharacterCustomizationUIToolkit] ✓ Character saved successfully!");
        }
    }
}
