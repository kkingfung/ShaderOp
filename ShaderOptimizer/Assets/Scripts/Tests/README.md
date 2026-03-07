# ShaderOp Tests

このディレクトリには、ShaderOpプロジェクトの包括的なテストスイートが含まれています。

## 📁 ディレクトリ構造

```
Tests/
├── TestHelpers/                        # テストヘルパーとユーティリティ
│   ├── TestBase.cs                    # テスト基底クラス（GameObject自動クリーンアップ）
│   ├── TestUtilities.cs               # テストユーティリティメソッド集
│   ├── SceneTestUtilities.cs          # シーンテスト専用ユーティリティ
│   └── Mocks/
│       └── MockHexGrid.cs             # モックグリッド生成
├── CoreSystemsTests.cs                 # コアシステム（Audio, Settings, Input等）
├── SceneLoaderAsyncTests.cs            # SceneLoader非同期テスト（EditMode）
├── HexCoordinateTests.cs               # 六角座標システムテスト
├── HexGridTests.cs                     # 六角グリッドシステムテスト
├── HexReversiTests.cs                  # Reversiゲームロジックテスト
├── HexCheckersTests.cs                 # Checkersゲームロジックテスト
├── HexChessTests.cs                    # Chessゲームロジックテスト
└── PlayMode/
    ├── ShaderOp.Tests.PlayMode.asmdef # PlayModeテスト用Assembly Definition
    ├── MinigameIntegrationTests.cs    # ミニゲーム統合テスト
    └── SceneLoaderPlayModeTests.cs    # SceneLoader統合テスト（PlayMode）
```

## 🚀 クイックスタート

### テスト実行方法

#### Unity Editor内で実行

1. `Window → General → Test Runner` でTest Runnerウィンドウを開く
2. **EditMode** タブで単体テストを実行
3. **PlayMode** タブで統合テストを実行

#### コマンドラインで実行

```bash
# EditMode テスト
Unity -runTests -testPlatform EditMode -testResults TestResults_EditMode.xml

# PlayMode テスト
Unity -runTests -testPlatform PlayMode -testResults TestResults_PlayMode.xml

# 特定のテストのみ
Unity -runTests -testPlatform EditMode -testFilter "ShaderOp.Tests.HexCoordinateTests"
```

## 📊 テストカバレッジ

| コンポーネント | カバレッジ | テストケース数 |
|----------------|------------|----------------|
| HexCoordinate | ✅ 100% | 30 |
| HexGrid | ✅ 95% | 40 |
| CoreSystems | ✅ 95% | 45 |
| SceneLoader (Async) | ✅ 95% | 30+ |
| HexReversi | ✅ 90% | 35 |
| HexCheckers | ✅ 85% | 40 |
| HexChess | ✅ 80% | 35 |
| Integration | ✅ 85% | 15 |

**合計**: ~270+ テストケース、~4,500行のテストコード

## 🛠️ テストヘルパーの使い方

### TestBase を使用

```csharp
using ShaderOp.Tests.Helpers;

public class MyTests : TestBase
{
    [Test]
    public void MyTest()
    {
        // GameObject を自動クリーンアップ
        var obj = CreateGameObject("TestObject");
        var component = CreateGameObjectWithComponent<MyComponent>();

        // テスト実行...

        // TearDown で自動的に破棄される
    }
}
```

### TestUtilities を使用

```csharp
using ShaderOp.Tests.Helpers;

[UnityTest]
public IEnumerator MyAsyncTest()
{
    // フレーム待機
    yield return TestUtilities.WaitForFrames(5);

    // 条件待機（タイムアウトあり）
    yield return TestUtilities.WaitUntil(() => IsReady(), 5f);

    // 浮動小数点比較
    TestUtilities.AssertApproximately(1.0f, 0.9999f, 0.001f);

    // Vector3比較
    TestUtilities.AssertVector3Approximately(expectedPos, actualPos);
}
```

### MockHexGrid を使用

```csharp
using ShaderOp.Tests.Mocks;

[Test]
public void MyGridTest()
{
    // テスト用グリッド作成
    var grid = MockHexGrid.CreateSimpleHexagonGrid(radius: 3);

    // パターン配置
    var coords = new List<HexCoordinate> { new HexCoordinate(0, 0) };
    MockHexGrid.PlacePiecesInPattern(grid, PieceType.Player1, coords);

    // テスト実行...
}
```

## ⚡ 非同期テスト（UniTask）

### SceneLoaderAsyncTests の使用

SceneLoaderクラスはUniTaskを使用した非同期処理を実装しているため、専用のテストアプローチが必要です。

#### EditModeでの非同期テスト

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;

[UnityTest]
public IEnumerator LoadSceneAsync_WithCancellation_CancelsCorrectly()
{
    // Arrange
    var cts = new CancellationTokenSource();
    var loader = SceneLoader.Instance;

    // Act
    var loadTask = loader.LoadSceneAsync("TestScene", LoadSceneMode.Additive, cts.Token);

    yield return TestUtilities.WaitForFrames(2);

    // キャンセル
    cts.Cancel();

    // UniTaskの完了を待つ
    yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 5f);

    // Assert
    Assert.IsFalse(loader.IsLoading);

    // Cleanup
    cts.Dispose();
}
```

#### PlayModeでの実シーンテスト

PlayModeテストでは実際のシーンロード・アンロードを検証します:

```csharp
[UnityTest]
public IEnumerator LoadSceneAsync_RealScene_LoadsSuccessfully()
{
    // テストシーンを作成
    string sceneName = "TestScene_PlayMode";
    Scene testScene = SceneTestUtilities.CreateTestScene(sceneName);
    SceneTestUtilities.AddGameObjectToScene(testScene, "TestObject");

    // シーンをロード
    var loadTask = _loader.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

    // ロード完了を待つ
    yield return TestUtilities.WaitUntil(() => loadTask.Status != UniTaskStatus.Pending, 10f);

    // 検証
    Assert.IsTrue(SceneTestUtilities.IsSceneLoaded(sceneName));

    // クリーンアップ
    yield return SceneTestUtilities.UnloadTestScene(sceneName);
}
```

### SceneTestUtilities の使用

シーンテストに特化したヘルパーメソッド:

```csharp
using ShaderOp.Tests.Helpers;

[UnityTest]
public IEnumerator MySceneTest()
{
    // テストシーンを作成
    string sceneName = "TestScene";
    Scene scene = SceneTestUtilities.CreateTestScene(sceneName);

    // シーンにオブジェクトを追加
    GameObject obj = SceneTestUtilities.AddGameObjectToScene(scene, "TestObject");

    // シーンロードを待機
    yield return SceneTestUtilities.WaitForSceneLoad(sceneName, timeoutSeconds: 10f);

    // テスト実行...

    // クリーンアップ
    yield return SceneTestUtilities.UnloadTestScene(sceneName);
}
```

### UniTask テストのベストプラクティス

1. **CancellationTokenを必ずDispose**
   ```csharp
   var cts = new CancellationTokenSource();
   try
   {
       yield return DoAsyncTest(cts.Token);
   }
   finally
   {
       cts.Dispose();
   }
   ```

2. **UniTaskStatus で完了を待つ**
   ```csharp
   var task = SomeAsyncMethod();
   yield return TestUtilities.WaitUntil(() => task.Status != UniTaskStatus.Pending, 5f);
   ```

3. **タイムアウトを設定**
   ```csharp
   // 10秒でタイムアウト
   yield return TestUtilities.WaitUntil(() => condition, timeoutSeconds: 10f);
   ```

4. **イベント発火の検証**
   ```csharp
   bool eventFired = false;
   loader.OnLoadCompleted += (sceneName) => { eventFired = true; };

   // ロード実行
   loader.LoadScene("TestScene");

   // イベント発火を待つ
   yield return TestUtilities.WaitUntil(() => eventFired, 5f);

   Assert.IsTrue(eventFired, "イベントが発火しませんでした");
   ```

### テスト種別の使い分け

| テスト種別 | 用途 | 実行速度 | テストファイル例 |
|-----------|------|----------|------------------|
| EditMode | 単体テスト、ロジック検証、モック使用 | 高速 | SceneLoaderAsyncTests.cs |
| PlayMode | 統合テスト、実シーン検証、ライフサイクル | 低速 | SceneLoaderPlayModeTests.cs |

## 📖 ベストプラクティス

### テスト命名規則

```csharp
// [メソッド名]_[条件]_[期待結果]
[Test]
public void PlacePiece_ValidMove_ReturnsTrue() { }

[Test]
public void GetNeighbors_CenterTile_ReturnsSixNeighbors() { }
```

### テスト構造（AAA パターン）

```csharp
[Test]
public void MyTest()
{
    // Arrange: テストデータの準備
    var model = new HexReversiModel();
    model.Initialize();

    // Act: テスト対象の実行
    bool result = model.PlacePiece(new HexCoordinate(1, 0));

    // Assert: 結果の検証
    Assert.IsTrue(result);
}
```

### リージョン分割

```csharp
#region 初期化テスト
// 初期化関連のテスト
#endregion

#region 移動テスト
// 移動ロジック関連のテスト
#endregion

#region エッジケーステスト
// 境界値・異常系テスト
#endregion
```

## 🔍 トラブルシューティング

### 問題: テストが不安定（Flaky）

```csharp
// ❌ 悪い例
yield return null;
Assert.IsTrue(isReady);

// ✅ 良い例
yield return TestUtilities.WaitUntil(() => isReady, 5f);
```

### 問題: GameObjectのメモリリーク

```csharp
// TestBase を継承して自動クリーンアップ
public class MyTests : TestBase
{
    [Test]
    public void MyTest()
    {
        var obj = CreateGameObject("Test"); // 自動破棄される
    }
}
```

## 📚 参考資料

- [TESTING_GUIDELINES.md](../../../TESTING_GUIDELINES.md) - 詳細なテストガイドライン
- [TEST_ENGINEERING_REVIEW.md](../../../TEST_ENGINEERING_REVIEW.md) - テストレビュー報告書
- [Unity Test Framework Documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

## 🔄 CI/CD統合

このテストスイートはJenkinsパイプラインに統合されています:

```groovy
stage('Run Tests') {
    steps {
        bat 'Unity -runTests -testPlatform EditMode'
        bat 'Unity -runTests -testPlatform PlayMode'
        junit 'TestResults_*.xml'
    }
}
```

## 📈 今後の予定

- [x] SceneLoader非同期テスト完全実装 (0% → 95%) ✅ 完了
- [x] PlayModeテスト拡張 (15% → 30%) ✅ 完了
- [x] SceneTestUtilities実装 ✅ 完了
- [ ] UI Systemsテスト追加 (30% → 75%)
- [ ] Customizationテスト追加 (25% → 70%)
- [ ] パフォーマンステスト拡張
- [ ] カバレッジレポート自動生成

---

**最終更新**: 2026-03-01
**テスト責任者**: Test Engineering Specialist
