#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// UI Toolkit コンポーネントファクトリー
    /// </summary>
    /// <remarks>
    /// UIComponentLibrary.uxml のテンプレートから UI 要素を動的生成します
    /// 再利用可能なコンポーネントをコードから簡単にインスタンス化するためのヘルパークラス
    /// </remarks>
    public static class UIComponentFactory
    {
        private static VisualTreeAsset? _componentLibrary;

        /// <summary>
        /// コンポーネントライブラリを初期化します
        /// </summary>
        /// <param name="componentLibraryPath">UIComponentLibrary.uxml のパス</param>
        public static void Initialize(string componentLibraryPath = "Assets/UI/Components/UIComponentLibrary")
        {
            _componentLibrary = Resources.Load<VisualTreeAsset>(componentLibraryPath);
            if (_componentLibrary == null)
            {
                Debug.LogError($"[UIComponentFactory] コンポーネントライブラリが見つかりません: {componentLibraryPath}");
            }
        }

        /// <summary>
        /// RGB カラーピッカーを作成します
        /// </summary>
        public static ColorPickerRGBComponent CreateColorPickerRGB(VisualElement parent)
        {
            var instance = InstantiateTemplate("ColorPickerRGB-Template");
            if (instance == null) return new ColorPickerRGBComponent(new VisualElement());

            parent.Add(instance);
            return new ColorPickerRGBComponent(instance);
        }

        /// <summary>
        /// プリセットボタングループを作成します
        /// </summary>
        public static PresetButtonGroupComponent CreatePresetButtonGroup(VisualElement parent, string[] presetNames)
        {
            var instance = InstantiateTemplate("PresetButtonGroup-Template");
            if (instance == null) return new PresetButtonGroupComponent(new VisualElement());

            parent.Add(instance);
            var component = new PresetButtonGroupComponent(instance);
            component.SetPresetNames(presetNames);
            return component;
        }

        /// <summary>
        /// 値表示付きスライダーを作成します
        /// </summary>
        public static SliderWithValueComponent CreateSliderWithValue(
            VisualElement parent,
            string label,
            float minValue,
            float maxValue,
            float initialValue)
        {
            var instance = InstantiateTemplate("SliderWithValue-Template");
            if (instance == null) return new SliderWithValueComponent(new VisualElement());

            parent.Add(instance);
            var component = new SliderWithValueComponent(instance);
            component.SetLabel(label);
            component.SetRange(minValue, maxValue);
            component.SetValue(initialValue);
            return component;
        }

        /// <summary>
        /// セクションヘッダーを作成します
        /// </summary>
        public static SectionHeaderComponent CreateSectionHeader(
            VisualElement parent,
            string title,
            string? subtitle = null)
        {
            var instance = InstantiateTemplate("SectionHeader-Template");
            if (instance == null) return new SectionHeaderComponent(new VisualElement());

            parent.Add(instance);
            var component = new SectionHeaderComponent(instance);
            component.SetTitle(title);
            if (subtitle != null) component.SetSubtitle(subtitle);
            return component;
        }

        /// <summary>
        /// 通知トーストを作成します
        /// </summary>
        public static NotificationToastComponent CreateNotificationToast(
            VisualElement parent,
            string title,
            string message,
            NotificationToastType type = NotificationToastType.Info)
        {
            var instance = InstantiateTemplate("NotificationToast-Template");
            if (instance == null) return new NotificationToastComponent(new VisualElement());

            parent.Add(instance);
            var component = new NotificationToastComponent(instance);
            component.SetContent(title, message, type);
            return component;
        }

        /// <summary>
        /// 確認ダイアログを作成します
        /// </summary>
        public static ConfirmDialogComponent CreateConfirmDialog(
            VisualElement parent,
            string title,
            string message,
            Action? onConfirm = null,
            Action? onCancel = null)
        {
            var instance = InstantiateTemplate("ConfirmDialog-Template");
            if (instance == null) return new ConfirmDialogComponent(new VisualElement());

            parent.Add(instance);
            var component = new ConfirmDialogComponent(instance);
            component.SetContent(title, message);
            if (onConfirm != null) component.OnConfirm += onConfirm;
            if (onCancel != null) component.OnCancel += onCancel;
            return component;
        }

        /// <summary>
        /// プログレスバーを作成します
        /// </summary>
        public static ProgressBarComponent CreateProgressBar(
            VisualElement parent,
            string label = "Loading...")
        {
            var instance = InstantiateTemplate("ProgressBar-Template");
            if (instance == null) return new ProgressBarComponent(new VisualElement());

            parent.Add(instance);
            var component = new ProgressBarComponent(instance);
            component.SetLabel(label);
            return component;
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// テンプレートからインスタンスを生成します
        /// </summary>
        private static VisualElement? InstantiateTemplate(string templateName)
        {
            if (_componentLibrary == null)
            {
                Initialize();
            }

            if (_componentLibrary == null)
            {
                Debug.LogError("[UIComponentFactory] コンポーネントライブラリが初期化されていません");
                return null;
            }

            // Unity UI Toolkit では Template から直接インスタンス化する方法が限定的
            // 代わりに CloneTree() でルート要素を複製し、指定された name の要素を探す
            var root = _componentLibrary.CloneTree();
            var template = root.Q(templateName);

            if (template == null)
            {
                Debug.LogError($"[UIComponentFactory] テンプレートが見つかりません: {templateName}");
                return null;
            }

            return template;
        }
    }

    // ============================================
    // Component Wrapper Classes
    // ============================================

    /// <summary>
    /// RGB カラーピッカーコンポーネント
    /// </summary>
    public class ColorPickerRGBComponent
    {
        private readonly VisualElement _root;
        private readonly Slider? _redSlider;
        private readonly Slider? _greenSlider;
        private readonly Slider? _blueSlider;
        private readonly Label? _redValue;
        private readonly Label? _greenValue;
        private readonly Label? _blueValue;
        private readonly VisualElement? _colorPreview;

        public event Action<Color>? OnColorChanged;

        public ColorPickerRGBComponent(VisualElement root)
        {
            _root = root;
            _redSlider = _root.Q<Slider>("RedSlider");
            _greenSlider = _root.Q<Slider>("GreenSlider");
            _blueSlider = _root.Q<Slider>("BlueSlider");
            _redValue = _root.Q<Label>("RedValue");
            _greenValue = _root.Q<Label>("GreenValue");
            _blueValue = _root.Q<Label>("BlueValue");
            _colorPreview = _root.Q<VisualElement>("ColorPreview");

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            if (_redSlider != null)
                _redSlider.RegisterValueChangedCallback(evt => OnSliderChanged());
            if (_greenSlider != null)
                _greenSlider.RegisterValueChangedCallback(evt => OnSliderChanged());
            if (_blueSlider != null)
                _blueSlider.RegisterValueChangedCallback(evt => OnSliderChanged());
        }

        private void OnSliderChanged()
        {
            UpdateValueLabels();
            UpdateColorPreview();
            OnColorChanged?.Invoke(GetColor());
        }

        private void UpdateValueLabels()
        {
            if (_redValue != null && _redSlider != null)
                _redValue.text = _redSlider.value.ToString("F2");
            if (_greenValue != null && _greenSlider != null)
                _greenValue.text = _greenSlider.value.ToString("F2");
            if (_blueValue != null && _blueSlider != null)
                _blueValue.text = _blueSlider.value.ToString("F2");
        }

        private void UpdateColorPreview()
        {
            if (_colorPreview != null)
            {
                _colorPreview.style.backgroundColor = new StyleColor(GetColor());
            }
        }

        public Color GetColor()
        {
            float r = _redSlider?.value ?? 0f;
            float g = _greenSlider?.value ?? 0f;
            float b = _blueSlider?.value ?? 0f;
            return new Color(r, g, b, 1f);
        }

        public void SetColor(Color color)
        {
            if (_redSlider != null) _redSlider.value = color.r;
            if (_greenSlider != null) _greenSlider.value = color.g;
            if (_blueSlider != null) _blueSlider.value = color.b;
            UpdateValueLabels();
            UpdateColorPreview();
        }
    }

    /// <summary>
    /// プリセットボタングループコンポーネント
    /// </summary>
    public class PresetButtonGroupComponent
    {
        private readonly VisualElement _root;
        private readonly Button? _preset1;
        private readonly Button? _preset2;
        private readonly Button? _preset3;
        private readonly Button? _preset4;

        public event Action<int>? OnPresetClicked;

        public PresetButtonGroupComponent(VisualElement root)
        {
            _root = root;
            _preset1 = _root.Q<Button>("Preset1");
            _preset2 = _root.Q<Button>("Preset2");
            _preset3 = _root.Q<Button>("Preset3");
            _preset4 = _root.Q<Button>("Preset4");

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            if (_preset1 != null) _preset1.clicked += () => OnPresetClicked?.Invoke(0);
            if (_preset2 != null) _preset2.clicked += () => OnPresetClicked?.Invoke(1);
            if (_preset3 != null) _preset3.clicked += () => OnPresetClicked?.Invoke(2);
            if (_preset4 != null) _preset4.clicked += () => OnPresetClicked?.Invoke(3);
        }

        public void SetPresetNames(string[] names)
        {
            if (names.Length > 0 && _preset1 != null) _preset1.text = names[0];
            if (names.Length > 1 && _preset2 != null) _preset2.text = names[1];
            if (names.Length > 2 && _preset3 != null) _preset3.text = names[2];
            if (names.Length > 3 && _preset4 != null) _preset4.text = names[3];
        }
    }

    /// <summary>
    /// 値表示付きスライダーコンポーネント
    /// </summary>
    public class SliderWithValueComponent
    {
        private readonly VisualElement _root;
        private readonly Label? _label;
        private readonly Slider? _slider;
        private readonly Label? _valueLabel;

        public event Action<float>? OnValueChanged;

        public SliderWithValueComponent(VisualElement root)
        {
            _root = root;
            _label = _root.Q<Label>("SliderLabel");
            _slider = _root.Q<Slider>("Slider");
            _valueLabel = _root.Q<Label>("SliderValue");

            if (_slider != null)
            {
                _slider.RegisterValueChangedCallback(evt =>
                {
                    UpdateValueLabel();
                    OnValueChanged?.Invoke(evt.newValue);
                });
            }
        }

        public void SetLabel(string text) => _label!.text = text;
        public void SetRange(float min, float max)
        {
            if (_slider != null)
            {
                _slider.lowValue = min;
                _slider.highValue = max;
            }
        }

        public void SetValue(float value)
        {
            if (_slider != null) _slider.value = value;
            UpdateValueLabel();
        }

        private void UpdateValueLabel()
        {
            if (_valueLabel != null && _slider != null)
            {
                _valueLabel.text = _slider.value.ToString("F2");
            }
        }
    }

    /// <summary>
    /// セクションヘッダーコンポーネント
    /// </summary>
    public class SectionHeaderComponent
    {
        private readonly VisualElement _root;
        private readonly Label? _title;
        private readonly Label? _subtitle;

        public SectionHeaderComponent(VisualElement root)
        {
            _root = root;
            _title = _root.Q<Label>("SectionTitle");
            _subtitle = _root.Q<Label>("SectionSubtitle");
        }

        public void SetTitle(string text) => _title!.text = text;
        public void SetSubtitle(string text) => _subtitle!.text = text;
    }

    /// <summary>
    /// 通知トーストタイプ
    /// </summary>
    public enum NotificationToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// 通知トーストコンポーネント
    /// </summary>
    public class NotificationToastComponent
    {
        private readonly VisualElement _root;
        private readonly VisualElement? _icon;
        private readonly Label? _title;
        private readonly Label? _message;
        private readonly Button? _closeButton;

        public event Action? OnClose;

        public NotificationToastComponent(VisualElement root)
        {
            _root = root;
            _icon = _root.Q<VisualElement>("ToastIcon");
            _title = _root.Q<Label>("ToastTitle");
            _message = _root.Q<Label>("ToastMessage");
            _closeButton = _root.Q<Button>("ToastCloseButton");

            if (_closeButton != null)
            {
                _closeButton.clicked += () =>
                {
                    OnClose?.Invoke();
                    _root.style.display = DisplayStyle.None;
                };
            }
        }

        public void SetContent(string title, string message, NotificationToastType type)
        {
            if (_title != null) _title.text = title;
            if (_message != null) _message.text = message;

            if (_icon != null)
            {
                _icon.style.backgroundColor = type switch
                {
                    NotificationToastType.Success => new StyleColor(new Color(0.39f, 0.78f, 0.47f)),
                    NotificationToastType.Warning => new StyleColor(new Color(1f, 0.71f, 0.24f)),
                    NotificationToastType.Error => new StyleColor(new Color(0.86f, 0.31f, 0.31f)),
                    _ => new StyleColor(new Color(0.39f, 0.59f, 1f))
                };
            }
        }

        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide() => _root.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// 確認ダイアログコンポーネント
    /// </summary>
    public class ConfirmDialogComponent
    {
        private readonly VisualElement _root;
        private readonly Label? _title;
        private readonly Label? _message;
        private readonly Button? _confirmButton;
        private readonly Button? _cancelButton;

        public event Action? OnConfirm;
        public event Action? OnCancel;

        public ConfirmDialogComponent(VisualElement root)
        {
            _root = root;
            _title = _root.Q<Label>("DialogTitle");
            _message = _root.Q<Label>("DialogMessage");
            _confirmButton = _root.Q<Button>("DialogConfirmButton");
            _cancelButton = _root.Q<Button>("DialogCancelButton");

            if (_confirmButton != null)
            {
                _confirmButton.clicked += () =>
                {
                    OnConfirm?.Invoke();
                    Hide();
                };
            }

            if (_cancelButton != null)
            {
                _cancelButton.clicked += () =>
                {
                    OnCancel?.Invoke();
                    Hide();
                };
            }
        }

        public void SetContent(string title, string message)
        {
            if (_title != null) _title.text = title;
            if (_message != null) _message.text = message;
        }

        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide() => _root.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// プログレスバーコンポーネント
    /// </summary>
    public class ProgressBarComponent
    {
        private readonly VisualElement _root;
        private readonly Label? _label;
        private readonly Label? _percentLabel;
        private readonly VisualElement? _progressFill;

        public ProgressBarComponent(VisualElement root)
        {
            _root = root;
            _label = _root.Q<Label>("ProgressLabel");
            _percentLabel = _root.Q<Label>("ProgressPercent");
            _progressFill = _root.Q<VisualElement>("ProgressFill");
        }

        public void SetLabel(string text) => _label!.text = text;

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (_progressFill != null)
            {
                _progressFill.style.width = new StyleLength(new Length(progress * 100f, LengthUnit.Percent));
            }
            if (_percentLabel != null)
            {
                _percentLabel.text = $"{(progress * 100f):F0}%";
            }
        }
    }
}
