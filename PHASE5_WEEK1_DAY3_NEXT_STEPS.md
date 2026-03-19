# Phase 5 Week 1 Day 3: Next Steps Guide

**Date**: 2026-03-16 23:10
**Status**: Ready for Unity Restart → Automated Completion
**Commit**: `4aff646`

---

## 🎯 Quick Start

### USER: Unity Restart (5 minutes)

```bash
1. Save all work in Unity Editor (Ctrl + S)
2. Close Unity Editor completely
3. Open File Explorer → Navigate to:
   D:\PersonalGameDev\ShaderOp\ShaderOptimizer\Library\
4. Delete folder: ScriptAssemblies
5. Reopen Unity Editor
6. Wait for "Compiling..." to complete (status bar)
7. Check Console → Should show 0 errors
```

### CLAUDE: Automated Completion (30 minutes)

After you confirm Unity restart successful, I will automatically:
- Fix invalid using statement
- Create GameSyncService.prefab
- Configure GameBootstrap
- Execute TC1 test
- Generate test report

---

## 📊 Current State Verification

Run this command to verify changes are ready:

```bash
# Check assembly definition
cat ShaderOptimizer/Assets/Scripts/Runtime/ShaderOp.Runtime.asmdef | grep -A2 "PhotonUnity"

# Expected output:
# "PhotonUnityNetworking",
# "PhotonRealtime"
```

---

## 🔄 Automated Workflow (After Unity Restart)

### Phase 1: Code Cleanup (1 min)

**Action**: Remove invalid using statement
**File**: `GameBootstrap.cs` line 6

```csharp
// BEFORE:
using UnityEngine;
using ShaderOp.Core.Services;
using ShaderOp.Core.Services.Online;
using ShaderOp.Online.Services;  // ← DELETE THIS LINE

// AFTER:
using UnityEngine;
using ShaderOp.Core.Services;
using ShaderOp.Core.Services.Online;
// Line 6 deleted
```

---

### Phase 2: Prefab Creation (5 min)

**Action**: Create GameSyncService.prefab via Unity MCP

```json
{
  "prefab_path": "Assets/Prefabs/Services/GameSyncService.prefab",
  "components": [
    "PhotonGameSyncService",
    "PhotonView"
  ],
  "photon_view_config": {
    "ViewID": 0,
    "OwnershipTransfer": "Fixed",
    "Synchronization": "Off"
  }
}
```

**Verification**:
```
Project → Assets/Prefabs/Services/
├── NetworkService.prefab ✅
└── GameSyncService.prefab ✅ (new)
```

---

### Phase 3: GameBootstrap Configuration (5 min)

**Action**: Assign prefabs via Unity MCP

**Scene**: MainMenu.unity
**GameObject**: GameBootstrap

```csharp
_networkServicePrefab = Load("Assets/Prefabs/Services/NetworkService.prefab");
_gameSyncServicePrefab = Load("Assets/Prefabs/Services/GameSyncService.prefab");
```

**Save Scene**: MainMenu.unity

---

### Phase 4: TC1 Test Execution (15 min)

**Test Case**: TC1 - Offline Mode (Graceful Degradation)

**Steps**:
1. Enter Play Mode
2. Verify Console Logs:
   ```
   [GameBootstrap] Initializing services...
   [GameBootstrap] INetworkService (Photon) registered.
   [GameBootstrap] IGameSyncService (Photon) registered.
   [GameBootstrap] 7 services registered successfully.
   ```
3. Check Hierarchy:
   ```
   DontDestroyOnLoad
   ├── GameBootstrap
   ├── PhotonNetworkService
   └── PhotonGameSyncService
   ```
4. Verify No Errors:
   - No "type not found" errors
   - No "component missing" errors
   - Photon services gracefully handle offline state

**Success Criteria**:
- ✅ 7 services registered
- ✅ Photon services instantiated
- ✅ No compilation errors
- ✅ No runtime errors

---

## 📝 Test Report Template

After TC1 completes, I will generate:

**File**: `PHASE5_WEEK1_DAY3_TC1_REPORT.md`

**Contents**:
```markdown
# TC1: Offline Mode Test Report

**Date**: 2026-03-16
**Duration**: 15 minutes
**Result**: PASS/FAIL

## Environment
- Unity Version: 2021.3 LTS
- Photon PUN 2: [version]
- Build: Unity Editor

## Test Execution
1. Services Registered: [count]/7
2. Photon Services Instantiated: YES/NO
3. Console Errors: 0
4. Hierarchy Verification: PASS

## Logs
[Console output]

## Screenshots
[Hierarchy screenshot]

## Conclusion
TC1 PASSED - Offline mode graceful degradation verified
```

---

## 🚨 Potential Issues & Solutions

### Issue 1: "PhotonUnityNetworking assembly not found"

**Cause**: Photon PUN 2 not installed
**Solution**:
```
Window → Package Manager
Search: "Photon PUN 2"
Install from Asset Store
```

---

### Issue 2: "NetworkService.prefab missing components"

**Cause**: Prefab created before assembly fix
**Solution**:
```
Delete: Assets/Prefabs/Services/NetworkService.prefab
Re-run: PhotonPrefabSetup.cs menu item
Tools → ShaderOp → Phase 5 → Create Photon Service Prefabs
```

---

### Issue 3: Compilation still fails after restart

**Cause**: ScriptAssemblies not fully cleared
**Solution**:
```
1. Close Unity
2. Delete entire Library folder (not just ScriptAssemblies)
3. Reopen Unity
4. Wait 5-10 minutes for full reimport
```

---

## 📁 File Checklist

### Created This Session ✅
```
PHASE5_WEEK1_DAY3_COMPILATION_BLOCKERS.md
PHASE5_WEEK1_DAY3_SESSION_SUMMARY.md
PHASE5_WEEK1_DAY3_NEXT_STEPS.md (this file)
```

### Modified This Session ✅
```
ShaderOptimizer/Assets/Scripts/Runtime/ShaderOp.Runtime.asmdef
ShaderOptimizer/Assets/Prefabs/Services/NetworkService.prefab (moved)
```

### To Be Created (Automated) ⏳
```
ShaderOptimizer/Assets/Prefabs/Services/GameSyncService.prefab
PHASE5_WEEK1_DAY3_TC1_REPORT.md
```

### To Be Modified (Automated) ⏳
```
ShaderOptimizer/Assets/Scripts/Runtime/Core/GameBootstrap.cs (line 6 delete)
ShaderOptimizer/Assets/Scenes/MainMenu.unity (prefab assignments)
```

---

## 🎯 Success Verification

After all automated steps complete, verify:

### 1. Compilation Clean
```bash
# Console should show:
Compilation finished with 0 errors
```

### 2. Prefabs Exist
```bash
# Both prefabs should exist:
ls ShaderOptimizer/Assets/Prefabs/Services/
# NetworkService.prefab
# GameSyncService.prefab
```

### 3. GameBootstrap Configured
```bash
# MainMenu.unity should have prefab references:
grep -A5 "_networkServicePrefab" ShaderOptimizer/Assets/Scenes/MainMenu.unity
# Should show: fileID pointing to NetworkService.prefab
```

### 4. Play Mode Test
```
Play Mode → Console:
[GameBootstrap] 7 services registered successfully.

Hierarchy → DontDestroyOnLoad:
- PhotonNetworkService
- PhotonGameSyncService
```

---

## 📈 Progress Tracking

### Session Progress
```
[✅] Test agents launched
[✅] Compilation errors diagnosed
[✅] Assembly definition fixed
[✅] NetworkService.prefab relocated
[✅] Documentation created (1000+ lines)
[⏸️] Unity restart (USER)
[⏳] Invalid using statement removal
[⏳] GameSyncService.prefab creation
[⏳] GameBootstrap configuration
[⏳] TC1 test execution
[⏳] TC1 report generation
```

### Day 3 Progress
```
Preparation: ✅ Complete
Prefab Setup: 50% (NetworkService done, GameSyncService pending)
Testing: 0% (blocked by prefab setup)
```

### Week 1 Progress
```
Day 1: ✅ 100% (6/6 tasks)
Day 2: ✅ 100% (5/5 tasks)
Day 3: ⏸️ 40% (4/10 sub-tasks)
Overall: 39% (11/28 tasks)
```

---

## 🔗 Quick Reference

### Documentation Index
```
COMPILATION_BLOCKERS.md  → Root cause analysis (400 lines)
SESSION_SUMMARY.md        → Complete session overview (300 lines)
NEXT_STEPS.md            → This file (200 lines)
TEST_PLAN.md             → TC1-TC10 test cases
PREFAB_SETUP_CHECKLIST   → Manual setup guide (15 min)
```

### Commands
```bash
# Check Unity version
cat ShaderOptimizer/ProjectSettings/ProjectVersion.txt

# Verify Photon files exist
find ShaderOptimizer/Assets/Scripts -name "*Photon*"

# Check assembly definition
cat ShaderOptimizer/Assets/Scripts/Runtime/ShaderOp.Runtime.asmdef
```

---

## 💾 Git Status

**Latest Commit**: `4aff646`
**Branch**: main
**Uncommitted Changes**: Various documentation files (not critical)

**Next Commit** (after automated completion):
```
feat: Phase 5 Week 1 Day 3 - Prefab Setup Complete

- Removed invalid using statement from GameBootstrap.cs
- Created GameSyncService.prefab with PhotonView
- Configured GameBootstrap prefab references in MainMenu.unity
- Executed TC1 offline mode test (PASS)

Files modified:
- GameBootstrap.cs (line 6 deleted)
- MainMenu.unity (prefab references assigned)

Files created:
- GameSyncService.prefab
- PHASE5_WEEK1_DAY3_TC1_REPORT.md
```

---

## ⏱️ Time Estimates

### User Time Required
```
Unity Restart: 5 minutes
Verification: 2 minutes
Total: 7 minutes
```

### Automated Time (Claude)
```
Phase 1 (Code cleanup): 1 minute
Phase 2 (Prefab creation): 5 minutes
Phase 3 (GameBootstrap config): 5 minutes
Phase 4 (TC1 test): 15 minutes
Phase 5 (Report generation): 4 minutes
Total: 30 minutes
```

### Total Session Time
```
User + Automated: 37 minutes to Day 3 first test complete
```

---

**最終更新**: 2026-03-16 23:10
**ステータス**: Unity再起動準備完了
**Next Action**: ユーザーによるUnity Editor再起動 → "Unity restarted successfully" と報告
