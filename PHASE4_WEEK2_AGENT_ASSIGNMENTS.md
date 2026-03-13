# Phase 4 Week 2: Agent Task Assignments

**作成日**: 2026-03-09
**Commander**: Claude Code (Main)
**Phase**: Phase 4 Week 2 - Async Integration & Critical Optimizations

---

## 🎯 Agent Deployment Strategy

Week 2では、**クリティカルな最適化**に集中するため、設計→実装→検証の3段階アプローチを採用します。

---

## 📋 Week 2 Task Assignments (2026-03-16 - 2026-03-22)

### Task 1: CheckWinCondition Optimization Algorithm Design
**担当エージェント**: `architect`
**優先度**: 🔴 S-Tier Critical
**依存関係**: なし（独立タスク）

**タスク内容**:
1. 現在の実装分析 (HexChessModel.cs - CheckWinCondition())
2. ボトルネック特定:
   - 32 pieces × 121 tiles = 3,872 iterations
   - 実行時間: 2,000ms (2秒フリーズ)
3. 最適化アルゴリズム設計:
   - King Position Caching戦略
   - Attack Map Pre-computation戦略
   - Incremental Update戦略
4. データ構造設計 (Dictionary, HashSet利用)
5. 疑似コード作成

**成果物**:
- `CHECKWINCONDITION_OPTIMIZATION_DESIGN.md`
- 最適化アルゴリズム疑似コード
- データ構造設計図
- 期待パフォーマンス分析 (2,000ms → 50ms)

---

### Task 2: GetValidMoves Optimization Algorithm Design
**担当エージェント**: `architect`
**優先度**: 🔴 S-Tier Critical
**依存関係**: なし（Task 1と並行可能）

**タスク内容**:
1. 現在の実装分析 (HexChessModel.cs - GetValidMoves())
2. ボトルネック特定:
   - 121タイル全スキャン
   - 実行時間: 62ms (UI lag発生)
   - GC Allocation: 50KB/call
3. 最適化アルゴリズム設計:
   - Move Pattern Pre-computation (駒タイプごと)
   - Direction-based Search (全スキャン回避)
   - Check Validation Optimization
4. List<HexCoordinate> Pooling統合設計

**成果物**:
- `GETVALIDMOVES_OPTIMIZATION_DESIGN.md`
- 最適化アルゴリズム疑似コード
- Move Pattern Cache設計
- 期待パフォーマンス分析 (62ms → 5ms)

---

### Task 3: Optimization Design Review
**担当エージェント**: `code-reviewer`
**優先度**: 🔴 Critical
**依存関係**: Task 1, 2完了後

**タスク内容**:
1. CheckWinCondition設計レビュー
2. GetValidMoves設計レビュー
3. SOLID原則準拠確認
4. パフォーマンス観点レビュー
5. エッジケース特定
6. 改善提案

**成果物**:
- `WEEK2_DESIGN_REVIEW_REPORT.md`
- 設計承認 or 改善提案

---

### Task 4: CheckWinCondition Implementation
**担当エージェント**: `unity-developer`
**優先度**: 🔴 Critical
**依存関係**: Task 3完了後

**タスク内容**:
1. HexChessModel.cs 修正
2. CheckWinCondition() 最適化実装
3. King Position Cache実装
4. Attack Map実装
5. Incremental Update実装
6. Unit Tests作成 (20+ tests)
7. パフォーマンス測定コード追加

**成果物**:
- `HexChessModel.cs` (CheckWinCondition最適化版)
- `HexChessOptimizationTests.cs` (20+ tests)
- パフォーマンス測定結果

---

### Task 5: GetValidMoves Implementation
**担当エージェント**: `unity-developer`
**優先度**: 🔴 Critical
**依存関係**: Task 3完了後（Task 4と並行可能）

**タスク内容**:
1. HexChessModel.cs 修正
2. GetValidMoves() 最適化実装
3. Move Pattern Cache実装
4. Direction-based Search実装
5. List<HexCoordinate> Pooling統合
6. Unit Tests作成 (15+ tests)
7. パフォーマンス測定コード追加

**成果物**:
- `HexChessModel.cs` (GetValidMoves最適化版)
- `GetValidMovesTests.cs` (15+ tests)
- パフォーマンス測定結果

---

### Task 6: List<HexCoordinate> Object Pool Implementation
**担当エージェント**: `unity-developer`
**優先度**: 🟡 High
**依存関係**: Task 5と統合（同時実装可能）

**タスク内容**:
1. UnityEngine.Pool.ListPool<T> 使用パターン実装
2. HexChessModel全メソッドにListPool適用:
   - GetValidMoves()
   - GetPiecesOfType()
   - GetAllPieces()
   - その他List<HexCoordinate>を返すメソッド
3. HexCheckersModelにも適用
4. GC Allocation測定

**成果物**:
- `HexChessModel.cs` (ListPool統合版)
- `HexCheckersModel.cs` (ListPool統合版)
- GC Allocation Before/After比較

---

### Task 7: SceneLoader Async Conversion
**担当エージェント**: `unity-developer`
**優先度**: 🟡 High
**依存関係**: Task 4, 5, 6完了後

**タスク内容**:
1. SceneLoader.cs 修正
2. LoadScene() → LoadSceneAsync() 変換
3. UniTask使用
4. Progress callback実装
5. CancellationToken対応
6. Fade In/Out統合準備（実装は Week 3）
7. Unit Tests作成

**成果物**:
- `SceneLoader.cs` (Async版)
- `SceneLoaderAsyncTests.cs` (10+ tests)
- 使用例ドキュメント

---

### Task 8: HexGrid Generation Async Conversion
**担当エージェント**: `unity-developer`
**優先度**: 🟡 High
**依存関係**: Task 7完了後

**タスク内容**:
1. HexGridVisualizer.cs 修正
2. GenerateVisuals() → GenerateVisualsAsync() 変換
3. UniTask使用
4. Progress callback実装
5. 10タイルごとにYield (60fps維持)
6. HexChess (121タイル) で検証
7. Unit Tests作成

**成果物**:
- `HexGridVisualizer.cs` (Async版)
- `HexGridAsyncTests.cs` (8+ tests)
- パフォーマンス測定結果

---

### Task 9: Post-Optimization Profiler Analysis
**担当エージェント**: `performance-analyzer`
**優先度**: 🟡 High
**依存関係**: Task 4, 5, 6完了後

**タスク内容**:
1. Unity Profiler測定 (HexChess Play Mode)
2. Before/After比較:
   - CheckWinCondition: 2,000ms → ???ms
   - GetValidMoves: 62ms → ???ms
   - GC Alloc/turn: 50KB → ???KB
3. 100-turn stress test
4. Memory leak確認
5. ボトルネック再評価

**成果物**:
- `PERFORMANCE_WEEK2_RESULTS.md`
- Unity Profiler screenshots
- Before/After比較表

---

### Task 10: Integration Tests
**担当エージェント**: `test-engineer`
**優先度**: 🟢 Medium
**依存関係**: Task 4, 5, 6, 7, 8完了後

**タスク内容**:
1. HexChess統合テスト作成
2. 最適化機能のエンドツーエンドテスト
3. Async処理のテスト
4. エッジケーステスト
5. リグレッションテスト

**成果物**:
- `HexChessIntegrationTests.cs` (15+ tests)
- `AsyncIntegrationTests.cs` (10+ tests)
- テストカバレッジレポート

---

### Task 11: Week 2 Documentation
**担当エージェント**: `doc-writer`
**優先度**: 🟢 Medium
**依存関係**: Task 9, 10完了後

**タスク内容**:
1. PHASE4_WEEK2_SUMMARY.md作成
2. CRITICAL_OPTIMIZATION_GUIDE.md作成
3. ASYNC_INTEGRATION_GUIDE.md作成
4. ROADMAP.md更新 (Phase 4: 25% → 50%)

**成果物**:
- `PHASE4_WEEK2_SUMMARY.md`
- `CRITICAL_OPTIMIZATION_GUIDE.md`
- `ASYNC_INTEGRATION_GUIDE.md`
- `ROADMAP.md` (更新)

---

## 🔄 Execution Order

### Day 1-2: Design Phase

**Sequential Tasks**:
```
Task 1: CheckWinCondition Design (architect)
  ↓
Task 2: GetValidMoves Design (architect)
  ↓
Task 3: Design Review (code-reviewer)
```

---

### Day 3-4: Implementation Phase

**Parallel Tasks**:
```
Task 4: CheckWinCondition Impl (unity-developer)
  ∥
Task 5: GetValidMoves Impl (unity-developer)
  ∥
Task 6: List Pooling Impl (unity-developer)
```

---

### Day 5-6: Async Integration Phase

**Sequential Tasks**:
```
Task 7: SceneLoader Async (unity-developer)
  ↓
Task 8: HexGrid Async (unity-developer)
```

---

### Day 7: Verification Phase

**Parallel Tasks**:
```
Task 9: Profiler Analysis (performance-analyzer)
  ∥
Task 10: Integration Tests (test-engineer)
  ∥
Task 11: Documentation (doc-writer)
```

---

## 📊 Agent Workload Distribution

| エージェント | タスク数 | 優先度 | 推定工数 |
|------------|---------|--------|---------|
| **architect** | 2 | 🔴 S-Tier | 高 (20%) |
| **code-reviewer** | 1 | 🔴 Critical | 低 (5%) |
| **unity-developer** | 5 | 🔴 Critical | 最高 (50%) |
| **performance-analyzer** | 1 | 🟡 High | 中 (10%) |
| **test-engineer** | 1 | 🟢 Medium | 中 (10%) |
| **doc-writer** | 1 | 🟢 Medium | 低 (5%) |

---

## 🚀 Execution Strategy

### Phase 1: Design (Day 1-2)
Commander が architect と code-reviewer を順次起動:

1. **architect**: CheckWinCondition設計 (Task 1)
2. **architect**: GetValidMoves設計 (Task 2)
3. **code-reviewer**: 設計レビュー (Task 3)

---

### Phase 2: Implementation (Day 3-4)
Critical Path完了後、unity-developer を集中起動:

4. **unity-developer**: CheckWinCondition実装 (Task 4)
5. **unity-developer**: GetValidMoves実装 (Task 5)
6. **unity-developer**: List Pooling統合 (Task 6)

---

### Phase 3: Async Integration (Day 5-6)
非同期処理統合:

7. **unity-developer**: SceneLoader Async (Task 7)
8. **unity-developer**: HexGrid Async (Task 8)

---

### Phase 4: Verification (Day 7)
3エージェントを並列起動:

9. **performance-analyzer**: Profiler分析 (Task 9)
10. **test-engineer**: 統合テスト (Task 10)
11. **doc-writer**: ドキュメント作成 (Task 11)

---

## 📝 Commander Checklist

**Week 2 実行チェックリスト**:

### Design Phase
- [ ] Day 1: architect エージェント起動 (CheckWinCondition design)
- [ ] Day 1: architect エージェント起動 (GetValidMoves design)
- [ ] Day 2: code-reviewer エージェント起動 (design review)

### Implementation Phase
- [ ] Day 3: unity-developer エージェント起動 (CheckWinCondition impl)
- [ ] Day 3: unity-developer エージェント起動 (GetValidMoves impl)
- [ ] Day 3: unity-developer エージェント起動 (List pooling)

### Async Phase
- [ ] Day 5: unity-developer エージェント起動 (SceneLoader async)
- [ ] Day 6: unity-developer エージェント起動 (HexGrid async)

### Verification Phase
- [ ] Day 7: performance-analyzer エージェント起動 (profiling)
- [ ] Day 7: test-engineer エージェント起動 (integration tests)
- [ ] Day 7: doc-writer エージェント起動 (documentation)

---

## 🎯 Success Criteria (Week 2)

**必須達成条件**:
- ✅ CheckWinCondition < 100ms (現在: 2,000ms)
- ✅ GetValidMoves < 10ms (現在: 62ms)
- ✅ GC Allocation < 10KB/turn (現在: 50KB)
- ✅ SceneLoader async conversion完了
- ✅ HexGrid async generation完了

**品質条件**:
- ✅ Code Review合格
- ✅ Unit Test Coverage > 85%
- ✅ Unity Profiler検証完了
- ✅ ドキュメント完備

---

**作成者**: Claude Code (Commander)
**作成日**: 2026-03-09
**Phase**: Phase 4 Week 2
**次回アクション**: Day 1 - architect エージェント起動 (Task 1)
