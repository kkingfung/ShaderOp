# Phase 4 Week 4 Task 2 - UI Integration Guide

**Date**: 2026-03-15  
**Phase**: Phase 4 Week 4 - Task 2  
**Status**: Implementation Guide  
**Agent**: unity-developer

---

## Executive Summary

このドキュメントは、AsyncTransitionManager と UIButtonSoundPlayer を全UIシーンに統合する手順を提供します。

**目標**: すべてのUIシーンでスムーズなトランジションとボタンサウンドフィードバックを実現

**対象シーン**:
1. MainMenu.unity
2. MainCustomization.unity
3. TicTacToeHex.unity
4. HexReversi.unity
5. HexCheckers.unity
6. HexChess.unity
7. RoomDecoration.unity

---

## 1. AsyncTransitionManager統合手順

### 1.1 前提条件

**必要なファイル**:
- `Assets/Scripts/Runtime/Core/UI/AsyncTransitionManager.cs` ✅ (Week 3作成済み)
- `Assets/UI/TransitionOverlay.uxml` ✅ (Week 3作成済み)
- `Assets/UI/Styles/Transitions.uss` ✅ (Week 3作成済み)

### 1.2 シーンごとの統合手順

#### Step 1: Persistent UI GameObjectの作成

**各シーンで実行**:

1. **Hierarchyで右クリック** → Create Empty
2. **名前を変更**: `TransitionUI` (推奨名)
3. **Tag設定**: `EditorOnly` または `DontDestroyOnLoad` (オプション)

#### Step 2: UIDocument コンポーネントの追加

1. `TransitionUI` GameObjectを選択
2. **Add Component** → UI Toolkit → **UI Document**
3. **Inspector設定**:
   - **Panel Settings**: `DefaultPanelSettings` (プロジェクト共通)
   - **Source Asset**: `Assets/UI/TransitionOverlay.uxml`
   - **Sort Order**: **999** (最前面表示)

#### Step 3: AsyncTransitionManager コンポーネントの追加

1. `TransitionUI` GameObjectを選択
2. **Add Component** → ShaderOp.Core.UI → **AsyncTransitionManager**
3. **Inspector設定**:
   ```
   Default Duration: 0.5 (秒)
   Show Loading Text: ✓ (チェック)
   Fade Color: Black (R:0, G:0, B:0, A:255)
   ```

#### Step 4: 動作確認

1. Play Modeで実行
2. Consoleで以下のログを確認:
   ```
   [AsyncTransitionManager] UI initialized
   ```
3. Scene遷移時にフェード効果が表示されることを確認

---

### 1.3 シーン別統合チェックリスト

#### MainMenu.unity

**統合手順**:
- [ ] `TransitionUI` GameObject作成
- [ ] UIDocument追加（TransitionOverlay.uxml）
- [ ] AsyncTransitionManager追加
- [ ] Inspector設定完了
- [ ] Play Modeテスト完了

**特記事項**:
- MainMenuは最初のシーンなので、フェードイン効果が重要
- Scene遷移ボタン（TicTacToe, Reversi等）にトランジション適用

**実装例** (MainMenuView.cs修正):
```csharp
using ShaderOp.Core.UI;
using Cysharp.Threading.Tasks;

public class MainMenuView : MonoBehaviour
{
    private AsyncTransitionManager? _transitionManager;

    private void Awake()
    {
        _transitionManager = FindObjectOfType<AsyncTransitionManager>();
    }

    private async void OnTicTacToeButtonClicked()
    {
        if (_transitionManager != null)
        {
            await _transitionManager.TransitionToSceneAsync("TicTacToeHex", 0.5f);
        }
        else
        {
            SceneManager.LoadScene("TicTacToeHex");
        }
    }
}
```

---

#### MainCustomization.unity

**統合手順**:
- [ ] `TransitionUI` GameObject作成
- [ ] UIDocument追加
- [ ] AsyncTransitionManager追加
- [ ] Inspector設定完了
- [ ] Play Modeテスト完了

**特記事項**:
- Backボタンでの遷移にトランジション適用
- カスタマイズプレビュー変更時はフェード不要（即座に反映）

---

#### TicTacToeHex.unity / HexReversi.unity / HexCheckers.unity / HexChess.unity

**統合手順** (4シーン共通):
- [ ] `TransitionUI` GameObject作成
- [ ] UIDocument追加
- [ ] AsyncTransitionManager追加
- [ ] Inspector設定完了
- [ ] Play Modeテスト完了

**特記事項**:
- ゲーム終了時の遷移（勝敗画面 → MainMenu）にトランジション適用
- ゲーム内UIは影響を受けない（Sort Order: 999で最前面）

**実装例** (各GameModel.cs修正):
```csharp
private async void OnGameEnd()
{
    var transitionManager = FindObjectOfType<AsyncTransitionManager>();
    if (transitionManager != null)
    {
        await UniTask.Delay(1000); // 1秒待機（勝敗表示）
        await transitionManager.TransitionToSceneAsync("MainMenu", 0.5f);
    }
}
```

---

#### RoomDecoration.unity

**統合手順**:
- [ ] `TransitionUI` GameObject作成
- [ ] UIDocument追加
- [ ] AsyncTransitionManager追加
- [ ] Inspector設定完了
- [ ] Play Modeテスト完了

**特記事項**:
- 部屋移動時のトランジション（オプション）
- MainMenuへの戻りにトランジション適用

---

## 2. UIButtonSoundPlayer統合手順

### 2.1 前提条件

**必要なファイル**:
- `Assets/Scripts/Runtime/Core/UI/UIButtonSoundPlayer.cs` ✅ (Week 3作成済み)
- `Assets/Audio/UI/button_hover.wav` ⚠️ (Task 3で作成予定)
- `Assets/Audio/UI/button_click.wav` ⚠️ (Task 3で作成予定)

### 2.2 シーンごとの統合手順

#### Step 1: UI GameObjectへのコンポーネント追加

**各シーンで実行**:

1. **UIDocumentを持つGameObjectを選択** (例: `TransitionUI` または既存のUIRoot)
2. **Add Component** → ShaderOp.Core.UI → **UIButtonSoundPlayer**
3. **Inspector設定**:
   ```
   Hover Sound: (AudioClip) Assets/Audio/UI/button_hover.wav
   Click Sound: (AudioClip) Assets/Audio/UI/button_click.wav
   Volume: 1.0
   Enable Hover Sound: ✓ (PC), ✗ (Mobile)
   Use Audio Manager: ✓ (推奨)
   ```

#### Step 2: ボタンへのクラス適用

**UXML修正** (各シーンのUXMLファイル):

```xml
<!-- Before -->
<ui:Button text="Start Game" name="StartButton" />

<!-- After -->
<ui:Button text="Start Game" name="StartButton" class="game-button" />
```

**適用対象**:
- すべてのインタラクティブボタン
- `.game-button` クラスを追加（PortraitMobile.ussのアニメーション適用）

#### Step 3: 動作確認

1. Play Modeで実行
2. ボタンにマウスホバー → `button_hover.wav` 再生確認
3. ボタンをクリック → `button_click.wav` 再生確認
4. Consoleでエラーがないことを確認

---

### 2.3 シーン別統合チェックリスト

#### MainMenu.unity

**統合手順**:
- [ ] UIButtonSoundPlayer追加（UIRootに）
- [ ] AudioClip設定（Hover, Click）
- [ ] UXMLのButtonに `.game-button` クラス追加
- [ ] Play Modeテスト完了

**対象ボタン**:
- TicTacToe Button
- Reversi Button
- Checkers Button
- Chess Button
- Customization Button
- Settings Button (もしあれば)
- Quit Button

---

#### MainCustomization.unity

**統合手順**:
- [ ] UIButtonSoundPlayer追加
- [ ] AudioClip設定
- [ ] UXMLのButtonに `.game-button` クラス追加
- [ ] Play Modeテスト完了

**対象ボタン**:
- Character Tab Button
- Mount Tab Button
- Hairstyle Arrow Buttons (< >)
- Color Arrow Buttons
- Reset Button
- Back Button

---

#### TicTacToeHex.unity / HexReversi.unity / HexCheckers.unity / HexChess.unity

**統合手順** (4シーン共通):
- [ ] UIButtonSoundPlayer追加
- [ ] AudioClip設定
- [ ] UXMLのButtonに `.game-button` クラス追加
- [ ] Play Modeテスト完了

**対象ボタン**:
- Restart Button
- Back to Menu Button
- Pause Button (もしあれば)

---

#### RoomDecoration.unity

**統合手順**:
- [ ] UIButtonSoundPlayer追加
- [ ] AudioClip設定
- [ ] UXMLのButtonに `.game-button` クラス追加
- [ ] Play Modeテスト完了

**対象ボタン**:
- すべてのインタラクティブボタン

---

## 3. USS統合（ボタンアニメーション）

### 3.1 PortraitMobile.uss確認

**Week 3で追加済み** のスタイル:

```css
.game-button {
    transition-property: scale, background-color;
    transition-duration: 0.2s;
    transition-timing-function: ease-out;
}

.game-button:hover {
    scale: 1.05;
    background-color: rgb(100, 150, 255);
}

.game-button:active {
    scale: 0.95;
    transition-duration: 0.1s;
}
```

### 3.2 バリアント適用（オプション）

**Primary ボタン** (推奨アクション):
```xml
<ui:Button text="Start Game" class="game-button game-button-primary" />
```

**Secondary ボタン** (通常アクション):
```xml
<ui:Button text="Settings" class="game-button game-button-secondary" />
```

**Danger ボタン** (危険な操作):
```xml
<ui:Button text="Quit" class="game-button game-button-danger" />
```

**Success ボタン** (成功アクション):
```xml
<ui:Button text="Apply" class="game-button game-button-success" />
```

---

## 4. 統合テストチェックリスト

### 4.1 AsyncTransitionManager テスト

**各シーンで確認**:
- [ ] Scene遷移時にフェードアウト（黒画面）
- [ ] ローディングテキスト表示（オプション）
- [ ] Scene読み込み完了
- [ ] フェードイン（画面表示）
- [ ] トランジション時間: 0.5秒
- [ ] 60fps維持（フレームドロップなし）
- [ ] Consoleエラーなし

### 4.2 UIButtonSoundPlayer テスト

**各シーンで確認**:
- [ ] ボタンホバーで `button_hover.wav` 再生（PC）
- [ ] ボタンクリックで `button_click.wav` 再生
- [ ] 音量適切（1.0）
- [ ] モバイルでホバー無効（タッチ操作）
- [ ] 複数ボタン連続クリックでサウンド重複再生
- [ ] Consoleエラーなし

### 4.3 ボタンアニメーション テスト

**各シーンで確認**:
- [ ] ホバー時にscale 1.05（拡大）
- [ ] クリック時にscale 0.95（縮小）
- [ ] トランジション0.2秒でスムーズ
- [ ] GPU加速（カクつきなし）
- [ ] モバイルでタッチ動作OK

---

## 5. トラブルシューティング

### 5.1 AsyncTransitionManager

**問題**: `[AsyncTransitionManager] FadePanel not found in UXML!`

**解決策**:
1. UIDocumentの `Source Asset` が `TransitionOverlay.uxml` になっているか確認
2. TransitionOverlay.uxmlに `<ui:VisualElement name="FadePanel">` が存在するか確認
3. Panel Settingsが正しく設定されているか確認

---

**問題**: フェードが表示されない

**解決策**:
1. UIDocumentの `Sort Order` を **999** に設定（最前面）
2. `FadePanel` のスタイルで `position: absolute` を確認
3. Play ModeでInspectorの `CurrentOpacity` を確認（0-1で変化するか）

---

### 5.2 UIButtonSoundPlayer

**問題**: サウンドが再生されない

**解決策**:
1. AudioClipが正しく設定されているか確認（Inspector）
2. `Use Audio Manager` をOFFにして `AudioSource.PlayClipAtPoint` で試す
3. Volumeが0になっていないか確認
4. AudioListenerがシーンに存在するか確認

---

**問題**: モバイルでホバーサウンドが再生される

**解決策**:
1. `Enable Hover Sound` を **OFF** に設定
2. または、UIButtonSoundPlayer.csで自動検出:
```csharp
#if UNITY_IOS || UNITY_ANDROID
    _enableHoverSound = false;
#endif
```

---

### 5.3 ボタンアニメーション

**問題**: ホバー/クリックアニメーションが動作しない

**解決策**:
1. UXMLのButtonに `.game-button` クラスが適用されているか確認
2. `PortraitMobile.uss` がUIDocumentで読み込まれているか確認
3. USS内の `transition-property` に `scale` が含まれているか確認
4. Runtime UI Debugger (Window → UI Toolkit → Debugger) でスタイル確認

---

## 6. コード例

### 6.1 Scene遷移時のトランジション使用例

**MainMenuView.cs**:
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.Core.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MainMenuView : MonoBehaviour
{
    private UIDocument? _uiDocument;
    private AsyncTransitionManager? _transitionManager;

    private Button? _ticTacToeButton;
    private Button? _reversiButton;
    private Button? _checkersButton;
    private Button? _chessButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _transitionManager = FindObjectOfType<AsyncTransitionManager>();
    }

    private void OnEnable()
    {
        if (_uiDocument?.rootVisualElement != null)
        {
            var root = _uiDocument.rootVisualElement;

            _ticTacToeButton = root.Q<Button>("TicTacToeButton");
            _reversiButton = root.Q<Button>("ReversiButton");
            _checkersButton = root.Q<Button>("CheckersButton");
            _chessButton = root.Q<Button>("ChessButton");

            RegisterEventHandlers();
        }
    }

    private void OnDisable()
    {
        UnregisterEventHandlers();
    }

    private void RegisterEventHandlers()
    {
        if (_ticTacToeButton != null)
            _ticTacToeButton.clicked += () => LoadSceneAsync("TicTacToeHex").Forget();

        if (_reversiButton != null)
            _reversiButton.clicked += () => LoadSceneAsync("HexReversi").Forget();

        if (_checkersButton != null)
            _checkersButton.clicked += () => LoadSceneAsync("HexCheckers").Forget();

        if (_chessButton != null)
            _chessButton.clicked += () => LoadSceneAsync("HexChess").Forget();
    }

    private void UnregisterEventHandlers()
    {
        if (_ticTacToeButton != null)
            _ticTacToeButton.clicked -= () => LoadSceneAsync("TicTacToeHex").Forget();

        // ... 他のボタンも同様
    }

    private async UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default)
    {
        if (_transitionManager != null)
        {
            await _transitionManager.TransitionToSceneAsync(sceneName, 0.5f, ct);
        }
        else
        {
            Debug.LogWarning("[MainMenuView] AsyncTransitionManager not found. Using direct load.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
```

---

### 6.2 ゲーム終了時のトランジション使用例

**HexChessModel.cs** (修正例):
```csharp
using ShaderOp.Core.UI;
using Cysharp.Threading.Tasks;

public class HexChessModel : MonoBehaviour
{
    private AsyncTransitionManager? _transitionManager;

    private void Awake()
    {
        _transitionManager = FindObjectOfType<AsyncTransitionManager>();
    }

    public void CheckWinCondition()
    {
        // ... 勝敗判定ロジック ...

        if (isGameOver)
        {
            OnGameEnd().Forget();
        }
    }

    private async UniTask OnGameEnd()
    {
        // 1. 勝敗表示（1秒待機）
        Debug.Log("Game Over!");
        await UniTask.Delay(1000);

        // 2. トランジション付きでMainMenuに戻る
        if (_transitionManager != null)
        {
            await _transitionManager.TransitionToSceneAsync("MainMenu", 0.5f);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
```

---

## 7. 完了条件

### Task 2完了チェックリスト

- [ ] **AsyncTransitionManager統合**:
  - [ ] MainMenu.unity
  - [ ] MainCustomization.unity
  - [ ] TicTacToeHex.unity
  - [ ] HexReversi.unity
  - [ ] HexCheckers.unity
  - [ ] HexChess.unity
  - [ ] RoomDecoration.unity

- [ ] **UIButtonSoundPlayer統合**:
  - [ ] MainMenu.unity
  - [ ] MainCustomization.unity
  - [ ] TicTacToeHex.unity
  - [ ] HexReversi.unity
  - [ ] HexCheckers.unity
  - [ ] HexChess.unity
  - [ ] RoomDecoration.unity

- [ ] **ボタンクラス適用**:
  - [ ] すべてのButtonに `.game-button` クラス追加
  - [ ] バリアント適用（Primary, Secondary, Danger, Success）

- [ ] **テスト完了**:
  - [ ] すべてのシーンでトランジション動作確認
  - [ ] すべてのシーンでボタンサウンド動作確認
  - [ ] すべてのシーンでボタンアニメーション動作確認
  - [ ] Consoleエラーなし
  - [ ] 60fps維持

---

## 8. 次のステップ

Task 2完了後、Task 3 (Audio Asset Integration) に進む:
- `button_hover.wav` の作成/取得
- `button_click.wav` の作成/取得
- AudioClip Inspector設定

---

**END OF DOCUMENT**
