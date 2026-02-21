# UniTask パターン

## 概要
UniTaskを使用した非同期処理のベストプラクティス。

## 基本パターン

```csharp
// 非同期メソッド定義
public async UniTask LoadAssetAsync()
{
    // Addressablesから非同期読み込み
    var handle = Addressables.LoadAssetAsync<Texture2D>("path");
    await handle.ToUniTask();
    return handle.Result;
}
```

## 並列処理

```csharp
// 複数の非同期処理を並行実行
public async UniTask LoadMultipleAsync()
{
    var task1 = LoadTexture1Async();
    var task2 = LoadTexture2Async();
    var task3 = LoadTexture3Async();

    // 全て完了するまで待機
    await UniTask.WhenAll(task1, task2, task3);
}
```

## タイムアウト処理

```csharp
// タイムアウトを設定
var cts = new CancellationTokenSource();
cts.CancelAfterSlim(TimeSpan.FromSeconds(5));

try
{
    await LoadAssetAsync().AttachExternalCancellation(cts.Token);
}
catch (OperationCanceledException)
{
    Debug.LogError("タイムアウト");
}
```

## ベストプラクティス

✅ **DO**:
- `async UniTask` を使用（`async void` は避ける）
- CancellationTokenを活用
- `.Forget()` で Fire-and-Forget

❌ **DON'T**:
- UniTask内で `Task.Run()` を使用
- デッドロックの原因となる `.Wait()` や `.Result`

信頼度: 0.92
