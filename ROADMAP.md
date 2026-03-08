# ShaderOp - Development Roadmap

**最終更新**: 2026-02-23

## ✅ Phase 1: Code Foundation + Firebase + Shaders (完了)

**期間**: 開始 - 2026-03-07
**ステータス**: 100% Complete ✅

### 完了項目
- [x] Hexグリッドシステム (HexCoordinate, HexGrid, HexTile)
- [x] MVCアーキテクチャ基底クラス
- [x] Tic-Tac-Toe Hex ミニゲーム
- [x] Hex Reversi ミニゲーム
- [x] 3Dキャラクターカスタマイズシステム
- [x] 部屋デコレーションシステム
- [x] ゲームフローシステム (GameManager, SceneLoader)
- [x] オーディオシステム (AudioManager)
- [x] 設定システム (SettingsManager)
- [x] 入力システム (InputManager)
- [x] カメラシステム (OrbitCameraController)
- [x] ビジュアライゼーションシステム
- [x] Editorツール (GameSetupUtility)
- [x] ユニットテスト (40 tests)
- [x] ドキュメント整備
- [x] Firebase Authentication統合 (IFirebaseAuthService, FirebaseAuthService, LoginView/ViewModel)
- [x] HTTP通信サービス (IHttpClientService, JWT自動付与)
- [x] 6つのHLSLシェーダー実装
  - [x] FabricCotton.shader (コットン素材)
  - [x] FabricSatin.shader (サテン素材・異方性反射)
  - [x] FabricLayered.shader (複数レイヤーブレンド)
  - [x] HexTileInteractive.shader (5ステート対応)
  - [x] GamePiece2D.shader (プレイヤーカラー対応)
  - [x] Avatar2D.shader (4チャンネルカスタマイズ)
- [x] Shader Helperスクリプト (HexTileShaderController, GamePieceShaderAnimator, Avatar2DShaderController)

**成果物**: 100+ C# Scripts, 60+ Unit Tests, 6 HLSL Shaders, 20+ Documentation Files

---

## 🔄 Phase 2: Asset Integration & Scene Building (進行中)

**目標期間**: 2026-02-24 - 2026-03-10
**ステータス**: 40% ✨

### 2.1 Unity Scene Setup ✅

**優先度**: 🔴 高

#### Tasks
- [ ] MainMenu シーン構築
  - [ ] Canvas/UI配置
  - [ ] Button配線
  - [ ] 背景画像設定
  - [ ] LoadingScreenUI配置

- [ ] MainCustomization シーン拡張
  - [ ] CharacterCustomizer配置
  - [ ] CharacterCustomizationUI配置
  - [ ] OrbitCameraController設定
  - [ ] Lighting設定
  - [ ] 3Dキャラクターモデル配置（プレースホルダー）

- [x] RoomDecoration シーン構築 ✅
  - [x] GameBootstrap配置
  - [x] サービス初期化設定
  - [ ] 床・壁メッシュ配置（保留）
  - [ ] RoomDecorator配置（保留）

- [x] TicTacToeHex シーン完全実装 ✅ **垂直スライス完成**
  - [x] HexTileプレハブ配置（3x3グリッド）
  - [x] MVC Components配線
  - [x] HexTileShaderController統合
  - [x] GamePieceShaderAnimator統合
  - [x] UI配置（ターン表示、リセットボタン）
  - [x] Camera設定
  - [x] ワンクリックセットアップツール
  - [x] 自動検証ツール

- [x] HexReversi シーン完全実装 ✅ **大規模実証**
  - [x] HexTileプレハブ配置（37タイル、半径3グリッド）
  - [x] MVC Components配線
  - [x] シェーダー統合（5状態+グロー）
  - [x] 有効手ヒント表示
  - [x] UI配置（スコア、ヒントトグル、リセット）
  - [x] Camera設定
  - [x] ワンクリックセットアップツール
  - [x] 性能検証ツール

- [x] HexCheckers/HexChess シーン基本設定 ✅
  - [x] GameBootstrap配置
  - [ ] ゲームロジック実装（Phase 3へ延期）

### 2.2 Prefab & Material Creation ✅

**優先度**: 🔴 高

#### Tasks
- [x] HexTile Prefab ✅
  - [x] HexTileVisualizer追加
  - [x] Material設定（Idle/Hover/Selected）
  - [x] HexTileShaderController統合
  - [x] Collider設定

- [x] Player Piece Prefabs ✅
  - [x] Player1Piece.prefab（GamePieceShaderAnimator）
  - [x] Player2Piece.prefab（GamePieceShaderAnimator）
  - [x] MAT_Player1Piece/MAT_Player2Piece

- [x] Cloth Materials ✅
  - [x] MAT_Cotton_New（FabricCotton shader）
  - [x] MAT_Silk_New（FabricSatin shader）
  - [x] MAT_Denim_New（FabricLayered shader）
  - [x] MAT_Leather_New（FabricSatin shader）

- [ ] UI Prefabs（延期）
  - [ ] LoadingScreen Panel
  - [ ] CustomizationUI Panel
  - [ ] MinigameUI Panel

### 2.3 Asset Integration

**優先度**: 🟡 中

#### Tasks
- [ ] Unity-Chan Toon Shader統合
  - [ ] Import package
  - [ ] Create character materials
  - [ ] Map shader properties to MaterialController
  - [ ] Test customization pipeline

- [ ] Hexタイルスプライト
  - [ ] 通常状態スプライト
  - [ ] ハイライト状態スプライト
  - [ ] 選択状態スプライト
  - [ ] Player1ピーススプライト
  - [ ] Player2ピーススプライト

- [ ] オーディオアセット
  - [ ] BGM - MainMenu
  - [ ] BGM - Customization
  - [ ] BGM - Minigame
  - [ ] SFX - Button Click
  - [ ] SFX - Piece Place
  - [ ] SFX - Win/Lose

- [ ] 3Dキャラクターモデル
  - [ ] Unity-Chan or 互換モデルimport
  - [ ] Rig設定
  - [ ] Animation Controller設定
  - [ ] Body parts分離（Hair, Eyes, Top, etc.）

- [ ] 家具プレハブ（各カテゴリ3-5個）
  - [ ] Floor（床置き家具）
  - [ ] Wall（壁掛け装飾）
  - [ ] Furniture（椅子、テーブル等）
  - [ ] Plant（観葉植物）
  - [ ] Decoration（小物）
  - [ ] Lighting（照明）

---

## 🎮 Phase 3: Additional Minigames (計画中)

**目標期間**: 2026-03-11 - 2026-03-31
**ステータス**: 0%

### 3.1 Hex Checkers (チェッカー)

**優先度**: 🟡 中

#### Features
- [ ] 8×8 Hexグリッド
- [ ] 駒の斜め移動
- [ ] ジャンプ（駒を飛び越える）
- [ ] キング化（端に到達）
- [ ] 連続ジャンプ
- [ ] 勝敗判定（相手の駒を全滅）

#### Tasks
- [ ] HexCheckersModel.cs
- [ ] HexCheckersView.cs
- [ ] HexCheckersController.cs
- [ ] Unit Tests (10+)
- [ ] Scene setup

### 3.2 Hex Chess (簡易チェス)

**優先度**: 🟢 低

#### Features
- [ ] 11×11 Hexグリッド
- [ ] 6種類の駒（King, Queen, Rook, Bishop, Knight, Pawn）
- [ ] Hex版移動ルール
- [ ] チェック/チェックメイト判定
- [ ] AI対戦モード（オプション）

#### Tasks
- [ ] HexChessModel.cs
- [ ] HexChessView.cs
- [ ] HexChessController.cs
- [ ] PieceType enum拡張
- [ ] Move validation logic
- [ ] Unit Tests (20+)
- [ ] Scene setup

### 3.3 その他ミニゲーム（MINIGAME_DESIGNS.md参照）

- [ ] Hex Connect Four
- [ ] Hex Puzzle
- [ ] Hex Strategy Game
- [ ] その他7種類

---

## ⚡ Phase 4: Performance & Polish (計画中)

**目標期間**: 2026-04-01 - 2026-04-15
**ステータス**: 0%

### 4.1 Performance Optimization

**優先度**: 🟡 中

#### Tasks
- [ ] UniTask統合
  - [ ] SceneLoader非同期化
  - [ ] Asset loading非同期化
  - [ ] 重い処理の非同期化

- [ ] Addressables統合
  - [ ] アセットグループ作成
  - [ ] 動的ロード実装
  - [ ] メモリ管理最適化

- [ ] Object Pooling
  - [ ] HexTile pooling
  - [ ] UI element pooling
  - [ ] Audio source pooling

- [ ] Profiling & Optimization
  - [ ] Unity Profiler分析
  - [ ] GC Allocation削減
  - [ ] Drawcall最適化

### 4.2 UI/UX Polish

**優先度**: 🟡 中

#### Tasks
- [ ] Transition Animations
  - [ ] Scene fade in/out
  - [ ] Button animations
  - [ ] Panel slide in/out

- [ ] Particle Effects
  - [ ] 勝利エフェクト
  - [ ] 駒配置エフェクト
  - [ ] カスタマイズ変更エフェクト

- [ ] Feedback Enhancement
  - [ ] Sound feedback
  - [ ] Visual feedback
  - [ ] Haptic feedback (mobile)

### 4.3 Accessibility

**優先度**: 🟢 低

#### Tasks
- [ ] Color blind mode
- [ ] Font size adjustment
- [ ] Audio cues
- [ ] Keyboard-only navigation

---

## 🧪 Phase 5: Testing & Bug Fixes (計画中)

**目標期間**: 2026-04-16 - 2026-04-30
**ステータス**: 0%

### 5.1 Automated Testing

**優先度**: 🟡 中

#### Tasks
- [ ] Integration Tests
  - [ ] Scene loading tests
  - [ ] Game flow tests
  - [ ] Save/Load tests

- [ ] Performance Tests
  - [ ] FPS benchmarks
  - [ ] Memory leak tests
  - [ ] Load time tests

- [ ] UI Tests
  - [ ] Button click tests
  - [ ] Input validation tests
  - [ ] Navigation flow tests

### 5.2 Manual Testing

**優先度**: 🔴 高

#### Tasks
- [ ] Playthrough testing（全シーン）
- [ ] Edge case testing
- [ ] Save/Load integrity testing
- [ ] Multi-resolution testing
- [ ] Mobile testing (if applicable)

### 5.3 Bug Tracking & Fixes

**優先度**: 🔴 高

#### Tasks
- [ ] Bug database setup (GitHub Issues)
- [ ] Priority classification
- [ ] Fix critical bugs
- [ ] Fix major bugs
- [ ] Fix minor bugs

---

## 📱 Phase 6: Mobile Port (オプション)

**目標期間**: 2026-05-01 - 2026-05-31
**ステータス**: 0%
**優先度**: 🟢 低

### Tasks
- [ ] Touch input adaptation
- [ ] UI scaling for mobile
- [ ] Performance optimization for mobile
- [ ] Android build
- [ ] iOS build (if applicable)

---

## 🚀 Phase 7: Release Preparation (計画中)

**目標期間**: 2026-06-01 - 2026-06-15
**ステータス**: 0%

### Tasks
- [ ] Final QA pass
- [ ] Performance profiling
- [ ] Documentation finalization
- [ ] Build optimization
- [ ] Platform-specific builds
- [ ] Release notes
- [ ] Marketing materials

---

## 📊 Progress Overview

| Phase | Status | Progress | Target Date |
|-------|--------|----------|-------------|
| Phase 1: Code Foundation + Firebase + Shaders | ✅ Complete | 100% | 2026-03-07 |
| Phase 2: Asset Integration | 🔄 Ready to Start | 0% | 2026-03-15 |
| Phase 3: Additional Minigames | ⏳ Planned | 0% | 2026-03-31 |
| Phase 4: Performance & Polish | ⏳ Planned | 0% | 2026-04-15 |
| Phase 5: Testing & Bug Fixes | ⏳ Planned | 0% | 2026-04-30 |
| Phase 6: Mobile Port | ⏳ Optional | 0% | 2026-05-31 |
| Phase 7: Release Prep | ⏳ Planned | 0% | 2026-06-15 |

---

## 🎯 Current Priority (Phase 2)

### 最優先タスク

1. **Unityエディタでシーン作成**
   ```
   ShaderOp > Setup > Create All Scenes
   ShaderOp > Setup > Create All Prefabs
   ```

2. **HexTile Prefab完成**
   - Sprite設定
   - Material設定
   - Collider設定

3. **TicTacToeHex シーンテスト**
   - 実際に動作確認
   - バグ修正

4. **HexReversi シーンテスト**
   - 実際に動作確認
   - バグ修正

5. **MainMenu シーン構築**
   - UI配置
   - Button配線
   - Scene transition確認

---

## 📝 Notes

### 技術的負債
- [ ] SceneLoader timeout issue（Unity MCP Server）
  - Workaround: Manual scene creation via editor

### 将来的な改善点
- [ ] UniRx統合（リアクティブプログラミング）
- [ ] Zenject/VContainer（DI framework）
- [ ] Custom shader development（専用シェーダー開発）
- [ ] Localization system（多言語対応）

### 参考資料
- ARCHITECTURE.md - システム設計
- PROJECT_STRUCTURE.md - プロジェクト構造
- MINIGAME_DESIGNS.md - ミニゲーム仕様
- IMPLEMENTATION_STATUS.md - 実装状況
- QUICK_START.md - クイックスタート
- PHASE1_COMPLETE.md - Phase 1完了報告

---

**Next Action**: Unityエディタで `ShaderOp > Setup > Create All Scenes` を実行してPhase 2を開始してください！ 🚀
