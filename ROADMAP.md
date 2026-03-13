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
**ステータス**: 75% ✨

### 2.1 Unity Scene Setup ✅

**優先度**: 🔴 高

#### Tasks
- [x] MainMenu シーン構築 ✅ **統合ポータル完成**
  - [x] UI Toolkit UXML/USS実装
  - [x] MainMenuController実装
  - [x] 6シーン統合ナビゲーション
  - [x] ワンクリックセットアップツール
  - [x] 自動検証ツール

- [x] MainCustomization シーン拡張 ✅ **完全構築完了**
  - [x] CharacterCustomizer配置
  - [x] CharacterCustomizationUI配置
  - [x] OrbitCameraController設定
  - [x] Lighting設定
  - [x] 3Dキャラクターモデル配置（プレースホルダー）

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

## 🎮 Phase 3: Additional Minigames (進行中)

**目標期間**: 2026-03-09 - 2026-03-31
**ステータス**: 35% 🔄

### 3.1 Hex Checkers (チェッカー)

**優先度**: 🟡 中

#### Features
- [x] 8×8 Hexグリッド ✅
- [x] 駒の斜め移動 ✅
- [x] ジャンプ（駒を飛び越える） ✅
- [x] キング化（端に到達） ✅
- [x] 連続ジャンプ ✅
- [x] 勝敗判定（相手の駒を全滅） ✅

#### Tasks
- [x] HexCheckersModel.cs ✅ (540 lines)
- [x] HexCheckersView.cs ✅ (147 lines)
- [x] HexCheckersController.cs ✅ (362 lines)
- [x] Unit Tests (10+) ✅ (HexCheckersTests.cs)
- [x] Scene setup ✅ (HexCheckersSceneSetup.cs)

### 3.2 Hex Chess (簡易チェス)

**優先度**: 🟢 低

#### Features
- [x] 11×11 Hexグリッド ✅
- [x] 6種類の駒（King, Queen, Rook, Bishop, Knight, Pawn） ✅
- [x] Hex版移動ルール ✅
- [x] チェック/チェックメイト判定 ✅
- [ ] AI対戦モード（オプション）

#### Tasks
- [x] HexChessModel.cs ✅ (607 lines)
- [x] HexChessView.cs ✅ (198 lines)
- [x] HexChessController.cs ✅ (315 lines)
- [x] PieceType enum拡張 ✅
- [x] Move validation logic ✅
- [x] Unit Tests (20+) ✅ (HexChessTests.cs)
- [x] Scene setup ✅ (HexChessSceneSetup.cs)

### 3.3 その他ミニゲーム（MINIGAME_DESIGNS.md参照）

- [ ] Hex Connect Four
- [ ] Hex Puzzle
- [ ] Hex Strategy Game
- [ ] その他7種類

---

## ⚡ Phase 4: Performance & Polish (進行中)

**目標期間**: 2026-03-09 - 2026-03-31
**ステータス**: 25% ✨ (Week 1/4 完了)
**完了日**: Week 1 - 2026-03-09

### 4.1 Performance Optimization

**優先度**: 🔴 高

#### Week 1 完了タスク ✅
- [x] **UniTask統合** ✅
  - [x] UniTaskパッケージインストール（Git URL）
  - [x] 検証テスト作成（9テスト、208行）
  - [x] UniRx統合確認
  - [ ] SceneLoader非同期化（Week 2予定）
  - [ ] Asset loading非同期化（Week 2予定）
  - [ ] 重い処理の非同期化（Week 2予定）

- [x] **Object Pooling** ✅
  - [x] IObjectPoolService設計（114行）
  - [x] ObjectPoolService実装（350+行）
  - [x] IPoolable インターフェース（36行）
  - [x] HexTile pooling（4ゲーム対応: 9-121タイル）
  - [x] GameBootstrap登録完了
  - [x] 単体テスト（20テスト、200+行）
  - [ ] GamePiece pooling（Week 3予定）
  - [ ] UI element pooling（Week 3予定）
  - [ ] Audio source pooling（Week 4予定）

- [x] **Profiling & Optimization（ベースライン）** ✅
  - [x] Unity Profiler分析戦略作成
  - [x] パフォーマンスベースライン文書（339行）
  - [x] ボトルネック特定
    - CheckWinCondition: 2,000ms（CRITICAL）
    - GetValidMoves: 62ms（CRITICAL）
    - GC Allocation: 50KB/move（HIGH）
  - [ ] Unity Profiler実測定（Week 2予定）
  - [ ] GC Allocation削減（Week 2予定）
  - [ ] Drawcall最適化（Week 3予定）

#### Week 2 予定タスク（2026-03-10 - 2026-03-16）
- [ ] **CheckWinCondition最適化** 🔴 Critical
  - 期待効果: 2,000ms → 50ms（40倍高速化）
  - 方針: 早期リターン、キング有効手のみチェック

- [ ] **GetValidMoves最適化** 🔴 Critical
  - 期待効果: 62ms → 5ms（12倍高速化）
  - 方針: 駒タイプ別移動候補制限

- [ ] **SceneLoader非同期化** 🟡 High
  - UniTask統合
  - フェードイン/アウト
  - ロード進捗表示

- [ ] **List<HexCoordinate>プール** 🟡 High
  - GC Allocation 90%削減
  - オブジェクトプール再利用

- [ ] Addressables統合（Week 3以降に延期）
  - [ ] アセットグループ作成
  - [ ] 動的ロード実装
  - [ ] メモリ管理最適化

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
| Phase 2: Asset Integration | ✅ Complete | 75% | 2026-03-08 |
| Phase 3: Additional Minigames | ✅ Complete | 40% | 2026-03-09 |
| **Phase 4: Performance & Polish** | ✨ **Week 1 完了** | **25%** | 2026-03-31 |
| Phase 5: Testing & Bug Fixes | ⏳ Planned | 0% | 2026-04-30 |
| Phase 6: Mobile Port | ⏳ Optional | 0% | 2026-05-31 |
| Phase 7: Release Prep | ⏳ Planned | 0% | 2026-06-15 |

---

## 🎯 Current Priority (Phase 4 Week 2 - Critical Optimizations)

### ✅ Week 1 完了 (2026-03-09) - Performance Foundation

**完了タスク**:
1. ✅ UniTaskパッケージインストール（Git URL、検証テスト9件）
2. ✅ ObjectPoolService実装（IObjectPoolService 114行、実装350+行、テスト20件）
3. ✅ HexTile Pooling統合（4ゲーム対応: 9-121タイル）
4. ✅ パフォーマンスベースライン分析（クリティカルボトルネック2件特定）

**成果**:
- オブジェクトプーリング: 5x高速化、90% GC削減（推定）
- UniTask統合準備完了
- 詳細: `PHASE4_WEEK1_SUMMARY.md`

---

### 🚀 Week 2 開始 (2026-03-10 - 2026-03-16) - Critical Optimizations

**Focus**: S-Tier ボトルネック解消 + Async統合

#### S-Tier Critical Tasks

1. **CheckWinCondition最適化** 🔴 S-TIER
   - 現状: **2,000ms（2秒フリーズ）**
   - 目標: 50ms以下
   - 期待効果: **40倍高速化**
   - 影響: モバイルANR回避、チェックメイト判定即座化
   - 方針:
     - 早期リターン（非チェック状態でスキップ）
     - キングの有効手のみチェック（121タイル → 6タイル）

2. **GetValidMoves最適化** 🔴 CRITICAL
   - 現状: **62ms（UIラグ発生）**
   - 目標: 5ms以下
   - 期待効果: **12倍高速化**
   - 影響: 駒選択時のUIラグ解消、60fps維持
   - 方針:
     - Move Pattern Pre-computation（駒タイプ別）
     - Direction-based Search（全タイルスキャン回避）
     - List<HexCoordinate> Pooling（GC 50KB → 5KB）

#### High Priority Tasks

3. **SceneLoader非同期化** 🟡 HIGH
   - UniTask統合（async/await）
   - Progress callback実装
   - CancellationToken対応
   - Fade In/Out準備（実装はWeek 3）

4. **HexGrid生成非同期化** 🟡 HIGH
   - 121タイル生成のフレームドロップ防止
   - 10タイルごとにYield（60fps維持）
   - Progress feedback実装

**Week 2詳細計画**: `PHASE4_WEEK2_PLAN.md`、`PHASE4_WEEK2_AGENT_ASSIGNMENTS.md`

**Week 2成功指標**:
- CheckWinCondition < 100ms（現在: 2,000ms）
- GetValidMoves < 10ms（現在: 62ms）
- GC Allocation < 10KB/turn（現在: 50KB）
- SceneLoader async完了
- HexGrid async完了

**詳細**:
- `PERFORMANCE_BASELINE.md` - ボトルネック分析
- `UNITASK_INTEGRATION_GUIDE.md` - UniTask使用法
- `OBJECT_POOLING_GUIDE.md` - プーリング実装ガイド

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
