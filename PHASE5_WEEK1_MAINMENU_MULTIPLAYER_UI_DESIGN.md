# Phase 5 Week 1: MainMenu Multiplayer UI Design

**作成日**: 2026-03-16
**対象**: MainMenu.uxml + MainMenuController.cs
**目的**: Photon PUN マルチプレイヤーUI追加（ルーム作成/参加/接続状態表示）

---

## 📋 目次

1. [UI設計概要](#1-ui設計概要)
2. [UXML構造設計](#2-uxml構造設計)
3. [USS スタイル設計](#3-uss-スタイル設計)
4. [MainMenuController 拡張設計](#4-mainmenucontroller-拡張設計)
5. [インタラクションフロー](#5-インタラクションフロー)
6. [実装チェックリスト](#6-実装チェックリスト)

---

## 1. UI設計概要

### 1.1 デザインコンセプト

**Cocone系ソーシャルゲームUI**（Pokecolo, Livly Island）を参考にした**縦画面モバイル最適化デザイン**:

- **片手操作最適化**: 主要ボタンは画面下部40%に配置
- **Safe Area対応**: iOS Notch + Home Barを考慮
- **タッチターゲット**: 最小48px（Appleガイドライン準拠）
- **接続状態の可視性**: オンライン/オフライン/接続中をアイコン+テキストで明示

### 1.2 追加UI要素

MainMenuに以下を追加:

| UI要素 | 種類 | 配置 | 目的 |
|-------|------|------|------|
| **Multiplayer Category** | Label | Content Section 最上部 | カテゴリラベル |
| **Play Online Button** | Button | 2列グリッド (左側) | オンライン対戦開始 |
| **Join Room Button** | Button | 2列グリッド (右側) | ルーム参加 |
| **Connection Status Indicator** | VisualElement + Label | Header Section 右上 | Photon接続状態表示 |
| **Room Join Modal** | Modal Overlay | 画面中央 | ルーム名入力ダイアログ |

### 1.3 接続状態表示仕様

| 状態 | インジケーター色 | テキスト | 備考 |
|------|---------------|---------|------|
| **Offline** | Gray (status-offline) | "Offline" | Photon未接続 |
| **Connecting** | Yellow (status-connecting) | "Connecting..." | 接続中 (アニメーション) |
| **Online** | Green (status-online) | "Online" | Photon接続済み |
| **In Room** | Blue (status-in-room) | "In Room" | ルーム参加中 |

---

## 2. UXML構造設計

### 2.1 MainMenu.uxml 変更点

**変更内容**:
1. Header Sectionに`ConnectionStatusPanel`追加
2. Content Sectionの最上部に`Multiplayer Category`追加
3. Multiplayer ボタン2つ（Play Online + Join Room）追加
4. Room Join Modalを追加（デフォルトで非表示）

### 2.2 完全UXML設計

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
    <Style src="MainMenu.uss" />
    <Style src="Styles/PortraitMobile.uss" />

    <!-- Root Container -->
    <ui:VisualElement name="MainMenuRoot" class="main-menu-root portrait-safe-area">

        <!-- Header Section (15%) -->
        <ui:VisualElement name="HeaderSection" class="header-section">
            <ui:VisualElement class="header-content">
                <ui:Label name="GameTitle" text="ShaderOp" class="title-text" />
                <ui:Label name="Subtitle" text="Hex Board Games Collection" class="subtitle-text" />
            </ui:VisualElement>

            <!-- Connection Status Indicator (Phase 5追加) -->
            <ui:VisualElement name="ConnectionStatusPanel" class="connection-status-panel">
                <ui:VisualElement name="StatusIndicator" class="status-indicator status-offline" />
                <ui:Label name="StatusText" text="Offline" class="status-text" />
            </ui:VisualElement>
        </ui:VisualElement>

        <!-- Content Section (70%) -->
        <ui:VisualElement name="ContentSection" class="content-section">

            <!-- Multiplayer Category (Phase 5追加) -->
            <ui:Label text="Multiplayer" class="category-label" />
            <ui:VisualElement class="menu-grid">
                <ui:Button name="PlayOnlineBtn" text="Play Online" class="menu-button game-button-primary" />
                <ui:Button name="JoinRoomBtn" text="Join Room" class="menu-button game-button-secondary" />
            </ui:VisualElement>

            <!-- Minigames Category -->
            <ui:Label text="Play Minigames" class="category-label" />
            <ui:VisualElement class="menu-grid">
                <ui:Button name="PlayTicTacToeBtn" text="Tic-Tac-Toe Hex (3x3)" class="menu-button" />
                <ui:Button name="PlayHexReversiBtn" text="Hex Reversi (37 tiles)" class="menu-button" />
                <ui:Button name="PlayHexCheckersBtn" text="Hex Checkers (Coming Soon)" class="menu-button disabled" />
                <ui:Button name="PlayHexChessBtn" text="Hex Chess (Coming Soon)" class="menu-button disabled" />
            </ui:VisualElement>

            <!-- Customization Category -->
            <ui:Label text="Customize" class="category-label" />
            <ui:VisualElement class="menu-grid">
                <ui:Button name="RoomDecorationBtn" text="Room Decoration" class="menu-button" />
                <ui:Button name="CharacterCustomizationBtn" text="Character (Coming Soon)" class="menu-button disabled" />
            </ui:VisualElement>

        </ui:VisualElement>

        <!-- Footer Section (15%) -->
        <ui:VisualElement name="FooterSection" class="footer-section">
            <ui:Label name="VersionLabel" text="v0.5.0 - Phase 5 Week 1 (2%)" class="version-text" />
            <ui:VisualElement class="footer-buttons">
                <ui:Button name="SettingsBtn" text="Settings" class="footer-button" />
                <ui:Button name="QuitBtn" text="Quit" class="footer-button footer-button-danger" />
            </ui:VisualElement>
        </ui:VisualElement>

        <!-- Room Join Modal (デフォルト非表示, Phase 5追加) -->
        <ui:VisualElement name="RoomJoinModal" class="modal-overlay d-none">
            <ui:VisualElement class="modal-content room-join-dialog">
                <!-- ダイアログヘッダー -->
                <ui:VisualElement class="dialog-header">
                    <ui:Label text="Join Room" class="dialog-title" />
                    <ui:Button name="CloseModalBtn" text="✕" class="button-icon close-button" />
                </ui:VisualElement>

                <!-- ダイアログボディ -->
                <ui:VisualElement class="dialog-body">
                    <ui:Label text="Enter Room Name:" class="text-mobile-body" />
                    <ui:TextField name="RoomNameInput" class="room-name-input" />

                    <ui:Label name="RoomJoinErrorText" text="" class="error-text d-none" />
                </ui:VisualElement>

                <!-- ダイアログフッター -->
                <ui:VisualElement class="dialog-footer">
                    <ui:Button name="CancelJoinBtn" text="Cancel" class="game-button-secondary button-mobile" />
                    <ui:Button name="ConfirmJoinBtn" text="Join" class="game-button-primary button-mobile" />
                </ui:VisualElement>
            </ui:VisualElement>
        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

### 2.3 主要変更箇所説明

#### (1) Header Section - Connection Status Panel

```xml
<ui:VisualElement name="ConnectionStatusPanel" class="connection-status-panel">
    <ui:VisualElement name="StatusIndicator" class="status-indicator status-offline" />
    <ui:Label name="StatusText" text="Offline" class="status-text" />
</ui:VisualElement>
```

**目的**: Photon接続状態をリアルタイム表示
**配置**: Header Section右上（Titleの反対側）
**状態遷移**: C#でclass切り替え（`status-offline` → `status-connecting` → `status-online`）

#### (2) Multiplayer Category

```xml
<ui:Label text="Multiplayer" class="category-label" />
<ui:VisualElement class="menu-grid">
    <ui:Button name="PlayOnlineBtn" text="Play Online" class="menu-button game-button-primary" />
    <ui:Button name="JoinRoomBtn" text="Join Room" class="menu-button game-button-secondary" />
</ui:VisualElement>
```

**目的**: オンライン対戦の入り口
**配置**: Content Sectionの最上部（Minigamesより上）
**レイアウト**: 2列グリッド（既存`.menu-grid`スタイル使用）

#### (3) Room Join Modal

```xml
<ui:VisualElement name="RoomJoinModal" class="modal-overlay d-none">
    <ui:VisualElement class="modal-content room-join-dialog">
        <!-- Header: タイトル + 閉じるボタン -->
        <!-- Body: TextField + エラーメッセージ -->
        <!-- Footer: Cancel + Join ボタン -->
    </ui:VisualElement>
</ui:VisualElement>
```

**目的**: ルーム名入力ダイアログ
**配置**: 画面中央（`modal-overlay`で全画面オーバーレイ）
**表示制御**: C#で`d-none`クラス追加/削除

---

## 3. USS スタイル設計

### 3.1 PortraitMobile.uss 追加スタイル

以下をPortraitMobile.ussに追加:

```css
/* ============================================
   PHASE 5: MULTIPLAYER UI STYLES
   ============================================ */

/* Connection Status Panel - Header右上配置 */
.connection-status-panel {
    position: absolute;
    top: var(--space-md);
    right: var(--space-md);
    flex-direction: row;
    align-items: center;
    background-color: rgba(40, 40, 60, 0.8);
    padding: var(--space-xs) var(--space-sm);
    border-radius: var(--radius-md);
    border-width: 1px;
    border-color: rgba(100, 120, 180, 0.4);
}

/* Status Indicator - Dot */
.status-indicator {
    width: 12px;
    height: 12px;
    border-radius: var(--radius-full);
    margin-right: var(--space-xs);
    transition-property: background-color;
    transition-duration: var(--transition-fast);
}

/* Status States */
.status-offline {
    background-color: rgb(120, 120, 130); /* Gray */
}

.status-connecting {
    background-color: rgb(255, 200, 60); /* Yellow */
    /* アニメーション: C#でpulse効果実装推奨 */
}

.status-online {
    background-color: rgb(60, 200, 100); /* Green */
}

.status-in-room {
    background-color: rgb(60, 150, 220); /* Blue */
}

/* Status Text */
.status-text {
    font-size: 12px;
    color: rgb(220, 220, 240);
    -unity-font-style: bold;
}

/* Header Content - Titleとの併用のため調整 */
.header-content {
    flex-grow: 1;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}

/* ============================================
   ROOM JOIN MODAL
   ============================================ */

/* Modal Overlay - 全画面オーバーレイ */
.modal-overlay {
    position: absolute;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.6); /* 60% opacity */
    justify-content: center;
    align-items: center;
    z-index: 1050;
}

/* Modal Content - ダイアログボックス */
.modal-content {
    background-color: rgb(35, 35, 50);
    border-radius: var(--radius-lg);
    border-width: 2px;
    border-color: rgba(100, 120, 200, 0.6);
    padding: var(--space-lg);
    min-width: 320px;
    max-width: 90%; /* モバイル対応 */
}

.room-join-dialog {
    flex-direction: column;
}

/* Dialog Header */
.dialog-header {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-md);
    border-bottom-width: 1px;
    border-bottom-color: rgba(100, 120, 180, 0.3);
    padding-bottom: var(--space-sm);
}

.dialog-title {
    font-size: 20px;
    color: rgb(220, 220, 240);
    -unity-font-style: bold;
}

.close-button {
    width: 36px;
    height: 36px;
    min-width: 36px;
    min-height: 36px;
    font-size: 20px;
    background-color: rgba(200, 60, 60, 0.6);
    border-radius: var(--radius-full);
}

.close-button:hover {
    background-color: rgba(220, 80, 80, 0.8);
    scale: 1.1;
}

/* Dialog Body */
.dialog-body {
    flex-direction: column;
    margin-bottom: var(--space-md);
}

.dialog-body > Label {
    margin-bottom: var(--space-xs);
    color: rgb(200, 200, 220);
    font-size: 14px;
}

/* Room Name Input */
.room-name-input {
    background-color: rgba(50, 50, 70, 0.8);
    border-radius: var(--radius-md);
    border-width: 2px;
    border-color: rgba(100, 120, 180, 0.4);
    padding: var(--space-sm);
    margin-bottom: var(--space-sm);
    min-height: 44px;
    font-size: 16px;
    color: rgb(255, 255, 255);
}

.room-name-input:focus {
    border-color: rgba(100, 150, 220, 1);
    border-width: 3px;
}

/* Error Text */
.error-text {
    color: rgb(255, 100, 100);
    font-size: 12px;
    -unity-font-style: italic;
    margin-top: var(--space-xs);
}

/* Dialog Footer */
.dialog-footer {
    flex-direction: row;
    justify-content: space-between;
}

.dialog-footer > Button {
    width: 48%;
}

/* ============================================
   MULTIPLAYER BUTTON VARIANTS
   ============================================ */

/* Play Onlineボタン - Primary強調 */
.game-button-primary {
    background-color: rgba(60, 120, 220, 0.9);
    border-color: rgba(80, 140, 240, 0.8);
    border-width: 2px;
}

.game-button-primary:hover {
    background-color: rgba(80, 140, 240, 1);
    border-color: rgba(100, 160, 255, 1);
    scale: 1.05;
}

/* Join Roomボタン - Secondary */
.game-button-secondary {
    background-color: rgba(100, 100, 140, 0.7);
    border-color: rgba(120, 120, 160, 0.6);
    border-width: 2px;
}

.game-button-secondary:hover {
    background-color: rgba(120, 120, 160, 0.9);
    border-color: rgba(140, 140, 180, 1);
    scale: 1.05;
}
```

### 3.2 スタイル適用ガイド

| クラス名 | 適用対象 | 目的 |
|---------|---------|------|
| `.connection-status-panel` | ConnectionStatusPanel | Header右上配置 |
| `.status-indicator` | StatusIndicator | 12px丸ドット |
| `.status-offline/.status-online/.status-connecting/.status-in-room` | StatusIndicator | 状態別色 |
| `.modal-overlay` | RoomJoinModal | 全画面オーバーレイ |
| `.room-join-dialog` | Modal内部 | ダイアログボックス |
| `.game-button-primary` | PlayOnlineBtn | 青色強調 |
| `.game-button-secondary` | JoinRoomBtn | グレー控えめ |

---

## 4. MainMenuController 拡張設計

### 4.1 追加フィールド

```csharp
// Phase 5追加フィールド
[Header("Multiplayer Services (Phase 5)")]
private INetworkService? _networkService;
private IGameSyncService? _gameSyncService;

// Multiplayer UI要素
private Button? _playOnlineBtn;
private Button? _joinRoomBtn;
private VisualElement? _connectionStatusPanel;
private VisualElement? _statusIndicator;
private Label? _statusText;

// Room Join Modal
private VisualElement? _roomJoinModal;
private TextField? _roomNameInput;
private Button? _confirmJoinBtn;
private Button? _cancelJoinBtn;
private Button? _closeModalBtn;
private Label? _roomJoinErrorText;

// 接続状態管理
private bool _isConnectingToPhoton = false;
```

### 4.2 Start() 変更

```csharp
private void Start()
{
    // 既存のServiceLocator取得
    _sceneLoader = ServiceLocator.Instance.Get<ISceneLoaderService>();

    // Phase 5追加: ネットワークサービス取得
    _networkService = ServiceLocator.Instance.Get<INetworkService>();
    _gameSyncService = ServiceLocator.Instance.Get<IGameSyncService>();

    if (_networkService == null)
    {
        Debug.LogWarning("[MainMenuController] INetworkService not found. Multiplayer features disabled.");
    }

    if (_gameSyncService == null)
    {
        Debug.LogWarning("[MainMenuController] IGameSyncService not found. Multiplayer features disabled.");
    }

    // UI要素をセットアップ
    SetupUI();

    // Phase 5追加: ネットワークイベント登録
    RegisterNetworkEvents();
}
```

### 4.3 GetUIElements() 拡張

```csharp
private void GetUIElements()
{
    if (_root == null) return;

    // 既存のUI要素取得
    _playTicTacToeBtn = _root.Q<Button>("PlayTicTacToeBtn");
    _playHexReversiBtn = _root.Q<Button>("PlayHexReversiBtn");
    _playHexCheckersBtn = _root.Q<Button>("PlayHexCheckersBtn");
    _playHexChessBtn = _root.Q<Button>("PlayHexChessBtn");
    _roomDecorationBtn = _root.Q<Button>("RoomDecorationBtn");
    _characterCustomizationBtn = _root.Q<Button>("CharacterCustomizationBtn");
    _settingsBtn = _root.Q<Button>("SettingsBtn");
    _quitBtn = _root.Q<Button>("QuitBtn");
    _versionLabel = _root.Q<Label>("VersionLabel");

    // Phase 5追加: Multiplayer UI要素
    _playOnlineBtn = _root.Q<Button>("PlayOnlineBtn");
    _joinRoomBtn = _root.Q<Button>("JoinRoomBtn");
    _connectionStatusPanel = _root.Q<VisualElement>("ConnectionStatusPanel");
    _statusIndicator = _root.Q<VisualElement>("StatusIndicator");
    _statusText = _root.Q<Label>("StatusText");

    // Room Join Modal
    _roomJoinModal = _root.Q<VisualElement>("RoomJoinModal");
    _roomNameInput = _root.Q<TextField>("RoomNameInput");
    _confirmJoinBtn = _root.Q<Button>("ConfirmJoinBtn");
    _cancelJoinBtn = _root.Q<Button>("CancelJoinBtn");
    _closeModalBtn = _root.Q<Button>("CloseModalBtn");
    _roomJoinErrorText = _root.Q<Label>("RoomJoinErrorText");
}
```

### 4.4 RegisterEventHandlers() 拡張

```csharp
private void RegisterEventHandlers()
{
    // 既存のイベントハンドラ登録（省略）

    // Phase 5追加: Multiplayer ボタン
    if (_playOnlineBtn != null)
    {
        _playOnlineBtn.clicked += OnPlayOnlineClicked;
    }

    if (_joinRoomBtn != null)
    {
        _joinRoomBtn.clicked += OnJoinRoomClicked;
    }

    // Room Join Modal
    if (_confirmJoinBtn != null)
    {
        _confirmJoinBtn.clicked += OnConfirmJoinClicked;
    }

    if (_cancelJoinBtn != null)
    {
        _cancelJoinBtn.clicked += OnCancelJoinClicked;
    }

    if (_closeModalBtn != null)
    {
        _closeModalBtn.clicked += OnCloseModalClicked;
    }
}
```

### 4.5 新規メソッド: ネットワークイベント登録

```csharp
/// <summary>
/// ネットワークサービスのイベントを登録
/// </summary>
private void RegisterNetworkEvents()
{
    if (_networkService == null) return;

    _networkService.OnConnectedChanged += OnPhotonConnectedChanged;
    _networkService.OnRoomJoined += OnPhotonRoomJoined;
    _networkService.OnRoomLeft += OnPhotonRoomLeft;

    Debug.Log("[MainMenuController] Network events registered.");

    // 初期接続状態を反映
    UpdateConnectionStatus();
}

/// <summary>
/// ネットワークイベントを解除
/// </summary>
private void UnregisterNetworkEvents()
{
    if (_networkService == null) return;

    _networkService.OnConnectedChanged -= OnPhotonConnectedChanged;
    _networkService.OnRoomJoined -= OnPhotonRoomJoined;
    _networkService.OnRoomLeft -= OnPhotonRoomLeft;
}
```

### 4.6 新規メソッド: 接続状態更新

```csharp
/// <summary>
/// Photon接続状態が変化したときの処理
/// </summary>
private void OnPhotonConnectedChanged(bool isConnected)
{
    Debug.Log($"[MainMenuController] Photon connection changed: {isConnected}");
    UpdateConnectionStatus();
}

/// <summary>
/// Photonルーム参加時の処理
/// </summary>
private void OnPhotonRoomJoined(string roomName)
{
    Debug.Log($"[MainMenuController] Joined room: {roomName}");
    UpdateConnectionStatus();

    // ルーム参加成功後、ゲーム選択画面に遷移（オプション）
    // TODO: ルーム内ロビーシーンを作成後、ここで遷移
}

/// <summary>
/// Photonルーム退出時の処理
/// </summary>
private void OnPhotonRoomLeft()
{
    Debug.Log("[MainMenuController] Left room.");
    UpdateConnectionStatus();
}

/// <summary>
/// 接続状態UIを更新
/// </summary>
private void UpdateConnectionStatus()
{
    if (_networkService == null || _statusIndicator == null || _statusText == null)
    {
        return;
    }

    // 既存のステータスクラスをすべて削除
    _statusIndicator.RemoveFromClassList("status-offline");
    _statusIndicator.RemoveFromClassList("status-connecting");
    _statusIndicator.RemoveFromClassList("status-online");
    _statusIndicator.RemoveFromClassList("status-in-room");

    // 接続状態に応じてクラスとテキストを設定
    if (_isConnectingToPhoton)
    {
        _statusIndicator.AddToClassList("status-connecting");
        _statusText.text = "Connecting...";
    }
    else if (_networkService.IsInRoom)
    {
        _statusIndicator.AddToClassList("status-in-room");
        _statusText.text = "In Room";
    }
    else if (_networkService.IsConnected)
    {
        _statusIndicator.AddToClassList("status-online");
        _statusText.text = "Online";
    }
    else
    {
        _statusIndicator.AddToClassList("status-offline");
        _statusText.text = "Offline";
    }
}
```

### 4.7 新規メソッド: Play Onlineボタンクリック

```csharp
/// <summary>
/// Play Onlineボタンクリック時の処理
/// </summary>
private void OnPlayOnlineClicked()
{
    Debug.Log("[MainMenuController] Play Online clicked.");

    if (_networkService == null)
    {
        Debug.LogError("[MainMenuController] NetworkService not available!");
        return;
    }

    // Photon接続 → ランダムルーム参加 → ゲーム選択画面
    ConnectAndJoinRandomRoomAsync().Forget();
}

/// <summary>
/// Photon接続してランダムルームに参加
/// </summary>
private async UniTaskVoid ConnectAndJoinRandomRoomAsync()
{
    if (_networkService == null) return;

    try
    {
        // 接続状態を "Connecting..." に設定
        _isConnectingToPhoton = true;
        UpdateConnectionStatus();

        // Photonサーバーに接続
        bool connected = await _networkService.ConnectToServerAsync();
        _isConnectingToPhoton = false;

        if (!connected)
        {
            Debug.LogError("[MainMenuController] Failed to connect to Photon server.");
            UpdateConnectionStatus();
            ShowErrorMessage("Failed to connect to server. Please check your internet connection.");
            return;
        }

        Debug.Log("[MainMenuController] Connected to Photon server.");
        UpdateConnectionStatus();

        // ランダムルームに参加
        bool joined = await _networkService.JoinRandomRoomAsync();

        if (!joined)
        {
            // ルームが見つからない場合、新規作成
            Debug.Log("[MainMenuController] No available rooms. Creating new room...");
            string roomName = "Room_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            bool created = await _networkService.CreateRoomAsync(roomName, maxPlayers: 2);

            if (!created)
            {
                Debug.LogError("[MainMenuController] Failed to create room.");
                ShowErrorMessage("Failed to create room. Please try again.");
                return;
            }

            Debug.Log($"[MainMenuController] Room created: {roomName}");
        }
        else
        {
            Debug.Log($"[MainMenuController] Joined random room: {_networkService.CurrentRoomName}");
        }

        // ルーム参加成功 → ゲーム選択画面に遷移（Phase 5 Week 2で実装予定）
        // TODO: LoadGameSelectionLobbyAsync();
    }
    catch (Exception e)
    {
        _isConnectingToPhoton = false;
        UpdateConnectionStatus();
        Debug.LogError($"[MainMenuController] Error during online matchmaking: {e.Message}");
        ShowErrorMessage("An error occurred. Please try again.");
    }
}
```

### 4.8 新規メソッド: Join Roomボタンクリック

```csharp
/// <summary>
/// Join Roomボタンクリック時の処理
/// </summary>
private void OnJoinRoomClicked()
{
    Debug.Log("[MainMenuController] Join Room clicked.");

    // Room Join Modalを表示
    ShowRoomJoinModal();
}

/// <summary>
/// Room Join Modalを表示
/// </summary>
private void ShowRoomJoinModal()
{
    if (_roomJoinModal == null) return;

    _roomJoinModal.RemoveFromClassList("d-none");

    // TextField をクリア
    if (_roomNameInput != null)
    {
        _roomNameInput.value = "";
    }

    // エラーテキストを非表示
    if (_roomJoinErrorText != null)
    {
        _roomJoinErrorText.AddToClassList("d-none");
        _roomJoinErrorText.text = "";
    }

    Debug.Log("[MainMenuController] Room Join Modal shown.");
}

/// <summary>
/// Room Join Modalを非表示
/// </summary>
private void HideRoomJoinModal()
{
    if (_roomJoinModal == null) return;

    _roomJoinModal.AddToClassList("d-none");
    Debug.Log("[MainMenuController] Room Join Modal hidden.");
}

/// <summary>
/// Join確認ボタンクリック時の処理
/// </summary>
private void OnConfirmJoinClicked()
{
    if (_roomNameInput == null || _networkService == null) return;

    string roomName = _roomNameInput.value.Trim();

    if (string.IsNullOrEmpty(roomName))
    {
        ShowRoomJoinError("Room name cannot be empty.");
        return;
    }

    Debug.Log($"[MainMenuController] Attempting to join room: {roomName}");

    // Photon接続 → ルーム参加
    ConnectAndJoinRoomAsync(roomName).Forget();
}

/// <summary>
/// Photon接続して指定ルームに参加
/// </summary>
private async UniTaskVoid ConnectAndJoinRoomAsync(string roomName)
{
    if (_networkService == null) return;

    try
    {
        // 接続状態を "Connecting..." に設定
        _isConnectingToPhoton = true;
        UpdateConnectionStatus();

        // Photonサーバーに接続
        bool connected = await _networkService.ConnectToServerAsync();
        _isConnectingToPhoton = false;

        if (!connected)
        {
            Debug.LogError("[MainMenuController] Failed to connect to Photon server.");
            UpdateConnectionStatus();
            ShowRoomJoinError("Failed to connect to server. Check your connection.");
            return;
        }

        Debug.Log("[MainMenuController] Connected to Photon server.");
        UpdateConnectionStatus();

        // ルーム参加
        bool joined = await _networkService.JoinRoomAsync(roomName);

        if (!joined)
        {
            Debug.LogError($"[MainMenuController] Failed to join room: {roomName}");
            ShowRoomJoinError($"Room '{roomName}' not found or is full.");
            return;
        }

        Debug.Log($"[MainMenuController] Successfully joined room: {roomName}");
        HideRoomJoinModal();

        // ルーム参加成功 → ゲーム選択画面に遷移（Phase 5 Week 2で実装予定）
        // TODO: LoadGameSelectionLobbyAsync();
    }
    catch (Exception e)
    {
        _isConnectingToPhoton = false;
        UpdateConnectionStatus();
        Debug.LogError($"[MainMenuController] Error joining room: {e.Message}");
        ShowRoomJoinError("An error occurred. Please try again.");
    }
}

/// <summary>
/// Cancelボタンクリック時の処理
/// </summary>
private void OnCancelJoinClicked()
{
    Debug.Log("[MainMenuController] Cancel Join clicked.");
    HideRoomJoinModal();
}

/// <summary>
/// 閉じるボタンクリック時の処理
/// </summary>
private void OnCloseModalClicked()
{
    Debug.Log("[MainMenuController] Close Modal clicked.");
    HideRoomJoinModal();
}

/// <summary>
/// Room Join Modalにエラーメッセージを表示
/// </summary>
private void ShowRoomJoinError(string errorMessage)
{
    if (_roomJoinErrorText == null) return;

    _roomJoinErrorText.text = errorMessage;
    _roomJoinErrorText.RemoveFromClassList("d-none");
}

/// <summary>
/// 汎用エラーメッセージ表示（TODO: モーダルダイアログで実装）
/// </summary>
private void ShowErrorMessage(string message)
{
    Debug.LogWarning($"[MainMenuController] Error: {message}");
    // TODO: Phase 5 Week 2でエラーモーダル実装
}
```

### 4.9 OnDestroy() 拡張

```csharp
private void OnDestroy()
{
    UnregisterEventHandlers();
    UnregisterNetworkEvents(); // Phase 5追加
}
```

---

## 5. インタラクションフロー

### 5.1 Play Onlineフロー

```
┌──────────────────────────────────────────────────────────────────┐
│ User: Play Onlineボタンをタップ                                    │
└────────────────────┬────────────────────────────────────────────┘
                     ▼
┌──────────────────────────────────────────────────────────────────┐
│ MainMenuController: OnPlayOnlineClicked()                         │
│ - Connection Status: "Connecting..." (Yellow)                     │
│ - await _networkService.ConnectToServerAsync()                    │
└────────────────────┬────────────────────────────────────────────┘
                     ▼
           ┌─────────┴─────────┐
           │ Success?          │
           └──┬───────────┬────┘
              │ Yes       │ No
              ▼           ▼
  ┌─────────────────┐   ┌────────────────────┐
  │ Status: "Online"│   │ Show Error Dialog  │
  │ (Green)         │   │ Status: "Offline"  │
  └────────┬────────┘   └────────────────────┘
           ▼
  ┌─────────────────────────────────────────────────────────────┐
  │ await _networkService.JoinRandomRoomAsync()                  │
  └────────────────────┬────────────────────────────────────────┘
                       ▼
             ┌─────────┴─────────┐
             │ Found Room?       │
             └──┬───────────┬────┘
                │ Yes       │ No
                ▼           ▼
    ┌─────────────────┐   ┌────────────────────────────────┐
    │ Join Room       │   │ Create New Room                 │
    │ Status: "In Room│   │ roomName: "Room_GUID"          │
    │ (Blue)          │   │ maxPlayers: 2                  │
    └────────┬────────┘   └────────┬───────────────────────┘
             └──────────┬──────────┘
                        ▼
         ┌──────────────────────────────────────────────────┐
         │ TODO: LoadGameSelectionLobbyAsync() (Week 2)    │
         └──────────────────────────────────────────────────┘
```

### 5.2 Join Roomフロー

```
┌──────────────────────────────────────────────────────────────────┐
│ User: Join Roomボタンをタップ                                      │
└────────────────────┬────────────────────────────────────────────┘
                     ▼
┌──────────────────────────────────────────────────────────────────┐
│ MainMenuController: ShowRoomJoinModal()                           │
│ - Room Join Modal表示（オーバーレイ）                              │
│ - TextField クリア                                                │
└────────────────────┬────────────────────────────────────────────┘
                     ▼
┌──────────────────────────────────────────────────────────────────┐
│ User: ルーム名入力 "MyRoom123"                                     │
│ User: Joinボタンをタップ                                           │
└────────────────────┬────────────────────────────────────────────┘
                     ▼
           ┌─────────┴─────────┐
           │ Empty Check?      │
           └──┬───────────┬────┘
              │ Not Empty │ Empty
              ▼           ▼
  ┌─────────────────┐   ┌────────────────────────────┐
  │ Proceed         │   │ Show Error:                │
  │                 │   │ "Room name cannot be empty"│
  └────────┬────────┘   └────────────────────────────┘
           ▼
  ┌─────────────────────────────────────────────────────────────┐
  │ await _networkService.ConnectToServerAsync()                 │
  └────────────────────┬────────────────────────────────────────┘
                       ▼
             ┌─────────┴─────────┐
             │ Success?          │
             └──┬───────────┬────┘
                │ Yes       │ No
                ▼           ▼
    ┌─────────────────┐   ┌─────────────────────────────┐
    │ Status: "Online"│   │ Show Error:                 │
    │ (Green)         │   │ "Failed to connect"         │
    └────────┬────────┘   └─────────────────────────────┘
             ▼
  ┌─────────────────────────────────────────────────────────────┐
  │ await _networkService.JoinRoomAsync("MyRoom123")             │
  └────────────────────┬────────────────────────────────────────┘
                       ▼
             ┌─────────┴─────────┐
             │ Success?          │
             └──┬───────────┬────┘
                │ Yes       │ No
                ▼           ▼
    ┌─────────────────┐   ┌─────────────────────────────────┐
    │ Join Success    │   │ Show Error:                     │
    │ Status: "In Room│   │ "Room not found or full"        │
    │ (Blue)          │   └─────────────────────────────────┘
    │ Hide Modal      │
    └────────┬────────┘
             ▼
  ┌──────────────────────────────────────────────────────────────┐
  │ TODO: LoadGameSelectionLobbyAsync() (Week 2)                 │
  └──────────────────────────────────────────────────────────────┘
```

### 5.3 接続状態遷移図

```
┌────────────┐
│  Offline   │ (Gray) - Photon未接続
│ (初期状態)  │
└──────┬─────┘
       │ ConnectToServerAsync() 呼び出し
       ▼
┌────────────┐
│ Connecting │ (Yellow) - 接続中（パルスアニメーション）
└──────┬─────┘
       │ OnConnectedToMaster イベント
       ▼
┌────────────┐
│   Online   │ (Green) - Photon接続済み
└──────┬─────┘
       │ JoinRoomAsync() / CreateRoomAsync() 成功
       ▼
┌────────────┐
│  In Room   │ (Blue) - ルーム参加中
└──────┬─────┘
       │ LeaveRoomAsync() 呼び出し
       ▼
┌────────────┐
│   Online   │ (Green) - ルーム退出後、再接続状態
└────────────┘
```

---

## 6. 実装チェックリスト

### 6.1 UXML修正

- [ ] MainMenu.uxmlを開く
- [ ] Header Sectionに`ConnectionStatusPanel`を追加
- [ ] Content Sectionの最上部に`Multiplayer Category`を追加
- [ ] `PlayOnlineBtn`と`JoinRoomBtn`を追加
- [ ] `RoomJoinModal`を追加（デフォルトで`d-none`クラス）
- [ ] 保存（Ctrl + S）

### 6.2 USS修正

- [ ] PortraitMobile.ussを開く
- [ ] `.connection-status-panel`スタイルを追加
- [ ] `.status-indicator`スタイルを追加
- [ ] `.status-offline/.status-connecting/.status-online/.status-in-room`を追加
- [ ] `.modal-overlay`スタイルを追加
- [ ] `.room-join-dialog`スタイルを追加
- [ ] `.game-button-primary`と`.game-button-secondary`を追加
- [ ] 保存（Ctrl + S）

### 6.3 MainMenuController.cs修正

- [ ] フィールド追加（`_networkService`, `_gameSyncService`, UI要素）
- [ ] `Start()`でサービス取得追加
- [ ] `GetUIElements()`でMultiplayer UI要素取得追加
- [ ] `RegisterEventHandlers()`でイベント登録追加
- [ ] `RegisterNetworkEvents()`メソッド追加
- [ ] `UpdateConnectionStatus()`メソッド追加
- [ ] `OnPlayOnlineClicked()`メソッド追加
- [ ] `ConnectAndJoinRandomRoomAsync()`メソッド追加
- [ ] `OnJoinRoomClicked()`メソッド追加
- [ ] `ShowRoomJoinModal()`メソッド追加
- [ ] `HideRoomJoinModal()`メソッド追加
- [ ] `OnConfirmJoinClicked()`メソッド追加
- [ ] `ConnectAndJoinRoomAsync(string roomName)`メソッド追加
- [ ] `OnCancelJoinClicked()`メソッド追加
- [ ] `OnCloseModalClicked()`メソッド追加
- [ ] `ShowRoomJoinError(string)`メソッド追加
- [ ] `UnregisterNetworkEvents()`メソッド追加
- [ ] `OnDestroy()`で`UnregisterNetworkEvents()`呼び出し追加
- [ ] 保存（Ctrl + S）

### 6.4 Unity Editor検証

- [ ] Startup.unityを開く
- [ ] GameBootstrapのNetwork Service Prefab設定確認
- [ ] Play Mode実行
- [ ] MainMenuシーンでConnection Status表示確認
- [ ] 初期状態: "Offline" (Gray)確認
- [ ] Play Onlineボタンクリック → "Connecting..." → "Online"確認
- [ ] Join Roomボタンクリック → Modalが表示される確認
- [ ] ルーム名入力 → Join → 接続成功/失敗確認
- [ ] Cancel/閉じるボタンでModal非表示確認

### 6.5 エラーハンドリングテスト

- [ ] Photon未接続時にPlay Onlineクリック → エラーメッセージ表示確認
- [ ] 存在しないルーム名で Join → エラーメッセージ表示確認
- [ ] 空のルーム名で Join → "Room name cannot be empty"表示確認
- [ ] ネットワーク切断時の動作確認

---

## 📊 統計情報

### コード量見積もり

| ファイル | 追加行数 | 変更行数 | 合計 |
|---------|---------|---------|------|
| MainMenu.uxml | +60 | 5 | 65 |
| PortraitMobile.uss | +150 | 0 | 150 |
| MainMenuController.cs | +300 | 20 | 320 |
| **合計** | **510** | **25** | **535** |

### UI要素数

- 新規UI要素: 11個
  - Buttons: 4個（PlayOnline, JoinRoom, ConfirmJoin, CancelJoin, CloseModal）
  - Labels: 3個（StatusText, DialogTitle, RoomJoinErrorText）
  - VisualElements: 3個（ConnectionStatusPanel, StatusIndicator, RoomJoinModal）
  - TextField: 1個（RoomNameInput）

### イベントハンドラ数

- ボタンクリック: 5個
- ネットワークイベント: 3個（OnConnectedChanged, OnRoomJoined, OnRoomLeft）

### 非同期メソッド

- UniTaskVoid: 2個（`ConnectAndJoinRandomRoomAsync`, `ConnectAndJoinRoomAsync`）
- UniTask: 0個（すべてVoid）

---

## 🎯 Week 1 Day 4-5 実装スケジュール

### Day 4 (2026-03-17)

**午前** (4時間):
1. MainMenu.uxml修正（60行追加）
2. PortraitMobile.uss修正（150行追加）
3. Unity Editor検証（UXML/USS表示確認）

**午後** (4時間):
4. MainMenuController.cs フィールド追加（20行）
5. GetUIElements() + RegisterEventHandlers() 拡張（40行）
6. UpdateConnectionStatus() 実装（60行）

### Day 5 (2026-03-18)

**午前** (4時間):
7. OnPlayOnlineClicked() + ConnectAndJoinRandomRoomAsync() 実装（100行）
8. OnJoinRoomClicked() + Room Join Modal実装（120行）
9. ネットワークイベント登録実装（40行）

**午後** (4時間):
10. Unity Play Modeテスト（全機能）
11. エラーハンドリングテスト
12. バグ修正
13. Day 4-5進捗サマリー作成

**合計見積もり**: 16時間（2日間）

---

## 📝 次のステップ (Week 2)

### Week 2 Day 1-2: ゲーム選択ロビーシーン作成

1. `GameLobbyScene.unity`作成
2. ゲーム選択UI（4ゲーム表示）
3. プレイヤー情報表示（Player 1 vs Player 2）
4. Start Gameボタン（Master Clientのみ有効）
5. Leave Roomボタン

### Week 2 Day 3-4: TicTacToeHexOnlineシーンへの遷移

1. `LoadGameSelectionLobbyAsync()`実装
2. `LoadTicTacToeHexOnlineAsync()`実装
3. シーン遷移時のPhoton状態保持
4. TicTacToeHexOnlineControllerとの連携テスト

### Week 2 Day 5-7: 残り3ゲームのオンライン対応

1. HexReversiOnlineController
2. HexCheckersOnlineController
3. HexChessOnlineController
4. Week 2完了サマリー作成

---

## 🔗 関連ドキュメント

- [PHASE5_WEEK1_IMPLEMENTATION_GUIDE.md](./PHASE5_WEEK1_IMPLEMENTATION_GUIDE.md) - Week 1全体実装ガイド
- [PHOTON_PUN_SETUP_GUIDE.md](./PHOTON_PUN_SETUP_GUIDE.md) - Photon PUN 2セットアップ
- [PHASE5_WEEK1_PREFAB_CONFIGURATION_GUIDE.md](./PHASE5_WEEK1_PREFAB_CONFIGURATION_GUIDE.md) - Prefab設定ガイド
- [PHASE5_WEEK1_DAY1_SUMMARY.md](./PHASE5_WEEK1_DAY1_SUMMARY.md) - Day 1完了サマリー

---

**ドキュメント作成日**: 2026-03-16
**作成者**: Claude (unity-developer agent)
**バージョン**: 1.0
**ステータス**: ✅ Complete - Ready for Implementation
