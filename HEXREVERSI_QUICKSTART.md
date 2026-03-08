# HexReversi クイックスタートガイド

## はじめに

HexReversiは、六角形グリッド上でプレイするリバーシ（オセロ）ゲームです。このガイドでは、5分でゲームを起動してプレイする方法を説明します。

## 必要なもの

- Unity 2022.3 LTS 以上
- URP (Universal Render Pipeline)
- UniTask パッケージ
- TextMeshPro パッケージ

## セットアップ（自動）

### ステップ1: エディターツールでシーン生成

1. Unityエディターを開く
2. メニューから **ShaderOp → Setup → HexReversi Complete Scene** を選択
3. ダイアログが表示されたら「OK」をクリック

これだけで、完全に構成されたシーンが生成されます！

### ステップ2: プレイテスト

1. **Play** ボタンを押す
2. タイルをクリックして駒を配置
3. 有効な手のみ配置可能（無効な手はクリックしても何も起こりません）

## ゲームの遊び方

### 基本ルール

1. **目的**: 最終的により多くの駒を持っているプレイヤーが勝利
2. **駒の配置**: 相手の駒を挟める場所にのみ配置可能
3. **駒の反転**: 挟んだ駒はすべて自分の色に変わる
4. **ターン**: 交互に駒を配置
5. **スキップ**: 置ける場所がない場合は自動的にスキップ
6. **ゲーム終了**: すべてのタイルが埋まるか、両者とも置けなくなったら終了

### UI操作

#### スコア表示
- **Player 1**: 青い駒の数
- **Player 2**: 赤い駒の数

#### ターン表示
現在どちらのプレイヤーのターンかを表示

#### Show Hintsトグル
- **ON**: 有効な手のタイルが緑色に光る
- **OFF**: ヒント非表示

#### Resetボタン
ゲームを最初からやり直す

#### Back to Menuボタン
メインメニューに戻る（実装予定）

### 視覚効果

#### タイルの状態
- **通常**: グレー
- **ホバー**: 明るくなる
- **有効手（ヒント表示時）**: 緑色に光る（点滅）

#### 駒のアニメーション
- **配置時**: フェードイン
- **反転時**: フェードアウト → 色変更 → フェードイン

## セットアップ（手動）

自動セットアップを使わない場合の手順です。

### 1. シーン作成

1. 新しいシーンを作成
2. **GameBootstrap** オブジェクトを追加（Prefabから）

### 2. カメラ設定

```
Position: (0, 10, -5)
Rotation: (60, 0, 0)
FOV: 60
Clear Flags: Solid Color
Background: (0.1, 0.1, 0.15)
```

### 3. ゲームコントローラー作成

1. 空のGameObjectを作成（名前: HexReversiController）
2. `HexReversiComplete` スクリプトをアタッチ
3. 以下のフィールドを設定:

#### Prefab References
- **Hex Tile Prefab**: `Assets/Prefabs/Minigames/HexTile.prefab`
- **Game Piece Prefab**: `Assets/Prefabs/Minigames/Player1Piece.prefab`

#### Materials
- **Hex Tile Material**: `Assets/Materials/Minigames/MAT_HexTile_Interactive.mat`
- **Player1 Piece Material**: `Assets/Materials/Minigames/MAT_Player1Piece.mat`
- **Player2 Piece Material**: `Assets/Materials/Minigames/MAT_Player2Piece.mat`

#### Grid Settings
- **Grid Radius**: 3
- **Hex Size**: 1.0
- **Tile Spacing**: 0.1

### 4. UI作成

#### Canvas
- **Render Mode**: Screen Space - Overlay
- **Canvas Scaler**:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1080 x 1920
  - Match: 0.5

#### UI Panel
- **Anchor**: Bottom 40% of screen
- **Background Color**: (0.1, 0.1, 0.1, 0.8)

#### UI要素（すべてTextMeshProGUI）
- Player1ScoreText: Position (-200, 150)
- Player2ScoreText: Position (200, 150)
- TurnIndicatorText: Position (0, 80)
- GameResultText: Position (0, 0), Initially Hidden
- ShowHintsToggle: Position (-200, -50)
- ResetButton: Position (0, -150)
- BackButton: Position (0, -230)

### 5. UI参照を設定

HexReversiCompleteコンポーネントのUI Referencesセクションに、作成したUI要素をドラッグ&ドロップ

## バリデーション

### シーン検証

メニューから **ShaderOp → Validate → HexReversi Scene** を選択

以下を自動チェック:
- GameBootstrap存在
- HexReversiComplete設定
- Prefab/Material参照
- UI要素
- カメラ設定
- シェーダープロパティ

### シェーダー統合チェック

メニューから **ShaderOp → Validate → Check Shader Integration** を選択

マテリアルのシェーダープロパティを検証

### パフォーマンスプロファイル

メニューから **ShaderOp → Validate → Performance Profile** を選択

期待されるパフォーマンス指標を確認

## トラブルシューティング

### タイルがクリックできない

**症状**: タイルをクリックしても何も起こらない

**解決方法**:
1. タイルPrefabに`Collider`があるか確認
2. シーンに`EventSystem`があるか確認
3. Cameraに`Physics Raycaster`がアタッチされているか確認

### アニメーションが動かない

**症状**: 駒が突然現れる（フェードしない）

**解決方法**:
1. UniTaskパッケージがインストールされているか確認
2. マテリアルに`_Fade`プロパティがあるか確認
3. `GamePieceShaderAnimator`がアタッチされているか確認

### ヒントが表示されない

**症状**: "Show Hints"をONにしても何も表示されない

**解決方法**:
1. マテリアルに`_GlowIntensity`プロパティがあるか確認
2. `HexTileShaderController`がアタッチされているか確認
3. シェーダーが正しくコンパイルされているか確認

### パフォーマンスが悪い

**症状**: カクつく、FPSが低い

**解決方法**:
1. すべてのマテリアルで**GPU Instancing**を有効化
2. Profilerを開いて原因を特定
3. Stats windowでドローコール数を確認（目標: 10以下）
4. Quality SettingsでVSyncをOFFにしてテスト

### シェーダーが正しく表示されない

**症状**: タイルや駒が正しい色で表示されない

**解決方法**:
1. URPアセットが正しく設定されているか確認
2. Shader Graphがエラーなくコンパイルされているか確認
3. マテリアルのシェーダーが正しく設定されているか確認
4. カメラのレンダリングパスがURPになっているか確認

## プレイのヒント

### 初心者向け戦略

1. **角を取る**: 角の駒は反転されないため、優位に立てます
2. **辺を制圧**: 辺の駒も比較的安全
3. **序盤は少なく**: 序盤で多く取りすぎると後半不利になることも
4. **ヒントを活用**: "Show Hints"をONにして有効手を確認

### 上級者向け戦略

1. **パリティ**: 最後の1手を打てるようにコントロール
2. **種まき**: わざと駒を少なく保ち、後半で逆転
3. **開放度理論**: 相手の選択肢を減らす手を選ぶ

## 次のステップ

### AI対戦を実装する

`HexReversiComplete.cs`に以下を追加:

```csharp
private async void PlayAIMove()
{
    List<HexCoordinate> validMoves = _model.GetValidMoves();
    if (validMoves.Count == 0) return;

    // 最も多く反転できる手を選択
    HexCoordinate bestMove = validMoves[0];
    int maxFlips = 0;

    foreach (var move in validMoves)
    {
        int flips = GetFlippedTiles(move, _model.CurrentPlayer).Count;
        if (flips > maxFlips)
        {
            maxFlips = flips;
            bestMove = move;
        }
    }

    // AIの思考時間（演出）
    await UniTask.Delay(500);

    // 手を実行
    OnTileClicked(bestMove);
}
```

### オンライン対戦を実装する

Photon Unity Networkingを使用して、リアルタイム対戦を実装できます。

### カスタムルールを追加する

- タイムリミット
- 特殊マス（2倍得点など）
- パワーアップアイテム

## サポート

問題が解決しない場合:
1. Consoleログを確認
2. Validationツールを実行
3. `HEXREVERSI_IMPLEMENTATION.md`を参照
4. GitHubでIssueを作成

## リソース

- [完全実装ドキュメント](HEXREVERSI_IMPLEMENTATION.md)
- [TicTacToeHex実装](ShaderOptimizer/Assets/Scripts/Runtime/Minigames/Games/TicTacToeHex*.cs)
- [HexGrid実装](ShaderOptimizer/Assets/Scripts/Runtime/Minigames/HexGrid/)

---

**最終更新**: 2026-03-08
**バージョン**: 1.0.0
