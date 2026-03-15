# Phase 5 Week 1: Service Prefab Configuration Guide

**Target**: Unity Editor上でPhotonサービスPrefab作成とGameBootstrap設定  
**所要時間**: 約1時間  
**前提条件**: Photon PUN 2 Assetインポート完了

---

## 📋 Table of Contents

1. [NetworkService Prefab作成](#networkservice-prefab作成)
2. [GameSyncService Prefab作成](#gamesyncservice-prefab作成)
3. [GameBootstrap設定](#gamebootstrap設定)
4. [動作確認](#動作確認)
5. [トラブルシューティング](#トラブルシューティング)

---

## 🎯 NetworkService Prefab作成

### Step 1: Empty GameObjectを作成

1. **Hierarchyで右クリック**
   ```
   右クリック → Create Empty
   ```

2. **名前を変更**
   ```
   Inspector → 上部の名前欄 → "NetworkService" に変更
   ```

3. **Transformリセット（推奨）**
   ```
   Inspector → Transform → 右クリック → Reset
   Position: (0, 0, 0)
   Rotation: (0, 0, 0)
   Scale: (1, 1, 1)
   ```

---

### Step 2: PhotonNetworkServiceコンポーネント追加

1. **Add Componentをクリック**
   ```
   Inspector → Add Component ボタン
   ```

2. **PhotonNetworkServiceを検索**
   ```
   検索欄に "PhotonNetworkService" と入力
   ```

3. **選択してEnter**
   ```
   PhotonNetworkService (Script) が表示される
   クリックまたはEnterで追加
   ```

**追加されたコンポーネント確認**:
```
Inspector:
├── Transform
└── Photon Network Service (Script)
```

---

### Step 3: Prefab化

1. **Prefabs/Servicesフォルダ作成（無ければ）**
   ```
   Project/Assets/ → 右クリック → Create → Folder
   名前: "Prefabs"
   
   Prefabs/ → 右クリック → Create → Folder
   名前: "Services"
   ```

2. **NetworkServiceをPrefab化**
   ```
   Hierarchy の "NetworkService" を
   Project/Assets/Prefabs/Services/ フォルダにドラッグ&ドロップ
   ```

3. **Hierarchyから削除**
   ```
   Hierarchy の "NetworkService" を右クリック → Delete
   （Prefabは Project に残る）
   ```

**作成結果確認**:
```
Project/Assets/Prefabs/Services/
└── NetworkService.prefab  ← 青いキューブアイコン
```

---

## 🎮 GameSyncService Prefab作成

### Step 1: Empty GameObjectを作成

1. **Hierarchyで右クリック**
   ```
   右クリック → Create Empty
   ```

2. **名前を変更**
   ```
   Inspector → 上部の名前欄 → "GameSyncService" に変更
   ```

3. **Transformリセット**
   ```
   Inspector → Transform → 右クリック → Reset
   ```

---

### Step 2: PhotonGameSyncServiceコンポーネント追加

1. **Add Component**
   ```
   Inspector → Add Component
   ```

2. **PhotonGameSyncServiceを検索**
   ```
   検索欄に "PhotonGameSyncService" と入力
   ```

3. **追加**
   ```
   PhotonGameSyncService (Script) をクリック
   ```

---

### Step 3: PhotonViewコンポーネント追加

**重要**: GameSyncServiceはRPCを使用するため、PhotonViewが必須です。

1. **Add Component**
   ```
   Inspector → Add Component
   ```

2. **Photon Viewを検索**
   ```
   検索欄に "Photon View" と入力
   ```

3. **追加**
   ```
   Photon View (Script) をクリック
   ```

---

### Step 4: PhotonView設定

**Inspector → Photon View (Script)**:

```
Photon View:
├── View ID: 0 （自動割り当て）
├── Observed Components: (Empty)  ← 空白のままでOK
├── Ownership: Fixed  ← デフォルトのまま
└── Synchronization: Off  ← デフォルトのまま
```

**注意**: Observed Componentsは空白で問題ありません。RPCメソッドのみ使用します。

---

### Step 5: Prefab化

1. **GameSyncServiceをPrefab化**
   ```
   Hierarchy の "GameSyncService" を
   Project/Assets/Prefabs/Services/ フォルダにドラッグ&ドロップ
   ```

2. **Hierarchyから削除**
   ```
   Hierarchy の "GameSyncService" を右クリック → Delete
   ```

**作成結果確認**:
```
Project/Assets/Prefabs/Services/
├── NetworkService.prefab
└── GameSyncService.prefab  ← 新規作成
```

---

## 🔧 GameBootstrap設定

### Step 1: Startupシーンを開く

```
Project/Assets/Scenes/ → Startup.unity をダブルクリック
```

---

### Step 2: GameBootstrapを選択

```
Hierarchy → GameBootstrap を選択
```

---

### Step 3: Inspector設定

**Inspector → Game Bootstrap (Script)**:

#### Services セクション
```
✓ Enable Network Service
✓ Enable Save Data Service
✓ Enable Firebase Auth
✓ Enable Object Pool Service
```

#### Network Service Prefabs (Phase 5) セクション

**重要**: このセクションにPrefabを設定します。

1. **Network Service Prefabを設定**
   ```
   Project/Assets/Prefabs/Services/NetworkService.prefab を
   Inspector の "Network Service Prefab" フィールドにドラッグ&ドロップ
   ```

2. **Game Sync Service Prefabを設定**
   ```
   Project/Assets/Prefabs/Services/GameSyncService.prefab を
   Inspector の "Game Sync Service Prefab" フィールドにドラッグ&ドロップ
   ```

**設定結果**:
```
Inspector → Game Bootstrap (Script):

Network Service Prefabs (Phase 5):
├── Network Service Prefab: NetworkService (GameObject)
└── Game Sync Service Prefab: GameSyncService (GameObject)
```

---

### Step 4: シーンを保存

```
Ctrl + S (Windows) / Cmd + S (Mac)
または
File → Save
```

---

## ✅ 動作確認

### Test 1: GameBootstrap初期化確認

1. **Play Modeに入る**
   ```
   Play ボタンをクリック
   ```

2. **Consoleを開く**
   ```
   Window → General → Console (Ctrl + Shift + C)
   ```

**期待されるログ**:
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

**成功判定**: 上記のログがすべて表示されればOK

---

### Test 2: DontDestroyOnLoad確認

1. **Play Mode中にHierarchyを確認**
   ```
   Hierarchy → DontDestroyOnLoad セクションを展開
   ```

**期待される構造**:
```
Hierarchy:
├── DontDestroyOnLoad
│   ├── GameBootstrap
│   ├── PhotonNetworkService  ← Prefabからインスタンス化
│   └── PhotonGameSyncService  ← Prefabからインスタンス化
└── (他のシーンオブジェクト)
```

**成功判定**: PhotonNetworkServiceとPhotonGameSyncServiceが表示されていればOK

---

### Test 3: サービス登録確認

**Console確認** (Play Mode):
```
[GameBootstrap] INetworkService (Photon) registered.
[GameBootstrap] IGameSyncService (Photon) registered.
```

**Hierarchy確認** (Play Mode):
```
DontDestroyOnLoad/PhotonNetworkService
DontDestroyOnLoad/PhotonGameSyncService
```

**成功判定**: 両方のログが表示され、Hierarchyに2つのサービスが存在すればOK

---

### Test 4: NetworkTestRunner実行（オプション）

前提条件: Photon接続設定完了（App ID設定済み）

1. **GameBootstrapにNetworkTestRunnerを追加**
   ```
   Hierarchy → GameBootstrap を選択
   Inspector → Add Component
   検索: "NetworkTestRunner"
   追加
   ```

2. **Play Mode実行**
   ```
   Play ボタンをクリック
   ```

3. **Console確認**

**期待されるログ**:
```
[NetworkTestRunner] 3秒待機中...
[NetworkTestRunner] ===== 接続テスト開始 =====
[NetworkTestRunner] Test 1: サービス登録確認
[NetworkTestRunner] ✓ INetworkService登録確認
[NetworkTestRunner] ✓ IGameSyncService登録確認
[NetworkTestRunner] Test 1: ✅ 成功
[NetworkTestRunner] Test 2: Photonサーバー接続確認
[PhotonNetworkService] Photonサーバーに接続中...
[PhotonNetworkService] Photonマスターサーバー接続成功
[NetworkTestRunner] ✓ Photon接続成功
[NetworkTestRunner] Test 2: ✅ 成功
[NetworkTestRunner] Test 3: ルーム作成確認
[PhotonNetworkService] ルーム作成中: TestRoom_xxxxxxxx
[PhotonNetworkService] ルーム作成成功: TestRoom_xxxxxxxx
[NetworkTestRunner] ✓ ルーム作成成功: TestRoom_xxxxxxxx
[NetworkTestRunner] Test 3: ✅ 成功
[NetworkTestRunner] Test 4: IGameSyncService確認
[NetworkTestRunner] ✓ IsSyncEnabled=false（正常: 2人未満）
[NetworkTestRunner] Test 4: ✅ 成功
[NetworkTestRunner] ===== 全テスト成功 ✅ =====
```

**成功判定**: すべてのテストが✅になればOK

---

## 🛠️ トラブルシューティング

### エラー1: "PhotonNetworkServiceコンポーネントが見つかりません"

**原因**: NetworkService.prefabにPhot onNetworkServiceが追加されていない

**解決策**:
1. Project/Assets/Prefabs/Services/NetworkService.prefab をダブルクリック
2. Inspector → Add Component → PhotonNetworkService を追加
3. Ctrl + S で保存
4. Hierarchy の NetworkService を削除
5. Play Mode再実行

---

### エラー2: "PhotonView could not find owner"

**原因**: PhotonViewがGameSyncService.prefabに追加されていない

**解決策**:
1. Project/Assets/Prefabs/Services/GameSyncService.prefab をダブルクリック
2. Inspector → Add Component → Photon View を追加
3. Ctrl + S で保存
4. Play Mode再実行

---

### エラー3: "NetworkServicePrefabが設定されていません"

**原因**: GameBootstrap InspectorにPrefabが設定されていない

**解決策**:
1. Hierarchy → GameBootstrap を選択
2. Inspector → Network Service Prefabs セクション確認
3. NetworkService.prefabとGameSyncService.prefabをドラッグ&ドロップ
4. Ctrl + S でシーン保存
5. Play Mode再実行

---

### エラー4: "7 services registered"が表示されない

**原因**: サービス登録が一部失敗している

**解決策**:
1. Consoleで最初のエラーログを確認
2. 該当サービスのPrefab設定を確認
3. PhotonNetworkService/PhotonGameSyncServiceコンポーネント確認
4. GameBootstrap.csのRegisterNetworkServices()メソッド確認

---

### エラー5: Prefabが見つからない

**原因**: Prefabの保存場所が異なる

**解決策**:
1. Projectウィンドウで検索
   ```
   検索欄: "NetworkService"
   検索欄: "GameSyncService"
   ```
2. 見つかったPrefabをAssets/Prefabs/Services/に移動
3. GameBootstrap InspectorでPrefab再設定

---

## 📊 確認チェックリスト

### Prefab作成確認

- [ ] NetworkService.prefab 作成済み（Assets/Prefabs/Services/）
- [ ] NetworkService.prefabにPhot onNetworkServiceコンポーネントあり
- [ ] GameSyncService.prefab 作成済み（Assets/Prefabs/Services/）
- [ ] GameSyncService.prefabにPhot onGameSyncServiceコンポーネントあり
- [ ] GameSyncService.prefabにPhot onViewコンポーネントあり

### GameBootstrap設定確認

- [ ] Startup.unityシーンを開いている
- [ ] GameBootstrap選択済み
- [ ] Network Service Prefabに NetworkService.prefab設定済み
- [ ] Game Sync Service Prefabに GameSyncService.prefab設定済み
- [ ] Startup.unityシーン保存済み（Ctrl + S）

### 動作確認

- [ ] Play Mode実行でエラーなし
- [ ] Console: "INetworkService (Photon) registered." 表示
- [ ] Console: "IGameSyncService (Photon) registered." 表示
- [ ] Console: "7 services registered successfully." 表示
- [ ] Hierarchy: DontDestroyOnLoad/PhotonNetworkService 存在
- [ ] Hierarchy: DontDestroyOnLoad/PhotonGameSyncService 存在

---

## 🚀 Next Steps

### 設定完了後

1. **TicTacToeHexシーン統合** (Day 2-3)
   - TicTacToeHexOnlineController使用
   - オンライン/オフライン切り替えUI実装

2. **MainMenu Multiplayer対応** (Day 4-5)
   - Multiplayerボタン追加
   - ルーム作成/参加UI実装
   - 接続状態表示

3. **2クライアント接続テスト** (Day 6-7)
   - Unity Editor + Standalone Build
   - 同期精度テスト
   - Week 1完了サマリー作成

---

## 📖 References

- **PHASE5_WEEK1_IMPLEMENTATION_GUIDE.md**: 実装詳細
- **PHOTON_PUN_SETUP_GUIDE.md**: Photonセットアップ
- **GameBootstrap.cs**: サービス登録コード
- **PhotonNetworkService.cs**: ネットワークサービス実装
- **PhotonGameSyncService.cs**: ゲーム同期実装

---

**最終更新**: 2026-03-16  
**ステータス**: Prefab Configuration Guide Complete  
**検証**: Unity Editor 2021.3 LTS
