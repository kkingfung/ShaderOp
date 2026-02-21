# Unity Test Framework パターン

## 概要
Unity Test Frameworkを使用した単体テスト、統合テスト、TDD実践パターン。

## 基本構造

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System.Collections;

/// <summary>
/// テストクラス
/// </summary>
public class SampleTests
{
    /// <summary>各テスト前の初期化</summary>
    [SetUp]
    public void SetUp()
    {
        // 初期化処理
    }

    /// <summary>各テスト後のクリーンアップ</summary>
    [TearDown]
    public void TearDown()
    {
        // クリーンアップ処理
    }

    /// <summary>EditModeテスト</summary>
    [Test]
    public void Test_正常系_期待結果()
    {
        // Arrange（準備）
        var expected = 10;

        // Act（実行）
        var actual = Calculate();

        // Assert（検証）
        Assert.AreEqual(expected, actual);
    }

    /// <summary>PlayModeテスト</summary>
    [UnityTest]
    public IEnumerator Test_非同期処理_完了する()
    {
        // Arrange
        var gameObject = new GameObject();

        // Act
        yield return new WaitForSeconds(1.0f);

        // Assert
        Assert.IsNotNull(gameObject);

        // クリーンアップ
        Object.Destroy(gameObject);
    }
}
```

## UniTask テスト

```csharp
/// <summary>
/// UniTaskを使用した非同期テスト
/// </summary>
[UnityTest]
public IEnumerator LoadAssetAsync_正常系_アセットが読み込まれる() => UniTask.ToCoroutine(async () =>
{
    // Arrange
    var loader = new AssetLoader();
    var address = "TestAsset";

    // Act
    var asset = await loader.LoadAssetAsync<Texture2D>(address);

    // Assert
    Assert.IsNotNull(asset);
    Assert.AreEqual("TestAsset", asset.name);

    // クリーンアップ
    loader.ReleaseAsset(address);
});
```

## パラメータ化テスト

```csharp
/// <summary>
/// パラメータ化テスト
/// </summary>
[Test]
[TestCase(0, 10, 5, ExpectedResult = true)]
[TestCase(0, 10, 0, ExpectedResult = true)]
[TestCase(0, 10, 10, ExpectedResult = true)]
[TestCase(0, 10, 11, ExpectedResult = false)]
[TestCase(0, 10, -1, ExpectedResult = false)]
public bool IsInRange_各種値_検証結果が正しい(int min, int max, int value)
{
    // Act & Assert
    return value >= min && value <= max;
}
```

## モック/スタブ

```csharp
/// <summary>
/// テスト用モックサービス
/// </summary>
public class MockDataService : IDataService
{
    private readonly Dictionary<string, string> _mockData = new Dictionary<string, string>();

    public void AddMockData(string key, string value)
    {
        _mockData[key] = value;
    }

    public async UniTask<string> LoadDataAsync(string key)
    {
        await UniTask.Delay(10); // 非同期処理をシミュレート

        if (_mockData.TryGetValue(key, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"Key not found: {key}");
    }
}

/// <summary>
/// モックを使用したテスト
/// </summary>
[UnityTest]
public IEnumerator LoadData_モック使用_データが取得できる() => UniTask.ToCoroutine(async () =>
{
    // Arrange
    var mockService = new MockDataService();
    mockService.AddMockData("test_key", "test_value");

    var loader = new DataLoader(mockService);

    // Act
    var data = await loader.LoadAsync("test_key");

    // Assert
    Assert.AreEqual("test_value", data);
});
```

## 例外テスト

```csharp
/// <summary>
/// 例外がスローされることを検証
/// </summary>
[Test]
public void SetValue_Null引数_例外がスローされる()
{
    // Arrange
    var service = new MyService();

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
    {
        service.SetValue(null);
    });
}

/// <summary>
/// 非同期メソッドの例外テスト
/// </summary>
[UnityTest]
public IEnumerator LoadAsync_無効なID_例外がスローされる() => UniTask.ToCoroutine(async () =>
{
    // Arrange
    var loader = new AssetLoader();

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(async () =>
    {
        await loader.LoadAssetAsync<Texture2D>("invalid_id");
    });
});
```

## TDD（テスト駆動開発）

```csharp
// 1. Red: 失敗するテストを書く
[Test]
public void Calculate_2つの数値_合計が返される()
{
    // Arrange
    var calculator = new Calculator();

    // Act
    var result = calculator.Add(2, 3);

    // Assert
    Assert.AreEqual(5, result); // まだ実装されていないため失敗
}

// 2. Green: テストを通す最小限の実装
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b; // 最小限の実装
    }
}

// 3. Refactor: コードを改善
public class Calculator
{
    /// <summary>2つの数値を加算します</summary>
    public int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>2つの数値を減算します</summary>
    public int Subtract(int a, int b)
    {
        return a - b;
    }
}
```

## ベストプラクティス

✅ **DO**:
- テスト名は明確に（メソッド名_前提条件_期待結果）
- Arrange-Act-Assertパターンを使用
- テストの独立性を保つ
- SetUp/TearDownでリソース管理

❌ **DON'T**:
- テスト間で状態を共有
- 複数のAssertを1つのテストに詰め込む
- 外部依存（ネットワーク、ファイルシステム）に依存

信頼度: 0.94
