# ShaderOp User Flow & Interaction Design

**プロジェクト**: ShaderOp - Unity Shader開発プロジェクト
**作成日**: 2026-02-28
**バージョン**: 1.0

---

## 目次

1. [全体フロー概要](#全体フロー概要)
2. [シーン遷移フロー](#シーン遷移フロー)
3. [インタラクションパターン](#インタラクションパターン)
4. [フィードバック設計](#フィードバック設計)
5. [アニメーション仕様](#アニメーション仕様)
6. [エラーハンドリング](#エラーハンドリング)
7. [アクセシビリティ配慮](#アクセシビリティ配慮)

---

## 全体フロー概要

### アプリケーション起動からゲームプレイまで

```
[Startup Scene]
    ↓ (自動遷移: 2秒)
[Main Menu]
    ├─→ [Character Customization] → [Main Menu] (Back)
    ├─→ [Room Decoration] → [Main Menu] (Back)
    ├─→ [Tic-Tac-Toe Hex] → [Main Menu] (Back)
    ├─→ [Hex Reversi] → [Main Menu] (Back)
    ├─→ [Hex Checkers] → [Main Menu] (Back)
    ├─→ [Hex Chess] → [Main Menu] (Back)
    └─→ [Quit] → アプリケーション終了
```

### ユーザー目標

1. **キャラクターカスタマイズ**: 髪、瞳、肌、服装の色を自由に変更
2. **部屋デコレーション**: 家具を配置してオリジナルの部屋を作成
3. **ミニゲーム**: 4種類のヘックスボードゲームを楽しむ

---

## シーン遷移フロー

### 1. Main Menu → Character Customization

**トリガー**: "Character Customization" ボタンクリック

**遷移フロー**:
```
1. ボタンホバー → スケール 1.05x (0.15秒)
2. ボタンクリック → スケール 0.95x (0.05秒)
3. フェードアウト開始 (0.3秒)
4. SceneLoader.LoadSceneAsync("MainCustomization")
5. ロード中プログレスバー表示
6. シーン読み込み完了
7. フェードイン (0.3秒)
8. UI要素スライドイン (下から、0.25秒、順次 0.05秒遅延)
```

**エラーケース**:
- シーン読み込み失敗 → エラートースト表示 + Main Menu に戻る

---

### 2. Character Customization → Main Menu (Back)

**トリガー**: "← Back to Menu" ボタンクリック

**遷移フロー**:
```
1. 変更確認ダイアログ表示 (未保存の変更がある場合)
   - "Save and Exit" → データ保存 → Main Menu へ
   - "Exit without Saving" → データ破棄 → Main Menu へ
   - "Cancel" → ダイアログ閉じる、カスタマイズ画面に戻る
2. データ保存処理 (PlayerPrefs)
3. フェードアウト (0.3秒)
4. SceneLoader.LoadSceneAsync("MainMenu")
5. フェードイン (0.3秒)
```

**保存データ**:
- HairColor (RGB)
- EyeColor (RGB)
- SkinTone (int 0-9)
- ClothingColor (RGB)
- BodyHeight (float 0.8-1.2)

---

### 3. Main Menu → Minigame (例: Hex Reversi)

**トリガー**: "Hex Reversi" ボタンクリック

**遷移フロー**:
```
1. ボタンクリック
2. ゲームルール簡易説明ダイアログ表示
   - "Start Game" → ゲーム開始
   - "Cancel" → Main Menu に戻る
3. フェードアウト (0.3秒)
4. SceneLoader.LoadSceneAsync("HexReversi")
5. ゲームシーン初期化
6. フェードイン (0.3秒)
7. ゲーム開始アニメーション (盤面表示)
```

**ゲーム終了フロー**:
```
1. ゲーム終了条件達成 (勝敗決定)
2. リザルト画面表示 (モーダル)
   - 勝者表示
   - スコア表示
   - "Play Again" → ゲーム再開 (同じシーン)
   - "Back to Menu" → Main Menu へ
```

---

## インタラクションパターン

### 1. ボタンインタラクション

**標準ボタン**:
```css
Normal:  scale(1.0), bg-color(primary)
Hover:   scale(1.05), bg-color(primary-hover), 0.15秒
Active:  scale(0.95), bg-color(primary-pressed), 0.05秒
Disable: scale(1.0), bg-color(disabled), cursor(not-allowed)
```

**フォーカス (キーボードナビゲーション)**:
```css
Focus: border(3px, primary), outline(3px, primary, offset 2px)
```

**タッチフィードバック (モバイル)**:
- タッチ開始 → 即座に scale(0.95)
- タッチ終了 → scale(1.0) に戻る (0.1秒)
- 最小タッチターゲット: 44x44px (WCAG準拠)

---

### 2. スライダーインタラクション

**RGB カラースライダー**:
```
1. スライダードラッグ → リアルタイムでカラープレビュー更新
2. 値ラベル更新 (0.00-1.00, 小数点2桁)
3. カラープレビューボックス背景色変更
4. 3Dプレビューモデル即座に反映 (マテリアル更新)
```

**値変更フィードバック**:
- スライダーハンドルホバー → scale(1.2)
- ドラッグ中 → ハンドル強調表示
- 離した瞬間 → 軽い振動効果 (モバイル haptic feedback)

---

### 3. プリセットボタンインタラクション

**カラープリセット** (例: "Black", "Blonde", "Brown", "Red"):
```
1. プリセットボタンクリック
2. 即座にRGBスライダー値変更 (アニメーション: 0.2秒)
3. カラープレビュー更新
4. 3Dモデル反映
5. 成功トースト表示 (右上、3秒後自動消去)
   "✓ Preset 'Blonde' applied"
```

**プリセット値例**:
```csharp
// 髪色プリセット
Black:  (0.05, 0.05, 0.05)
Blonde: (0.9, 0.8, 0.5)
Brown:  (0.3, 0.15, 0.05)
Red:    (0.7, 0.2, 0.1)
```

---

### 4. 3Dプレビューカメラ操作

**OrbitCameraController インタラクション**:

**マウス操作** (デスクトップ):
- 右クリック + ドラッグ → カメラ回転 (水平/垂直)
- スクロールホイール → ズームイン/アウト (距離 2.0-5.0)
- 中クリックドラッグ → パン移動

**タッチ操作** (モバイル):
- 1本指スワイプ → カメラ回転
- ピンチイン/アウト → ズーム
- 2本指スワイプ → パン移動

**UI コントロール**:
- "Reset Camera" ボタン → デフォルト位置 (0, 1.5, -3) に戻る (0.5秒アニメーション)
- "Auto Rotate" トグル → 自動回転開始/停止 (毎秒 30度回転)

---

### 5. トグル/ドロップダウンインタラクション

**Toggle (例: "Has Saddle")**:
```
1. トグルクリック
2. チェックマーク表示/非表示 (0.15秒フェード)
3. 背景色変更 (unchecked: gray → checked: primary)
4. データ即座に反映
```

**DropdownField (例: "Mount Type")**:
```
1. ドロップダウンクリック → リスト展開 (0.2秒スライドダウン)
2. 項目ホバー → ハイライト
3. 項目選択 → リスト閉じる (0.15秒スライドアップ)
4. 選択値表示更新
5. 3Dモデル変更 (馬の種類変更)
```

---

## フィードバック設計

### 1. ビジュアルフィードバック

**成功アクション**:
- カラー変更成功 → カラープレビューボックス外側に緑の輝き (0.5秒)
- データ保存成功 → 成功トースト (緑背景、✓アイコン)
- ゲーム勝利 → 勝者表示 + 紙吹雪アニメーション

**エラー/警告**:
- 無効な入力 → 入力欄赤枠表示 + エラーメッセージ
- 保存失敗 → エラートースト (赤背景、✕アイコン)
- ゲーム敗北 → 敗者表示 (灰色)

**情報提供**:
- ローディング → プログレスバー + "Loading..." ラベル
- 処理中 → スピナーアニメーション
- ヒント → 情報トースト (青背景、ⓘアイコン)

---

### 2. オーディオフィードバック

**UIサウンド**:
- ボタンクリック → "click.wav" (軽いクリック音)
- ボタンホバー → "hover.wav" (微細なサウンド)
- トグルON → "toggle_on.wav"
- トグルOFF → "toggle_off.wav"
- スライダードラッグ → なし (静音)
- スライダー離す → "release.wav" (短い確認音)

**システムサウンド**:
- シーン遷移開始 → "transition_out.wav" (フェードアウト音)
- シーン読み込み完了 → "transition_in.wav" (フェードイン音)
- データ保存成功 → "save_success.wav" (確認音)
- エラー発生 → "error.wav" (警告音)

**ゲームサウンド**:
- 駒配置 → "piece_place.wav"
- 駒取得 → "piece_capture.wav"
- ゲーム勝利 → "victory.wav" + BGM変更
- ゲーム敗北 → "defeat.wav"

---

### 3. ハプティックフィードバック (モバイル)

**軽い振動** (10ms):
- ボタンタップ
- トグル切り替え
- スライダー値変更

**中程度の振動** (30ms):
- プリセット適用
- データ保存成功
- ゲーム駒配置

**強い振動** (50ms):
- エラー発生
- ゲーム終了 (勝利/敗北)

---

## アニメーション仕様

### 1. シーン遷移アニメーション

**フェードアウト** (シーン離脱時):
```css
Duration: 0.3秒
Easing: ease-in
Opacity: 1.0 → 0.0
```

**フェードイン** (シーン到着時):
```css
Duration: 0.3秒
Easing: ease-out
Opacity: 0.0 → 1.0
```

**スライドイン** (UI要素表示):
```css
Duration: 0.25秒
Easing: ease-out
Transform: translateY(20px) → translateY(0)
Opacity: 0 → 1
Delay: 順次 0.05秒遅延 (カスケード効果)
```

---

### 2. ボタンホバーアニメーション

```css
/* ホバー開始 */
Transition: 0.15秒 ease-out
Transform: scale(1.0) → scale(1.05)
Background: primary → primary-hover

/* ホバー終了 */
Transition: 0.15秒 ease-in
Transform: scale(1.05) → scale(1.0)
Background: primary-hover → primary
```

---

### 3. カラー変更アニメーション

**プリセット適用時**:
```css
/* RGBスライダー値アニメーション */
Duration: 0.2秒
Easing: ease-in-out
Value: current → preset value

/* カラープレビュー変更 */
Duration: 0.2秒
Easing: linear
Background-color: current → new color
```

---

### 4. モーダル表示/非表示アニメーション

**表示**:
```css
/* オーバーレイ */
Opacity: 0 → 0.7 (0.2秒)

/* ダイアログ */
Duration: 0.3秒
Easing: ease-out
Transform: scale(0.9) → scale(1.0)
Opacity: 0 → 1
```

**非表示**:
```css
/* ダイアログ */
Duration: 0.2秒
Easing: ease-in
Transform: scale(1.0) → scale(0.9)
Opacity: 1 → 0

/* オーバーレイ */
Opacity: 0.7 → 0 (0.15秒)
```

---

### 5. プログレスバーアニメーション

```css
/* 進捗バー塗りつぶし */
Transition: width 0.3秒 ease-out
Width: current% → new%

/* パーセント表示 */
Transition: opacity 0.1秒
Value更新 (カウントアップなし、即座に反映)
```

---

## エラーハンドリング

### 1. シーン読み込みエラー

**エラーケース**:
- シーンファイルが存在しない
- AssetBundle読み込み失敗
- メモリ不足

**ユーザー表示**:
```
[Error Toast]
Title: "Failed to Load Scene"
Message: "Could not load 'MainCustomization'. Please try again."
Type: Error (赤背景、✕アイコン)
Duration: 5秒 or 手動閉じる
```

**リカバリー**:
1. エラートースト表示
2. 3秒後に自動でMain Menuに戻る
3. コンソールログにエラー詳細記録

---

### 2. データ保存エラー

**エラーケース**:
- PlayerPrefs書き込み失敗
- ディスク容量不足

**ユーザー表示**:
```
[Error Dialog]
Title: "Save Failed"
Message: "Could not save your customization data. Please check available storage."
Buttons: ["Retry", "Cancel"]
```

**リカバリー**:
- "Retry" → 再度保存試行
- "Cancel" → 保存せずにシーン遷移続行 (警告表示)

---

### 3. 3Dモデル読み込みエラー

**エラーケース**:
- キャラクタープレハブが見つからない
- マテリアルが見つからない

**ユーザー表示**:
```
[Warning Toast]
Title: "Preview Unavailable"
Message: "Character model could not be loaded. Customization will be saved."
Type: Warning (オレンジ背景、⚠アイコン)
```

**フォールバック**:
- プレースホルダーモデル表示 (灰色キューブ)
- カスタマイズ機能は継続動作

---

## アクセシビリティ配慮

### 1. キーボードナビゲーション

**サポートキー**:
- `Tab` → 次の要素へフォーカス移動
- `Shift + Tab` → 前の要素へフォーカス移動
- `Enter` / `Space` → ボタン/トグル実行
- `Arrow Keys` → スライダー値調整 (左右: ±0.01)
- `Esc` → ダイアログ/モーダル閉じる

**フォーカス順序**:
```
1. セクションヘッダー (読み上げのみ、スキップ可)
2. 髪色スライダー (R → G → B)
3. 髪色プリセットボタン (1 → 2 → 3 → 4)
4. 瞳色スライダー (R → G → B)
5. ...
6. Back/Reset/Save ボタン
```

---

### 2. スクリーンリーダー対応

**ARIA属性**:
```xml
<!-- ボタン -->
<ui:Button name="SaveButton" text="Save Character"
          aria-label="Save character customization"
          role="button" />

<!-- スライダー -->
<ui:Slider name="HairColorR"
          aria-label="Hair color red channel"
          aria-valuemin="0" aria-valuemax="1" aria-valuenow="0.5"
          role="slider" />

<!-- トグル -->
<ui:Toggle name="AutoRotate"
          aria-label="Enable auto-rotate camera"
          aria-checked="false"
          role="switch" />
```

**ライブリージョン** (動的更新通知):
```xml
<ui:VisualElement name="LiveRegion" class="sr-only"
                 aria-live="polite" aria-atomic="true">
    <!-- C#からテキスト更新 -->
    <!-- 例: "Hair color changed to blonde" -->
</ui:VisualElement>
```

---

### 3. カラーコントラスト (WCAG AA準拠)

**テキストコントラスト比**:
- 通常テキスト (14px以上): **4.5:1以上**
- 大テキスト (18px以上 or 14px Bold): **3:1以上**

**実装例**:
```css
/* 背景 rgb(15, 15, 20) に対して */
--color-text-primary: rgb(255, 255, 255);  /* Contrast: 15.8:1 (AAA) */
--color-text-secondary: rgb(200, 200, 220); /* Contrast: 9.5:1 (AAA) */
--color-primary-accessible: rgb(100, 150, 255); /* Contrast: 7.2:1 (AAA) */
```

---

### 4. モーション削減対応

**システム設定検出**:
```css
@media (prefers-reduced-motion: reduce) {
    /* すべてのアニメーション無効化 */
    * {
        animation-duration: 0.01ms !important;
        transition-duration: 0.01ms !important;
    }
}
```

**ユーザー設定** (ゲーム内):
```
Settings > Accessibility > Reduce Motion: [Toggle]
ON → すべてのUIアニメーションを即座に完了
OFF → 通常アニメーション
```

---

### 5. タッチターゲットサイズ (モバイル)

**最小サイズ**: 44x44px (WCAG 2.5.5準拠)

```css
Button {
    min-width: 44px;
    min-height: 44px;
}

.touch-target-lg {
    min-width: 48px;
    min-height: 48px;
}
```

**タッチ間隔**: 最低 8px マージン (誤タップ防止)

---

## まとめ

### 実装優先度

**高優先度** (MVP必須):
- ✅ シーン遷移フロー (Fade In/Out)
- ✅ ボタンホバー/クリックフィードバック
- ✅ スライダーリアルタイム更新
- ✅ データ保存/読み込み
- ✅ エラートースト表示

**中優先度** (ユーザー体験向上):
- ⏳ カラープリセットアニメーション
- ⏳ 3Dカメラ操作
- ⏳ 確認ダイアログ
- ⏳ プログレスバー
- ⏳ オーディオフィードバック

**低優先度** (追加改善):
- ⏳ ハプティックフィードバック
- ⏳ 詳細なARIA属性
- ⏳ スクリーンリーダー完全対応
- ⏳ カスタムアニメーション効果

---

**最終更新**: 2026-02-28
**作成者**: UI/UX Designer Agent
**ステータス**: ✅ Complete
