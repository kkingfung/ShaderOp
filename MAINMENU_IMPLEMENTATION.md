# MainMenu 完全実装 - 技術ドキュメント

## 概要

MainMenuは、ShaderOpゲームの中央ハブとして機能するメインメニュー画面です。すべてのゲームシーン(ミニゲーム、カスタマイズ)への入口として、縦画面向けUI Toolkitで実装されています。

## 技術スタック

- **Unity 2022.3 LTS** (URP)
- **C# 11** (`#nullable enable`)
- **UI Toolkit** - UXML/USS による宣言的UI
- **Portrait Layout** - 縦画面最適化 (9:16 aspect ratio)
- **Scene Management** - Unity SceneManager使用

## アーキテクチャ

### コンポーネント構成

```
MainMenuUI (GameObject)
├── UIDocument
│   ├── VisualTreeAsset: MainMenu.uxml
│   └── RootVisualElement
│       ├── HeaderSection (15%)
│       │   ├── GameTitle
│       │   └── Subtitle
│       ├── ContentSection (70%)
│       │   ├── Play Minigames
│       │   │   ├── TicTacToeHex (実装済み)
│       │   │   ├── HexReversi (実装済み)
│       │   │   ├── HexCheckers (Phase 3)
│       │   │   └── HexChess (Phase 3)
│       │   └── Customize
│       │       ├── RoomDecoration (実装済み)
│       │       └── CharacterCustomization (Phase 2後半)
│       └── FooterSection (15%)
│           ├── VersionLabel
│           ├── SettingsBtn
│           └── QuitBtn
└── MainMenuController (C# MonoBehaviour)
```

### ファイル構成

```
ShaderOp/
├── Assets/
│   ├── UI/
│   │   ├── MainMenu.uxml (UI構造定義)
│   │   └── MainMenu.uss (スタイル定義)
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   └── UI/
│   │   │       └── MainMenuController.cs (ロジック)
│   │   └── Editor/
│   │       ├── MainMenuSceneSetup.cs (自動セットアップ)
│   │       └── MainMenuValidator.cs (検証ツール)
│   └── Scenes/
│       └── MainMenu.unity
└── Documentation/
    ├── MAINMENU_IMPLEMENTATION.md (このファイル)
    └── MAINMENU_QUICKSTART.md
```

## UI設計（縦画面レイアウト）

### レイアウト原則

**3セクション構成**:
- **Header**: 15% - ゲームタイトル・サブタイトル
- **Content**: 70% - メインコンテンツ(ボタン群)
- **Footer**: 15% - バージョン情報・システムボタン

### UXML構造詳細

```xml
<ui:UXML>
  <Style src="MainMenu.uss"/>
  
  <ui:VisualElement name="MainMenuRoot">
    
    <!-- ヘッダー 15% -->
    <ui:VisualElement name="HeaderSection">
      <ui:Label name="GameTitle" text="ShaderOp"/>
      <ui:Label name="Subtitle" text="Hex Board Games Collection"/>
    </ui:VisualElement>
    
    <!-- コンテンツ 70% -->
    <ui:VisualElement name="ContentSection">
      
      <!-- ミニゲームセクション -->
      <ui:Label text="Play Minigames"/>
      <ui:Button name="PlayTicTacToeBtn" 
                 text="Tic-Tac-Toe Hex (3x3)" 
                 class="menu-button"/>
      <ui:Button name="PlayHexReversiBtn" 
                 text="Hex Reversi (37 tiles)" 
                 class="menu-button"/>
      <ui:Button name="PlayHexCheckersBtn" 
                 text="Hex Checkers (Coming Soon)" 
                 class="menu-button-disabled"/>
      <ui:Button name="PlayHexChessBtn" 
                 text="Hex Chess (Coming Soon)" 
                 class="menu-button-disabled"/>
      
      <!-- カスタマイズセクション -->
      <ui:Label text="Customize"/>
      <ui:Button name="RoomDecorationBtn" 
                 text="Room Decoration" 
                 class="menu-button"/>
      <ui:Button name="CharacterCustomizationBtn" 
                 text="Character (Coming Soon)" 
                 class="menu-button-disabled"/>
    </ui:VisualElement>
    
    <!-- フッター 15% -->
    <ui:VisualElement name="FooterSection">
      <ui:Label name="VersionLabel" text="v0.2.0 - Phase 2 (55%)"/>
      <ui:Button name="SettingsBtn" text="Settings"/>
      <ui:Button name="QuitBtn" text="Quit"/>
    </ui:VisualElement>
    
  </ui:VisualElement>
</ui:UXML>
```

### USS スタイリング詳細

**カラーパレット**:
- 背景: `rgb(15, 20, 30)` - ダークブルー
- セクション背景: `rgba(20, 30, 45, 0.9)` - 半透明ブルー
- アクティブボタン: `rgb(40, 80, 120)` - 中間ブルー
- ホバー: `rgb(60, 100, 140)` - ライトブルー
- 無効ボタン: `rgb(30, 35, 45)` - ダークグレー

**ボタンスタイル**:
```css
.menu-button {
    height: 60px;
    margin: 8px 0;
    border-radius: 10px;
    font-size: 20px;
    -unity-font-style: bold;
    background-color: rgb(40, 80, 120);
    border-width: 2px;
    border-color: rgb(60, 120, 180);
    transition-duration: 0.2s;
}

.menu-button:hover {
    background-color: rgb(60, 100, 140);
    border-color: rgb(100, 160, 220);
    scale: 1.02;
}

.menu-button:active {
    scale: 0.98;
}
```

**特定ゲームボタンのカラーコーディング**:
- **TicTacToeHex**: 緑系 `rgb(60, 120, 80)`
- **HexReversi**: 紫系 `rgb(120, 60, 120)`
- **RoomDecoration**: オレンジ系 `rgb(180, 100, 60)`

## C# 実装詳細

### MainMenuController.cs

**責務**:
- UI要素の初期化と参照管理
- ボタンクリックイベント処理
- シーン遷移制御
- バージョン情報表示

**主要メソッド**:

```csharp
// 初期化
private void Awake()
private void OnEnable()
private void OnDisable()

// UI取得
private void GetUIElements()
private void RegisterEventHandlers()
private void UnregisterEventHandlers()

// イベントハンドラ
private void OnPlayTicTacToeClicked()
private void OnPlayHexReversiClicked()
private void OnRoomDecorationClicked()
private void OnSettingsClicked()
private void OnQuitClicked()

// シーン遷移
private void LoadScene(string sceneName)

// UI更新
private void UpdateVersionInfo()
```

**シーン遷移の実装**:
```csharp
private void LoadScene(string sceneName)
{
    // Unity標準のSceneManagerを使用
    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
}
```

**Quit処理**:
```csharp
private void OnQuitClicked()
{
    Debug.Log("[MainMenuController] ゲームを終了");

#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
}
```

## エディターツール

### MainMenuSceneSetup.cs

**メニュー**: `ShaderOp → Setup → MainMenu Scene`

**実行内容**:
1. アセット存在確認 (UXML/USS)
2. シーンロードまたは作成
3. GameBootstrap作成
4. カメラ設定
5. UI作成 (UIDocument + MainMenuController)
6. シーン保存

**ワンクリックセットアップ**:
```csharp
[MenuItem("ShaderOp/Setup/MainMenu Scene")]
public static void SetupMainMenuScene()
{
    ValidateAssets();
    LoadOrCreateScene();
    SetupGameBootstrap();
    SetupCamera();
    SetupUI();
    SaveScene();
}
```

### MainMenuValidator.cs

**メニュー**: `ShaderOp → Validate → MainMenu Scene`

**検証項目**:
1. ✅ GameBootstrap存在確認
2. ✅ Camera設定確認
3. ✅ UI構造確認
4. ✅ Controller確認
5. ✅ Assets確認

**追加検証メニュー**:
- `ShaderOp → Validate → MainMenu UI Elements` - UI要素の詳細検証
- `ShaderOp → Validate → MainMenu Assets` - アセットのクイック確認

## デバッグ機能

### コンテキストメニュー

```csharp
[ContextMenu("Force Refresh UI")]
private void ForceRefreshUI()
{
    OnDisable();
    OnEnable();
}

[ContextMenu("Log Button States")]
private void LogButtonStates()
{
    // すべてのボタンの存在状態をログ出力
}
```

## パフォーマンス考慮事項

### UI Toolkit最適化

1. **静的UI** - ランタイムでUI要素を生成しない
2. **イベント登録/解除** - OnEnable/OnDisableで適切に管理
3. **Null安全** - すべてのUI要素参照でnullチェック

### メモリ管理

- UI要素の参照はフィールドにキャッシュ
- イベントハンドラは必ず解除 (メモリリーク防止)
- `#nullable enable` による静的安全性

## 拡張性

### 新しいシーンを追加する方法

1. **UXML編集**:
```xml
<ui:Button name="NewGameBtn" text="New Game" class="menu-button"/>
```

2. **USS編集** (オプション):
```css
#NewGameBtn {
    background-color: rgb(80, 80, 180);
    border-color: rgb(120, 120, 220);
}
```

3. **MainMenuController.cs編集**:
```csharp
// フィールド追加
private Button? _newGameBtn;

// GetUIElements()に追加
_newGameBtn = _root.Q<Button>("NewGameBtn");

// RegisterEventHandlers()に追加
if (_newGameBtn != null)
{
    _newGameBtn.clicked += OnNewGameClicked;
}

// イベントハンドラ追加
private void OnNewGameClicked()
{
    LoadScene("NewGame");
}
```

## トラブルシューティング

### 問題: ボタンがクリックできない

**症状**: ボタンを押しても何も起こらない

**解決策**:
1. Consoleでエラーログ確認
2. UI要素がnullでないか確認 (`Log Button States` コンテキストメニュー)
3. EventSystemが存在するか確認
4. UIDocumentのvisualTreeAssetが設定されているか確認

### 問題: スタイルが適用されない

**症状**: UIが白背景で表示される

**解決策**:
1. MainMenu.ussが正しく読み込まれているか確認
2. UXMLの`<Style src="MainMenu.uss"/>`行が存在するか確認
3. USS内のセレクタ名が正しいか確認 (name vs class)

### 問題: シーン遷移が機能しない

**症状**: ボタンを押してもシーンが切り替わらない

**解決策**:
1. Build Settingsに対象シーンが追加されているか確認
2. シーン名が正確か確認 (大文字小文字区別)
3. Consoleでエラーログ確認

## ベストプラクティス

### UI Toolkit パターン

1. **名前付け規則**:
   - Button: `...Btn` (例: `PlayTicTacToeBtn`)
   - Label: `...Label` (例: `VersionLabel`)
   - Section: `...Section` (例: `HeaderSection`)

2. **クラス vs ID**:
   - 共通スタイル: `.class` (例: `.menu-button`)
   - 個別スタイル: `#id` (例: `#PlayTicTacToeBtn`)

3. **イベント管理**:
   - 必ずOnEnable/OnDisableで登録/解除
   - null安全を保証

## Phase 2における位置づけ

**優先度**: Priority 2 (プロジェクト計画エージェントより)

**理由**:
- ✅ ゲーム全体の統合デモ
- ✅ 6シーン間のナビゲーション
- ✅ UI Toolkit縦画面実装の証明

**貢献**:
- TicTacToeHex、HexReversi、RoomDecorationへの統一アクセスポイント
- ユーザー体験の向上
- プロジェクトの完成度向上

## 今後の改善予定

1. **アニメーション**:
   - ボタンホバー時のパーティクルエフェクト
   - シーン遷移時のフェードイン/アウト

2. **サウンド**:
   - ボタンクリック音
   - ホバー音

3. **ServiceLocator統合**:
   - ISceneManagementService使用
   - Advanced Scene Manager統合

4. **セーブ/ロード**:
   - 最後にプレイしたゲームをハイライト
   - プレイ統計表示

## 参考リンク

- [UI Toolkit Documentation](https://docs.unity3d.com/Manual/UIElements.html)
- [UXML Reference](https://docs.unity3d.com/Manual/UIE-UXML.html)
- [USS Reference](https://docs.unity3d.com/Manual/UIE-USS.html)
- [Scene Management](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html)

---

**作成日**: 2026-03-08
**バージョン**: 1.0.0
**作成者**: Claude Code (Anthropic)
