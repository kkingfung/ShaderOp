# UniTask クイックリファレンス

## プロジェクト向け UniTask/UniRx 実装パターン集

このドキュメントは、ShaderOpプロジェクトでUniTask/UniRxを使用する際の実装パターンをまとめたものです。

---

## 目次

1. [基本パターン](#1-基本パターン)
2. [Addressables連携](#2-addressables連携)
3. [シーンロード](#3-シーンロード)
4. [UI Toolkit連携](#4-ui-toolkit連携)
5. [UniRxパターン](#5-unirxパターン)
6. [エラーハンドリング](#6-エラーハンドリング)
7. [ベストプラクティス](#7-ベストプラクティス)

---

## 1. 基本パターン

### async/await 基本形

```csharp
#nullable enable

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class BasicExample : MonoBehaviour
{
    private CancellationTokenSource? _cts;

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        LoadDataAsync(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// データを非同期で読み込みます
    /// </summary>
    private async UniTaskVoid LoadDataAsync(CancellationToken ct)
    {
        try
        {
            // 1秒待機
            await UniTask.Delay(1000, cancellationToken: ct);

            Debug.Log("データ読み込み完了");
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("キャンセルされました");
        }
    }
}
```

### フレーム待機

```csharp
/// <summary>
/// 次のフレームまで待機
/// </summary>
private async UniTask WaitNextFrameExample()
{
    await UniTask.Yield();
    Debug.Log("1フレーム経過");
}

/// <summary>
/// 複数フレーム待機
/// </summary>
private async UniTask WaitMultipleFramesExample()
{
    for (int i = 0; i < 10; i++)
    {
        await UniTask.Yield();
    }
    Debug.Log("10フレーム経過");
}

/// <summary>
/// 条件待機
/// </summary>
private async UniTask WaitUntilExample(CancellationToken ct)
{
    await UniTask.WaitUntil(() => IsDataReady(), cancellationToken: ct);
    Debug.Log("データ準備完了");
}
```

---

## 2. Addressables連携

### アセット読み込み

```csharp
#nullable enable

using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;

public class AssetLoader : MonoBehaviour
{
    /// <summary>
    /// Materialを非同期で読み込みます
    /// </summary>
    public async UniTask<Material?> LoadMaterialAsync(string key, CancellationToken ct)
    {
        try
        {
            var handle = Addressables.LoadAssetAsync<Material>(key);
            var material = await handle.ToUniTask(cancellationToken: ct);

            if (material == null)
            {
                Debug.LogWarning($"Material読み込み失敗: {key}");
                return null;
            }

            Debug.Log($"Material読み込み完了: {key}");
            return material;
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log($"Material読み込みキャンセル: {key}");
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Material読み込みエラー: {key}, {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 複数アセットを並列で読み込みます
    /// </summary>
    public async UniTask<(Material?, Texture2D?, GameObject?)> LoadMultipleAssetsAsync(
        string materialKey,
        string textureKey,
        string prefabKey,
        CancellationToken ct)
    {
        var materialTask = LoadMaterialAsync(materialKey, ct);
        var textureTask = LoadTextureAsync(textureKey, ct);
        var prefabTask = LoadPrefabAsync(prefabKey, ct);

        // 並列実行
        await UniTask.WhenAll(materialTask, textureTask, prefabTask);

        return (materialTask.GetAwaiter().GetResult(),
                textureTask.GetAwaiter().GetResult(),
                prefabTask.GetAwaiter().GetResult());
    }

    private async UniTask<Texture2D?> LoadTextureAsync(string key, CancellationToken ct)
    {
        var handle = Addressables.LoadAssetAsync<Texture2D>(key);
        return await handle.ToUniTask(cancellationToken: ct);
    }

    private async UniTask<GameObject?> LoadPrefabAsync(string key, CancellationToken ct)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        return await handle.ToUniTask(cancellationToken: ct);
    }
}
```

### プログレス表示付きロード

```csharp
/// <summary>
/// プログレス表示付きでアセットを読み込みます
/// </summary>
public async UniTask LoadWithProgressAsync(string key, System.IProgress<float> progress, CancellationToken ct)
{
    var handle = Addressables.LoadAssetAsync<GameObject>(key);

    while (!handle.IsDone)
    {
        progress?.Report(handle.PercentComplete);
        await UniTask.Yield(cancellationToken: ct);
    }

    progress?.Report(1f);
    Debug.Log($"読み込み完了: {key}");
}
```

---

## 3. シーンロード

### SceneLoader実装例

```csharp
#nullable enable

using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Threading;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// シーンを非同期で読み込みます
    /// </summary>
    public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode, CancellationToken ct)
    {
        try
        {
            Debug.Log($"[SceneLoader] シーン読み込み開始: {sceneName}");

            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            if (operation == null)
            {
                Debug.LogError($"[SceneLoader] シーン読み込み失敗: {sceneName}");
                return;
            }

            // プログレス監視
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                Debug.Log($"[SceneLoader] 読み込み進行: {progress * 100:F0}%");

                await UniTask.Yield(cancellationToken: ct);
            }

            Debug.Log($"[SceneLoader] シーン読み込み完了: {sceneName}");
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log($"[SceneLoader] シーン読み込みキャンセル: {sceneName}");
        }
    }

    /// <summary>
    /// シーンをアンロードします
    /// </summary>
    public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct)
    {
        var operation = SceneManager.UnloadSceneAsync(sceneName);
        if (operation == null)
        {
            Debug.LogWarning($"[SceneLoader] アンロード失敗: {sceneName}");
            return;
        }

        await operation.ToUniTask(cancellationToken: ct);
        Debug.Log($"[SceneLoader] シーンアンロード完了: {sceneName}");
    }
}
```

---

## 4. UI Toolkit連携

### ボタンクリック非同期処理

```csharp
#nullable enable

using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;
using System.Threading;

public class UIController : MonoBehaviour
{
    private Button? _loadButton;
    private CancellationTokenSource? _cts;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _loadButton = root.Q<Button>("LoadButton");

        if (_loadButton != null)
        {
            _loadButton.clicked += OnLoadButtonClicked;
        }
    }

    private void OnDisable()
    {
        if (_loadButton != null)
        {
            _loadButton.clicked -= OnLoadButtonClicked;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// ロードボタンクリック時の処理
    /// </summary>
    private void OnLoadButtonClicked()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        LoadDataAsync(_cts.Token).Forget();
    }

    /// <summary>
    /// データ読み込み処理
    /// </summary>
    private async UniTaskVoid LoadDataAsync(CancellationToken ct)
    {
        try
        {
            if (_loadButton != null)
            {
                _loadButton.SetEnabled(false);
                _loadButton.text = "読み込み中...";
            }

            await UniTask.Delay(2000, cancellationToken: ct);

            Debug.Log("データ読み込み完了");
        }
        finally
        {
            if (_loadButton != null)
            {
                _loadButton.SetEnabled(true);
                _loadButton.text = "読み込み";
            }
        }
    }
}
```

---

## 5. UniRxパターン

### ReactiveProperty基本

```csharp
#nullable enable

using UniRx;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    /// <summary>現在のスコア</summary>
    public ReactiveProperty<int> Score { get; } = new ReactiveProperty<int>(0);

    /// <summary>ハイスコア</summary>
    public ReactiveProperty<int> HighScore { get; } = new ReactiveProperty<int>(0);

    /// <summary>ゲームオーバーフラグ</summary>
    public ReactiveProperty<bool> IsGameOver { get; } = new ReactiveProperty<bool>(false);

    private void Start()
    {
        // スコア変更を監視
        Score.Subscribe(value =>
        {
            Debug.Log($"スコア更新: {value}");

            // ハイスコア更新
            if (value > HighScore.Value)
            {
                HighScore.Value = value;
            }
        }).AddTo(this);

        // ゲームオーバー監視
        IsGameOver.Where(x => x).Subscribe(_ =>
        {
            Debug.Log("ゲームオーバー!");
        }).AddTo(this);
    }

    /// <summary>
    /// スコアを加算します
    /// </summary>
    public void AddScore(int points)
    {
        if (!IsGameOver.Value)
        {
            Score.Value += points;
        }
    }
}
```

### ViewModel + ReactiveProperty

```csharp
#nullable enable

using UniRx;
using System;

/// <summary>
/// カスタマイズ画面のViewModel
/// </summary>
public class CustomizationViewModel : IDisposable
{
    /// <summary>ヘアカラーID</summary>
    public ReactiveProperty<int> HairColorId { get; } = new ReactiveProperty<int>(1);

    /// <summary>アイカラーID</summary>
    public ReactiveProperty<int> EyeColorId { get; } = new ReactiveProperty<int>(1);

    /// <summary>肌色ID</summary>
    public ReactiveProperty<int> SkinToneId { get; } = new ReactiveProperty<int>(1);

    /// <summary>変更検出フラグ</summary>
    public IReadOnlyReactiveProperty<bool> HasChanges { get; }

    private CompositeDisposable _disposables = new CompositeDisposable();
    private int _initialHairColor;
    private int _initialEyeColor;
    private int _initialSkinTone;

    public CustomizationViewModel()
    {
        // 初期値を保存
        _initialHairColor = HairColorId.Value;
        _initialEyeColor = EyeColorId.Value;
        _initialSkinTone = SkinToneId.Value;

        // 変更検出
        HasChanges = Observable.CombineLatest(
            HairColorId,
            EyeColorId,
            SkinToneId,
            (hair, eye, skin) =>
                hair != _initialHairColor ||
                eye != _initialEyeColor ||
                skin != _initialSkinTone
        ).ToReadOnlyReactiveProperty();

        HasChanges.AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
```

---

## 6. エラーハンドリング

### 基本的なエラーハンドリング

```csharp
/// <summary>
/// エラーハンドリング付き非同期処理
/// </summary>
private async UniTask<bool> LoadWithErrorHandlingAsync(string key, CancellationToken ct)
{
    try
    {
        await LoadAssetAsync(key, ct);
        return true;
    }
    catch (System.OperationCanceledException)
    {
        Debug.Log($"キャンセルされました: {key}");
        return false;
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"エラーが発生しました: {key}, {ex.Message}");
        return false;
    }
}
```

### リトライロジック

```csharp
/// <summary>
/// リトライ付き非同期処理
/// </summary>
private async UniTask<T?> LoadWithRetryAsync<T>(string key, int maxRetries, CancellationToken ct) where T : UnityEngine.Object
{
    int retryCount = 0;

    while (retryCount < maxRetries)
    {
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            return await handle.ToUniTask(cancellationToken: ct);
        }
        catch (System.OperationCanceledException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            retryCount++;
            Debug.LogWarning($"読み込み失敗 ({retryCount}/{maxRetries}): {key}, {ex.Message}");

            if (retryCount >= maxRetries)
            {
                Debug.LogError($"最大リトライ回数を超えました: {key}");
                return null;
            }

            // 指数バックオフ
            await UniTask.Delay(1000 * (int)Math.Pow(2, retryCount - 1), cancellationToken: ct);
        }
    }

    return null;
}
```

---

## 7. ベストプラクティス

### ✅ DO (推奨)

```csharp
// ✅ CancellationTokenを必ず渡す
private async UniTask GoodExample(CancellationToken ct)
{
    await UniTask.Delay(1000, cancellationToken: ct);
}

// ✅ OnDisableでキャンセル
private void OnDisable()
{
    _cts?.Cancel();
    _cts?.Dispose();
}

// ✅ try-finallyでクリーンアップ
private async UniTaskVoid LoadAsync(CancellationToken ct)
{
    try
    {
        _isLoading = true;
        await LoadDataAsync(ct);
    }
    finally
    {
        _isLoading = false;
    }
}

// ✅ AddTo()でライフサイクル管理
Score.Subscribe(value => Debug.Log(value)).AddTo(this);
```

### ❌ DON'T (非推奨)

```csharp
// ❌ CancellationTokenを渡さない
private async UniTask BadExample()
{
    await UniTask.Delay(1000); // キャンセルできない
}

// ❌ Forgetを使わない
private void Start()
{
    LoadAsync(); // 警告が出る
}

// ❌ using忘れ
using Cysharp.Threading.Tasks; // ← これを忘れない

// ❌ Disposeし忘れ
private void OnDestroy()
{
    // _cts.Dispose(); ← これを忘れない
}
```

---

## 8. パフォーマンス最適化

### オブジェクトプール + UniTask

```csharp
/// <summary>
/// プール済みオブジェクトを非同期で取得
/// </summary>
private async UniTask<GameObject> GetPooledObjectAsync(CancellationToken ct)
{
    // プールから取得を試みる
    while (!_pool.TryGetObject(out var obj))
    {
        // プールが空の場合は待機
        await UniTask.Yield(cancellationToken: ct);
    }

    return obj;
}
```

### バッチ処理

```csharp
/// <summary>
/// 大量のアセットをバッチで読み込み
/// </summary>
private async UniTask LoadBatchAsync(List<string> keys, int batchSize, CancellationToken ct)
{
    for (int i = 0; i < keys.Count; i += batchSize)
    {
        var batch = keys.Skip(i).Take(batchSize);
        var tasks = batch.Select(key => LoadAssetAsync(key, ct));

        await UniTask.WhenAll(tasks);

        // GC負荷軽減のため少し待機
        await UniTask.Yield(cancellationToken: ct);
    }
}
```

---

## 参考リンク

- **UniTask GitHub**: https://github.com/Cysharp/UniTask
- **UniRx GitHub**: https://github.com/neuecc/UniRx
- **プロジェクトテスト**: `Assets/Scripts/Tests/UniTaskUniRxVerificationTest.cs`
- **検証レポート**: `UNITASK_INSTALLATION_VERIFICATION.md`

---

**最終更新**: 2026-03-09
**担当**: Claude Code (Unity Development Specialist)
