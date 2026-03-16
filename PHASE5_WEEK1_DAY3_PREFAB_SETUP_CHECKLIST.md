# Phase 5 Week 1 Day 3: Prefab Setup Checklist

**Status**: Ready for manual execution
**Duration**: 15 minutes
**Date**: 2026-03-16

---

## 📋 Quick Start Checklist

### Prerequisites
- [ ] Unity Editor 2021.3 LTS起動済み
- [ ] Photon PUN 2 Asset インポート完了
- [ ] Day 2 コミット完了 (commit b316b6a)

---

## ✅ Step-by-Step Instructions

### 1. NetworkService.prefab 作成 (5分)

#### 1.1 GameObject作成
```
Hierarchy → 右クリック → Create Empty
名前: "NetworkService"
```

#### 1.2 コンポーネント追加
```
Inspector → Add Component
検索: "PhotonNetworkService"
Enter
```

#### 1.3 Prefab保存
```
Hierarchy: NetworkService を選択
Project/Assets/Prefabs/Services/ フォルダにドラッグ&ドロップ
確認: NetworkService.prefab が作成される（青いキューブアイコン）
```

#### 1.4 Hierarchyから削除
```
Hierarchy: NetworkService を右クリック → Delete
```

**確認**:
- [ ] `Assets/Prefabs/Services/NetworkService.prefab` が存在
- [ ] Prefabを選択 → Inspector で PhotonNetworkService コンポーネント確認
- [ ] Hierarchy に NetworkService が存在しない

---

### 2. GameSyncService.prefab 作成 (5分)

#### 2.1 GameObject作成
```
Hierarchy → 右クリック → Create Empty
名前: "GameSyncService"
```

#### 2.2 PhotonGameSyncService追加
```
Inspector → Add Component
検索: "PhotonGameSyncService"
Enter
```

#### 2.3 PhotonView追加 ⚠️ 必須
```
Inspector → Add Component
検索: "Photon View"
Enter
```

#### 2.4 PhotonView設定確認
```
Inspector → Photon View (Script):
├── View ID: 0 (自動割り当て)
├── Observed Components: (空) ← そのままでOK
├── Ownership: Fixed
└── Synchronization: Off
```

#### 2.5 Prefab保存
```
Hierarchy: GameSyncService を選択
Project/Assets/Prefabs/Services/ フォルダにドラッグ&ドロップ
確認: GameSyncService.prefab が作成される
```

#### 2.6 Hierarchyから削除
```
Hierarchy: GameSyncService を右クリック → Delete
```

**確認**:
- [ ] `Assets/Prefabs/Services/GameSyncService.prefab` が存在
- [ ] Prefabを選択 → PhotonGameSyncService コンポーネント確認
- [ ] Prefabを選択 → PhotonView コンポーネント確認
- [ ] Hierarchy に GameSyncService が存在しない

---

### 3. GameBootstrap設定 (5分)

#### 3.1 Startupシーンを開く
```
Project/Assets/Scenes/ → Startup.unity をダブルクリック
```

#### 3.2 GameBootstrapを選択
```
Hierarchy → GameBootstrap を選択
```

#### 3.3 Prefab設定
```
Inspector → Game Bootstrap (Script)

Network Service Prefabs (Phase 5) セクション:

1. Network Service Prefab フィールド:
   Project/Assets/Prefabs/Services/NetworkService.prefab を
   ドラッグ&ドロップ

2. Game Sync Service Prefab フィールド:
   Project/Assets/Prefabs/Services/GameSyncService.prefab を
   ドラッグ&ドロップ
```

#### 3.4 シーン保存
```
Ctrl + S (Windows) / Cmd + S (Mac)
```

**確認**:
- [ ] Network Service Prefab: NetworkService (GameObject) 表示
- [ ] Game Sync Service Prefab: GameSyncService (GameObject) 表示
- [ ] Startup.unity シーン保存済み

---

## 🧪 動作確認 (Play Mode)

### Test 1: Prefab初期化確認

#### 実行
```
Play ボタンをクリック
```

#### 期待されるConsoleログ
```
[GameBootstrap] Initializing services...
[GameBootstrap] INetworkService (Photon) registered.
[GameBootstrap] IGameSyncService (Photon) registered.
[GameBootstrap] SaveDataService registered.
[GameBootstrap] FirebaseAuthService registered.
[GameBootstrap] HttpClientService registered.
[GameBootstrap] SceneLoaderService registered.
[GameBootstrap] ObjectPoolService registered.
[GameBootstrap] 7 services registered successfully.
[GameBootstrap] Initialization complete.
```

#### 確認項目
- [ ] Console: "INetworkService (Photon) registered." 表示
- [ ] Console: "IGameSyncService (Photon) registered." 表示
- [ ] Console: "7 services registered successfully." 表示
- [ ] エラーログなし

---

### Test 2: DontDestroyOnLoad確認

#### 実行
```
Play Mode中: Hierarchy → DontDestroyOnLoad セクション確認
```

#### 期待される構造
```
Hierarchy:
├── DontDestroyOnLoad
│   ├── GameBootstrap
│   ├── PhotonNetworkService  ← Prefabからインスタンス化
│   └── PhotonGameSyncService  ← Prefabからインスタンス化
└── (他のオブジェクト)
```

#### 確認項目
- [ ] DontDestroyOnLoad セクション存在
- [ ] PhotonNetworkService オブジェクト存在
- [ ] PhotonGameSyncService オブジェクト存在
- [ ] 両サービスが破棄されずに維持されている

---

## 🚨 トラブルシューティング

### エラー1: "PhotonNetworkServiceコンポーネントが見つかりません"

**原因**: コンポーネント名のスペルミス

**解決策**:
1. Add Component で "Photon" と入力
2. 候補リストから PhotonNetworkService を選択
3. スペルを完全一致させる

---

### エラー2: "PhotonView could not find owner"

**原因**: GameSyncService.prefab に PhotonView コンポーネントがない

**解決策**:
1. Project: GameSyncService.prefab をダブルクリック
2. Inspector → Add Component → Photon View
3. Ctrl + S で保存
4. Play Mode再実行

---

### エラー3: "7 services registered"が表示されない

**原因**: GameBootstrap Inspector で Prefab が設定されていない

**解決策**:
1. Startup.unity シーンを開く
2. Hierarchy → GameBootstrap 選択
3. Inspector → Network Service Prefabs セクション確認
4. NetworkService.prefab と GameSyncService.prefab をドラッグ&ドロップ
5. Ctrl + S でシーン保存
6. Play Mode再実行

---

### エラー4: Prefabが見つからない

**原因**: Prefab保存場所が異なる

**解決策**:
1. Project ウィンドウ検索欄: "NetworkService"
2. 見つかったPrefabを Assets/Prefabs/Services/ に移動
3. 同様に "GameSyncService" を検索して移動
4. GameBootstrap Inspector で再設定

---

## 📊 完了チェックリスト

### Prefab作成
- [ ] NetworkService.prefab 作成済み (Assets/Prefabs/Services/)
- [ ] NetworkService.prefab に PhotonNetworkService コンポーネントあり
- [ ] GameSyncService.prefab 作成済み (Assets/Prefabs/Services/)
- [ ] GameSyncService.prefab に PhotonGameSyncService コンポーネントあり
- [ ] GameSyncService.prefab に PhotonView コンポーネントあり

### GameBootstrap設定
- [ ] Startup.unity シーン開いた
- [ ] GameBootstrap 選択
- [ ] Network Service Prefab 設定済み
- [ ] Game Sync Service Prefab 設定済み
- [ ] Startup.unity シーン保存済み (Ctrl + S)

### 動作確認
- [ ] Play Mode 実行成功
- [ ] Console: "INetworkService (Photon) registered." 表示
- [ ] Console: "IGameSyncService (Photon) registered." 表示
- [ ] Console: "7 services registered successfully." 表示
- [ ] Hierarchy: DontDestroyOnLoad/PhotonNetworkService 存在
- [ ] Hierarchy: DontDestroyOnLoad/PhotonGameSyncService 存在
- [ ] エラーなし

---

## 🎯 Next Steps

### After Prefab Setup Complete:

1. **Git Commit** (推奨):
   ```bash
   git add ShaderOptimizer/Assets/Prefabs/Services/
   git add ShaderOptimizer/Assets/Scenes/Startup.unity
   git commit -m "feat: Phase 5 Week 1 Day 3 - Photon Service Prefabs Setup"
   ```

2. **Proceed to TicTacToeHex Scene Integration**:
   - TicTacToeHex.unity シーンを開く
   - HexBoardGameController を TicTacToeHexOnlineController に変更
   - 詳細: `PHASE5_WEEK1_DAY3_TEST_PLAN.md` Section 1.3

3. **2-Client Connection Testing**:
   - Unity Editor + Standalone Build テスト
   - 詳細: `PHASE5_WEEK1_DAY3_TEST_PLAN.md` Section 2

---

## 📖 Related Documents

- **PHASE5_WEEK1_PREFAB_CONFIGURATION_GUIDE.md**: 詳細手順 (500行)
- **PHASE5_WEEK1_DAY3_TEST_PLAN.md**: テスト計画 (10テストケース)
- **PHASE5_WEEK1_DAY2_SUMMARY.md**: Day 2完了サマリー
- **GameBootstrap.cs**: サービス登録コード (Line 90-130)

---

**最終更新**: 2026-03-16 21:50
**ステータス**: Manual Setup Guide Complete
**所要時間**: 15分（手動実行）
