# Phase 4: Agent Task Assignments

**作成日**: 2026-03-09
**Commander**: Claude Code (Main)
**Phase**: Phase 4 - Performance & Polish

---

## 🎯 Agent Deployment Strategy

Phase 4では、各専門エージェントに適切なタスクを割り当てて並列実行します。

---

## 📋 Week 1 Task Assignments (2026-03-09 - 2026-03-15)

### Task 1: UniTask Package Installation & Verification
**担当エージェント**: `unity-developer`
**優先度**: 🔴 Critical
**依存関係**: なし（独立タスク）

**タスク内容**:
1. UniTaskパッケージをgit URLからインストール
2. インストール検証スクリプト作成
3. async/await動作確認
4. 簡単なサンプルコード作成

**成果物**:
- UniTaskパッケージインストール完了
- `Assets/Scripts/Tests/UniTaskVerificationTest.cs`

---

### Task 2: IObjectPoolService Interface Design
**担当エージェント**: `architect`
**優先度**: 🔴 Critical
**依存関係**: なし（Task 3の前提）

**タスク内容**:
1. IObjectPoolService インターフェース設計
2. ジェネリック型対応（IObjectPoolService<T>）
3. メソッド定義:
   - `T Get()`
   - `void Return(T obj)`
   - `void Prewarm(int count)`
   - `void Clear()`
4. アーキテクチャドキュメント作成

**成果物**:
- `Assets/Scripts/Runtime/Core/Services/IObjectPoolService.cs`
- インターフェース設計ドキュメント

---

### Task 3: ObjectPoolManager Implementation
**担当エージェント**: `unity-developer`
**優先度**: 🔴 Critical
**依存関係**: Task 2完了後

**タスク内容**:
1. ObjectPoolManager クラス実装
2. Dictionary<Type, Queue<T>> によるプール管理
3. GameObject専用最適化
4. ServiceLocator登録コード追加
5. Unit Tests作成

**成果物**:
- `Assets/Scripts/Runtime/Core/ObjectPoolManager.cs`
- `Assets/Scripts/Tests/ObjectPoolManagerTests.cs`

---

### Task 4: HexGrid Pooling Integration
**担当エージェント**: `unity-developer`
**優先度**: 🔴 Critical
**依存関係**: Task 3完了後

**タスク内容**:
1. HexGrid.cs修正（Instantiate → Pool.Get()）
2. ClearBoard時にPool.Return()呼び出し
3. 4ゲームで動作検証:
   - TicTacToeHex (9タイル)
   - HexReversi (37タイル)
   - HexCheckers (64タイル)
   - HexChess (121タイル)

**成果物**:
- `Assets/Scripts/Runtime/Minigames/HexGrid/HexGrid.cs` (修正)
- 動作検証レポート

---

### Task 5: Unity Profiler Baseline Analysis
**担当エージェント**: `performance-analyzer`
**優先度**: 🟡 High
**依存関係**: Task 4完了後（最適化前ベースライン）

**タスク内容**:
1. HexCheckers Play Mode Profiling
   - CPU Usage測定
   - Memory測定
   - GC Allocation測定
2. HexChess Play Mode Profiling
   - 同上（11×11グリッドで負荷高い）
3. ボトルネック特定
4. 最適化ターゲット選定

**成果物**:
- `PERFORMANCE_BASELINE.md` (詳細レポート)
- Unity Profiler スクリーンショット

---

### Task 6: Code Quality Review (Phase 4対応コード)
**担当エージェント**: `code-reviewer`
**優先度**: 🟡 High
**依存関係**: Task 3, 4完了後

**タスク内容**:
1. ObjectPoolManager コードレビュー
2. HexGrid修正箇所レビュー
3. SOLID原則準拠確認
4. パフォーマンス観点レビュー
5. 改善提案

**成果物**:
- コードレビューレポート
- 改善提案リスト

---

### Task 7: Test Coverage Analysis
**担当エージェント**: `test-engineer`
**優先度**: 🟢 Medium
**依存関係**: Task 3, 4完了後

**タスク内容**:
1. ObjectPoolManager Unit Tests実装
2. HexGrid Pooling Integration Tests
3. テストカバレッジ測定
4. エッジケーステスト追加

**成果物**:
- 追加Unit Tests
- テストカバレッジレポート

---

### Task 8: Documentation Writing
**担当エージェント**: `doc-writer`
**優先度**: 🟢 Medium
**依存関係**: Task 3, 4, 5完了後

**タスク内容**:
1. OBJECT_POOLING_GUIDE.md作成
2. UNITASK_INTEGRATION_GUIDE.md作成
3. PERFORMANCE_BASELINE.md補完
4. Week 1進捗サマリー作成

**成果物**:
- `OBJECT_POOLING_GUIDE.md`
- `UNITASK_INTEGRATION_GUIDE.md`
- `PHASE4_WEEK1_SUMMARY.md`

---

## 🔄 Execution Order

### Parallel Track A (Performance Foundation)
```
Task 1: UniTask Installation (unity-developer)
  ↓
Task 3: ObjectPoolManager (unity-developer) [待機: Task 2]
  ↓
Task 4: HexGrid Pooling (unity-developer)
  ↓
Task 5: Profiler Baseline (performance-analyzer)
```

### Parallel Track B (Architecture & Design)
```
Task 2: IObjectPoolService Design (architect)
  ↓ (Task 3へ引き継ぎ)
```

### Parallel Track C (Quality & Documentation)
```
Task 6: Code Review (code-reviewer) [待機: Task 3, 4]
Task 7: Test Coverage (test-engineer) [待機: Task 3, 4]
Task 8: Documentation (doc-writer) [待機: Task 3, 4, 5]
```

---

## 📊 Agent Workload Distribution

| エージェント | タスク数 | 優先度 | 推定工数 |
|------------|---------|--------|---------|
| **unity-developer** | 3 | 🔴 Critical | 高（60%） |
| **architect** | 1 | 🔴 Critical | 中（10%） |
| **performance-analyzer** | 1 | 🟡 High | 中（15%） |
| **code-reviewer** | 1 | 🟡 High | 低（5%） |
| **test-engineer** | 1 | 🟢 Medium | 低（5%） |
| **doc-writer** | 1 | 🟢 Medium | 低（5%） |

---

## 🚀 Execution Strategy

### Step 1: Sequential Critical Path (必須タスク)
Commander が unity-developer と architect を順次起動:

1. **architect**: IObjectPoolService設計 (Task 2)
2. **unity-developer**: UniTask インストール (Task 1)
3. **unity-developer**: ObjectPoolManager実装 (Task 3) [Task 2完了後]
4. **unity-developer**: HexGrid Pooling (Task 4)

### Step 2: Parallel Analysis & Quality (並列タスク)
Critical Path完了後、3エージェントを並列起動:

5. **performance-analyzer**: Profiler分析 (Task 5)
6. **code-reviewer**: コードレビュー (Task 6)
7. **test-engineer**: テスト追加 (Task 7)

### Step 3: Documentation (最終タスク)
8. **doc-writer**: ドキュメント作成 (Task 8)

---

## 📝 Commander Checklist

**Week 1 実行チェックリスト**:

- [ ] Step 1-1: architect エージェント起動 (IObjectPoolService設計)
- [ ] Step 1-2: unity-developer エージェント起動 (UniTask インストール)
- [ ] Step 1-3: unity-developer エージェント起動 (ObjectPoolManager実装)
- [ ] Step 1-4: unity-developer エージェント起動 (HexGrid Pooling)
- [ ] Step 2: 3エージェント並列起動
  - [ ] performance-analyzer (Profiler分析)
  - [ ] code-reviewer (コードレビュー)
  - [ ] test-engineer (テスト追加)
- [ ] Step 3: doc-writer エージェント起動 (ドキュメント作成)
- [ ] Week 1完了確認・次週計画

---

## 🎯 Success Criteria (Week 1)

**必須達成条件**:
- ✅ UniTaskパッケージインストール完了
- ✅ ObjectPoolManager実装・テスト完了
- ✅ HexGrid Pooling統合完了（4ゲーム動作確認）
- ✅ Unity Profilerベースライン測定完了

**品質条件**:
- ✅ Code Review合格（SOLID原則準拠）
- ✅ Unit Test Coverage > 80%
- ✅ ドキュメント完備

---

**作成者**: Claude Code (Commander)
**作成日**: 2026-03-09
**Phase**: Phase 4 Week 1
**次回アクション**: Step 1-1 - architect エージェント起動
