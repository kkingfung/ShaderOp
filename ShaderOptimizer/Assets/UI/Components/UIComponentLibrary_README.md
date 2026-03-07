# UI Component Library - 使用ガイド

## 重要な注意事項

**`UIComponentLibrary.uxml` は Unity UI Toolkit の制約により、テンプレートとして直接使用できません。**

Unity UI Toolkit の `<ui:Template>` は外部UXMLファイルへの参照が必要で、このような単一ファイル内でのテンプレート定義はサポートされていません。

## 推奨される使用方法

### 方法1: C# で直接ビルド（推奨）

`UIComponentFactory.cs` を使用して、コンポーネントを動的に生成します：

```csharp
// 初期化
UIComponentFactory.Initialize(rootVisualElement);

// カラーピッカーを作成
var colorPicker = UIComponentFactory.CreateColorPickerRGB("HairColor");
colorPicker.OnColorChanged += (color) => ApplyColor(color);
parentElement.Add(colorPicker.Root);

// スライダーを作成
var slider = UIComponentFactory.CreateSliderWithValue("Height", 0.8f, 1.2f, 1.0f);
slider.OnValueChanged += (value) => UpdateHeight(value);
parentElement.Add(slider.Root);
```

### 方法2: 個別UXMLファイルとして作成

各コンポーネントを個別のUXMLファイルとして作成します：

```
Assets/UI/Components/
├── ColorPickerRGB.uxml
├── PresetButtonGroup.uxml
├── SliderWithValue.uxml
└── ...
```

そして、メインUXMLから参照：

```xml
<ui:UXML>
    <ui:Template name="ColorPickerRGB" src="project://database/Assets/UI/Components/ColorPickerRGB.uxml" />
    <ui:Instance template="ColorPickerRGB" />
</ui:UXML>
```

## UIComponentLibrary.uxml の用途

このファイルは以下の目的で保持されています：

1. **リファレンス**: 各コンポーネントの構造を確認するためのドキュメント
2. **コピー&ペースト**: 手動でUXMLに貼り付ける際のテンプレート
3. **UI Builder プレビュー**: UI Builderで開いてプレビュー確認

## コンポーネント一覧

### 利用可能なコンポーネント

1. **ColorPickerRGB** - RGB スライダー付きカラーピッカー
2. **PresetButtonGroup** - プリセット選択ボタングループ
3. **SliderWithValue** - 値表示付きスライダー
4. **SectionHeader** - セクションヘッダー
5. **ActionButtonGroup** - アクションボタングループ
6. **NotificationToast** - 通知トースト
7. **ConfirmDialog** - 確認ダイアログ
8. **ProgressBar** - プログレスバー
9. **TabGroup** - タブナビゲーション
10. **DropdownMenu** - ドロップダウンメニュー
11. **ToggleGroup** - トグルグループ
12. **Card** - カードコンポーネント

## 今後の改善

- [ ] 各コンポーネントを個別UXMLファイルに分離
- [ ] UIComponentFactory のメソッド追加
- [ ] カスタムコントロール（CustomControl）として実装
- [ ] UI Builder 統合
