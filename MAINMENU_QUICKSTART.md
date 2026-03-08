# MainMenu クイックスタートガイド

## はじめに

MainMenuは、ShaderOpゲームの中央ハブです。すべてのミニゲームとカスタマイズ画面への入口として機能します。このガイドでは、3分でMainMenuを起動する方法を説明します。

## 必要なもの

- Unity 2022.3 LTS 以上
- URP (Universal Render Pipeline)
- UI Toolkit パッケージ

## セットアップ（自動）

### ステップ1: エディターツールでシーン生成

1. Unityエディターを開く
2. メニューから **ShaderOp → Setup → MainMenu Scene** を選択
3. ダイアログが表示されたら「OK」をクリック

これだけで、完全に構成されたMainMenuシーンが生成されます！

### ステップ2: プレイテスト

1. **Play** ボタンを押す
2. 各ボタンをクリックして対応するシーンに移動
3. 実装済みのシーン:
   - ✅ **Tic-Tac-Toe Hex** - 3x3ヘックスボードゲーム
   - ✅ **Hex Reversi** - 37タイル大規模リバーシ
   - ✅ **Room Decoration** - 布シェーダーデモ

## UI構成

### ヘッダーセクション (15%)

- **GameTitle**: "ShaderOp" - ゲームタイトル
- **Subtitle**: "Hex Board Games Collection" - サブタイトル

### コンテンツセクション (70%)

#### Play Minigames

| ボタン | ステータス | 説明 |
|--------|-----------|------|
| Tic-Tac-Toe Hex (3x3) | ✅ 実装済み | 3x3グリッドの三目並べ |
| Hex Reversi (37 tiles) | ✅ 実装済み | 37タイルのリバーシゲーム |
| Hex Checkers | ⏳ Coming Soon | Phase 3で実装予定 |
| Hex Chess | ⏳ Coming Soon | Phase 3で実装予定 |

#### Customize

| ボタン | ステータス | 説明 |
|--------|-----------|------|
| Room Decoration | ✅ 実装済み | 4種類の布シェーダーデモ |
| Character | ⏳ Coming Soon | Phase 2後半で実装予定 |

### フッターセクション (15%)

- **VersionLabel**: "v0.2.0 - Phase 2 (55%)" - バージョン情報
- **Settings**: 設定画面 (将来実装予定)
- **Quit**: ゲーム終了

## 操作方法

### ミニゲームをプレイ

1. **"Play Minigames"** セクションから好きなゲームを選択
2. ボタンをクリック
3. ゲームが起動

**おすすめ**:
- 初めての方: **Tic-Tac-Toe Hex** (シンプルなルール)
- 戦略好き: **Hex Reversi** (深い戦略性)

### カスタマイズを試す

1. **"Customize"** セクションから **Room Decoration** を選択
2. 3Dルームで布の色をカスタマイズ
3. カーテン、ラグ、クッションの色を変更可能

### ゲームを終了

1. **Quit** ボタンをクリック
2. エディタではPlayモードが終了
3. ビルド版ではアプリケーションが終了

## セットアップ（手動）

自動セットアップを使わない場合の手順です。

### 1. シーン作成

1. 新しいシーンを作成 (`File → New Scene`)
2. シーンを保存: `Assets/Scenes/MainMenu.unity`

### 2. GameBootstrap作成

1. 空のGameObjectを作成 (名前: `GameBootstrap`)
2. タグを `GameController` に設定

### 3. カメラ設定

```
Position: (0, 0, -10)
Rotation: (0, 0, 0)
Projection: Perspective
FOV: 60
Clear Flags: Solid Color
Background: RGB(15, 20, 30)
```

### 4. UI作成

1. 空のGameObjectを作成 (名前: `MainMenuUI`)
2. **UIDocument** コンポーネントを追加
3. **Visual Tree Asset** に `Assets/UI/MainMenu.uxml` を設定
4. **MainMenuController** スクリプトをアタッチ

### 5. シーン保存

`Ctrl+S` または `File → Save` でシーンを保存

## バリデーション

### シーン検証

メニューから **ShaderOp → Validate → MainMenu Scene** を選択

以下を自動チェック:
- ✅ GameBootstrap存在
- ✅ Camera設定
- ✅ UI構造
- ✅ MainMenuController設定
- ✅ Assets (UXML/USS)

### UI要素検証

メニューから **ShaderOp → Validate → MainMenu UI Elements** を選択

すべてのボタンとラベルの存在を確認

### アセット確認

メニューから **ShaderOp → Validate → MainMenu Assets** を選択

UXML/USSファイルの存在を確認

## トラブルシューティング

### UIが表示されない

**症状**: 真っ黒な画面が表示される

**解決策**:
1. `ShaderOp → Validate → MainMenu Scene` を実行
2. Consoleでエラーログ確認
3. UIDocumentのVisualTreeAssetが設定されているか確認

### ボタンが押せない

**症状**: クリックしても反応しない

**解決策**:
1. EventSystemがシーンに存在するか確認
2. MainMenuControllerがアタッチされているか確認
3. Consoleでエラーログ確認

### シーンが切り替わらない

**症状**: ボタンを押してもシーンが変わらない

**解決策**:
1. **Build Settings** (`File → Build Settings`) を開く
2. 対象シーンが **Scenes In Build** リストに追加されているか確認
3. シーンがない場合:
   - `Add Open Scenes` をクリック
   - または対象シーンをドラッグ&ドロップ

**必要なシーン**:
- ✅ MainMenu
- ✅ TicTacToeHex
- ✅ HexReversi
- ✅ RoomDecoration

### スタイルが正しく表示されない

**症状**: 白背景でボタンが表示される

**解決策**:
1. `Assets/UI/MainMenu.uss` が存在するか確認
2. UXML内で `<Style src="MainMenu.uss"/>` が記述されているか確認
3. USSファイルを再インポート (`右クリック → Reimport`)

## カスタマイズ

### ボタンの色を変更

`Assets/UI/MainMenu.uss` を編集:

```css
#PlayTicTacToeBtn {
    background-color: rgb(60, 120, 80); /* 緑系 */
    border-color: rgb(80, 160, 100);
}

#PlayTicTacToeBtn:hover {
    background-color: rgb(80, 140, 100); /* ホバー時 */
    border-color: rgb(120, 200, 140);
}
```

### バージョン情報を更新

`MainMenuController.cs` の `UpdateVersionInfo()` メソッドを編集:

```csharp
private void UpdateVersionInfo()
{
    if (_versionLabel != null)
    {
        _versionLabel.text = "v1.0.0 - Release";
    }
}
```

### 新しいゲームを追加

1. **UXML編集** (`Assets/UI/MainMenu.uxml`):
```xml
<ui:Button name="NewGameBtn" text="New Game" class="menu-button"/>
```

2. **MainMenuController.cs編集**:
```csharp
private Button? _newGameBtn;

private void GetUIElements()
{
    // 既存のコード...
    _newGameBtn = _root.Q<Button>("NewGameBtn");
}

private void RegisterEventHandlers()
{
    // 既存のコード...
    if (_newGameBtn != null)
    {
        _newGameBtn.clicked += OnNewGameClicked;
    }
}

private void OnNewGameClicked()
{
    LoadScene("NewGame");
}
```

## パフォーマンス

### 期待される動作

- **起動時間**: < 1秒
- **メモリ使用量**: < 50 MB
- **UI応答性**: 即座 (60 FPS維持)

### 最適化のヒント

- UI要素は静的 (ランタイムで生成しない)
- イベントハンドラは適切に登録/解除
- Null安全を保証 (`#nullable enable`)

## よくある質問

### Q: Coming Soonボタンは機能しますか？

**A**: いいえ、Phase 3以降で実装予定です。現在はクリックしても何も起こりません。

### Q: Settingsボタンは何をしますか？

**A**: 現在は未実装です。将来的に音量設定、解像度設定などを実装予定です。

### Q: Build Settingsに追加するシーンは？

**A**: Phase 2時点では以下のシーンを追加:
- MainMenu
- TicTacToeHex
- HexReversi
- RoomDecoration

### Q: モバイルでも動作しますか？

**A**: はい、縦画面向けに最適化されています。Android/iOSビルドで動作します。

## 次のステップ

### ミニゲームをプレイ

1. **Tic-Tac-Toe Hex** で3x3グリッドの三目並べを体験
2. **Hex Reversi** で37タイルの大規模戦略ゲームを楽しむ

### カスタマイズを試す

1. **Room Decoration** で4種類の布シェーダーを確認
2. カーテン、ラグ、クッションの色を変更

### 開発を継続

1. Phase 3でHex Checkers/Chessを実装
2. Character Customizationを追加
3. オンラインマルチプレイ対応 (Photon)

## サポート

問題が解決しない場合:
1. Consoleログを確認
2. Validationツールを実行
3. `MAINMENU_IMPLEMENTATION.md` を参照
4. GitHubでIssueを作成

## リソース

- [完全実装ドキュメント](MAINMENU_IMPLEMENTATION.md)
- [UI Toolkit Documentation](https://docs.unity3d.com/Manual/UIElements.html)
- [TicTacToeHex実装](TICTACTOEHEX_COMPLETE_SUMMARY.md)
- [HexReversi実装](HEXREVERSI_COMPLETE_SUMMARY.md)

---

**最終更新**: 2026-03-08
**バージョン**: 1.0.0
