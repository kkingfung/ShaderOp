# UniTask 統合ガイド

**対象読者**: Unity開発者、ShaderOpプロジェクトメンバー
**最終更新**: 2026-03-09
**Phase**: Phase 4 Week 1

---

## 目次

1. [UniTaskとは](#unitaskとは)
2. [インストール検証](#インストール検証)
3. [async/await基本パターン](#asyncawait基本パターン)
4. [SceneLoader非同期変換（Week 2）](#sceneloader非同期変換week-2)
5. [AssetLoader非同期変換（Week 2）](#assetloader非同期変換week-2)
6. [UniTask + UniRx統合パターン](#unitask--unirx統合パターン)
7. [CancellationToken使用法](#cancellationtoken使用法)
8. [エラーハンドリング](#エラーハンドリング)
9. [よくある間違い](#よくある間違い)
10. [パフォーマンス最適化](#パフォーマンス最適化)

---

## UniTaskとは

### 概要

**UniTask**は、Unity向けに最適化された非同期処理ライブラリです。C#標準の`Task`と比較して、**GC Allocation ゼロ**を実現します。

**公式リポジトリ**: https://github.com/Cysharp/UniTask

### C# Task vs UniTask比較

| 特徴 | C# Task | UniTask |
|------|---------|---------|
| **GC Allocation** | あり（boxing） | ゼロ |
| **パフォーマンス** | 汎用的 | Unity最適化 |
| **Unity統合** | 限定的 | 完全統合 |
| **メインスレッド** | SynchronizationContext | PlayerLoopSystem |
| **Coroutine変換** | 不可 | 可能 |

### インストール済み確認

```json
// Packages/manifest.json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.neuecc.unirx": "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts"
  }
}
```

**確認コマンド**:
```csharp
using Cysharp.Threading.Tasks;

// コンパイルが通ればOK
public async UniTask TestAsync()
{
    await UniTask.Delay(100);
}
```

---

## インストール検証

### 検証テストファイル

**ファイルパス**: `ShaderOptimizer/Assets/Scripts/Tests/UniTaskUniRxVerificationTest.cs`

**実装済みテスト**（9件）:

1. **UniTask_IsImportedCorrectly**: UniTask型が正しくインポートされているか
2. **UniRx_IsImportedCorrectly**: UniRx型が正しくインポートされているか
3. **UniTask_DelayWorks**: `UniTask.Delay()` 動作確認（100ms待機）
4. **UniTask_YieldWorks**: `UniTask.Yield()` 動作確認（3フレーム待機）
5. **UniRx_ReactivePropertyWorks**: `ReactiveProperty<T>` 値変更通知
6. **UniTask_WhenAllWorks**: 複数タスク並列待機
7. **UniTask_WhenAnyWorks**: 最速タスク判定
8. **UniTask_CancellationWorks**: CancellationToken機能
9. **UniRx_ReactivePropertyDisposeWorks**: Dispose機能

### 検証実行

```bash
# Unity Test Runnerで実行
Window → General → Test Runner → PlayMode → Run All

# コマンドライン実行
Unity -runTests -testPlatform PlayMode -testFilter "ShaderOp.Tests.UniTaskUniRxVerificationTest"
```

**期待される出力**:
```
✅ All tests passed (9/9)
[Test] UniTask型が正常にインポートされました
[Test] UniTask.Delayが正常に動作しました: 102.3ms
[Test] ReactivePropertyが正常に動作しました: 42
```

---

## async/await基本パターン

### 1. 基本的な遅延実行

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DelayExample : MonoBehaviour
{
    /// <summary>
    /// 2秒後にログ出力
    /// </summary>
    private async void Start()
    {
        Debug.Log("開始");

        // 2秒待機（GC Allocation: 0）
        await UniTask.Delay(2000);

        Debug.Log("2秒経過");
    }
}
```

**出力**:
```
開始
（2秒待機）
2秒経過
```

### 2. フレーム待機

```csharp
/// <summary>
/// 次のフレームまで待機
/// </summary>
private async UniTask WaitNextFrame()
{
    await UniTask.Yield();
    Debug.Log("次のフレーム");
}

/// <summary>
/// 10フレーム待機
/// </summary>
private async UniTask Wait10Frames()
{
    for (int i = 0; i < 10; i++)
    {
        await UniTask.Yield();
    }
    Debug.Log("10フレーム経過");
}
```

### 3. Unity APIの非同期変換

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Animatorアニメーション完了待機
/// </summary>
private async UniTask WaitForAnimation(Animator animator, string stateName)
{
    // ステート遷移待機
    await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

    // アニメーション完了待機
    await UniTask.WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);

    Debug.Log($"アニメーション完了: {stateName}");
}

/// <summary>
/// AudioSource再生完了待機
/// </summary>
private async UniTask WaitForAudio(AudioSource audioSource)
{
    audioSource.Play();

    // 再生時間待機
    await UniTask.Delay((int)(audioSource.clip.length * 1000));

    Debug.Log("オーディオ再生完了");
}
```

### 4. Coroutineからの変換

```csharp
// Before: Coroutine
IEnumerator LoadDataCoroutine()
{
    yield return new WaitForSeconds(2f);
    Debug.Log("データロード完了");
}

// After: UniTask
async UniTask LoadDataAsync()
{
    await UniTask.Delay(2000);
    Debug.Log("データロード完了");
}

// 使用例
private async void Start()
{
    await LoadDataAsync(); // await可能
}
```

---

## SceneLoader非同期変換（Week 2）

### Before: 同期ロード

```csharp
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// シーンを同期ロード（フレームドロップの原因）
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName); // ← ブロッキング
        Debug.Log($"Scene loaded: {sceneName}");
    }
}
```

**問題点**:
- ロード中にフレームドロップ（Unity固まる）
- 進捗表示不可
- キャンセル不可

### After: UniTask非同期ロード

```csharp
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// シーンを非同期ロード
    /// </summary>
    /// <param name="sceneName">シーン名</param>
    /// <param name="progress">進捗コールバック（0.0-1.0）</param>
    /// <param name="ct">キャンセルトークン</param>
    public async UniTask LoadSceneAsync(
        string sceneName,
        IProgress<float>? progress = null,
        CancellationToken ct = default)
    {
        Debug.Log($"[SceneLoader] Loading scene: {sceneName}");

        // AsyncOperation取得
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"[SceneLoader] Failed to load scene: {sceneName}");
            return;
        }

        // ロード完了まで待機（進捗付き）
        while (!operation.isDone)
        {
            // 進捗通知（0.0-0.9）
            progress?.Report(operation.progress);

            // 次のフレームまで待機
            await UniTask.Yield(ct);
        }

        // 完了通知
        progress?.Report(1.0f);

        Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
    }
}
```

### 使用例: 進捗表示

```csharp
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Slider? _progressBar;
    [SerializeField] private Text? _progressText;
    private SceneLoader? _sceneLoader;

    private async void Start()
    {
        _sceneLoader = FindObjectOfType<SceneLoader>();

        // 進捗コールバック作成
        var progress = new Progress<float>(value =>
        {
            // プログレスバー更新
            if (_progressBar != null)
            {
                _progressBar.value = value;
            }

            // テキスト更新
            if (_progressText != null)
            {
                _progressText.text = $"Loading... {value * 100:F0}%";
            }

            Debug.Log($"Progress: {value * 100:F1}%");
        });

        // シーン非同期ロード
        await _sceneLoader.LoadSceneAsync("MainGame", progress);

        Debug.Log("ロード完了！");
    }
}
```

**出力**:
```
[SceneLoader] Loading scene: MainGame
Progress: 0.0%
Progress: 15.3%
Progress: 42.7%
Progress: 78.9%
Progress: 90.0%
Progress: 100.0%
[SceneLoader] Scene loaded: MainGame
ロード完了！
```

### フェードイン/アウト統合

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image? _fadeImage;
    [SerializeField] private SceneLoader? _sceneLoader;

    /// <summary>
    /// フェードアウト → シーンロード → フェードイン
    /// </summary>
    public async UniTask TransitionToSceneAsync(string sceneName, CancellationToken ct = default)
    {
        // フェードアウト（0.5秒）
        await FadeOutAsync(0.5f, ct);

        // シーンロード（進捗表示）
        var progress = new Progress<float>(value =>
        {
            Debug.Log($"Loading: {value * 100:F0}%");
        });
        await _sceneLoader.LoadSceneAsync(sceneName, progress, ct);

        // フェードイン（0.5秒）
        await FadeInAsync(0.5f, ct);
    }

    /// <summary>
    /// フェードアウト（透明 → 黒）
    /// </summary>
    private async UniTask FadeOutAsync(float duration, CancellationToken ct)
    {
        if (_fadeImage == null) return;

        float elapsed = 0f;
        Color color = _fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            color.a = alpha;
            _fadeImage.color = color;

            await UniTask.Yield(ct);
        }

        color.a = 1f;
        _fadeImage.color = color;
    }

    /// <summary>
    /// フェードイン（黒 → 透明）
    /// </summary>
    private async UniTask FadeInAsync(float duration, CancellationToken ct)
    {
        if (_fadeImage == null) return;

        float elapsed = 0f;
        Color color = _fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            color.a = alpha;
            _fadeImage.color = color;

            await UniTask.Yield(ct);
        }

        color.a = 0f;
        _fadeImage.color = color;
    }
}
```

---

## AssetLoader非同期変換（Week 2）

### Before: Addressables同期ロード

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AssetLoader : MonoBehaviour
{
    /// <summary>
    /// マテリアルを同期ロード（非推奨）
    /// </summary>
    public Material LoadMaterial(string key)
    {
        var handle = Addressables.LoadAssetAsync<Material>(key);
        return handle.WaitForCompletion(); // ← ブロッキング
    }
}
```

### After: UniTask非同期ロード

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetLoader : MonoBehaviour
{
    /// <summary>
    /// マテリアルを非同期ロード
    /// </summary>
    public async UniTask<Material> LoadMaterialAsync(string key, CancellationToken ct = default)
    {
        Debug.Log($"[AssetLoader] Loading material: {key}");

        // Addressablesで非同期ロード
        AsyncOperationHandle<Material> handle = Addressables.LoadAssetAsync<Material>(key);

        // UniTaskに変換して待機
        Material material = await handle.ToUniTask(cancellationToken: ct);

        Debug.Log($"[AssetLoader] Material loaded: {material.name}");
        return material;
    }

    /// <summary>
    /// 複数のマテリアルを並列ロード
    /// </summary>
    public async UniTask<Material[]> LoadMaterialsAsync(string[] keys, CancellationToken ct = default)
    {
        Debug.Log($"[AssetLoader] Loading {keys.Length} materials in parallel");

        // 並列ロード
        UniTask<Material>[] tasks = new UniTask<Material>[keys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            tasks[i] = LoadMaterialAsync(keys[i], ct);
        }

        // すべて完了を待機
        Material[] materials = await UniTask.WhenAll(tasks);

        Debug.Log($"[AssetLoader] All {materials.Length} materials loaded");
        return materials;
    }

    /// <summary>
    /// Prefabを非同期ロード & インスタンス化
    /// </summary>
    public async UniTask<GameObject> InstantiatePrefabAsync(string key, Transform parent, CancellationToken ct = default)
    {
        Debug.Log($"[AssetLoader] Instantiating prefab: {key}");

        // Prefabロード
        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        GameObject prefab = await handle.ToUniTask(cancellationToken: ct);

        // インスタンス化
        GameObject instance = Instantiate(prefab, parent);

        Debug.Log($"[AssetLoader] Prefab instantiated: {instance.name}");
        return instance;
    }
}
```

### 使用例: Cloth Material動的ロード

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField] private Renderer? _clothRenderer;
    private AssetLoader? _assetLoader;
    private CancellationTokenSource? _cts;

    private async void Start()
    {
        _assetLoader = FindObjectOfType<AssetLoader>();
        _cts = new CancellationTokenSource();

        // Cloth Materialロード
        await LoadClothMaterialAsync("materials/cloth_satin", _cts.Token);
    }

    /// <summary>
    /// Cloth Materialをロードして適用
    /// </summary>
    private async UniTask LoadClothMaterialAsync(string key, CancellationToken ct)
    {
        try
        {
            // マテリアルロード
            Material material = await _assetLoader.LoadMaterialAsync(key, ct);

            // Rendererに適用
            if (_clothRenderer != null)
            {
                _clothRenderer.material = material;
                Debug.Log($"Cloth material applied: {material.name}");
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Material loading cancelled");
        }
    }

    private void OnDestroy()
    {
        // キャンセル
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

---

## UniTask + UniRx統合パターン

### ReactiveProperty + UniTask

```csharp
using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public class ScoreManager : MonoBehaviour, IDisposable
{
    /// <summary>現在のスコア（リアクティブ）</summary>
    public ReactiveProperty<int> Score { get; } = new ReactiveProperty<int>(0);

    private CompositeDisposable _disposables = new CompositeDisposable();

    private void Start()
    {
        // スコア変更を監視
        Score
            .Subscribe(value => Debug.Log($"スコア: {value}"))
            .AddTo(_disposables);

        // スコア加算タスク開始
        IncrementScoreAsync().Forget();
    }

    /// <summary>
    /// 1秒ごとにスコア加算
    /// </summary>
    private async UniTaskVoid IncrementScoreAsync()
    {
        while (true)
        {
            await UniTask.Delay(1000);
            Score.Value += 10;

            if (Score.Value >= 100)
            {
                Debug.Log("スコア100到達！");
                break;
            }
        }
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        Score?.Dispose();
    }

    private void OnDestroy()
    {
        Dispose();
    }
}
```

**出力**:
```
スコア: 0
スコア: 10
スコア: 20
...
スコア: 100
スコア100到達！
```

### Observable → UniTask変換

```csharp
using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

/// <summary>
/// ボタンクリックを待機
/// </summary>
private async UniTask WaitForButtonClickAsync()
{
    var button = GetComponent<UnityEngine.UI.Button>();

    // Observableを最初の1回だけUniTaskに変換
    await button.OnClickAsObservable().First().ToUniTask();

    Debug.Log("ボタンがクリックされました");
}

/// <summary>
/// 特定条件を満たすまで待機
/// </summary>
private async UniTask WaitForConditionAsync(IObservable<int> source, int threshold)
{
    // 条件を満たす最初の値を待機
    int result = await source
        .Where(value => value >= threshold)
        .First()
        .ToUniTask();

    Debug.Log($"条件達成: {result} >= {threshold}");
}
```

---

## CancellationToken使用法

### 基本的なキャンセル

```csharp
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CancellableTask : MonoBehaviour
{
    private CancellationTokenSource? _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();

        // キャンセル可能なタスク開始
        LongRunningTaskAsync(_cts.Token).Forget();
    }

    /// <summary>
    /// 長時間実行タスク（キャンセル可能）
    /// </summary>
    private async UniTaskVoid LongRunningTaskAsync(CancellationToken ct)
    {
        try
        {
            for (int i = 0; i < 100; i++)
            {
                // キャンセルチェック
                ct.ThrowIfCancellationRequested();

                Debug.Log($"Progress: {i + 1}/100");
                await UniTask.Delay(100, cancellationToken: ct);
            }

            Debug.Log("タスク完了");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("タスクがキャンセルされました");
        }
    }

    /// <summary>
    /// タスクをキャンセル
    /// </summary>
    public void CancelTask()
    {
        _cts?.Cancel();
    }

    private void OnDestroy()
    {
        // OnDestroyでキャンセル（重要！）
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

### GameObject破棄時の自動キャンセル

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AutoCancelExample : MonoBehaviour
{
    private async void Start()
    {
        // このGameObjectが破棄されたら自動キャンセル
        await LongTaskAsync(this.GetCancellationTokenOnDestroy());
    }

    private async UniTask LongTaskAsync(CancellationToken ct)
    {
        try
        {
            await UniTask.Delay(5000, cancellationToken: ct);
            Debug.Log("5秒経過");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("GameObject破棄によりキャンセル");
        }
    }
}
```

### タイムアウト付きタスク

```csharp
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// 3秒でタイムアウト
/// </summary>
private async UniTask<bool> LoadWithTimeoutAsync(CancellationToken ct)
{
    try
    {
        // 3秒タイムアウトのCancellationTokenSource作成
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        // ロード処理
        await LoadDataAsync(linkedCts.Token);

        Debug.Log("ロード成功");
        return true;
    }
    catch (OperationCanceledException)
    {
        Debug.LogWarning("ロードタイムアウト or キャンセル");
        return false;
    }
}
```

---

## エラーハンドリング

### try-catch パターン

```csharp
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// エラーハンドリング付き非同期処理
/// </summary>
private async UniTaskVoid LoadDataWithErrorHandlingAsync()
{
    try
    {
        // データロード
        var data = await LoadDataAsync();

        // 成功時の処理
        Debug.Log($"Data loaded: {data}");
    }
    catch (OperationCanceledException)
    {
        // キャンセル時の処理
        Debug.LogWarning("Data loading was cancelled");
    }
    catch (Exception ex)
    {
        // エラー時の処理
        Debug.LogError($"Failed to load data: {ex.Message}");
    }
}
```

### AttachExternalCancellation

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 外部CancellationTokenを統合
/// </summary>
private async UniTask LoadWithExternalCancellationAsync(CancellationToken externalCt)
{
    // このGameObjectのDestroyと外部CTの両方でキャンセル
    var ct = this.GetCancellationTokenOnDestroy();
    await LoadDataAsync()
        .AttachExternalCancellation(externalCt)
        .AttachExternalCancellation(ct);
}
```

---

## よくある間違い

### ❌ 間違い1: async voidの乱用

```csharp
// ❌ Bad: 例外がキャッチできない
private async void LoadDataBad()
{
    await UniTask.Delay(1000);
    throw new Exception("Error!"); // アプリクラッシュ！
}

// ✅ Good: UniTaskVoid使用
private async UniTaskVoid LoadDataGood()
{
    try
    {
        await UniTask.Delay(1000);
        throw new Exception("Error!");
    }
    catch (Exception ex)
    {
        Debug.LogError(ex); // 正常にキャッチ
    }
}
```

### ❌ 間違い2: Forgetの忘れ

```csharp
// ❌ Bad: 警告が出る
private void Start()
{
    LongTaskAsync(); // CS4014: この呼び出しを待機していないため...
}

// ✅ Good: Forget()呼び出し
private void Start()
{
    LongTaskAsync().Forget();
}

private async UniTaskVoid LongTaskAsync()
{
    await UniTask.Delay(1000);
}
```

### ❌ 間違い3: CancellationTokenの破棄忘れ

```csharp
// ❌ Bad: メモリリーク
public class BadExample : MonoBehaviour
{
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private void OnDestroy()
    {
        // Disposeしていない → リーク
    }
}

// ✅ Good: 確実にDispose
public class GoodExample : MonoBehaviour
{
    private CancellationTokenSource? _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

### ❌ 間違い4: デッドロック

```csharp
// ❌ Bad: デッドロック
private void Start()
{
    var result = LoadDataAsync().GetAwaiter().GetResult(); // 固まる
}

// ✅ Good: awaitまたはForget
private async void Start()
{
    var result = await LoadDataAsync(); // OK
}
```

---

## パフォーマンス最適化

### 1. UniTask.Yieldの活用

```csharp
// ❌ Bad: WaitForSeconds使用（GC Allocation）
IEnumerator WaitCoroutine()
{
    yield return new WaitForSeconds(1f); // 毎回newでGC
}

// ✅ Good: UniTask.Delay使用（GC Allocation: 0）
async UniTask WaitAsync()
{
    await UniTask.Delay(1000); // GC: 0
}
```

### 2. PlayerLoopTimingの最適化

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

// Update相当の処理
await UniTask.Yield(PlayerLoopTiming.Update);

// LateUpdate相当の処理
await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

// FixedUpdate相当の処理
await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
```

### 3. WhenAllで並列化

```csharp
// ❌ Bad: 順次実行（合計6秒）
await Task1Async(); // 2秒
await Task2Async(); // 2秒
await Task3Async(); // 2秒

// ✅ Good: 並列実行（合計2秒）
await UniTask.WhenAll(
    Task1Async(),
    Task2Async(),
    Task3Async()
);
```

### 4. ValueTaskの活用

```csharp
// ✅ Good: 値型タスク（ヒープ割り当てなし）
public async UniTask<int> GetScoreAsync()
{
    await UniTask.Delay(100);
    return 42; // ボクシングなし
}

// 使用例
int score = await GetScoreAsync(); // GC: 0
```

---

## まとめ

### UniTask導入効果

| 項目 | 効果 |
|------|------|
| **GC Allocation** | C# Taskの100%削減 |
| **パフォーマンス** | Unity最適化されたスケジューリング |
| **統合性** | Addressables, DOTween等と完全統合 |
| **開発効率** | async/await による可読性向上 |

### Week 2実装予定

1. **SceneLoader非同期化**: フェード + 進捗表示
2. **AssetLoader非同期化**: Addressables統合
3. **HexGrid非同期生成**: 121タイル分割生成

### 参考リソース

- **UniTask公式**: https://github.com/Cysharp/UniTask
- **UniTask日本語記事**: https://qiita.com/toRisouP/items/8f66fd952eaffeead3c2
- **検証テスト**: `ShaderOptimizer/Assets/Scripts/Tests/UniTaskUniRxVerificationTest.cs`
- **Week 1レポート**: `PHASE4_WEEK1_SUMMARY.md`

---

**作成者**: doc-writer (Claude Code)
**作成日**: 2026-03-09
**Phase**: Phase 4 Week 1
**関連ドキュメント**:
- `PHASE4_WEEK1_SUMMARY.md`
- `OBJECT_POOLING_GUIDE.md`
- `PERFORMANCE_BASELINE.md`
