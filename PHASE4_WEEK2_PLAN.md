# Phase 4 Week 2: Async Integration & Critical Optimizations

**期間**: 2026-03-16 - 2026-03-22
**ステータス**: 0% → Starting 🚀
**優先度**: 🔴 Critical

---

## 📋 Week 2 概要

Week 1でオブジェクトプーリングとパフォーマンスベースラインを確立しました。
Week 2では**クリティカルなボトルネックの最適化**と**非同期処理統合**を実施します。

### Week 1からの引き継ぎ

**完了済み**:
- ✅ UniTask/UniRx インストール・検証
- ✅ ObjectPoolService 実装 (350+ lines, 20 tests)
- ✅ HexGrid pooling 統合
- ✅ Performance Baseline 作成

**発見された課題** (PERFORMANCE_BASELINE.mdより):
- 🔴 **CRITICAL**: HexChess `CheckWinCondition()` - **2,000ms freeze** (チェックメイト判定時)
- 🔴 **CRITICAL**: HexChess `GetValidMoves()` - **62ms lag** (駒選択時)
- 🟡 **HIGH**: GC Allocation - 50KB/turn from List creation

---

## 🎯 Week 2 目標

### 主要目標

1. **クリティカル最適化 (60%)**
   - CheckWinCondition: **2,000ms → 50ms** (40x speedup)
   - GetValidMoves: **62ms → 5ms** (12x speedup)
   - List<HexCoordinate> pooling: GC allocation **50KB → 5KB** (90% reduction)

2. **非同期処理統合 (30%)**
   - SceneLoader async conversion with UniTask
   - HexGrid generation async (large grids)
   - Progress feedback during async operations

3. **検証 (10%)**
   - Unity Profiler post-optimization measurement
   - Before/After comparison report
   - 100-turn stress test

---

## 📊 Week 2 タスク分類

### 2.1 Critical Performance Optimization (優先度: S-Tier)

#### Task 1: CheckWinCondition Optimization 🚨

**現状の問題**:
```csharp
// HexChessModel.cs - CheckWinCondition()
// 現在: 32 pieces × 121 tiles = 3,872 iterations
// 実行時間: 2,000ms (2秒フリーズ)
public bool CheckWinCondition()
{
    foreach (var piece in _pieces) // 32 pieces
    {
        foreach (var tile in _grid.GetAllTiles()) // 121 tiles
        {
            // Check if this tile is under attack by opponent
            // → 毎回GetValidMoves()を呼び出し
        }
    }
}
```

**最適化戦略**:
1. **King Position Caching** - 毎回全タイルをスキャンしない
2. **Attack Map Pre-computation** - 攻撃可能位置を事前計算
3. **Early Exit on Non-King Pieces** - King以外はスキップ
4. **Incremental Update** - 手番ごとに差分更新

**期待効果**:
- **2,000ms → 50ms** (40x speedup)
- モバイルでのANR (Application Not Responding) 回避
- チェックメイト判定が即座に完了

**担当エージェント**: `architect` → `unity-developer`
- architect: 最適化アルゴリズム設計
- unity-developer: 実装 + Unit Tests

---

#### Task 2: GetValidMoves Optimization 🚨

**現状の問題**:
```csharp
// HexChessModel.cs - GetValidMoves()
// 現在: 121タイル全スキャン + チェック判定
// 実行時間: 62ms (UI lag発生)
public List<HexCoordinate> GetValidMoves(HexCoordinate position)
{
    var piece = GetPiece(position);
    var moves = new List<HexCoordinate>(); // GC Allocation!

    // 121タイル全スキャン
    foreach (var tile in _grid.GetAllTiles())
    {
        if (IsValidMove(position, tile.Coordinate))
        {
            // チェック判定 (さらに時間がかかる)
            if (!WouldBeInCheck(position, tile.Coordinate))
            {
                moves.Add(tile.Coordinate);
            }
        }
    }
    return moves;
}
```

**最適化戦略**:
1. **Move Pattern Pre-computation** - 駒タイプごとの移動パターンをキャッシュ
2. **Direction-based Search** - 全タイルスキャンではなく方向ベース探索
3. **List<HexCoordinate> Pooling** - GC allocation削減
4. **Check Validation Optimization** - 不要なチェック判定を省略

**期待効果**:
- **62ms → 5ms** (12x speedup)
- UI lag解消 (16.67ms/frame以下)
- GC allocation 50KB → 5KB (90% reduction)

**担当エージェント**: `architect` → `unity-developer`

---

#### Task 3: List<HexCoordinate> Object Pool 🔄

**現状の問題**:
- `GetValidMoves()`, `GetPiecesOfType()` 等で毎回 `new List<HexCoordinate>()`
- 1手あたり 50KB GC allocation
- 10手で 500KB → GC spike発生

**実装パターン**:
```csharp
// ListPool<T> の利用
using UnityEngine.Pool;

public class HexChessModel
{
    public List<HexCoordinate> GetValidMoves(HexCoordinate position)
    {
        var moves = ListPool<HexCoordinate>.Get(); // Poolから取得
        try
        {
            // moves に追加
            return new List<HexCoordinate>(moves); // 呼び出し側に返すコピー
        }
        finally
        {
            ListPool<HexCoordinate>.Release(moves); // Pool に返却
        }
    }
}
```

**期待効果**:
- GC allocation: **50KB → 5KB** (90% reduction)
- GC spike削減 → frame drops削減

**担当エージェント**: `unity-developer`

---

### 2.2 Async Integration (優先度: High)

#### Task 4: SceneLoader Async Conversion 🔄

**目標**:
- `SceneLoader.LoadScene()` → `SceneLoader.LoadSceneAsync()`
- Progress callback実装
- Fade In/Out transition統合

**実装パターン** (UNITASK_INTEGRATION_GUIDE.md より):
```csharp
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public async UniTask LoadSceneAsync(
        string sceneName,
        IProgress<float> progress = null,
        CancellationToken ct = default)
    {
        // Fade Out
        await FadeOut(ct);

        // Scene Load
        var operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            progress?.Report(operation.progress);
            await UniTask.Yield(ct);
        }

        // Fade In
        await FadeIn(ct);
    }
}
```

**期待効果**:
- ロード中のフレーム落ち防止
- Progress bar表示可能
- スムーズなシーン遷移

**担当エージェント**: `unity-developer`

---

#### Task 5: HexGrid Generation Async 🔄

**目標**:
- HexChess (121タイル) 生成を非同期化
- フレームドロップ防止 (60fps維持)

**実装パターン**:
```csharp
public class HexGridVisualizer : MonoBehaviour
{
    public async UniTask GenerateVisualsAsync(
        HexGrid grid,
        IProgress<float> progress = null,
        CancellationToken ct = default)
    {
        int totalTiles = grid.GetAllTiles().Count;
        int processed = 0;

        foreach (var tile in grid.GetAllTiles())
        {
            CreateTileVisual(tile);
            processed++;

            // 10タイルごとに1フレーム待機
            if (processed % 10 == 0)
            {
                progress?.Report((float)processed / totalTiles);
                await UniTask.Yield(ct);
            }
        }
    }
}
```

**期待効果**:
- HexChess起動時のフレームドロップ防止
- Progress feedback (0% → 100%)
- 60fps維持

**担当エージェント**: `unity-developer`

---

### 2.3 Verification & Documentation (優先度: Medium)

#### Task 6: Post-Optimization Profiler Analysis 📊

**目標**:
- Unity Profiler で最適化後のメトリクス測定
- Before/After comparison
- PERFORMANCE_WEEK2_RESULTS.md 作成

**測定項目**:
| 項目 | Before (Week 1) | Target (Week 2) | Actual |
|------|----------------|-----------------|--------|
| CheckWinCondition | 2,000ms | 50ms | ??? |
| GetValidMoves | 62ms | 5ms | ??? |
| GC Alloc/turn | 50KB | 5KB | ??? |
| HexChess起動時間 | 推定 2s | <1s | ??? |

**担当エージェント**: `performance-analyzer`

---

#### Task 7: Week 2 Summary Documentation 📝

**成果物**:
1. **PHASE4_WEEK2_SUMMARY.md** - Week 2完了レポート
2. **CRITICAL_OPTIMIZATION_GUIDE.md** - 最適化手法ガイド
3. **ASYNC_INTEGRATION_GUIDE.md** - 非同期統合パターン

**担当エージェント**: `doc-writer`

---

## 🛠️ 実装優先順位

### Day 1-2 (2026-03-16 - 2026-03-17): Critical Optimization Design

**Sequential Tasks**:
1. **architect**: CheckWinCondition 最適化アルゴリズム設計
2. **architect**: GetValidMoves 最適化アルゴリズム設計
3. **code-reviewer**: 設計レビュー

---

### Day 3-4 (2026-03-18 - 2026-03-19): Implementation

**Parallel Tasks**:
- **unity-developer**: CheckWinCondition 実装 (Task 1)
- **unity-developer**: GetValidMoves 実装 (Task 2)
- **unity-developer**: List pooling 実装 (Task 3)

---

### Day 5-6 (2026-03-20 - 2026-03-21): Async Integration

**Sequential Tasks**:
4. **unity-developer**: SceneLoader async conversion (Task 4)
5. **unity-developer**: HexGrid async generation (Task 5)

---

### Day 7 (2026-03-22): Verification & Documentation

**Parallel Tasks**:
- **performance-analyzer**: Post-optimization profiling (Task 6)
- **test-engineer**: Integration tests
- **doc-writer**: Documentation (Task 7)

---

## 📈 成功指標

### Performance Metrics

| 指標 | Week 1 Baseline | Week 2 Target | Acceptance Criteria |
|------|----------------|---------------|---------------------|
| **CheckWinCondition** | 2,000ms | 50ms | < 100ms |
| **GetValidMoves** | 62ms | 5ms | < 10ms |
| **GC Alloc/turn** | 50KB | 5KB | < 10KB |
| **HexChess起動時間** | ~2s | <1s | < 1.5s |
| **Frame Rate (HexChess)** | 不明 | 60fps | > 55fps |

### Code Quality Metrics

- ✅ All Unit Tests passing (50+ tests)
- ✅ Code Review approval
- ✅ Zero NullReferenceExceptions
- ✅ #nullable enable maintained
- ✅ Japanese comments complete

---

## 🔄 Agent Deployment Strategy

### Phase 1: Design (Day 1-2)

**architect** (2 tasks):
- Task 1 Design: CheckWinCondition optimization algorithm
- Task 2 Design: GetValidMoves optimization algorithm

**code-reviewer** (1 task):
- Review architecture designs

---

### Phase 2: Implementation (Day 3-4)

**unity-developer** (3 tasks in parallel):
- Task 1 Implementation: CheckWinCondition
- Task 2 Implementation: GetValidMoves
- Task 3 Implementation: List<HexCoordinate> pooling

**test-engineer** (support):
- Unit test creation for optimized methods

---

### Phase 3: Async Integration (Day 5-6)

**unity-developer** (2 tasks sequential):
- Task 4: SceneLoader async
- Task 5: HexGrid async

---

### Phase 4: Verification (Day 7)

**performance-analyzer** (1 task):
- Task 6: Post-optimization profiling

**doc-writer** (1 task):
- Task 7: Documentation

---

## 📝 Commander Checklist

**Week 2 実行チェックリスト**:

### Phase 1: Design
- [ ] Day 1: architect エージェント起動 (CheckWinCondition design)
- [ ] Day 1: architect エージェント起動 (GetValidMoves design)
- [ ] Day 2: code-reviewer エージェント起動 (design review)

### Phase 2: Implementation
- [ ] Day 3: unity-developer エージェント起動 (CheckWinCondition impl)
- [ ] Day 3: unity-developer エージェント起動 (GetValidMoves impl)
- [ ] Day 3: unity-developer エージェント起動 (List pooling)
- [ ] Day 4: test-engineer エージェント起動 (unit tests)

### Phase 3: Async Integration
- [ ] Day 5: unity-developer エージェント起動 (SceneLoader async)
- [ ] Day 6: unity-developer エージェント起動 (HexGrid async)

### Phase 4: Verification
- [ ] Day 7: performance-analyzer エージェント起動 (profiling)
- [ ] Day 7: doc-writer エージェント起動 (documentation)

---

## 🎯 Week 2 完了条件

**必須条件**:
- ✅ CheckWinCondition < 100ms (現在: 2,000ms)
- ✅ GetValidMoves < 10ms (現在: 62ms)
- ✅ GC Allocation < 10KB/turn (現在: 50KB)
- ✅ SceneLoader async conversion完了
- ✅ HexGrid async generation完了
- ✅ Unity Profiler post-optimization測定完了

**品質条件**:
- ✅ All Unit Tests passing (60+ tests)
- ✅ Code Review approval
- ✅ Performance Profiler verification
- ✅ Documentation complete

---

## 🔄 Week 3への移行

Week 2完了後、**Week 3: UI/UX Polish**へ移行します。

Week 3では:
- Scene Fade In/Out実装
- Button Animations実装
- Sound Feedback実装
- Particle Effects実装

---

## 📚 参考資料

**Week 1 Deliverables**:
- `PERFORMANCE_BASELINE.md` - ボトルネック分析
- `OBJECT_POOLING_GUIDE.md` - Pooling実装パターン
- `UNITASK_INTEGRATION_GUIDE.md` - Async/await パターン

**Phase 4 Planning**:
- `PHASE4_KICKOFF.md` - Phase 4全体計画
- `PHASE4_AGENT_ASSIGNMENTS.md` - Week 1エージェント割り当て
- `PHASE4_WEEK1_SUMMARY.md` - Week 1完了レポート

**実装ファイル**:
- `HexChessModel.cs` - 最適化対象
- `HexCheckersModel.cs` - 参考実装
- `SceneLoader.cs` - Async化対象
- `HexGridVisualizer.cs` - Async化対象

---

**作成者**: Claude Code (Commander)
**作成日**: 2026-03-09
**Phase**: Phase 4 Week 2
**次回アクション**: Day 1 - architect エージェント起動 (CheckWinCondition design)
