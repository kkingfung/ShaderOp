# Lobby UI Integration Guide (Phase 5 Week 2 Day 4-5)

**Agent Duel Challenge: unity-developer ↔ ui-ux-designer**

## 概要

オンラインマルチプレイヤーロビーUIの統合ガイド。
**unity-developer**がバックエンド実装を完了。
**ui-ux-designer**はUXML/USSを作成してください。

## 実装済みファイル

### 1. LobbyViewModel.cs
**場所**: `ShaderOptimizer/Assets/Scripts/Runtime/UI/ViewModels/LobbyViewModel.cs`

**主要プロパティ**:
```csharp
// リアクティブプロパティ（UniRx）
ReactiveProperty<string> JoinCode           // 6桁のJoin Code
ObservableCollection<LobbyPlayerInfo> PlayerList  // プレイヤーリスト（動的更新）
ReactiveProperty<bool> LocalPlayerReady     // ローカルプレイヤーの準備状態
ReactiveProperty<bool> IsHost               // ホストプレイヤーか
ReactiveProperty<bool> CanStartGame         // ゲーム開始可能か
ReactiveProperty<int> PlayerCount           // 現在のプレイヤー数
ReactiveProperty<int> MaxPlayers            // 最大プレイヤー数

// イベント
event Action OnStartGameRequested           // ゲーム開始要求（ホスト専用）
event Action OnLeaveRoomRequested           // ロビー退出要求
event Action<bool> OnReadyToggleRequested   // Ready状態トグル
event Action<string> OnCopyJoinCodeRequested // Join Codeコピー
```

**主要メソッド**:
```csharp
void SetJoinCode(string code)               // Join Code設定
void AddPlayer(string playerId, int gameId, bool isHost) // プレイヤー追加
void RemovePlayer(int gameId)               // プレイヤー削除
void UpdatePlayerReady(int gameId, bool isReady) // Ready状態更新
void ToggleReady()                          // Ready状態トグル
void StartGame()                            // ゲーム開始（ホスト専用）
void LeaveRoom()                            // ロビー退出
void CopyJoinCode()                         // Join Codeコピー
```

### 2. LobbyView.cs
**場所**: `ShaderOptimizer/Assets/Scripts/Runtime/UI/LobbyView.cs`

**サービス依存**:
- `INetworkService`: ルーム管理、プレイヤー参加/離脱
- `IGameSyncService`: ゲーム開始同期
- `IPlayerIdService`: PlayerId↔GameId変換

**UI要素バインディング** (UXML element names):
```csharp
// Header
Label "TitleLabel"                  // ロビータイトル
Button "BackButton"                 // 戻るボタン

// Join Code
Label "JoinCodeLabel"               // 6桁のJoin Code表示
Button "CopyJoinCodeButton"         // Join Codeコピーボタン

// Player List
VisualElement "PlayerListContainer" // プレイヤーリストコンテナ
ScrollView "PlayerListScrollView"   // プレイヤーリストのスクロールビュー

// Controls
Button "ReadyButton"                // 準備完了ボタン（"Ready" / "Cancel Ready"）
Button "StartGameButton"            // ゲーム開始ボタン（ホスト専用、CanStartGameで有効化）
Label "StatusLabel"                 // ステータスメッセージ（例: "Join Code copied!"）
```

## UXML要件（ui-ux-designer向け）

### 必須UI要素

#### 1. ヘッダーセクション
```xml
<ui:Label name="TitleLabel" text="Online Lobby" class="lobby-title" />
<ui:Button name="BackButton" text="Back" class="back-button" />
```

#### 2. Join Codeセクション
```xml
<ui:VisualElement class="join-code-section">
    <ui:Label text="Join Code:" class="join-code-label" />
    <ui:Label name="JoinCodeLabel" text="------" class="join-code-value" />
    <ui:Button name="CopyJoinCodeButton" text="Copy" class="copy-button" />
</ui:VisualElement>
```

#### 3. プレイヤーリストセクション
```xml
<ui:VisualElement name="PlayerListContainer" class="player-list-container">
    <ui:Label text="Players:" class="section-label" />
    <ui:ScrollView name="PlayerListScrollView" class="player-list-scrollview">
        <!-- プレイヤー要素は動的に追加されます -->
    </ui:ScrollView>
</ui:VisualElement>
```

**プレイヤー要素の動的生成**（LobbyView.csで実装済み）:
```csharp
// 各プレイヤーに対して以下の構造を生成:
<ui:VisualElement class="player-item">
    <ui:Label text="Player 0 (Host)" class="player-name" />
    <ui:Label text="Ready ✓" class="player-status" />
</ui:VisualElement>
```

#### 4. コントロールセクション
```xml
<ui:VisualElement class="controls-section">
    <ui:Button name="ReadyButton" text="Ready" class="ready-button" />
    <ui:Button name="StartGameButton" text="Start Game" class="start-game-button" />
    <ui:Label name="StatusLabel" text="" class="status-label" />
</ui:VisualElement>
```

### USS要件

#### レイアウト
- **縦画面（Portrait）専用**
- ヘッダー（タイトル + 戻るボタン）
- Join Code（中央揃え、大きめのフォント）
- プレイヤーリスト（スクロール可能、最大2人）
- コントロール（Ready/Start Gameボタン）

#### スタイリング推奨事項
```css
/* Join Code */
.join-code-value {
    font-size: 32px;
    font-weight: bold;
    color: #FFD700; /* Gold */
    text-align: center;
}

/* プレイヤーリスト */
.player-item {
    flex-direction: row;
    justify-content: space-between;
    padding: 10px;
    margin: 5px 0;
    background-color: rgba(255, 255, 255, 0.1);
    border-radius: 5px;
}

.player-name {
    font-size: 18px;
    color: #FFFFFF;
}

.player-status {
    font-size: 16px;
    color: #00FF00; /* Green for Ready */
}

/* ボタン */
.ready-button {
    background-color: #4CAF50; /* Green */
    font-size: 20px;
    padding: 15px;
    margin: 10px 0;
}

.start-game-button {
    background-color: #FF5722; /* Orange */
    font-size: 24px;
    padding: 20px;
    margin: 10px 0;
}

.start-game-button:disabled {
    background-color: #555555; /* Gray when disabled */
}

/* ステータスラベル */
.status-label {
    font-size: 14px;
    color: #AAAAAA;
    text-align: center;
    margin-top: 10px;
}
```

## 動作仕様

### 1. ルーム作成とJoin Code生成
- `LobbyView.OnEnable()` で自動的にルームを作成
- `UnityMultiplayerNetworkService.CreateRoomWithCodeAsync()` で6桁のJoin Code生成
- `JoinCodeLabel` に表示

### 2. プレイヤー参加/離脱
- `INetworkService.OnPlayerJoined` イベント → `LobbyViewModel.AddPlayer()`
- `INetworkService.OnPlayerLeft` イベント → `LobbyViewModel.RemovePlayer()`
- `PlayerListScrollView` に動的にプレイヤー要素を追加/削除

### 3. Ready状態管理
- `ReadyButton` クリック → `LobbyViewModel.ToggleReady()`
- ボタンテキストが "Ready" ↔ "Cancel Ready" に変更
- プレイヤーリストの `player-status` ラベルに "Ready ✓" 表示

### 4. ゲーム開始（ホスト専用）
- `IsHost.Value == true` のとき `StartGameButton` を表示
- `CanStartGame.Value == true` のとき `StartGameButton` を有効化
- 条件:
  - ホストである
  - 最低2人のプレイヤー
  - 全員がReady状態
- クリック → `IGameSyncService.SyncGameStartAsync()` → ゲームシーンへ遷移

### 5. ロビー退出
- `BackButton` クリック → `LobbyViewModel.LeaveRoom()`
- `INetworkService.LeaveRoomAsync()` → メインメニューへ遷移

### 6. Join Codeコピー
- `CopyJoinCodeButton` クリック → `GUIUtility.systemCopyBuffer` にコピー
- `StatusLabel` に "Join Code copied to clipboard!" 表示

## テスト項目（2デバイステスト用）

### Device 1 (Host)
1. ロビーシーンを開く
2. Join Codeが表示される（例: "123456"）
3. "Copy" ボタンでJoin Codeをコピー
4. プレイヤーリストに "Player 0 (Host)" が表示
5. "Ready" ボタンをクリック → "Cancel Ready" に変更
6. Device 2が参加するまで "Start Game" ボタンは無効

### Device 2 (Guest)
1. メインメニューで "Join Room" を選択
2. Device 1のJoin Codeを入力
3. ロビーに参加 → プレイヤーリストに "Player 1" が追加
4. "Ready" ボタンをクリック

### Device 1 (Host) - ゲーム開始
1. Device 2が参加 → プレイヤーリストに "Player 1" 表示
2. 両方のプレイヤーがReady → "Start Game" ボタンが有効化
3. "Start Game" クリック → ゲームシーンへ遷移

## ui-ux-designerへの要求

### 作成すべきファイル

1. **LobbyView.uxml**
   - 場所: `ShaderOptimizer/Assets/UI/LobbyView.uxml`
   - 上記のUI要素を含む完全なUXML

2. **LobbyView.uss** (または既存のスタイルシートに追加)
   - 場所: `ShaderOptimizer/Assets/UI/Styles/LobbyView.uss`
   - 縦画面レイアウト
   - Cocone風のソーシャルゲームデザイン
   - アクセシビリティ対応（コントラスト、タッチターゲットサイズ）

3. **LobbyView.uxml.meta** と **LobbyView.uss.meta**
   - Unity .metaファイル（GUID生成）

### デザインガイドライン

- **縦画面（Portrait）**: 解像度 1080x1920 (9:16)
- **タッチターゲット**: 最小44x44px（iOS Human Interface Guidelines準拠）
- **フォント**: Roboto-Medium (既存のFlatSkinフォント使用)
- **カラースキーム**:
  - プライマリ: #4CAF50 (Green)
  - アクセント: #FF5722 (Orange)
  - 背景: #1E1E1E (Dark Gray)
  - テキスト: #FFFFFF (White)
- **アニメーション**: トランジション（opacity, scale）で視覚的フィードバック

### 提出物チェックリスト

- [ ] LobbyView.uxml（すべてのUI要素を含む）
- [ ] LobbyView.uss（レスポンシブレイアウト + スタイリング）
- [ ] .metaファイル作成
- [ ] LobbyView.csとの統合確認（element name一致）
- [ ] アクセシビリティ検証（コントラスト、フォントサイズ）

## 統合テスト手順

1. ui-ux-designerがLobbyView.uxml/USSを作成
2. unity-developerがUIDocumentにLobbyView.uxmlを割り当て
3. Unity Editorで再生 → Join Code表示確認
4. 2デバイスでテスト（Host/Guest）
5. プレイヤー参加/離脱の動的更新確認
6. Ready状態とゲーム開始フロー確認

---

**unity-developer**: バックエンド実装完了 ✅
**ui-ux-designer**: UXML/USS実装待ち ⏳
