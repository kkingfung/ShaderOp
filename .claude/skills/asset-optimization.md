# アセット最適化パターン

## 概要
モバイルゲーム向けアセット最適化のベストプラクティス。

## テクスチャ最適化

```csharp
using UnityEditor;
using UnityEngine;

/// <summary>
/// テクスチャインポート設定を最適化
/// </summary>
public class TextureOptimizer
{
    /// <summary>テクスチャサイズの上限（キャラクター用）</summary>
    private const int MAX_CHARACTER_TEXTURE_SIZE = 2048;

    /// <summary>
    /// テクスチャ設定を検証・最適化
    /// </summary>
    public static bool ValidateTexture(Texture2D texture, out string[] errors)
    {
        var errorList = new List<string>();

        // サイズチェック
        if (texture.width > MAX_CHARACTER_TEXTURE_SIZE || texture.height > MAX_CHARACTER_TEXTURE_SIZE)
        {
            errorList.Add($"サイズ超過: {texture.width}x{texture.height} (上限: {MAX_CHARACTER_TEXTURE_SIZE})");
        }

        // 2のべき乗チェック（POT）
        if (!IsPowerOfTwo(texture.width) || !IsPowerOfTwo(texture.height))
        {
            errorList.Add("テクスチャサイズは2のべき乗である必要があります");
        }

        errors = errorList.ToArray();
        return errorList.Count == 0;
    }

    private static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }
}
```

## メッシュ最適化

```csharp
/// <summary>
/// メッシュのポリゴン数を検証
/// </summary>
public class MeshOptimizer
{
    /// <summary>キャラクターメッシュの推奨ポリゴン数上限</summary>
    private const int MAX_CHARACTER_POLYGONS = 10000;

    public static bool ValidateMesh(Mesh mesh, out string error)
    {
        int triangleCount = mesh.triangles.Length / 3;

        if (triangleCount > MAX_CHARACTER_POLYGONS)
        {
            error = $"ポリゴン数超過: {triangleCount} (推奨上限: {MAX_CHARACTER_POLYGONS})";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
```

## Addressables最適化

```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressablesを使用した効率的なアセット管理
/// </summary>
public class AssetLoader
{
    private Dictionary<string, AsyncOperationHandle> _loadedAssets = new Dictionary<string, AsyncOperationHandle>();

    /// <summary>
    /// アセットを非同期ロード（キャッシュ付き）
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
    {
        // キャッシュ確認
        if (_loadedAssets.TryGetValue(address, out var cachedHandle))
        {
            return cachedHandle.Result as T;
        }

        // ロード
        var handle = Addressables.LoadAssetAsync<T>(address);
        await handle.ToUniTask();

        _loadedAssets[address] = handle;
        return handle.Result;
    }

    /// <summary>
    /// 使用していないアセットを解放
    /// </summary>
    public void ReleaseAsset(string address)
    {
        if (_loadedAssets.TryGetValue(address, out var handle))
        {
            Addressables.Release(handle);
            _loadedAssets.Remove(address);
        }
    }

    /// <summary>
    /// すべてのアセットを解放
    /// </summary>
    public void ReleaseAll()
    {
        foreach (var handle in _loadedAssets.Values)
        {
            Addressables.Release(handle);
        }
        _loadedAssets.Clear();
    }
}
```

## モバイル向けシェーダー最適化

```hlsl
// ✅ Good: モバイル向け軽量シェーダー
Shader "Custom/MobileCharacter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // ✅ モバイル最適化: half精度を使用
            half4 frag(v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return col * _Color;
            }
            ENDHLSL
        }
    }
}
```

## ベストプラクティス

✅ **DO**:
- テクスチャは2のべき乗サイズ（512, 1024, 2048）
- モバイルでは`half`精度を活用（float→half）
- Addressablesでアセット管理・メモリ制御
- 不要なアセットは即座に`Release()`

❌ **DON'T**:
- 巨大テクスチャ（4096以上）の使用
- ポリゴン数無制限のメッシュ
- Resources.Load()の多用（Addressables推奨）
- アセットのメモリリーク

信頼度: 0.93
