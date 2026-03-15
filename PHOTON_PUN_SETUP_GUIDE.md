# Photon PUN 2 Setup Guide - ShaderOp Project

**Phase 5 Week 1**: Photon PUN統合セットアップガイド  
**対象**: Unity 2021.3 LTS以降  
**Photon Version**: PUN 2 FREE  
**所要時間**: 約2時間

---

## 📋 Table of Contents

1. [前提条件](#前提条件)
2. [Photonアカウント作成](#photonアカウント作成)
3. [Unity Asset Storeからインポート](#unity-asset-storeからインポート)
4. [App ID設定](#app-id設定)
5. [Service Prefab作成](#service-prefab作成)
6. [GameBootstrap統合](#gamebootstrap統合)
7. [接続テスト](#接続テスト)
8. [トラブルシューティング](#トラブルシューティング)

---

## 🎯 前提条件

### 必須環境

- Unity 2021.3 LTS以降
- UniTask 2.3.3以降（既にインストール済み）
- インターネット接続（Photonサーバー通信用）

### 確認項目

**UniTaskインストール確認**:
```
Packages/manifest.json に以下が存在すること:
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
```

**既存サービス確認**:
```csharp
// ShaderOptimizer/Assets/Scripts/Runtime/Core/Services/ に以下が存在:
- INetworkService.cs
- IGameSyncService.cs
- PhotonNetworkService.cs
- PhotonGameSyncService.cs
```

---

## 🌐 Photonアカウント作成

### Step 1: アカウント登録

1. **Photon公式サイトにアクセス**
   - URL: https://www.photonengine.com/

2. **Sign Upをクリック**
   - 右上の "Sign Up" ボタン

3. **登録情報入力**
   - Email Address: メールアドレス入力
   - Password: パスワード設定（8文字以上）
   - ✓ I agree to the Terms of Service

4. **メール認証**
   - 受信トレイで "Verify Email" リンクをクリック

---

### Step 2: App ID取得

1. **Dashboard にログイン**
   - URL: https://dashboard.photonengine.com/

2. **Create New App をクリック**
   - Dashboard → Applications → "Create New App"

3. **アプリ設定**
   ```
   Photon Type: Photon PUN
   Name: ShaderOp
   Description: Online Social Mobile Game with Hex Board Games
   Url: (空白でOK)
   ```

4. **Create をクリック**
   - App ID が自動生成される（例: 12345678-abcd-1234-abcd-1234567890ab）

5. **App ID をコピー**
   - App ID欄の右側にあるコピーアイコンをクリック
   - **重要**: このIDは後で使用するため、メモ帳等に保存

---

### Photon FREE Plan制限確認

| 項目 | FREE Plan | 備考 |
|-----|----------|-----|
| 同時接続ユーザー数 | 最大20人 | CCU (Concurrent Users) |
| メッセージ数/秒 | 無制限 | 帯域制限なし |
| ルーム数 | 無制限 | 複数ルーム同時作成可能 |
| データ転送量 | 無制限 | 商用利用不可 |

**ShaderOpでの影響**: 開発・テスト段階では十分。リリース時はPLUS Plan検討。

---

## 📦 Unity Asset Storeからインポート

### Step 1: Asset Storeアクセス

**方法1: Unity Editor内から**
```
Window → Asset Store
検索欄に "PUN 2 FREE" と入力
```

**方法2: Webブラウザから**
```
URL: https://assetstore.unity.com/packages/tools/network/pun-2-free-119922
"Add to My Assets" をクリック
```

---

### Step 2: Package Managerからインポート

1. **Package Manager を開く**
   ```
   Window → Package Manager
   ```

2. **My Assets を選択**
   ```
   左上のドロップダウン → "My Assets"
   ```

3. **PUN 2 - FREE を検索**
   ```
   検索欄に "PUN 2" と入力
   ```

4. **Download → Import**
   ```
   Download ボタン → ダウンロード完了後 → Import ボタン
   ```

5. **Import Unity Package ウィンドウ**
   ```
   All チェック → Import
   ```

**インポート時間**: 約3-5分

---

### Step 3: インポート確認

**以下のフォルダが作成されること**:
```
Assets/
├── Photon/
│   ├── PhotonUnityNetworking/
│   │   ├── Code/
│   │   ├── Resources/
│   │   │   └── PhotonServerSettings.asset  ← 重要
│   │   └── ...
│   └── PhotonRealtime/
│       └── Code/
├── PhotonChatApi/ (オプション)
└── ...
```

---

## 🔧 App ID設定

### Step 1: PUN Wizardを開く

```
Window → Photon Unity Networking → PUN Wizard
```

**PUN Wizardウィンドウが表示される**

---

### Step 2: Setup Projectを選択

```
PUN Wizard:
[Setup Project]
[Convert PUN 1 Project]
```

**"Setup Project" をクリック**

---

### Step 3: App ID貼り付け

```
App Id or Email:
[____________________________________________________]
                                         [Setup Project]
```

1. **前の手順でコピーしたApp IDを貼り付け**
   - 例: `12345678-abcd-1234-abcd-1234567890ab`

2. **Setup Project をクリック**

**成功メッセージ**:
```
✓ PUN is set up and ready to use!
PhotonServerSettings has been updated with your App ID.
```

---

### Step 4: PhotonServerSettings確認

**ファイル**: `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`

**Inspector確認項目**:
```
App Id PUN: 12345678-abcd-1234-abcd-1234567890ab  ← 設定されている
App Version: 1.0
Protocol: Udp
Region: jp  ← 日本リージョン推奨
```

**Region設定（オプション）**:
- `jp`: 日本（低レイテンシー）
- `asia`: アジア（バックアップ）
- `usw`: 米国西海岸（グローバルテスト）

---

## 🎨 Service Prefab作成

### Step 1: NetworkService Prefab作成

1. **Hierarchy → Create Empty**
   ```
   右クリック → Create Empty
   名前: NetworkService
   ```

2. **PhotonNetworkServiceコンポーネント追加**
   ```
   Inspector → Add Component
   検索: "PhotonNetworkService"
   PhotonNetworkService (Script) を選択
   ```

3. **Prefab化**
   ```
   Hierarchy の NetworkService を
   Project/Assets/Prefabs/Services/ にドラッグ&ドロップ
   ```

**Prefab作成場所**:
```
Assets/
└── Prefabs/
    └── Services/
        └── NetworkService.prefab  ← 作成
```

---

### Step 2: GameSyncService Prefab作成

1. **Hierarchy → Create Empty**
   ```
   右クリック → Create Empty
   名前: GameSyncService
   ```

2. **PhotonGameSyncServiceコンポーネント追加**
   ```
   Inspector → Add Component
   検索: "PhotonGameSyncService"
   PhotonGameSyncService (Script) を選択
   ```

3. **PhotonViewコンポーネント追加**
   ```
   Inspector → Add Component
   検索: "Photon View"
   Photon View (Script) を選択
   ```

   **Photon View設定**:
   ```
   Observed Components: (空白でOK)
   Ownership: Fixed
   ```

4. **Prefab化**
   ```
   Hierarchy の GameSyncService を
   Project/Assets/Prefabs/Services/ にドラッグ&ドロップ
   ```

**Prefab作成場所**:
```
Assets/
└── Prefabs/
    └── Services/
        ├── NetworkService.prefab
        └── GameSyncService.prefab  ← 作成
```

---

## 🚀 GameBootstrap統合

### Step 1: GameBootstrap.csを開く

**ファイル**: `ShaderOptimizer/Assets/Scripts/Runtime/Core/GameBootstrap.cs`

---

### Step 2: フィールド追加

**既存コードの最後に追加**:

```csharp
[Header("Network Services")]
[SerializeField] private GameObject? _networkServicePrefab;
[SerializeField] private GameObject? _gameSyncServicePrefab;
```

---

### Step 3: RegisterNetworkServices()メソッド追加

**Awake()メソッド内に追加**:

```csharp
private void Awake()
{
    // ... 既存のサービス登録コード ...

    // Network Services登録（最後に追加）
    RegisterNetworkServices();
}
```

**メソッド実装**:

```csharp
/// <summary>
/// ネットワークサービスを登録（Phase 5 Week 1）
/// </summary>
private void RegisterNetworkServices()
{
    // PhotonNetworkService登録
    if (_networkServicePrefab != null)
    {
        GameObject networkServiceObj = Instantiate(_networkServicePrefab);
        DontDestroyOnLoad(networkServiceObj);

        var networkService = networkServiceObj.GetComponent<PhotonNetworkService>();
        if (networkService != null)
        {
            ServiceLocator.Instance.Register<INetworkService>(networkService);
            Debug.Log("[GameBootstrap] INetworkService登録完了");
        }
        else
        {
            Debug.LogError("[GameBootstrap] PhotonNetworkServiceコンポーネントが見つかりません");
        }
    }
    else
    {
        Debug.LogWarning("[GameBootstrap] NetworkServicePrefabが設定されていません");
    }

    // PhotonGameSyncService登録
    if (_gameSyncServicePrefab != null)
    {
        GameObject gameSyncServiceObj = Instantiate(_gameSyncServicePrefab);
        DontDestroyOnLoad(gameSyncServiceObj);

        var gameSyncService = gameSyncServiceObj.GetComponent<PhotonGameSyncService>();
        if (gameSyncService != null)
        {
            ServiceLocator.Instance.Register<IGameSyncService>(gameSyncService);
            Debug.Log("[GameBootstrap] IGameSyncService登録完了");
        }
        else
        {
            Debug.LogError("[GameBootstrap] PhotonGameSyncServiceコンポーネントが見つかりません");
        }
    }
    else
    {
        Debug.LogWarning("[GameBootstrap] GameSyncServicePrefabが設定されていません");
    }
}
```

---

### Step 4: Inspector設定

1. **Startupシーンを開く**
   ```
   Assets/Scenes/Startup.unity をダブルクリック
   ```

2. **Hierarchy → GameBootstrap を選択**

3. **Inspector → Network Services セクション**
   ```
   Network Service Prefab: Assets/Prefabs/Services/NetworkService.prefab
   Game Sync Service Prefab: Assets/Prefabs/Services/GameSyncService.prefab
   ```

   **ドラッグ&ドロップで設定**:
   ```
   Project/Assets/Prefabs/Services/NetworkService.prefab
   → Inspector の Network Service Prefab フィールドにドラッグ

   Project/Assets/Prefabs/Services/GameSyncService.prefab
   → Inspector の Game Sync Service Prefab フィールドにドラッグ
   ```

4. **シーンを保存**
   ```
   Ctrl + S (Windows) / Cmd + S (Mac)
   ```

---

## ✅ 接続テスト

### Test 1: 基本接続テスト（Unity Editor）

**目的**: Photonサーバー接続確認

---

#### 手順

1. **Unity Editor → Play Mode**
   ```
   Play ボタンをクリック
   ```

2. **Console確認**
   ```
   Window → General → Console (Ctrl + Shift + C)
   ```

**期待されるログ**:
```
[GameBootstrap] INetworkService登録完了
[GameBootstrap] IGameSyncService登録完了
[PhotonNetworkService] Photonサーバーに接続中...
[PhotonNetworkService] Photonマスターサーバー接続成功
```

**エラーが出る場合**: [トラブルシューティング](#トラブルシューティング) を参照

---

### Test 2: ルーム作成テスト

**目的**: ルーム作成・参加機能確認

---

#### テストスクリプト作成

**ファイル**: `Assets/Scripts/Tests/NetworkTestRunner.cs`

```csharp
using UnityEngine;
using ShaderOp.Core.Services;
using Cysharp.Threading.Tasks;

/// <summary>
/// ネットワーク接続テスト用スクリプト
/// </summary>
public class NetworkTestRunner : MonoBehaviour
{
    private async void Start()
    {
        // 3秒待機（GameBootstrap初期化完了待ち）
        await UniTask.Delay(3000);

        Debug.Log("[NetworkTestRunner] 接続テスト開始");

        // INetworkService取得
        var networkService = ServiceLocator.Instance.Get<INetworkService>();
        if (networkService == null)
        {
            Debug.LogError("[NetworkTestRunner] INetworkService未登録");
            return;
        }

        // Photonサーバー接続
        bool connected = await networkService.ConnectToServerAsync();
        if (!connected)
        {
            Debug.LogError("[NetworkTestRunner] Photon接続失敗");
            return;
        }

        Debug.Log("[NetworkTestRunner] ✓ Photon接続成功");

        // ルーム作成
        string roomName = "TestRoom_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        bool roomCreated = await networkService.CreateRoomAsync(roomName, maxPlayers: 2);
        if (!roomCreated)
        {
            Debug.LogError("[NetworkTestRunner] ルーム作成失敗");
            return;
        }

        Debug.Log($"[NetworkTestRunner] ✓ ルーム作成成功: {roomName}");
        Debug.Log($"[NetworkTestRunner] 現在のプレイヤー数: {networkService.PlayerCount}");
        Debug.Log($"[NetworkTestRunner] ローカルプレイヤーID: {networkService.LocalPlayerId}");

        Debug.Log("[NetworkTestRunner] ===== 全テスト成功 =====");
    }
}
```

---

#### 実行手順

1. **Startupシーンにテストスクリプト追加**
   ```
   Hierarchy → GameBootstrap を選択
   Inspector → Add Component
   検索: "NetworkTestRunner"
   ```

2. **Play Mode実行**
   ```
   Play ボタンをクリック
   ```

3. **Console確認**

**期待されるログ**:
```
[NetworkTestRunner] 接続テスト開始
[PhotonNetworkService] Photonサーバーに接続中...
[PhotonNetworkService] Photonマスターサーバー接続成功
[NetworkTestRunner] ✓ Photon接続成功
[PhotonNetworkService] ルーム作成中: TestRoom_12345678
[PhotonNetworkService] ルーム作成成功: TestRoom_12345678
[PhotonNetworkService] ルーム参加成功: TestRoom_12345678 (Players: 1)
[NetworkTestRunner] ✓ ルーム作成成功: TestRoom_12345678
[NetworkTestRunner] 現在のプレイヤー数: 1
[NetworkTestRunner] ローカルプレイヤーID: 1
[NetworkTestRunner] ===== 全テスト成功 =====
```

---

### Test 3: 2クライアント同期テスト

**目的**: 2つのクライアント間で移動同期確認

---

#### 手順

1. **Standalone Buildを作成**
   ```
   File → Build Settings
   Platform: Windows/Mac/Linux
   Target Platform: 選択
   Build → ビルド先フォルダ選択 → Build
   ```

2. **クライアント1 (Unity Editor) 起動**
   ```
   Play Mode実行
   Console確認: ルーム作成成功ログ
   ```

3. **クライアント2 (Standalone Build) 起動**
   ```
   ビルドした実行ファイルを起動
   同じルーム名に参加（コード修正必要）
   ```

**期待される動作**:
- クライアント1: "プレイヤー参加: Player2 (ID: 2)"
- クライアント2: "ルーム参加成功: TestRoom_12345678 (Players: 2)"
- ゲーム開始自動発火（PhotonGameSyncService.OnJoinedRoom）

---

## 🛠️ トラブルシューティング

### エラー1: "The type or namespace name 'Photon' could not be found"

**原因**: PUN 2 Assetがインポートされていない

**解決策**:
1. Package Manager → My Assets → PUN 2 FREE → Import
2. Unity Editor再起動
3. Assets/Photon フォルダが存在するか確認

---

### エラー2: "OnConnectedToMaster is never called"

**原因**: App IDが未設定または無効

**解決策**:
1. PhotonServerSettings.asset を開く
2. App Id PUNフィールドが空白でないか確認
3. Photon Dashboardで App IDが有効か確認
4. Unity Editor再起動

---

### エラー3: "DisconnectCause: InvalidAuthentication"

**原因**: App IDの形式が不正

**解決策**:
1. App IDを再コピー（前後にスペースが無いか確認）
2. PhotonServerSettings.asset → App Id PUNに貼り直し
3. Unity Editor再起動

---

### エラー4: "ルーム作成失敗: Room name already exists"

**原因**: 同じルーム名が既に存在

**解決策**:
```csharp
// ルーム名にGUIDを使用してユニーク化
string roomName = "Room_" + System.Guid.NewGuid().ToString().Substring(0, 8);
await networkService.CreateRoomAsync(roomName, 2);
```

---

### エラー5: "RPC method 'RPC_ReceiveMove' not found"

**原因**: PhotonViewが正しく設定されていない

**解決策**:
1. GameSyncService Prefab → PhotonViewコンポーネント確認
2. Observed Components: (空白でOK)
3. PhotonGameSyncService.cs の [PunRPC] 属性確認

---

### エラー6: 接続が遅い（5秒以上かかる）

**原因**: リージョン設定が遠い

**解決策**:
1. PhotonServerSettings.asset → Region: `jp` に変更
2. または `asia` に設定（日本から近い）

---

## 📊 接続確認チェックリスト

### Setup完了確認

- [ ] Photonアカウント作成済み
- [ ] App ID取得済み
- [ ] PUN 2 FREE インポート済み
- [ ] PhotonServerSettings.asset にApp ID設定済み
- [ ] NetworkService.prefab 作成済み
- [ ] GameSyncService.prefab 作成済み
- [ ] GameBootstrap.cs に RegisterNetworkServices() 追加済み
- [ ] Startup.unity にPrefab設定済み

### 接続テスト確認

- [ ] Test 1: 基本接続成功（Console: "Photonマスターサーバー接続成功"）
- [ ] Test 2: ルーム作成成功（Console: "ルーム作成成功"）
- [ ] Test 3: 2クライアント同期成功（両方のConsoleでログ確認）

---

## 📖 Next Steps

### Week 1 残タスク

1. **TicTacToeHex統合** (Day 3-4)
   - TicTacToeHexController.cs にIGameSyncService統合
   - OnMoveReceived イベントハンドラ実装
   - オフライン/オンライン切り替え対応

2. **UI統合** (Day 5)
   - MainMenuView.cs にMultiplayerボタン追加
   - 接続状態表示UI実装
   - ルーム一覧表示（オプション）

3. **Week 1ドキュメント作成** (Day 6-7)
   - Week 1完了サマリー
   - 統合ガイド（Phase 4 → Phase 5移行）
   - パフォーマンス検証レポート

---

## 📚 References

- **Photon PUN 2 Documentation**: https://doc.photonengine.com/pun/current/getting-started/pun-intro
- **UniTask GitHub**: https://github.com/Cysharp/UniTask
- **ShaderOp Phase 5 Kickoff**: `PHASE5_KICKOFF.md`
- **Phase 4 Complete Summary**: `PHASE4_COMPLETE_SUMMARY.md`

---

**最終更新**: 2026-03-16  
**ステータス**: Setup Guide 完成  
**検証**: 未実施（Photon Assetインポート待ち）
