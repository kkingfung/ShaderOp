# Lobby UI Element Documentation

**For unity-developer**: C# Binding Reference

**Project**: ShaderOp - Phase 5 Week 2 Day 4-5
**Target**: Portrait Mobile (1080x1920)
**UXML**: `Assets/UI/LobbyView.uxml`
**USS**: `Assets/UI/LobbyView.uss`

---

## Element Naming Convention

All interactive elements use clear, descriptive names for easy C# binding.

---

## Interactive Elements (C# Binding Required)

### Header Section

| Element Name | Type | Purpose | C# Binding |
|--------------|------|---------|------------|
| `LeaveButton` | Button | 退出ロビー | `clicked += OnLeaveButtonClicked` |
| `LobbyTitle` | Label | ロビータイトル表示 | Read-only (optional update) |

### Join Code Section

| Element Name | Type | Purpose | C# Binding |
|--------------|------|---------|------------|
| `JoinCodeLabel` | Label | 6桁の参加コード表示 | `text = roomCode` |
| `CopyCodeButton` | Button | コードをクリップボードにコピー | `clicked += OnCopyCodeButtonClicked` |

### Player List Section

| Element Name | Type | Purpose | C# Binding |
|--------------|------|---------|------------|
| `PlayerCountLabel` | Label | プレイヤー数表示 (例: "Players (2/4)") | `text = $"Players ({current}/{max})"` |
| `PlayerListScrollView` | ScrollView | プレイヤーリストのスクロール領域 | Container only |
| `PlayerListContainer` | VisualElement | プレイヤーアイテムの親コンテナ | `Add(playerItem)` で動的追加 |

### Status Section

| Element Name | Type | Purpose | C# Binding |
|--------------|------|---------|------------|
| `StatusLabel` | Label | 接続・待機状態の表示 | `text = statusMessage` |

### Footer Section (Action Buttons)

| Element Name | Type | Purpose | C# Binding |
|--------------|------|---------|------------|
| `ReadyButton` | Button | Ready/Not Readyトグル | `clicked += OnReadyButtonClicked` |
| `StartGameButton` | Button | ゲーム開始 (Host専用) | `clicked += OnStartGameButtonClicked` |

---

## Dynamic Player Item Structure

プレイヤーリストのアイテムは **C#で動的生成** してください。

### Player Item Template (C# で生成)

```csharp
var playerItem = new VisualElement();
playerItem.name = $"PlayerItem_{playerId}";
playerItem.AddToClassList("player-item");

// ローカルプレイヤーの場合
if (isLocalPlayer)
{
    playerItem.AddToClassList("local-player");
}

// Avatar
var avatar = new VisualElement();
avatar.name = "PlayerAvatar";
avatar.AddToClassList("player-avatar");
var avatarEmoji = new Label("🎮");
avatarEmoji.AddToClassList("avatar-emoji");
avatar.Add(avatarEmoji);

// Player Info Container
var info = new VisualElement();
info.name = "PlayerInfo";
info.AddToClassList("player-info");

var nameLabel = new Label(playerName);
nameLabel.name = "PlayerName";
nameLabel.AddToClassList("player-name");

var statusLabel = new Label(isReady ? "Ready ✓" : "Not Ready ✗");
statusLabel.name = "PlayerStatus";
statusLabel.AddToClassList("player-status");
statusLabel.AddToClassList(isReady ? "ready" : "not-ready");

info.Add(nameLabel);
info.Add(statusLabel);

// Host Icon (Hostの場合のみ表示)
Label? hostIcon = null;
if (isHost)
{
    hostIcon = new Label("⭐");
    hostIcon.name = "HostIcon";
    hostIcon.AddToClassList("host-icon");
}

// Assemble
playerItem.Add(avatar);
playerItem.Add(info);
if (hostIcon != null)
{
    playerItem.Add(hostIcon);
}

// Add to container
PlayerListContainer.Add(playerItem);
```

---

## USS Class Reference (Dynamic State Control)

### Ready Button States

```csharp
// Not Ready → Ready
readyButton.AddToClassList("ready");

// Ready → Not Ready
readyButton.RemoveFromClassList("ready");
```

### Player Status States

```csharp
// Ready
statusLabel.AddToClassList("ready");
statusLabel.RemoveFromClassList("not-ready");

// Not Ready
statusLabel.AddToClassList("not-ready");
statusLabel.RemoveFromClassList("ready");
```

### Start Button Visibility

```csharp
// Host: Enable
startButton.SetEnabled(true);

// Non-Host: Disable/Hide
startButton.SetEnabled(false);
// Or hide completely:
startButton.style.display = DisplayStyle.None;
```

---

## Recommended C# Workflow

### 1. Get Elements (OnEnable or Awake)

```csharp
private Button? _leaveButton;
private Button? _copyCodeButton;
private Label? _joinCodeLabel;
private Label? _playerCountLabel;
private VisualElement? _playerListContainer;
private Label? _statusLabel;
private Button? _readyButton;
private Button? _startGameButton;

private void GetUIElements()
{
    _leaveButton = root.Q<Button>("LeaveButton");
    _copyCodeButton = root.Q<Button>("CopyCodeButton");
    _joinCodeLabel = root.Q<Label>("JoinCodeLabel");
    _playerCountLabel = root.Q<Label>("PlayerCountLabel");
    _playerListContainer = root.Q<VisualElement>("PlayerListContainer");
    _statusLabel = root.Q<Label>("StatusLabel");
    _readyButton = root.Q<Button>("ReadyButton");
    _startGameButton = root.Q<Button>("StartGameButton");
}
```

### 2. Register Event Handlers

```csharp
private void RegisterEventHandlers()
{
    _leaveButton?.clicked += OnLeaveButtonClicked;
    _copyCodeButton?.clicked += OnCopyCodeButtonClicked;
    _readyButton?.clicked += OnReadyButtonClicked;
    _startGameButton?.clicked += OnStartGameButtonClicked;
}
```

### 3. Update UI from ViewModel/Model

```csharp
public void UpdateJoinCode(string code)
{
    if (_joinCodeLabel != null)
    {
        _joinCodeLabel.text = code;
    }
}

public void UpdatePlayerCount(int current, int max)
{
    if (_playerCountLabel != null)
    {
        _playerCountLabel.text = $"Players ({current}/{max})";
    }
}

public void UpdateStatus(string message)
{
    if (_statusLabel != null)
    {
        _statusLabel.text = message;
    }
}

public void SetReadyState(bool isReady)
{
    if (_readyButton != null)
    {
        if (isReady)
        {
            _readyButton.AddToClassList("ready");
            _readyButton.text = "Not Ready";
        }
        else
        {
            _readyButton.RemoveFromClassList("ready");
            _readyButton.text = "Ready";
        }
    }
}

public void SetStartButtonEnabled(bool enabled)
{
    _startGameButton?.SetEnabled(enabled);
}
```

### 4. Dynamic Player List Management

```csharp
public void ClearPlayerList()
{
    _playerListContainer?.Clear();
}

public void AddPlayer(string playerId, string playerName, bool isHost, bool isReady, bool isLocalPlayer)
{
    // Use template from "Dynamic Player Item Structure" section above
    var playerItem = CreatePlayerItem(playerId, playerName, isHost, isReady, isLocalPlayer);
    _playerListContainer?.Add(playerItem);
}

public void RemovePlayer(string playerId)
{
    var playerItem = _playerListContainer?.Q<VisualElement>($"PlayerItem_{playerId}");
    playerItem?.RemoveFromHierarchy();
}

public void UpdatePlayerReady(string playerId, bool isReady)
{
    var statusLabel = _playerListContainer?.Q<Label>($"PlayerItem_{playerId}/PlayerInfo/PlayerStatus");
    if (statusLabel != null)
    {
        statusLabel.text = isReady ? "Ready ✓" : "Not Ready ✗";
        statusLabel.RemoveFromClassList("ready");
        statusLabel.RemoveFromClassList("not-ready");
        statusLabel.AddToClassList(isReady ? "ready" : "not-ready");
    }
}
```

---

## Status Message Examples

Use `StatusLabel` to communicate lobby state:

```csharp
UpdateStatus("Waiting for players...");
UpdateStatus("All players ready!");
UpdateStatus("Starting game in 3...");
UpdateStatus("Connection lost. Reconnecting...");
UpdateStatus("Host left. Returning to menu...");
```

---

## Touch Target Requirements

All interactive elements meet **WCAG 2.1 mobile guidelines**:

- **Minimum Touch Target**: 44x44px (defined in USS)
- **Large Buttons**: 60px+ height (ReadyButton, StartGameButton)
- **Icon Buttons**: 44x44px (LeaveButton, CopyCodeButton)

---

## Color Scheme

Following **ShaderOp Design System**:

- **Primary Blue**: `rgb(60, 100, 180)` - Default buttons
- **Success Green**: `rgb(60, 180, 100)` - Ready state
- **Info Blue**: `rgb(100, 150, 255)` - Status messages, Join Code
- **Error Red**: `rgb(220, 80, 80)` - Leave button, disconnections

---

## Responsive Breakpoints

Defined in `LobbyView.uss`:

- **@media (max-height: 1600px)**: Reduced font sizes
- **@media (max-height: 1280px)**: Compact layout for small screens

---

## Accessibility Features

- ✅ **Touch-friendly**: All buttons ≥60px
- ✅ **High Contrast**: Border highlights on focus
- ✅ **Readable Text**: Minimum 14px body, 18px headings
- ✅ **Reduced Motion**: `@media (prefers-reduced-motion: reduce)` support
- ✅ **Visual Feedback**: Hover/Active states for all buttons

---

## Example: Complete C# Integration

```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyView : MonoBehaviour
{
    [SerializeField] private UIDocument? _uiDocument;

    private Button? _leaveButton;
    private Button? _copyCodeButton;
    private Label? _joinCodeLabel;
    private Label? _playerCountLabel;
    private VisualElement? _playerListContainer;
    private Label? _statusLabel;
    private Button? _readyButton;
    private Button? _startGameButton;

    private bool _isReady = false;

    private void OnEnable()
    {
        var root = _uiDocument?.rootVisualElement;
        if (root == null) return;

        GetUIElements(root);
        RegisterEventHandlers();
        InitializeUI();
    }

    private void OnDisable()
    {
        UnregisterEventHandlers();
    }

    private void GetUIElements(VisualElement root)
    {
        _leaveButton = root.Q<Button>("LeaveButton");
        _copyCodeButton = root.Q<Button>("CopyCodeButton");
        _joinCodeLabel = root.Q<Label>("JoinCodeLabel");
        _playerCountLabel = root.Q<Label>("PlayerCountLabel");
        _playerListContainer = root.Q<VisualElement>("PlayerListContainer");
        _statusLabel = root.Q<Label>("StatusLabel");
        _readyButton = root.Q<Button>("ReadyButton");
        _startGameButton = root.Q<Button>("StartGameButton");
    }

    private void RegisterEventHandlers()
    {
        if (_leaveButton != null) _leaveButton.clicked += OnLeaveButtonClicked;
        if (_copyCodeButton != null) _copyCodeButton.clicked += OnCopyCodeButtonClicked;
        if (_readyButton != null) _readyButton.clicked += OnReadyButtonClicked;
        if (_startGameButton != null) _startGameButton.clicked += OnStartGameButtonClicked;
    }

    private void UnregisterEventHandlers()
    {
        if (_leaveButton != null) _leaveButton.clicked -= OnLeaveButtonClicked;
        if (_copyCodeButton != null) _copyCodeButton.clicked -= OnCopyCodeButtonClicked;
        if (_readyButton != null) _readyButton.clicked -= OnReadyButtonClicked;
        if (_startGameButton != null) _startGameButton.clicked -= OnStartGameButtonClicked;
    }

    private void InitializeUI()
    {
        UpdateJoinCode("ABC123");
        UpdatePlayerCount(1, 4);
        UpdateStatus("Waiting for players...");
        SetStartButtonEnabled(false); // Non-host by default
    }

    private void OnLeaveButtonClicked()
    {
        Debug.Log("[LobbyView] Leave button clicked");
        // TODO: Call ViewModel or Service
    }

    private void OnCopyCodeButtonClicked()
    {
        if (_joinCodeLabel != null)
        {
            GUIUtility.systemCopyBuffer = _joinCodeLabel.text;
            UpdateStatus("Code copied to clipboard!");
            Debug.Log($"[LobbyView] Copied code: {_joinCodeLabel.text}");
        }
    }

    private void OnReadyButtonClicked()
    {
        _isReady = !_isReady;
        SetReadyState(_isReady);
        Debug.Log($"[LobbyView] Ready state: {_isReady}");
        // TODO: Send ready state to ViewModel/Network
    }

    private void OnStartGameButtonClicked()
    {
        Debug.Log("[LobbyView] Start game button clicked");
        UpdateStatus("Starting game...");
        // TODO: Call ViewModel to start game
    }

    public void UpdateJoinCode(string code)
    {
        if (_joinCodeLabel != null)
        {
            _joinCodeLabel.text = code;
        }
    }

    public void UpdatePlayerCount(int current, int max)
    {
        if (_playerCountLabel != null)
        {
            _playerCountLabel.text = $"Players ({current}/{max})";
        }
    }

    public void UpdateStatus(string message)
    {
        if (_statusLabel != null)
        {
            _statusLabel.text = message;
        }
    }

    public void SetReadyState(bool isReady)
    {
        if (_readyButton != null)
        {
            if (isReady)
            {
                _readyButton.AddToClassList("ready");
                _readyButton.text = "Not Ready";
            }
            else
            {
                _readyButton.RemoveFromClassList("ready");
                _readyButton.text = "Ready";
            }
        }
    }

    public void SetStartButtonEnabled(bool enabled)
    {
        _startGameButton?.SetEnabled(enabled);
    }
}
```

---

## Coordination Notes for unity-developer

1. **Element Names**: All names in UXML match this documentation exactly
2. **Dynamic Player Items**: Use the provided template in "Dynamic Player Item Structure"
3. **State Management**: Use USS classes (`ready`, `not-ready`, `local-player`) for dynamic styling
4. **Status Updates**: Use `StatusLabel` for real-time feedback
5. **Touch Targets**: All buttons meet 44x44px minimum (enforced in USS)
6. **Responsive**: Tested for 1080x1920, 720x1280 resolutions

---

## Files Delivered

- ✅ **UXML**: `D:\PersonalGameDev\ShaderOp\ShaderOptimizer\Assets\UI\LobbyView.uxml`
- ✅ **USS**: `D:\PersonalGameDev\ShaderOp\ShaderOptimizer\Assets\UI\LobbyView.uss`
- ✅ **Documentation**: `D:\PersonalGameDev\ShaderOp\LOBBY_UI_ELEMENT_DOCUMENTATION.md`

---

**Ready for C# integration by unity-developer!**
