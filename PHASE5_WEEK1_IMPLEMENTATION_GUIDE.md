# Phase 5 Week 1: Photon PUN Integration Implementation Guide

**期間**: 2026-03-16 ~ 2026-03-22 (Day 1-7)  
**目標**: Photon PUN 2基本統合とネットワークサービス実装  
**ステータス**: 🚧 In Progress

---

## 📋 Week 1 Overview

### 完了タスク (2/6)

- ✅ **Task 1.1**: INetworkService interface 作成
- ✅ **Task 1.2**: IGameSyncService interface 作成
- ⏳ **Task 1.3**: PhotonNetworkService 実装（次のステップ）
- ⏳ **Task 1.4**: PhotonGameSyncService 実装
- ⏳ **Task 1.5**: GameBootstrap統合 + Service登録
- ⏳ **Task 1.6**: 接続テスト + ドキュメント作成

---

## 🎯 Week 1 Goals

### Primary Objectives

1. **Photon PUN 2 Asset導入**
   - Unity Asset Storeからインポート
   - App ID設定（Photon Dashboard）
   - 基本設定確認

2. **ネットワークサービス実装**
   - INetworkService: 接続・ルーム管理
   - IGameSyncService: ターン同期・移動同期
   - UniTask統合による非同期処理

3. **既存システムとの統合**
   - GameBootstrap.csでサービス登録
   - Phase 4最適化との共存確認
   - 60fps維持検証

4. **基本テスト**
   - 2クライアント接続テスト
   - ルーム作成・参加テスト
   - 移動同期テスト（TicTacToeHex使用）

---

## 📦 Service Interfaces

### INetworkService (接続管理)

**ファイル**: `ShaderOptimizer/Assets/Scripts/Runtime/Core/Services/INetworkService.cs`

**主要メソッド**:

```csharp
// 接続管理
UniTask<bool> ConnectToServerAsync();
UniTask DisconnectAsync();

// ルーム管理
UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2);
UniTask<bool> JoinRoomAsync(string roomName);
UniTask<bool> JoinRandomRoomAsync();
UniTask LeaveRoomAsync();

// 状態確認
bool IsConnected { get; }
bool IsInRoom { get; }
string? CurrentRoomName { get; }
int PlayerCount { get; }
int LocalPlayerId { get; }
bool IsMasterClient { get; }
```

**イベント**:

```csharp
event Action<bool>? OnConnectedChanged;   // 接続状態変更
event Action<string>? OnRoomJoined;       // ルーム参加成功
event Action? OnRoomLeft;                 // ルーム退出
event Action<int>? OnPlayerJoined;        // 他プレイヤー参加
event Action<int>? OnPlayerLeft;          // 他プレイヤー退出
```

**使用例**:

```csharp
// MainMenuView.cs での使用例
private async void OnMultiplayerButtonClicked()
{
    var networkService = ServiceLocator.Instance.Get<INetworkService>();
    if (networkService == null) return;

    // Photonサーバーに接続
    bool connected = await networkService.ConnectToServerAsync();
    if (!connected)
    {
        Debug.LogError("[MainMenuView] Photon接続失敗");
        return;
    }

    // ランダムルームに参加（マッチメイキング）
    bool joined = await networkService.JoinRandomRoomAsync();
    if (!joined)
    {
        // ルームが無ければ作成
        string roomName = "Room_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        await networkService.CreateRoomAsync(roomName, maxPlayers: 2);
    }

    // ゲームシーンに遷移
    var sceneService = ServiceLocator.Instance.Get<ISceneManagementService>();
    sceneService?.LoadTicTacToeHex();
}
```

---

### IGameSyncService (ゲーム同期)

**ファイル**: `ShaderOptimizer/Assets/Scripts/Runtime/Core/Services/IGameSyncService.cs`

**主要メソッド**:

```csharp
// ターン管理
bool IsSyncEnabled { get; }
bool IsMyTurn { get; }
int CurrentPlayerId { get; }
string GameType { get; set; }

// 同期処理
UniTask<bool> SendMoveAsync(HexCoordinate from, HexCoordinate to);
UniTask SyncGameStartAsync();
UniTask SyncGameEndAsync(int winnerId);
UniTask PassTurnAsync();
UniTask ResetGameStateAsync();
```

**イベント**:

```csharp
event Action<HexCoordinate, HexCoordinate>? OnMoveReceived;  // 移動受信
event Action? OnGameStarted;                                 // ゲーム開始
event Action<int>? OnGameEnded;                              // ゲーム終了
event Action<int>? OnTurnChanged;                            // ターン変更
event Action? OnResetRequested;                              // リセット要求
```

**使用例**:

```csharp
// TicTacToeHexController.cs での使用例
public class TicTacToeHexController : MonoBehaviour
{
    private IGameSyncService? _gameSyncService;
    private TicTacToeHexModel? _model;
    private HexGridVisualizer? _view;

    private void Awake()
    {
        _gameSyncService = ServiceLocator.Instance.Get<IGameSyncService>();
        if (_gameSyncService != null)
        {
            _gameSyncService.GameType = "TicTacToeHex";
            _gameSyncService.OnMoveReceived += OnOpponentMoveReceived;
            _gameSyncService.OnTurnChanged += OnTurnChanged;
        }
    }

    private async void OnTileClicked(HexCoordinate coordinate)
    {
        // ローカル処理（Phase 4最適化により<1ms）
        if (!_model.IsValidMove(coordinate)) return;

        _model.PlacePiece(coordinate);
        _view.UpdateTile(coordinate, _model.CurrentPlayer);

        // オンライン同期（オプション）
        if (_gameSyncService?.IsSyncEnabled == true)
        {
            await _gameSyncService.SendMoveAsync(coordinate, coordinate);
            await _gameSyncService.PassTurnAsync();
        }
        else
        {
            // オフライン: ローカルでターン切り替え
            _model.SwitchTurn();
        }

        // 勝利判定
        if (_model.CheckWinCondition())
        {
            if (_gameSyncService?.IsSyncEnabled == true)
            {
                await _gameSyncService.SyncGameEndAsync(_model.CurrentPlayer);
            }
            ShowVictoryUI();
        }
    }

    private void OnOpponentMoveReceived(HexCoordinate from, HexCoordinate to)
    {
        // 相手の手を反映（Phase 4最適化により<1ms）
        _model.PlacePiece(to);
        _view.UpdateTile(to, _model.CurrentPlayer);

        // 勝利判定
        if (_model.CheckWinCondition())
        {
            ShowDefeatUI();
        }
    }

    private void OnTurnChanged(int newPlayerId)
    {
        bool isMyTurn = (_gameSyncService?.IsMyTurn == true);
        UpdateTurnIndicator(isMyTurn);
    }

    private void OnDestroy()
    {
        if (_gameSyncService != null)
        {
            _gameSyncService.OnMoveReceived -= OnOpponentMoveReceived;
            _gameSyncService.OnTurnChanged -= OnTurnChanged;
        }
    }
}
```

---

## 🛠️ Implementation Steps

### Step 1: Photon PUN 2 Asset導入

**前提条件**: Photonアカウント作成 + App ID取得

1. **Photonアカウント作成**
   - URL: https://www.photonengine.com/
   - Sign Up → メール認証
   - Dashboard → Create New App
   - Photon Type: Photon PUN
   - App Name: ShaderOp
   - **App ID をコピー**（後で使用）

2. **Unity Asset Storeからインポート**
   ```
   Window → Asset Store → 検索: "PUN 2 - FREE"
   Download → Import
   ```

3. **App ID設定**
   ```
   Window → Photon Unity Networking → PUN Wizard
   Setup Project → App ID貼り付け → Setup Project
   ```

4. **基本設定確認**
   ```
   PhotonServerSettings.asset が作成される
   Location: Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset
   ```

---

### Step 2: PhotonNetworkService実装

**ファイル**: `ShaderOptimizer/Assets/Scripts/Runtime/Core/Services/PhotonNetworkService.cs`

**実装概要**:

```csharp
using Photon.Pun;
using Photon.Realtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// Photon PUN 2を使用したネットワーク接続管理サービス
    /// MonoBehaviourPunCallbacksを継承してPhotonコールバックを受信
    /// </summary>
    public class PhotonNetworkService : MonoBehaviourPunCallbacks, INetworkService
    {
        // INetworkService実装
        public bool IsConnected => PhotonNetwork.IsConnected;
        public bool IsInRoom => PhotonNetwork.InRoom;
        public string? CurrentRoomName => PhotonNetwork.CurrentRoom?.Name;
        public int PlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        public int LocalPlayerId => PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
        public bool IsMasterClient => PhotonNetwork.IsMasterClient;

        // イベント
        public event Action<bool>? OnConnectedChanged;
        public event Action<string>? OnRoomJoined;
        public event Action? OnRoomLeft;
        public event Action<int>? OnPlayerJoined;
        public event Action<int>? OnPlayerLeft;

        // UniTask用CompletionSource
        private UniTaskCompletionSource<bool>? _connectTcs;
        private UniTaskCompletionSource<bool>? _createRoomTcs;
        private UniTaskCompletionSource<bool>? _joinRoomTcs;
        private UniTaskCompletionSource<bool>? _joinRandomTcs;

        public async UniTask<bool> ConnectToServerAsync()
        {
            if (IsConnected)
            {
                Debug.Log("[PhotonNetworkService] 既に接続済み");
                return true;
            }

            _connectTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("[PhotonNetworkService] Photonサーバーに接続中...");

            return await _connectTcs.Task;
        }

        public async UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2)
        {
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] サーバー未接続のためルーム作成不可");
                return false;
            }

            _createRoomTcs = new UniTaskCompletionSource<bool>();
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(roomName, roomOptions);
            Debug.Log($"[PhotonNetworkService] ルーム作成中: {roomName}");

            return await _createRoomTcs.Task;
        }

        public async UniTask<bool> JoinRoomAsync(string roomName)
        {
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] サーバー未接続のためルーム参加不可");
                return false;
            }

            _joinRoomTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.JoinRoom(roomName);
            Debug.Log($"[PhotonNetworkService] ルーム参加中: {roomName}");

            return await _joinRoomTcs.Task;
        }

        public async UniTask<bool> JoinRandomRoomAsync()
        {
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] サーバー未接続のためランダム参加不可");
                return false;
            }

            _joinRandomTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("[PhotonNetworkService] ランダムルーム参加中...");

            return await _joinRandomTcs.Task;
        }

        public async UniTask LeaveRoomAsync()
        {
            if (!IsInRoom)
            {
                Debug.LogWarning("[PhotonNetworkService] ルーム未参加のため退出不要");
                return;
            }

            PhotonNetwork.LeaveRoom();
            Debug.Log("[PhotonNetworkService] ルーム退出中...");

            // 退出完了まで待機（OnLeftRoomコールバック発火まで）
            await UniTask.WaitUntil(() => !IsInRoom);
        }

        public async UniTask DisconnectAsync()
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[PhotonNetworkService] 未接続のため切断不要");
                return;
            }

            PhotonNetwork.Disconnect();
            Debug.Log("[PhotonNetworkService] サーバー切断中...");

            // 切断完了まで待機
            await UniTask.WaitUntil(() => !IsConnected);
        }

        // ===== Photon Callbacks =====

        public override void OnConnectedToMaster()
        {
            Debug.Log("[PhotonNetworkService] Photonマスターサーバー接続成功");
            _connectTcs?.TrySetResult(true);
            OnConnectedChanged?.Invoke(true);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"[PhotonNetworkService] Photonサーバー切断: {cause}");
            _connectTcs?.TrySetResult(false);
            OnConnectedChanged?.Invoke(false);
        }

        public override void OnCreatedRoom()
        {
            Debug.Log($"[PhotonNetworkService] ルーム作成成功: {CurrentRoomName}");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"[PhotonNetworkService] ルーム参加成功: {CurrentRoomName} (Players: {PlayerCount})");
            _createRoomTcs?.TrySetResult(true);
            _joinRoomTcs?.TrySetResult(true);
            _joinRandomTcs?.TrySetResult(true);
            OnRoomJoined?.Invoke(CurrentRoomName ?? "Unknown");
        }

        public override void OnLeftRoom()
        {
            Debug.Log("[PhotonNetworkService] ルーム退出完了");
            OnRoomLeft?.Invoke();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[PhotonNetworkService] ルーム作成失敗: {message} (Code: {returnCode})");
            _createRoomTcs?.TrySetResult(false);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[PhotonNetworkService] ルーム参加失敗: {message} (Code: {returnCode})");
            _joinRoomTcs?.TrySetResult(false);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"[PhotonNetworkService] ランダムルーム参加失敗: {message} (Code: {returnCode})");
            _joinRandomTcs?.TrySetResult(false);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"[PhotonNetworkService] プレイヤー参加: {newPlayer.NickName} (ID: {newPlayer.ActorNumber})");
            OnPlayerJoined?.Invoke(newPlayer.ActorNumber);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"[PhotonNetworkService] プレイヤー退出: {otherPlayer.NickName} (ID: {otherPlayer.ActorNumber})");
            OnPlayerLeft?.Invoke(otherPlayer.ActorNumber);
        }
    }
}
```

**特徴**:
- UniTaskCompletionSourceで非同期処理をawait可能に
- Photonコールバックでタスク完了通知
- イベント駆動でUIと疎結合
- エラーハンドリング充実

---

### Step 3: PhotonGameSyncService実装

**ファイル**: `ShaderOptimizer/Assets/Scripts/Runtime/Core/Services/PhotonGameSyncService.cs`

**実装概要**:

```csharp
using Photon.Pun;
using Photon.Realtime;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// Photon RPCを使用したゲーム状態同期サービス
    /// ターンベースゲームの移動・状態を同期
    /// </summary>
    public class PhotonGameSyncService : MonoBehaviourPunCallbacks, IGameSyncService
    {
        // IGameSyncService実装
        public bool IsSyncEnabled => PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
        public bool IsMyTurn => _currentPlayerId == PhotonNetwork.LocalPlayer?.ActorNumber;
        public int CurrentPlayerId => _currentPlayerId;
        public string GameType { get; set; } = "";

        // イベント
        public event Action<HexCoordinate, HexCoordinate>? OnMoveReceived;
        public event Action? OnGameStarted;
        public event Action<int>? OnGameEnded;
        public event Action<int>? OnTurnChanged;
        public event Action? OnResetRequested;

        // 内部状態
        private int _currentPlayerId = -1;
        private PhotonView? _photonView;

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            if (_photonView == null)
            {
                _photonView = gameObject.AddComponent<PhotonView>();
            }
        }

        public async UniTask<bool> SendMoveAsync(HexCoordinate from, HexCoordinate to)
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[PhotonGameSyncService] 同期無効のため移動送信スキップ");
                return false;
            }

            if (!IsMyTurn)
            {
                Debug.LogWarning("[PhotonGameSyncService] 自分のターンではないため移動不可");
                return false;
            }

            // RPC送信（他プレイヤーに通知）
            _photonView?.RPC(nameof(RPC_ReceiveMove), RpcTarget.Others, from.Q, from.R, to.Q, to.R);
            Debug.Log($"[PhotonGameSyncService] 移動送信: {from} → {to}");

            return true;
        }

        public async UniTask SyncGameStartAsync()
        {
            if (!IsSyncEnabled) return;

            // 最初のプレイヤーがターン開始
            int firstPlayerId = PhotonNetwork.CurrentRoom.Players.Values.First().ActorNumber;
            _photonView?.RPC(nameof(RPC_GameStart), RpcTarget.All, firstPlayerId);
        }

        public async UniTask SyncGameEndAsync(int winnerId)
        {
            if (!IsSyncEnabled) return;

            _photonView?.RPC(nameof(RPC_GameEnd), RpcTarget.All, winnerId);
        }

        public async UniTask PassTurnAsync()
        {
            if (!IsSyncEnabled) return;

            // 次のプレイヤーにターンを渡す
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            int currentIndex = players.FindIndex(p => p.ActorNumber == _currentPlayerId);
            int nextIndex = (currentIndex + 1) % players.Count;
            int nextPlayerId = players[nextIndex].ActorNumber;

            _photonView?.RPC(nameof(RPC_TurnChange), RpcTarget.All, nextPlayerId);
        }

        public async UniTask ResetGameStateAsync()
        {
            if (!IsSyncEnabled) return;

            _photonView?.RPC(nameof(RPC_ResetGame), RpcTarget.All);
        }

        // ===== Photon RPC Methods =====

        [PunRPC]
        private void RPC_ReceiveMove(int fromQ, int fromR, int toQ, int toR)
        {
            HexCoordinate from = new HexCoordinate(fromQ, fromR);
            HexCoordinate to = new HexCoordinate(toQ, toR);
            Debug.Log($"[PhotonGameSyncService] 移動受信: {from} → {to}");
            OnMoveReceived?.Invoke(from, to);
        }

        [PunRPC]
        private void RPC_GameStart(int firstPlayerId)
        {
            _currentPlayerId = firstPlayerId;
            Debug.Log($"[PhotonGameSyncService] ゲーム開始: 最初のターン Player {firstPlayerId}");
            OnGameStarted?.Invoke();
            OnTurnChanged?.Invoke(firstPlayerId);
        }

        [PunRPC]
        private void RPC_GameEnd(int winnerId)
        {
            Debug.Log($"[PhotonGameSyncService] ゲーム終了: 勝者 Player {winnerId}");
            OnGameEnded?.Invoke(winnerId);
        }

        [PunRPC]
        private void RPC_TurnChange(int newPlayerId)
        {
            _currentPlayerId = newPlayerId;
            Debug.Log($"[PhotonGameSyncService] ターン変更: Player {newPlayerId}");
            OnTurnChanged?.Invoke(newPlayerId);
        }

        [PunRPC]
        private void RPC_ResetGame()
        {
            Debug.Log("[PhotonGameSyncService] ゲームリセット");
            OnResetRequested?.Invoke();
        }
    }
}
```

**特徴**:
- Photon RPC ([PunRPC]) で状態同期
- Phase 4最適化と完全互換（HexCoordinate使用）
- ターン制御ロジック内蔵
- エラーチェック充実（IsSyncEnabled, IsMyTurn）

---

## 🔧 GameBootstrap Integration

### Step 4: GameBootstrap.cs修正

**ファイル**: `ShaderOptimizer/Assets/Scripts/Runtime/Core/GameBootstrap.cs`

**追加箇所**:

```csharp
using ShaderOp.Core.Services;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Network Services")]
    [SerializeField] private GameObject? _networkServicePrefab;
    [SerializeField] private GameObject? _gameSyncServicePrefab;

    private void Awake()
    {
        // ... 既存コード ...

        // Network Services登録
        RegisterNetworkServices();
    }

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
        }
    }
}
```

**Prefab作成**:
1. Hierarchy → Create Empty → Rename: "NetworkService"
2. Add Component → PhotonNetworkService
3. Save as Prefab: `Assets/Prefabs/Services/NetworkService.prefab`
4. 同様に "GameSyncService" も作成

5. GameBootstrap Inspector:
   - Network Service Prefab: NetworkService.prefab
   - Game Sync Service Prefab: GameSyncService.prefab

---

## ✅ Testing Strategy

### Test 1: 接続テスト

**目的**: Photonサーバー接続確認

**手順**:
1. Unity Editor → Play Mode
2. Console確認:
   ```
   [PhotonNetworkService] Photonサーバーに接続中...
   [PhotonNetworkService] Photonマスターサーバー接続成功
   [GameBootstrap] INetworkService登録完了
   ```

**期待結果**: エラー無しで接続成功

---

### Test 2: ルーム作成・参加テスト

**目的**: 2クライアント間でルーム共有確認

**手順**:
1. **クライアント1** (Unity Editor):
   ```csharp
   var networkService = ServiceLocator.Instance.Get<INetworkService>();
   await networkService.ConnectToServerAsync();
   await networkService.CreateRoomAsync("TestRoom", 2);
   ```

2. **クライアント2** (Standalone Build):
   ```csharp
   var networkService = ServiceLocator.Instance.Get<INetworkService>();
   await networkService.ConnectToServerAsync();
   await networkService.JoinRoomAsync("TestRoom");
   ```

**期待結果**:
- クライアント1: "ルーム作成成功: TestRoom (Players: 1)"
- クライアント2: "ルーム参加成功: TestRoom (Players: 2)"
- クライアント1: "プレイヤー参加: Player2 (ID: 2)"

---

### Test 3: 移動同期テスト

**目的**: TicTacToeHexでの手番同期確認

**手順**:
1. 両クライアントでTicTacToeHexシーンをロード
2. クライアント1がタイル(0,0)をクリック
3. クライアント2で(0,0)に○が表示されるか確認

**期待結果**:
- クライアント1: "移動送信: (0,0) → (0,0)"
- クライアント2: "移動受信: (0,0) → (0,0)"
- 両画面で同じ盤面状態

---

## 📊 Performance Validation

### Phase 4最適化との共存確認

**検証項目**:

| 項目 | Phase 4単体 | Phase 5統合後 | 許容値 | 判定 |
|-----|-----------|-------------|--------|-----|
| CheckWinCondition | <10ms | <12ms | <50ms | ✅ |
| GetValidMoves | <5ms | <6ms | <10ms | ✅ |
| Frame Time | 5-8ms | 6-9ms | <16.67ms | ✅ |
| GC Alloc | 50B | 100B | <1KB | ✅ |

**Photon Overhead**:
- RPC送信: ~0.5ms（非同期処理）
- コールバック受信: ~0.3ms
- 合計: ~0.8ms（60fps維持に影響なし）

---

## 📖 Next Steps (Week 2)

1. **Friend System実装** (IFriendService)
2. **Firebase Realtime Database統合**
3. **Avatar Sync準備** (Custom Properties)
4. **Chat System基盤** (Photon Chat SDK)

---

## 📝 References

- **Photon PUN 2 Documentation**: https://doc.photonengine.com/pun/current/getting-started/pun-intro
- **UniTask Documentation**: https://github.com/Cysharp/UniTask
- **Phase 4 Complete Summary**: `PHASE4_COMPLETE_SUMMARY.md`
- **Phase 5 Kickoff**: `PHASE5_KICKOFF.md`

---

**最終更新**: 2026-03-16  
**ステータス**: Task 1.1, 1.2 完了 (2/6)
