# パフォーマンスプロファイリング

## 概要
Unity Profilerを活用したパフォーマンス分析・最適化パターン。

## Profiler基本操作

```csharp
using Unity.Profiling;

/// <summary>
/// カスタムプロファイラーマーカー
/// </summary>
public class CharacterLoader
{
    private static readonly ProfilerMarker s_loadMarker = new ProfilerMarker("CharacterLoader.Load");
    private static readonly ProfilerMarker s_applyMarker = new ProfilerMarker("CharacterLoader.ApplyCustomization");

    public async UniTask LoadCharacterAsync(string characterId)
    {
        using (s_loadMarker.Auto())
        {
            // ロード処理
            await LoadAssetAsync(characterId);
        }
    }

    public void ApplyCustomization(CustomizationData data)
    {
        using (s_applyMarker.Auto())
        {
            // カスタマイズ適用処理
            ApplyMaterials(data);
        }
    }
}
```

## GC Alloc削減

```csharp
// ❌ Bad: Update内でのGC Alloc
void Update()
{
    var enemies = GameObject.FindGameObjectsWithTag("Enemy"); // 毎フレームGC Alloc
    foreach (var enemy in enemies)
    {
        // 処理
    }
}

// ✅ Good: キャッシュして再利用
private List<Enemy> _cachedEnemies = new List<Enemy>();

void Start()
{
    // 初期化時に取得
    _cachedEnemies.AddRange(FindObjectsOfType<Enemy>());
}

void Update()
{
    foreach (var enemy in _cachedEnemies)
    {
        // 処理（GC Allocなし）
    }
}
```

## オブジェクトプール

```csharp
using UnityEngine.Pool;

/// <summary>
/// オブジェクトプールを使用したインスタンス管理
/// </summary>
public class EffectPoolManager
{
    private ObjectPool<ParticleSystem> _effectPool;
    private ParticleSystem _effectPrefab;

    public void Initialize(ParticleSystem prefab, int initialSize = 10)
    {
        _effectPrefab = prefab;

        _effectPool = new ObjectPool<ParticleSystem>(
            createFunc: () => Object.Instantiate(_effectPrefab),
            actionOnGet: effect => effect.gameObject.SetActive(true),
            actionOnRelease: effect => effect.gameObject.SetActive(false),
            actionOnDestroy: effect => Object.Destroy(effect.gameObject),
            defaultCapacity: initialSize,
            maxSize: 50
        );
    }

    /// <summary>
    /// エフェクトを取得（プールから）
    /// </summary>
    public ParticleSystem GetEffect()
    {
        return _effectPool.Get();
    }

    /// <summary>
    /// エフェクトを返却（プールへ）
    /// </summary>
    public void ReleaseEffect(ParticleSystem effect)
    {
        _effectPool.Release(effect);
    }
}
```

## Frame Debuggerによるシェーダー最適化

```csharp
/// <summary>
/// ドローコール削減のためのバッチング確認
/// </summary>
public class BatchingHelper
{
    /// <summary>
    /// 静的バッチングを有効化
    /// </summary>
    [MenuItem("Tools/Enable Static Batching")]
    public static void EnableStaticBatching()
    {
        var staticObjects = GameObject.FindObjectsOfType<MeshRenderer>()
            .Where(mr => mr.gameObject.isStatic)
            .ToArray();

        Debug.Log($"静的オブジェクト数: {staticObjects.Length}");

        // 静的バッチング確認用ログ
        foreach (var mr in staticObjects)
        {
            Debug.Log($"バッチング対象: {mr.gameObject.name}");
        }
    }
}
```

## Deep Profiling注意事項

```csharp
// Deep Profilingは重いため、本番ビルドでは無効化
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class ProfilerSettings
{
    static ProfilerSettings()
    {
        // Editorでのみ有効
        UnityEngine.Profiling.Profiler.enabled = true;
    }
}
#endif
```

## メモリプロファイリング

```csharp
using UnityEngine.Profiling;

/// <summary>
/// メモリ使用状況をログ出力
/// </summary>
public class MemoryMonitor
{
    [MenuItem("Tools/Log Memory Usage")]
    public static void LogMemoryUsage()
    {
        long totalMemory = Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;
        long allocatedMemory = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024;
        long monoHeap = Profiler.GetMonoHeapSizeLong() / 1024 / 1024;
        long monoUsed = Profiler.GetMonoUsedSizeLong() / 1024 / 1024;

        Debug.Log($"=== メモリ使用状況 ===");
        Debug.Log($"総予約メモリ: {totalMemory} MB");
        Debug.Log($"総確保メモリ: {allocatedMemory} MB");
        Debug.Log($"Monoヒープサイズ: {monoHeap} MB");
        Debug.Log($"Mono使用量: {monoUsed} MB");
    }
}
```

## ベストプラクティス

✅ **DO**:
- ProfilerMarkerでカスタム計測
- GC Allocを削減（Update内でのnewを避ける）
- オブジェクトプールを活用
- Frame Debuggerでドローコール確認
- メモリスナップショットで比較分析

❌ **DON'T**:
- 本番ビルドでDeep Profilingを有効化
- プロファイリングなしで最適化（計測してから最適化）
- GC Allocを無視
- ドローコール無制限

信頼度: 0.90
