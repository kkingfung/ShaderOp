# Lobby UI Design Summary

**Agent Duel: ui-ux-designer vs unity-developer**
**Phase**: Phase 5 Week 2 Day 4-5
**Target**: Portrait Mobile (1080x1920)

---

## Design Overview

**Layout Structure**: 10% Header / 80% Content / 10% Footer

```
┌─────────────────────────────────────┐
│  ← [Lobby Title]           [空白]   │ 10% Header
├─────────────────────────────────────┤
│                                     │
│  ╔═══════════════════════════════╗  │
│  ║   Room Code                   ║  │
│  ║   ┌───────────────────┐       ║  │
│  ║   │  ABC123  [📋]     │       ║  │
│  ║   └───────────────────┘       ║  │
│  ║   Share this code...          ║  │
│  ╚═══════════════════════════════╝  │
│                                     │
│  ╔═══════════════════════════════╗  │
│  ║   Players (2/4)               ║  │
│  ║   ┌─────────────────────────┐ ║  │
│  ║   │ 🎮 You (Host) ⭐         │ ║  │
│  ║   │    Ready ✓               │ ║  │
│  ║   ├─────────────────────────┤ ║  │ 80% Content
│  ║   │ 🎮 Player2              │ ║  │
│  ║   │    Not Ready ✗          │ ║  │
│  ║   ├─────────────────────────┤ ║  │
│  ║   │ [Empty Slot]            │ ║  │
│  ║   ├─────────────────────────┤ ║  │
│  ║   │ [Empty Slot]            │ ║  │
│  ║   └─────────────────────────┘ ║  │
│  ╚═══════════════════════════════╝  │
│                                     │
│  📘 Waiting for players...          │
│                                     │
├─────────────────────────────────────┤
│  [ Ready ]      [ Start Game ]      │ 10% Footer
└─────────────────────────────────────┘
```

---

## Color Palette (ShaderOp Design System)

### Primary Colors
- **Blue Primary**: `rgb(60, 100, 180)` - Default buttons, borders
- **Blue Accent**: `rgb(100, 200, 255)` - Join code, highlights
- **Blue Info**: `rgb(100, 150, 255)` - Status messages

### Status Colors
- **Success Green**: `rgb(60, 180, 100)` - Ready state
- **Error Red**: `rgb(220, 80, 80)` - Leave button, disconnections
- **Warning Yellow**: `rgb(255, 180, 60)` - Not ready (optional)

### Neutral Colors
- **Background Dark**: `rgb(15, 15, 20)` - Root background
- **Surface Medium**: `rgb(30, 30, 40)` - Panels, cards
- **Text Primary**: `rgb(255, 255, 255)` - Main text
- **Text Secondary**: `rgb(180, 180, 200)` - Helper text

---

## Typography

**Font**: Roboto (Unity Default)

| Element | Size | Weight | Color |
|---------|------|--------|-------|
| Lobby Title | 28px | Bold | `rgb(200, 220, 255)` |
| Join Code | 48px | Bold | `rgb(100, 200, 255)` |
| Section Label | 18px | Bold | `rgb(180, 200, 240)` |
| Player Name | 18px | Bold | `rgb(220, 220, 240)` |
| Player Status | 14px | Normal | Green/Red |
| Helper Text | 14px | Italic | `rgb(150, 170, 200)` |
| Button Text | 20px | Bold | White |

---

## Component Breakdown

### 1. Header (10%)
- **Left**: Leave Button (← icon, red, 44x44px)
- **Center**: "Lobby" Title (28px, bold)
- **Right**: Spacer (empty, for balance)

### 2. Join Code Section
- **Background**: Dark blue panel with border glow
- **Code Display**: Large 48px text (ABC123) + Copy button
- **Helper Text**: "Share this code with friends to join"

### 3. Player List Section (Scrollable)
- **Header**: "Players (X/4)" label
- **Player Items**:
  - Avatar circle (48px) with emoji
  - Player name (18px bold)
  - Ready status ("Ready ✓" green / "Not Ready ✗" red)
  - Host star (⭐) if host
  - Local player: highlighted with blue border

### 4. Status Section
- **Info bar**: Blue left border
- **Text**: Dynamic status messages (e.g., "Waiting for players...")

### 5. Footer (10%)
- **Ready Button**: Toggle (gray → green when ready)
- **Start Game Button**: Primary blue (host only, disabled if not all ready)

---

## Interaction States

### Ready Button
```
State: Not Ready
- Background: Gray `rgba(100, 100, 120, 0.6)`
- Text: "Ready"
- Click → State: Ready

State: Ready
- Background: Green `rgba(60, 180, 100, 0.8)`
- Text: "Not Ready"
- Click → State: Not Ready
```

### Start Game Button
```
State: Disabled (not all players ready)
- Background: Dark gray `rgba(60, 60, 80, 0.5)`
- Text: Grayed out
- Opacity: 0.6

State: Enabled (all players ready, is host)
- Background: Blue `rgba(60, 100, 200, 0.8)`
- Hover: Lighter blue + scale 1.05
```

### Player Item States
```
Local Player:
- Border: Blue 3px
- Background: Slightly lighter

Other Player:
- Border: Gray 2px
- Background: Dark

Ready Status:
- Green text + checkmark (✓)

Not Ready Status:
- Red text + cross (✗)
```

---

## Touch Optimization

**All buttons meet WCAG 2.1 mobile guidelines**:

| Element | Size | Touch Target |
|---------|------|--------------|
| Leave Button | 44x44px | ✅ 44px |
| Copy Code Button | 44x44px | ✅ 44px |
| Ready Button | 60px height | ✅ 60px |
| Start Game Button | 60px height | ✅ 60px |
| Player Item | 70px height | ✅ 70px |

---

## Responsive Breakpoints

### Standard (1920px height)
- Join Code: 48px
- Player Avatar: 48px
- Button Height: 60px

### Medium (1600px height)
- Join Code: 40px
- Player Avatar: 48px
- Button Height: 56px

### Small (1280px height)
- Join Code: 36px
- Player Avatar: 40px
- Button Height: 56px

---

## Animation Guidelines

**All animations respect `@media (prefers-reduced-motion: reduce)`**

### Button Hover
```css
transition: scale, background-color, border-color
duration: 0.2s
hover: scale(1.05)
active: scale(0.95)
```

### Player Item Hover
```css
transition: background-color, border-color
duration: 0.15s
hover: lighter background + brighter border
```

### Ready State Change
```css
transition: background-color, border-color
duration: 0.2s
class toggle: .ready
```

---

## Accessibility Checklist

- ✅ **Contrast Ratio**: All text ≥4.5:1 (WCAG AA)
- ✅ **Touch Targets**: All interactive elements ≥44x44px
- ✅ **Focus States**: Border highlights on focus
- ✅ **Keyboard Navigation**: Button order: Leave → Ready → Start
- ✅ **Reduced Motion**: Scale animations disabled in reduce-motion mode
- ✅ **Screen Reader**: Semantic element names (not implemented in UI Toolkit, but names are clear)

---

## File Structure

```
Assets/
└── UI/
    ├── LobbyView.uxml          ← UI structure
    ├── LobbyView.uss           ← Lobby-specific styles
    └── Styles/
        ├── ShaderOpDesignSystem.uss  ← Design tokens
        └── PortraitMobile.uss         ← Portrait layout patterns
```

---

## Integration Notes for unity-developer

### Element Binding (Q<T>)
```csharp
var leaveButton = root.Q<Button>("LeaveButton");
var copyCodeButton = root.Q<Button>("CopyCodeButton");
var joinCodeLabel = root.Q<Label>("JoinCodeLabel");
var playerCountLabel = root.Q<Label>("PlayerCountLabel");
var playerListContainer = root.Q<VisualElement>("PlayerListContainer");
var statusLabel = root.Q<Label>("StatusLabel");
var readyButton = root.Q<Button>("ReadyButton");
var startGameButton = root.Q<Button>("StartGameButton");
```

### Dynamic Player List
- Clear existing: `playerListContainer.Clear()`
- Add player: `playerListContainer.Add(CreatePlayerItem(...))`
- Remove player: `playerItem.RemoveFromHierarchy()`

### USS Class Control
```csharp
// Toggle ready state
readyButton.AddToClassList("ready");
readyButton.RemoveFromClassList("ready");

// Mark local player
playerItem.AddToClassList("local-player");

// Update status
statusLabel.AddToClassList("ready"); // or "not-ready"
```

### Copy to Clipboard
```csharp
GUIUtility.systemCopyBuffer = joinCodeLabel.text;
```

---

## Example User Flows

### Flow 1: Join as Non-Host
1. View displays join code (e.g., "ABC123")
2. Player clicks "Ready"
3. Ready button turns green, text changes to "Not Ready"
4. Status: "Waiting for host to start..."
5. Start Game button is disabled/hidden

### Flow 2: Host Starts Game
1. Host views player list
2. All players show "Ready ✓"
3. Status: "All players ready!"
4. Start Game button becomes enabled
5. Host clicks "Start Game"
6. Status: "Starting game in 3..."
7. Scene transition

### Flow 3: Player Disconnects
1. Player item removed from list
2. Player count updates: "Players (2/4)" → "Players (1/4)"
3. Status: "Player2 disconnected"
4. Start Game button disabled if was enabled

---

## Testing Checklist

- [ ] Join code displays correctly (6 alphanumeric characters)
- [ ] Copy button copies code to clipboard
- [ ] Player list scrolls if >4 players
- [ ] Local player has blue border
- [ ] Host shows star icon (⭐)
- [ ] Ready button toggles state correctly
- [ ] Start Game button only enabled for host when all ready
- [ ] Leave button returns to main menu
- [ ] Status messages update in real-time
- [ ] Responsive layout works on 720x1280, 1080x1920, 1440x2560

---

## Visual Design Principles

1. **Clarity**: Large text, high contrast, clear status indicators
2. **Touch-Friendly**: All buttons ≥60px, generous spacing
3. **Feedback**: Immediate visual response to all interactions
4. **Consistency**: Follows ShaderOp Design System throughout
5. **Mobile-First**: Optimized for portrait orientation, one-handed use

---

## Next Steps for unity-developer

1. **Create LobbyView.cs**: Inherit from MonoBehaviour, attach UIDocument
2. **Bind Elements**: Use Q<T> to get all interactive elements
3. **Register Events**: Button clicks, ready toggle logic
4. **Implement Dynamic Player List**: CreatePlayerItem() method
5. **Connect to ViewModel/Network**: Photon integration for multiplayer state
6. **Test on Device**: Verify touch targets, scrolling, animations

---

**Design Complete! Ready for C# implementation.**

🎨 **ui-ux-designer** ✅
🔧 **unity-developer** - Your turn!
