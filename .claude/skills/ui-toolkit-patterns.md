# UI Toolkit パターン

## 概要
Unity UI Toolkitを使用したUI開発のベストプラクティス。UXML/USS設計、イベント処理、データバインディングパターン。

## 基本構造

### UXML（レイアウト）
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="Styles/MainMenuStyles.uss" />

    <ui:VisualElement name="Root" class="root-container">
        <!-- ヘッダー -->
        <ui:VisualElement name="Header" class="header">
            <ui:Label text="タイトル" name="TitleLabel" class="title" />
        </ui:VisualElement>

        <!-- メインコンテンツ -->
        <ui:VisualElement name="Content" class="content">
            <!-- コントロール -->
            <ui:VisualElement class="control-row">
                <ui:Label text="名前" class="control-label" />
                <ui:TextField name="NameField" class="control-input" />
            </ui:VisualElement>

            <ui:VisualElement class="control-row">
                <ui:Label text="カテゴリ" class="control-label" />
                <ui:DropdownField name="CategoryDropdown" class="control-input" />
            </ui:VisualElement>
        </ui:VisualElement>

        <!-- ボタン -->
        <ui:VisualElement name="ButtonContainer" class="button-container">
            <ui:Button text="適用" name="ApplyButton" class="button-primary" />
            <ui:Button text="キャンセル" name="CancelButton" class="button-secondary" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### USS（スタイル）
```css
/* ルートコンテナ */
.root-container {
    flex-grow: 1;
    background-color: rgb(30, 30, 30);
    padding: 20px;
}

/* ヘッダー */
.header {
    margin-bottom: 20px;
    padding: 10px;
    background-color: rgb(45, 45, 45);
    border-radius: 5px;
}

.title {
    font-size: 24px;
    color: rgb(255, 255, 255);
    -unity-font-style: bold;
    -unity-text-align: middle-center;
}

/* コントロール行（左ラベル、右入力欄） */
.control-row {
    flex-direction: row;
    margin: 15px 2%;
    align-items: center;
}

.control-label {
    min-width: 250px;
    flex-grow: 1;
    margin-right: 20px;
    color: rgb(255, 255, 255);
    font-size: 16px;
}

.control-input {
    width: 300px;
    flex-shrink: 0;
    height: 30px;
    background-color: rgb(50, 50, 50);
    color: rgb(255, 255, 255);
}

/* ボタンコンテナ */
.button-container {
    flex-direction: row;
    justify-content: center;
    margin-top: 30px;
}

/* プライマリボタン */
.button-primary {
    width: 150px;
    height: 40px;
    margin: 0 10px;
    background-color: rgb(0, 120, 215);
    color: rgb(255, 255, 255);
    font-size: 16px;
    border-radius: 5px;
}

.button-primary:hover {
    background-color: rgb(0, 140, 235);
}

.button-primary:active {
    background-color: rgb(0, 100, 195);
}

/* セカンダリボタン */
.button-secondary {
    width: 150px;
    height: 40px;
    margin: 0 10px;
    background-color: rgb(70, 70, 70);
    color: rgb(255, 255, 255);
    font-size: 16px;
    border-radius: 5px;
}

.button-secondary:hover {
    background-color: rgb(90, 90, 90);
}
```

## C# スクリプト

### View基底クラス
```csharp
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit View基底クラス
/// </summary>
[RequireComponent(typeof(UIDocument))]
public abstract class UIViewBase : MonoBehaviour
{
    protected UIDocument UIDocument { get; private set; }
    protected VisualElement Root { get; private set; }

    protected virtual void Awake()
    {
        UIDocument = GetComponent<UIDocument>();
    }

    protected virtual void OnEnable()
    {
        if (UIDocument != null && UIDocument.rootVisualElement != null)
        {
            Root = UIDocument.rootVisualElement;
            OnRootReady();
        }
    }

    protected virtual void OnDisable()
    {
        OnRootDispose();
    }

    /// <summary>ルート要素が準備完了時に呼ばれる</summary>
    protected abstract void OnRootReady();

    /// <summary>ルート要素が破棄される前に呼ばれる</summary>
    protected abstract void OnRootDispose();

    /// <summary>UI要素を取得</summary>
    protected T Q<T>(string name = null) where T : VisualElement
    {
        return Root?.Q<T>(name);
    }
}
```

### View実装例
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// カスタマイズ画面View
/// </summary>
public class CustomizationView : UIViewBase
{
    // UI要素のキャッシュ
    private Label _titleLabel;
    private TextField _nameField;
    private DropdownField _categoryDropdown;
    private Button _applyButton;
    private Button _cancelButton;

    protected override void OnRootReady()
    {
        // UI要素を取得
        GetUIElements();

        // Dropdownの選択肢を設定
        SetupDropdowns();

        // イベントハンドラを登録
        RegisterEventHandlers();

        // 初期値を設定
        InitializeUI();
    }

    protected override void OnRootDispose()
    {
        // イベントハンドラを解除
        UnregisterEventHandlers();
    }

    /// <summary>
    /// UI要素を取得します
    /// </summary>
    private void GetUIElements()
    {
        _titleLabel = Q<Label>("TitleLabel");
        _nameField = Q<TextField>("NameField");
        _categoryDropdown = Q<DropdownField>("CategoryDropdown");
        _applyButton = Q<Button>("ApplyButton");
        _cancelButton = Q<Button>("CancelButton");
    }

    /// <summary>
    /// Dropdownを設定します
    /// </summary>
    private void SetupDropdowns()
    {
        if (_categoryDropdown != null)
        {
            _categoryDropdown.choices = new List<string>
            {
                "キャラクター",
                "馬",
                "武器"
            };
            _categoryDropdown.value = "キャラクター";
        }
    }

    /// <summary>
    /// イベントハンドラを登録します
    /// </summary>
    private void RegisterEventHandlers()
    {
        if (_applyButton != null)
        {
            _applyButton.clicked += OnApplyButtonClicked;
        }

        if (_cancelButton != null)
        {
            _cancelButton.clicked += OnCancelButtonClicked;
        }

        if (_categoryDropdown != null)
        {
            _categoryDropdown.RegisterValueChangedCallback(OnCategoryChanged);
        }

        if (_nameField != null)
        {
            _nameField.RegisterValueChangedCallback(OnNameChanged);
        }
    }

    /// <summary>
    /// イベントハンドラを解除します
    /// </summary>
    private void UnregisterEventHandlers()
    {
        if (_applyButton != null)
        {
            _applyButton.clicked -= OnApplyButtonClicked;
        }

        if (_cancelButton != null)
        {
            _cancelButton.clicked -= OnCancelButtonClicked;
        }

        if (_categoryDropdown != null)
        {
            _categoryDropdown.UnregisterValueChangedCallback(OnCategoryChanged);
        }

        if (_nameField != null)
        {
            _nameField.UnregisterValueChangedCallback(OnNameChanged);
        }
    }

    /// <summary>
    /// UIを初期化します
    /// </summary>
    private void InitializeUI()
    {
        if (_titleLabel != null)
        {
            _titleLabel.text = "カスタマイズ設定";
        }

        if (_nameField != null)
        {
            _nameField.value = "デフォルト名";
        }
    }

    // イベントハンドラ
    private void OnApplyButtonClicked()
    {
        Debug.Log($"適用: 名前={_nameField?.value}, カテゴリ={_categoryDropdown?.value}");
    }

    private void OnCancelButtonClicked()
    {
        Debug.Log("キャンセル");
    }

    private void OnCategoryChanged(ChangeEvent<string> evt)
    {
        Debug.Log($"カテゴリ変更: {evt.previousValue} → {evt.newValue}");
    }

    private void OnNameChanged(ChangeEvent<string> evt)
    {
        Debug.Log($"名前変更: {evt.newValue}");
    }
}
```

## データバインディングパターン

### ViewModel連携
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using System.ComponentModel;

/// <summary>
/// ViewModelと連携するView
/// </summary>
public class CustomizationViewWithViewModel : UIViewBase
{
    private CustomizationViewModel _viewModel;
    private TextField _nameField;

    protected override void OnRootReady()
    {
        _nameField = Q<TextField>("NameField");

        // ViewModelを作成
        _viewModel = new CustomizationViewModel();

        // ViewModelのプロパティ変更イベントを購読
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // UIからViewModelへの変更
        _nameField.RegisterValueChangedCallback(evt =>
        {
            _viewModel.Name = evt.newValue;
        });

        // 初期値を設定
        _nameField.value = _viewModel.Name;
    }

    protected override void OnRootDispose()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// ViewModelのプロパティ変更時の処理
    /// </summary>
    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.Name))
        {
            // ViewModelからUIへの変更
            if (_nameField != null && _nameField.value != _viewModel.Name)
            {
                _nameField.value = _viewModel.Name;
            }
        }
    }
}

/// <summary>
/// カスタマイズViewModel
/// </summary>
public class CustomizationViewModel : INotifyPropertyChanged
{
    private string _name = "デフォルト名";

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## カスタムコントロール

### Arrow Button コントロール
```xml
<!-- UXML -->
<ui:VisualElement class="arrow-control">
    <ui:Label text="値" class="arrow-label" />
    <ui:Button text="&lt;" name="PrevButton" class="arrow-button" />
    <ui:Label text="0" name="ValueLabel" class="arrow-value" />
    <ui:Button text="&gt;" name="NextButton" class="arrow-button" />
</ui:VisualElement>
```

```css
/* USS */
.arrow-control {
    flex-direction: row;
    align-items: center;
    margin: 10px 0;
}

.arrow-label {
    min-width: 250px;
    flex-grow: 1;
    margin-right: 20px;
    color: rgb(255, 255, 255);
}

.arrow-button {
    width: 40px;
    height: 30px;
    background-color: rgb(70, 70, 70);
    color: rgb(255, 255, 255);
}

.arrow-value {
    width: 80px;
    color: rgb(255, 255, 255);
    -unity-text-align: middle-center;
    margin: 0 10px;
}
```

```csharp
// C# 実装
private Button _prevButton;
private Button _nextButton;
private Label _valueLabel;
private int _currentValue = 0;
private int _minValue = 0;
private int _maxValue = 10;

private void SetupArrowControl()
{
    _prevButton = Q<Button>("PrevButton");
    _nextButton = Q<Button>("NextButton");
    _valueLabel = Q<Label>("ValueLabel");

    _prevButton.clicked += () => OnArrowButtonClicked(-1);
    _nextButton.clicked += () => OnArrowButtonClicked(1);

    UpdateValueLabel();
}

private void OnArrowButtonClicked(int direction)
{
    // ラッピング計算
    _currentValue += direction;

    if (_currentValue < _minValue)
    {
        _currentValue = _maxValue;
    }
    else if (_currentValue > _maxValue)
    {
        _currentValue = _minValue;
    }

    UpdateValueLabel();
}

private void UpdateValueLabel()
{
    if (_valueLabel != null)
    {
        _valueLabel.text = _currentValue.ToString();
    }
}
```

## 動的UI生成

```csharp
/// <summary>
/// 動的にUI要素を生成
/// </summary>
public void CreateDynamicList(List<string> items)
{
    var container = Q<VisualElement>("ListContainer");

    // 既存の要素をクリア
    container.Clear();

    // 動的に要素を生成
    foreach (var item in items)
    {
        var row = new VisualElement();
        row.AddToClassList("list-item");

        var label = new Label(item);
        label.AddToClassList("list-item-label");

        var button = new Button(() => OnItemClicked(item));
        button.text = "選択";
        button.AddToClassList("list-item-button");

        row.Add(label);
        row.Add(button);

        container.Add(row);
    }
}

private void OnItemClicked(string item)
{
    Debug.Log($"アイテムがクリックされました: {item}");
}
```

## ベストプラクティス

✅ **DO**:
- UI要素はOnEnableでキャッシュ
- イベントはOnDisableで必ず解除
- USSでスタイルを管理（C#でスタイルを設定しない）
- VisualElement階層を浅く保つ
- UXMLを再利用可能にする

❌ **DON'T**:
- UI要素を毎フレームQ()で取得
- イベント解除し忘れ（メモリリーク）
- 深すぎるVisualElement階層
- C#でスタイルをハードコード
- Update内でUI更新

信頼度: 0.93
