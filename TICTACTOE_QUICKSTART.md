# TicTacToeHex Vertical Slice - クイックスタートガイド

## 5分でプレイ可能にする手順

### 方法1: 自動セットアップ（推奨）

1. **Unityエディターを開く**
   ```
   Unity 2022.3以上でプロジェクトを開く
   ```

2. **TicTacToeHex.unity シーンを開く**
   ```
   Assets/Scenes/TicTacToeHex.unity
   ```

3. **自動セットアップを実行**
   ```
   メニューバー → ShaderOp → Setup → TicTacToeHex Vertical Slice
   ```

4. **Play Mode実行**
   ```
   Ctrl + P または Playボタンクリック
   ```

5. **ゲームプレイ**
   - タイルにマウスホバー → ハイライト
   - クリック → 駒配置
   - 3連続で勝利！

---

### 方法2: 手動セットアップ

#### ステップ1: TicTacToeHexGameオブジェクト作成
1. Hierarchy右クリック → `Create Empty`
2. 名前を `TicTacToeHexGame` に変更
3. `Add Component` → `TicTacToeHexVerticalSlice`

#### ステップ2: BoardParent作成
1. TicTacToeHexGame右クリック → `Create Empty`
2. 名前を `BoardParent` に変更

#### ステップ3: Inspector設定

**ゲーム設定**:
- Board Parent: `BoardParent` をドラッグ
- Tile Prefab: `Assets/Prefabs/Minigames/HexTile.prefab`
- Player1 Piece Prefab: `Assets/Prefabs/Minigames/Player1Piece.prefab`
- Player2 Piece Prefab: `Assets/Prefabs/Minigames/Player2Piece.prefab`

**マテリアル**:
- Hex Tile Idle Material: `Assets/Materials/Minigames/MAT_HexTile_Idle.mat`
- Hex Tile Hover Material: `Assets/Materials/Minigames/MAT_HexTile_Hover.mat`
- Hex Tile Selected Material: `Assets/Materials/Minigames/MAT_HexTile_Selected.mat`
- Player1 Piece Material: `Assets/Materials/Minigames/MAT_Player1Piece.mat`
- Player2 Piece Material: `Assets/Materials/Minigames/MAT_Player2Piece.mat`

**レイアウト設定**:
- Tile Spacing: `1.2`
- Board Offset: `X: 0, Y: 2`

#### ステップ4: UIキャンバス作成
1. Hierarchy右クリック → `UI → Canvas`
2. Canvas名を `UI_Canvas` に変更
3. CanvasScaler設定:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `X: 1080, Y: 1920`
   - Match: `0.5`

#### ステップ5: ゲームパネル作成
1. UI_Canvas右クリック → `UI → Panel`
2. 名前を `GamePanel` に変更
3. RectTransform設定:
   - Anchor Min: `X: 0, Y: 0`
   - Anchor Max: `X: 1, Y: 0.4`
   - Left/Right/Top/Bottom: すべて `0`
4. Image色: `R: 0.1, G: 0.1, B: 0.15, A: 0.9`

#### ステップ6: ターン表示テキスト
1. GamePanel右クリック → `UI → Text`
2. 名前を `TurnIndicatorText` に変更
3. RectTransform:
   - Anchor Min: `X: 0.1, Y: 0.6`
   - Anchor Max: `X: 0.9, Y: 0.9`
4. Text設定:
   - Text: `プレイヤー1のターン`
   - Font Size: `48`
   - Alignment: Center
   - Color: White

#### ステップ7: ゲーム状態テキスト
1. GamePanel右クリック → `UI → Text`
2. 名前を `GameStatusText` に変更
3. RectTransform:
   - Anchor Min: `X: 0.1, Y: 0.4`
   - Anchor Max: `X: 0.9, Y: 0.6`
4. Text設定:
   - Text: （空）
   - Font Size: `36`
   - Alignment: Center
   - Color: Yellow

#### ステップ8: リセットボタン
1. GamePanel右クリック → `UI → Button`
2. 名前を `ResetButton` に変更
3. RectTransform:
   - Anchor Min: `X: 0.3, Y: 0.1`
   - Anchor Max: `X: 0.7, Y: 0.35`
4. Button Image色: `R: 0.3, G: 0.6, B: 1`
5. Text設定: `リセット`, Font Size: `32`

#### ステップ9: UI参照を設定
TicTacToeHexGameのInspectorで:
- UI Canvas: `UI_Canvas` のCanvasコンポーネント
- UI Panel: `GamePanel`
- Turn Indicator Text: `TurnIndicatorText`
- Game Status Text: `GameStatusText`
- Reset Button: `ResetButton`

#### ステップ10: カメラ設定確認
Main Camera:
- Position: `X: 0, Y: 0, Z: -10`
- Projection: `Orthographic`
- Size: `5`

#### ステップ11: Play!
Playボタンを押してゲーム開始！

---

## トラブルシューティング

### タイルが表示されない
- BoardParentが設定されているか確認
- TilePrefabが正しく設定されているか確認
- Consoleでエラーメッセージを確認

### タイルをクリックできない
- HexTile PrefabにBoxCollider2Dがあるか確認
- EventSystemがシーンにあるか確認
- Camera設定が正しいか確認

### シェーダーが動かない
- Materialが正しく設定されているか確認
- Materialのシェーダーが正しいか確認
  - MAT_HexTile_Idle → `Shader Graphs/SG_HexTile_Interactive`
  - MAT_Player1Piece → `Shader Graphs/SG_GamePiece_2D`

### UIが表示されない
- CanvasのRender Modeが`Screen Space - Overlay`か確認
- GamePanelのRectTransformが正しく設定されているか確認

### 駒が配置されない
- Player1Piece/Player2Piece Prefabが設定されているか確認
- Consoleで`[TicTacToeHexVerticalSlice]`のログを確認

---

## 動作確認チェックリスト

- [ ] シーン起動で3x3グリッドが表示される
- [ ] タイルにマウスホバーで明るくなる
- [ ] タイルクリックで駒が配置される（青/赤）
- [ ] 駒がフェードインアニメーションする
- [ ] ターン表示が更新される
- [ ] 3連続で勝利メッセージが表示される
- [ ] リセットボタンでゲームが再開する

---

## デバッグモード

### Consoleログ確認
Play Modeで以下のログが表示されるはずです:
```
[TicTacToeHexVerticalSlice] ゲーム初期化完了
[TicTacToeHexVerticalSlice] 9個のタイルを生成しました
[TicTacToeHexVerticalSlice] 駒を配置: (0, 0), Player1
...
```

### Scene Viewでの確認
- Hierarchyで `BoardParent` を展開
- 9個の `HexTile_(座標)` が生成されているか確認
- 各タイルに `HexTileShaderController` と `TileClickHandler` があるか確認

---

## パフォーマンスチェック

### Stats確認（Play Mode）
- FPS: `60` 維持
- Batches: `< 20` 推奨
- SetPass calls: `< 10` 推奨

### Profiler確認
1. Window → Analysis → Profiler
2. CPU Usage確認
3. Rendering確認

---

## 次のステップ

1. **オンライン対戦追加**
   - Photon PUN2統合
   - マッチメイキング

2. **AI対戦追加**
   - MinMaxアルゴリズム
   - 難易度設定

3. **ビジュアル強化**
   - 勝利ラインアニメーション
   - パーティクルエフェクト
   - サウンドエフェクト

4. **他のミニゲーム実装**
   - HexCheckers
   - HexChess
   - HexReversi

---

**作成日**: 2026-03-07
**最終更新**: 2026-03-07
