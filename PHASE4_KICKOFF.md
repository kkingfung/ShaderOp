# Phase 4: Performance & Polish - Kickoff Plan

**開始日**: 2026-03-09
**目標期間**: 2026-03-09 - 2026-03-31
**ステータス**: 0% → Starting 🚀

---

## 📋 Phase 4 概要

Phase 3で完成した2つのミニゲーム（HexCheckers、HexChess）の**パフォーマンス最適化**と**UX/UI磨き上げ**を行います。

### Phase 3からの引き継ぎ状態

**完成済み**:
- ✅ HexCheckers MVC実装 (1049 lines)
- ✅ HexChess MVC実装 (1120 lines)
- ✅ Scene Setup Tools (2 tools)
- ✅ HexCheckers.unity シーン構築
- ✅ HexChess.unity シーン構築
- ✅ Unit Tests (HexCheckersTests, HexChessTests)

**Phase 3進捗**: 40% Complete

---

## 🎯 Phase 4 目標

### 主要目標

1. **パフォーマンス最適化**
   - UniTask統合（非同期処理）
   - Object Pooling（HexTile再利用）
   - GC Allocation削減

2. **UI/UX Polish**
   - アニメーション追加（駒配置、勝利演出）
   - オーディオフィードバック
   -視覚的フィードバック強化

3. **Mobile最適化**
   - タッチ入力対応確認
   - 縦画面レイアウト最適化
   - パフォーマンスプロファイリング

---

## 📊 Phase 4 タスク分類

### 4.1 Performance Optimization（優先度: 高）

#### A. UniTask統合 ✨
**理由**: 非同期処理によるフレームレート向上、スムーズなゲーム体験

**タスク**:
- [ ] UniTaskパッケージインストール
- [ ] SceneLoader非同期化
  - [ ] `SceneLoader.LoadSceneAsync()` → UniTask版
  - [ ] ロード中プログレス表示
- [ ] HexGrid生成非同期化
  - [ ] `HexGrid.GenerateGrid()` → `GenerateGridAsync()`
  - [ ] 大規模グリッド（11×11）の分割生成
- [ ] 駒配置非同期化
  - [ ] `InitializePieces()` → `InitializePiecesAsync()`

**期待効果**:
- ロード時間の体感速度向上
- フレームドロップ防止
- 大規模グリッド生成時のスムーズさ

---

#### B. Object Pooling 🔄
**理由**: HexTile/駒のInstantiate/Destroyコストを削減

**タスク**:
- [ ] ObjectPoolManagerサービス実装
  - [ ] `IObjectPoolService` インターフェース
  - [ ] `ObjectPoolManager` 実装
  - [ ] ServiceLocator登録
- [ ] HexTile Pooling
  - [ ] HexGridでPool使用に変更
  - [ ] TicTacToeHex（9タイル）
  - [ ] HexReversi（37タイル）
  - [ ] HexCheckers（64タイル）
  - [ ] HexChess（121タイル）
- [ ] GamePiece Pooling
  - [ ] Player1Piece/Player2Piece Pool
  - [ ] 駒配置/削除時にPool使用

**期待効果**:
- GC Allocationの大幅削減
- ゲームリセット時の高速化
- メモリ効率向上

---

#### C. Profiling & Optimization 📈
**理由**: ボトルネック特定と最適化

**タスク**:
- [ ] Unity Profiler分析
  - [ ] CPU Usage分析（HexCheckers Play Mode）
  - [ ] CPU Usage分析（HexChess Play Mode）
  - [ ] Memory分析（ヒープ、GC頻度）
  - [ ] Rendering分析（Drawcall、Batching）
- [ ] GC Allocation削減
  - [ ] `GetValidMoves()` のList再利用
  - [ ] LINQ除去（Where/Select等）
  - [ ] String concatenation削減
- [ ] Drawcall最適化
  - [ ] Static Batching有効化
  - [ ] Material数削減
  - [ ] UI Atlasing

**期待効果**:
- 60fps安定動作
- GC spike削減
- バッテリー消費削減（モバイル）

---

### 4.2 UI/UX Polish（優先度: 中）

#### A. Transition Animations 🎬
**理由**: シーン遷移の滑らかさ、プロフェッショナルな印象

**タスク**:
- [ ] Scene Fade In/Out
  - [ ] FadeController実装
  - [ ] SceneLoader統合
  - [ ] Canvas Groupアニメーション
- [ ] Button Animations
  - [ ] Hover時の拡大/縮小
  - [ ] Click時のパルスエフェクト
  - [ ] UI Toolkit Transition対応
- [ ] Panel Slide In/Out
  - [ ] メニューパネルスライド
  - [ ] ゲーム結果パネルポップアップ

**実装方針**:
- DOTween使用（または Unity Animation System）
- 所要時間: 0.2-0.3秒（速すぎず遅すぎず）

---

#### B. Particle Effects ✨
**理由**: 視覚的フィードバック、ゲームの楽しさ向上

**タスク**:
- [ ] 勝利エフェクト
  - [ ] Confetti（紙吹雪）パーティクル
  - [ ] ゲーム終了時に再生
- [ ] 駒配置エフェクト
  - [ ] Sparkle（キラキラ）エフェクト
  - [ ] 駒配置時に短時間再生
- [ ] キング化エフェクト（HexCheckers）
  - [ ] Crown（王冠）出現エフェクト
- [ ] チェックメイトエフェクト（HexChess）
  - [ ] 王が倒れるアニメーション

**実装方針**:
- Unity Particle System使用
- モバイル性能考慮（パーティクル数制限）

---

#### C. Feedback Enhancement 🔊
**理由**: ユーザー操作への即座のフィードバック

**タスク**:
- [ ] Sound Feedback
  - [ ] Button Clickサウンド
  - [ ] 駒配置サウンド
  - [ ] ジャンプ/キャプチャサウンド
  - [ ] 勝利ジングル
  - [ ] チェック警告サウンド
- [ ] Visual Feedback
  - [ ] タイルホバー時のハイライト強化
  - [ ] 選択フィードバック（パルス）
  - [ ] 無効手クリック時のシェイク
- [ ] Haptic Feedback（Mobile）
  - [ ] ボタンタップ時
  - [ ] 駒配置時
  - [ ] ゲーム終了時

**実装方針**:
- AudioManagerサービス使用
- オーディオアセットは後で追加（まずはプレースホルダー）

---

### 4.3 Mobile Optimization（優先度: 中）

#### Tasks
- [ ] タッチ入力対応確認
  - [ ] InputManager.GetPointerDown() 検証
  - [ ] マルチタッチ無効化
- [ ] Portrait Layout最適化
  - [ ] 9:16アスペクト比検証
  - [ ] Safe Area対応
- [ ] Performance Profiling (Mobile)
  - [ ] Android Build Profiling
  - [ ] FPS測定（ターゲット: 60fps）
  - [ ] バッテリー消費測定

---

## 🛠️ 実装優先順位

### Week 1 (2026-03-09 - 2026-03-15): Performance Foundation
**Priority: 🔴 High**

1. **UniTaskパッケージインストール**
2. **ObjectPoolManager実装**
3. **HexTile Pooling実装**
4. **Unity Profiler分析（ベースライン測定）**

### Week 2 (2026-03-16 - 2026-03-22): Async Integration
**Priority: 🔴 High**

5. **SceneLoader非同期化**
6. **HexGrid生成非同期化**
7. **GC Allocation削減（GetValidMoves最適化）**

### Week 3 (2026-03-23 - 2026-03-29): UI/UX Polish
**Priority: 🟡 Medium**

8. **Scene Fade In/Out実装**
9. **Button Animations実装**
10. **Sound Feedback実装（プレースホルダー）**

### Week 4 (2026-03-30 - 2026-03-31): Final Polish
**Priority: 🟢 Low**

11. **Particle Effects実装（勝利エフェクト）**
12. **Mobile最適化検証**
13. **Phase 4完了レポート作成**

---

## 📈 成功指標

### Performance Metrics

| 指標 | 現在（推定） | 目標 |
|------|------------|------|
| **HexCheckers起動時間** | 不明 | < 1秒 |
| **HexChess起動時間** | 不明 | < 2秒 |
| **FPS（HexCheckers）** | 不明 | 60fps安定 |
| **FPS（HexChess）** | 不明 | 60fps安定 |
| **GC Allocation（1手）** | 不明 | < 100KB |
| **メモリ使用量** | 不明 | < 200MB |

### UX Metrics

- ✅ シーン遷移にフェードアニメーション
- ✅ ボタンクリックに視覚的フィードバック
- ✅ 駒配置にサウンドフィードバック
- ✅ 勝利時にエフェクト再生

---

## 🚀 最初のタスク

**推奨開始タスク**: **UniTaskパッケージインストール**

**理由**:
1. 後続の非同期処理実装の基盤
2. 依存関係がない（独立して実装可能）
3. インストール後すぐに効果確認可能

**具体的手順**:
1. Package Manager → Add package from git URL
2. `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`
3. インストール確認（`using Cysharp.Threading.Tasks;`）
4. 簡単なテストコード作成（async/await動作確認）

---

## 📝 ドキュメント構成

Phase 4で作成するドキュメント:

1. **PHASE4_KICKOFF.md** (この文書) - Phase 4計画
2. **PHASE4_PROGRESS_SUMMARY.md** - 進捗管理
3. **PERFORMANCE_BASELINE.md** - 最適化前ベースライン
4. **PERFORMANCE_REPORT.md** - 最適化後レポート
5. **UNITASK_INTEGRATION_GUIDE.md** - UniTask統合ガイド
6. **OBJECT_POOLING_GUIDE.md** - Object Pooling実装ガイド

---

## 🎉 Phase 4完了条件

以下の条件を満たした時点でPhase 4完了とします:

**必須条件**:
- ✅ UniTask統合完了（SceneLoader、HexGrid非同期化）
- ✅ Object Pooling実装（HexTile、GamePiece）
- ✅ Unity Profiler分析完了（最適化前後比較）
- ✅ GC Allocation削減（目標: 50%削減）
- ✅ Scene Fade In/Out実装
- ✅ Sound Feedback実装（プレースホルダー）

**オプション条件**:
- Particle Effects実装
- Haptic Feedback実装
- Addressables統合

---

## 🔄 Phase 5への移行

Phase 4完了後、**Phase 5: Testing & Bug Fixes**へ移行します。

Phase 5では:
- Play Modeテスト（全シーン）
- パフォーマンステスト
- バグ修正
- リリース準備

---

**作成者**: Claude Code (Anthropic)
**作成日**: 2026-03-09
**バージョン**: Phase 4 - Kickoff
**次回アクション**: UniTaskパッケージインストール
