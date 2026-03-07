# Responsive Layouts - Unity UI Toolkit 制約について

## Unity UI Toolkit 未サポート機能

このスタイルシート（ResponsiveLayouts.uss）には、標準CSSでは有効だがUnity UI Toolkitでは未サポートのプロパティが含まれています。

### 警告が表示されるプロパティ

これらのプロパティは、将来のUnity UI Toolkitアップデートでサポートされる可能性があるため、意図的に残しています：

#### 1. `gap` プロパティ（Flexbox gap）
```css
.grid-gap-md { gap: var(--space-md); }
```
**代替手段**: `margin` を使用

#### 2. `line-height` プロパティ
```css
.text-desktop { line-height: 1.75; }
```
**代替手段**: Unity UI Toolkitでは未サポート（固定）

#### 3. `overflow-wrap` プロパティ
```css
.text-wrap { overflow-wrap: break-word; }
```
**代替手段**: `white-space: normal` を使用

#### 4. `max()` 関数（Safe Area Insets）
```css
.safe-area-top { padding-top: max(var(--space-md), env(safe-area-inset-top, 0)); }
```
**代替手段**: C#で動的に計算

#### 5. `overflow: auto`
```css
.overflow-auto { overflow: auto; }
```
**代替手段**: `overflow: scroll` を使用

#### 6. `z-index` プロパティ
```css
.z-modal { z-index: 1050; }
```
**代替手段**: VisualElement の親子関係で制御（後に追加された要素が上に表示される）

## 対処方法

### オプション1: 警告を無視する（推奨）

これらの警告は機能に影響しません。Unity UI Toolkitは未知のプロパティを単純に無視します。

**利点**:
- 将来のUnityバージョンで自動的にサポートされる可能性
- 標準CSSとの互換性を保持
- 他のプラットフォーム移植時の参考になる

### オプション2: コメントアウトする

警告を完全に消したい場合は、該当プロパティをコメントアウトします：

```css
.grid-gap-md {
    /* gap: var(--space-md); */ /* Unity UI Toolkit 未サポート */
}
```

### オプション3: C#で実装する

動的な値が必要な場合は、C#側で実装します：

```csharp
// gap の代替
container.style.marginLeft = 16;
container.style.marginRight = 16;

// z-index の代替
modalOverlay.BringToFront();

// Safe Area Insets
var safeAreaTop = Screen.safeArea.yMin;
header.style.paddingTop = Math.Max(16, safeAreaTop);
```

## Unity UI Toolkit サポート状況（2024年現在）

### ✅ サポート済み
- Flexbox (flex-direction, justify-content, align-items)
- Position (absolute, relative)
- Transform (translate, rotate, scale)
- Transitions
- @media (prefers-reduced-motion, prefers-contrast)

### ❌ 未サポート
- gap プロパティ
- line-height
- overflow-wrap
- max(), min(), clamp() 関数
- env() 関数（safe-area-inset）
- z-index
- overflow: auto
- :last-child, :nth-child 疑似クラス

## 参考リンク

- Unity UI Toolkit Documentation: https://docs.unity3d.com/Manual/UIElements.html
- USS Supported Properties: https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html
- USS vs CSS Differences: https://docs.unity3d.com/Manual/UIE-USS.html

---

**最終更新**: 2026-03-01
**Unity Version**: 2023.2+
