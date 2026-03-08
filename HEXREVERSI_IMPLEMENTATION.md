# HexReversi 完全実装 - 技術ドキュメント

## 概要

HexReversiは、六角形グリッド上で行うリバーシ（オセロ）ゲームです。TicTacToeHexの垂直スライス実装をベースに、より大規模な7x7グリッド（37タイル）でのゲームプレイを実現しています。

## 技術スタック

- **Unity 2022.3 LTS** (URP)
- **C# 11** (`#nullable enable`)
- **UniTask** - 非同期アニメーション処理
- **UI Toolkit** - 縦画面UI（未実装、TextMeshProGUI使用）
- **Shader Graph** - カスタムシェーダー
- **MVC Pattern** - ゲームロジック分離

## アーキテクチャ

### コンポーネント構成

```
HexReversiComplete (MonoBehaviour)
├── Model: HexReversiModel
│   └── Grid: HexGrid (37 tiles, radius 3)
├── View: Runtime-generated GameObjects
│   ├── GridContainer (37 HexTile instances)
│   └── PiecesContainer (Dynamic GamePiece instances)
└── Controller: HexReversiComplete
    ├── Event handling (click, hover)
    ├── Animation orchestration
    └── UI updates
```

### クラス設計

#### HexReversiComplete.cs
**責務**: ゲーム全体の統合・制御
- グリッド生成とビジュアル管理
- プレイヤー入力処理
- アニメーション制御
- UI更新

**主要メソッド**:
```csharp
// ゲーム初期化
private void InitializeGame()

// タイルクリック処理（async）
private async void OnTileClicked(HexCoordinate coord)

// 駒配置アニメーション
private async UniTask PlacePieceAsync(HexCoordinate coord, PieceType piece)

// 複数駒反転アニメーション
private async UniTask FlipPiecesAsync(List<HexCoordinate> coords, PieceType newPiece)
```

#### HexReversiModel.cs
**責務**: ゲームロジック・ルール管理
- 有効手判定 (`IsValidMove`)
- 駒配置と反転処理 (`PlacePiece`)
- 勝敗判定 (`CheckWinCondition`)
- スコア計算 (`GetPieceCounts`)

**グリッド生成**:
```csharp
// 半径3の六角形グリッド = 37タイル
Grid.GenerateHexagon(GRID_RADIUS = 3);
```

## シェーダー統合

### HexTileShaderController
**用途**: タイルの状態表示

**状態遷移**:
- `Normal (0)`: デフォルト状態
- `Hover (1)`: マウスホバー時
- `Selected (2)`: 選択状態（未使用）
- `Disabled (3)`: 無効状態（未使用）

**有効手ヒント機能**:
```csharp
// グロー表示ON/OFF
shader.ShowValidMoveGlow(bool show, float intensity)

// 点滅速度設定
shader.SetGlowSpeed(float speed)
```

### GamePieceShaderAnimator
**用途**: 駒のアニメーション

**主要機能**:
```csharp
// プレイヤーカラー設定
animator.SetPlayerColor(Color color, float tintStrength)

// フェードインアニメーション（配置時）
await animator.FadeIn(float duration)

// フェードアウトアニメーション（反転前）
await animator.FadeOut(float duration)
```

## ゲームフロー

### 1. 初期化フェーズ
1. `HexReversiModel`作成・初期化
2. 37タイルのグリッド生成
3. タイルビジュアル生成（Prefabインスタンス化）
4. シェーダーコントローラー取得/追加
5. クリックイベント設定
6. 初期駒配置（4駒）

### 2. ゲームプレイフェーズ
1. プレイヤーがタイルをクリック
2. 有効手判定 (`IsValidMove`)
3. 反転される駒のリストを取得 (`GetFlippedTiles`)
4. 駒配置アニメーション (`PlacePieceAsync`)
5. 複数駒を同時に反転アニメーション (`FlipPiecesAsync`)
6. スコア・ターン表示更新
7. 勝敗判定
8. ターン交代（有効手がない場合はスキップ）

### 3. ゲーム終了フェーズ
1. すべてのタイルが埋まる、または両者とも置けない
2. 最終スコア計算
3. 勝者判定・表示

## アニメーション戦略

### 非同期処理（UniTask）
```csharp
_isAnimating = true;

// 駒配置
await PlacePieceAsync(coord, currentPlayer);

// 複数駒を同時反転（UniTask.WhenAll）
if (flippedTiles.Count > 0)
{
    await FlipPiecesAsync(flippedTiles, currentPlayer);
}

_isAnimating = false;
```

### 反転アニメーション詳細
```csharp
private async UniTask FlipSinglePieceAsync(HexCoordinate coord, PieceType newPiece)
{
    // フェードアウト（古い色）
    await animator.FadeOut(_flipAnimationDuration / 2);

    // 駒を再生成（新しい色のマテリアル）
    CreatePieceVisual(coord, newPiece);

    // フェードイン（新しい色）
    await newAnimator.FadeIn(_flipAnimationDuration / 2);
}
```

## UI設計（縦画面レイアウト）

### レイアウト構成
```
Canvas (1080 x 1920, Portrait)
├── Game Board Area (60% top)
│   └── 3D Camera View
└── UI Panel (40% bottom)
    ├── Player 1 Score (left)
    ├── Player 2 Score (right)
    ├── Turn Indicator (center top)
    ├── Game Result (center, hidden initially)
    ├── Show Hints Toggle (left)
    ├── Reset Button (center)
    └── Back to Menu Button (center bottom)
```

### UI更新ロジック
```csharp
// スコア表示
private void UpdateScoreDisplay()
{
    (int p1, int p2) = _model.GetPieceCounts();
    _player1ScoreText.text = $"Player 1: {p1}";
    _player2ScoreText.text = $"Player 2: {p2}";
}

// ターン表示
private void UpdateTurnDisplay()
{
    string playerName = _model.CurrentPlayer == PieceType.Player1 ? "Player 1" : "Player 2";
    _turnIndicatorText.text = $"Turn: {playerName}";
}
```

## パフォーマンス最適化

### 1. GPU Instancing
- すべてのマテリアルで有効化
- 期待されるドローコール数: **2〜4**
  - タイル（1 batch）
  - 駒（1〜2 batches）
  - UI（1 batch）

### 2. オブジェクトプール（今後の改善）
現在は`Destroy/Instantiate`を使用。将来的にはプール化を検討:
```csharp
// TODO: ObjectPoolManager統合
// var piece = _piecePool.Get(coord);
// _piecePool.Release(piece);
```

### 3. GCアロケーション削減
- `foreach`ループ使用（LINQ回避）
- List容量を事前推定
- Dictionaryで高速アクセス

### 4. 非同期処理の最適化
- `async/await`で長時間のブロッキング回避
- `UniTask.WhenAll`で並列アニメーション実行
- `_isAnimating`フラグでダブルクリック防止

## テスト戦略

### ユニットテスト（既存）
- `HexReversiTests.cs`
- `HexGridTests.cs`
- `HexCoordinateTests.cs`

### 統合テスト（新規）
- シーン起動テスト
- UI操作テスト
- アニメーション完了テスト

## エディターツール

### 1. シーン自動セットアップ
**メニュー**: `ShaderOp → Setup → HexReversi Complete Scene`

**実行内容**:
1. GameBootstrap作成
2. カメラ設定（俯瞰視点）
3. HexReversiCompleteコントローラー作成
4. Prefab/Material参照設定
5. UI構築（Canvas, Button, Text等）
6. すべてのコンポーネントを自動配線

### 2. バリデーションツール
**メニュー**: `ShaderOp → Validate → HexReversi Scene`

**検証項目**:
- GameBootstrap存在確認
- HexReversiComplete設定確認
- Prefab/Material参照確認
- UI要素確認
- カメラ設定確認
- シェーダープロパティ確認

### 3. シェーダー統合チェック
**メニュー**: `ShaderOp → Validate → Check Shader Integration`

**検証内容**:
- マテリアルの必須プロパティ確認
- シェーダーコントローラーインスタンス数確認

### 4. パフォーマンスプロファイル
**メニュー**: `ShaderOp → Validate → Performance Profile`

**期待値出力**:
- タイル数: 37
- 最大駒数: 37
- ドローコール数: 2〜4
- 目標FPS: 60

## デバッグ機能

### コンテキストメニュー（Play Mode中）
```csharp
[ContextMenu("Force Update All Pieces")]
private void ForceUpdateAllPieces()

[ContextMenu("Show Valid Moves")]
private void ForceShowHints()

[ContextMenu("Hide Valid Moves")]
private void ForceHideHints()
```

## 拡張機能（今後の実装）

### 1. AI対戦モード
```csharp
// SimpleAI: 最も多く反転できる手を選択
private HexCoordinate? GetBestMove()
{
    List<HexCoordinate> validMoves = _model.GetValidMoves();
    int maxFlips = 0;
    HexCoordinate? bestMove = null;

    foreach (var move in validMoves)
    {
        int flips = GetFlippedTiles(move, _model.CurrentPlayer).Count;
        if (flips > maxFlips)
        {
            maxFlips = flips;
            bestMove = move;
        }
    }

    return bestMove;
}
```

### 2. 手の履歴・Undo機能
```csharp
private Stack<GameState> _history = new();

public void Undo()
{
    if (_history.Count > 0)
    {
        var previousState = _history.Pop();
        RestoreState(previousState);
    }
}
```

### 3. チュートリアルモード
- 初回プレイ時に有効手を強制表示
- ルール説明のポップアップ
- ステップバイステップガイド

## トラブルシューティング

### 問題: タイルがクリックできない
**解決策**:
- タイルにColliderがあるか確認
- EventSystemがシーンに存在するか確認
- Cameraに`Physics Raycaster`があるか確認

### 問題: アニメーションが動かない
**解決策**:
- マテリアルに`_Fade`プロパティがあるか確認
- GamePieceShaderAnimatorがアタッチされているか確認
- UniTaskがインポートされているか確認

### 問題: シェーダーが正しく表示されない
**解決策**:
- Shader Graphが正しくコンパイルされているか確認
- マテリアルのシェーダーが正しく設定されているか確認
- URPアセット設定を確認

### 問題: パフォーマンスが低い
**解決策**:
- GPU Instancingを有効化
- Profilerでボトルネック特定
- ドローコール数を確認（Stats window）
- バッチング設定を確認

## ファイル構成

```
ShaderOp/
├── Assets/
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   └── Minigames/
│   │   │       └── Games/
│   │   │           ├── HexReversiComplete.cs (新規)
│   │   │           ├── HexReversiModel.cs (既存)
│   │   │           ├── HexReversiView.cs (既存)
│   │   │           └── HexReversiController.cs (既存)
│   │   └── Editor/
│   │       ├── HexReversiSceneSetup.cs (新規)
│   │       └── HexReversiValidator.cs (新規)
│   ├── Prefabs/
│   │   └── Minigames/
│   │       ├── HexTile.prefab
│   │       ├── Player1Piece.prefab
│   │       └── Player2Piece.prefab
│   ├── Materials/
│   │   └── Minigames/
│   │       ├── MAT_HexTile_Interactive.mat
│   │       ├── MAT_Player1Piece.mat
│   │       └── MAT_Player2Piece.mat
│   ├── Shaders/
│   │   └── ShaderGraphs/
│   │       └── Minigames/
│   │           ├── SG_HexTile_Interactive.shadergraph
│   │           └── SG_GamePiece_2D.shadergraph
│   └── Scenes/
│       └── HexReversi.unity
└── Documentation/
    ├── HEXREVERSI_IMPLEMENTATION.md (このファイル)
    └── HEXREVERSI_QUICKSTART.md
```

## 参考リンク

- [UniTask Documentation](https://github.com/Cysharp/UniTask)
- [Unity Shader Graph Manual](https://docs.unity3d.com/Packages/com.unity.shadergraph@latest)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

---

**作成日**: 2026-03-08
**バージョン**: 1.0.0
**作成者**: Claude Code (Anthropic)
