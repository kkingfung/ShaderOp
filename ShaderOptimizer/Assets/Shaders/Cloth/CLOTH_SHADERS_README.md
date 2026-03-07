# Cloth Shaders - Usage Guide

## Overview

ShaderOpの衣装用シェーダーライブラリ。キャラクターカスタマイズシステムと完全統合。

## Shader List

### 1. ShaderOp/Cloth/FabricSatin
**用途**: シルク、サテン、光沢のある布地

**特徴**:
- 異方性ハイライト（サテン特有の光沢）
- トゥーンシェーディング対応
- リアルタイムカラー変更
- 法線マップ対応（オプション）

**最適な用途**: ドレス、シルクシャツ、光沢のあるスカート

### 2. ShaderOp/Cloth/FabricCotton
**用途**: コットン、デニム、マットな布地

**特徴**:
- ディフューズのみ（スペキュラーなし）
- 織り目テクスチャサポート
- ラフネス調整可能
- リムライト搭載

**最適な用途**: Tシャツ、ジーンズ、カジュアルウェア

---

## Shader Properties Reference

### FabricSatin (ShaderOp/Cloth/FabricSatin)

#### C# Property Names
```csharp
// ベース設定
"_BaseColor"           // Color - 基本色
"_MainTex"             // Texture2D - メインテクスチャ

// サテン設定
"_SatinColor"          // Color - ハイライト色
"_Anisotropy"          // Float (0-1) - 異方性の強さ
"_Glossiness"          // Float (0-1) - 光沢度
"_AnisotropicDirection" // Vector4 - 異方性方向

// トゥーンシェーディング
"_ShadowColor"         // Color - 影色
"_ShadowThreshold"     // Float (0-1) - 影のしきい値
"_ShadowSmoothness"    // Float (0-0.5) - 影の滑らかさ

// オプション
"_UseNormalMap"        // Float (0 or 1) - 法線マップ使用フラグ
"_BumpMap"             // Texture2D - 法線マップ
"_BumpScale"           // Float (0-2) - 法線強度
```

#### 推奨設定値

**シルク（Silk）**
```csharp
_BaseColor = new Color(1f, 0.95f, 0.9f);
_SatinColor = new Color(1f, 1f, 0.95f);
_Anisotropy = 0.8f;
_Glossiness = 0.9f;
_ShadowThreshold = 0.6f;
_ShadowSmoothness = 0.03f;
```

**サテン（Satin）**
```csharp
_BaseColor = new Color(0.8f, 0.2f, 0.3f); // 赤サテン
_SatinColor = new Color(1f, 0.8f, 0.8f);
_Anisotropy = 0.7f;
_Glossiness = 0.85f;
_ShadowThreshold = 0.55f;
_ShadowSmoothness = 0.05f;
```

**レザー（Leather）**
```csharp
_BaseColor = new Color(0.3f, 0.2f, 0.15f);
_SatinColor = new Color(0.8f, 0.7f, 0.6f);
_Anisotropy = 0.4f;
_Glossiness = 0.6f;
_ShadowThreshold = 0.5f;
_ShadowSmoothness = 0.08f;
```

---

### FabricCotton (ShaderOp/Cloth/FabricCotton)

#### C# Property Names
```csharp
// ベース設定
"_BaseColor"           // Color - 基本色
"_MainTex"             // Texture2D - メインテクスチャ

// ファブリック設定
"_FabricTex"           // Texture2D - 織り目テクスチャ
"_FabricStrength"      // Float (0-1) - 織り目の強さ
"_Roughness"           // Float (0-1) - 粗さ

// トゥーンシェーディング
"_ShadowColor"         // Color - 影色
"_ShadowThreshold"     // Float (0-1) - 影のしきい値
"_ShadowSmoothness"    // Float (0-0.5) - 影の滑らかさ

// リムライト
"_RimColor"            // Color - リム色
"_RimPower"            // Float (0-10) - リム強度
"_RimIntensity"        // Float (0-1) - リム明度
```

#### 推奨設定値

**コットン（Cotton）**
```csharp
_BaseColor = new Color(0.9f, 0.9f, 0.95f); // 白コットン
_FabricStrength = 0.3f;
_Roughness = 0.85f;
_ShadowThreshold = 0.5f;
_ShadowSmoothness = 0.08f;
_RimPower = 2.5f;
_RimIntensity = 0.3f;
```

**デニム（Denim）**
```csharp
_BaseColor = new Color(0.2f, 0.3f, 0.5f); // インディゴブルー
_FabricStrength = 0.5f;
_Roughness = 0.9f;
_ShadowThreshold = 0.45f;
_ShadowSmoothness = 0.1f;
_RimPower = 3.0f;
_RimIntensity = 0.2f;
```

**ウール（Wool）**
```csharp
_BaseColor = new Color(0.7f, 0.65f, 0.6f);
_FabricStrength = 0.4f;
_Roughness = 0.95f;
_ShadowThreshold = 0.5f;
_ShadowSmoothness = 0.12f;
_RimPower = 2.0f;
_RimIntensity = 0.25f;
```

---

## C# Integration Examples

### MaterialController経由（推奨）

```csharp
using ShaderOp.Customization;
using UnityEngine;

public class ClothingCustomizer : MonoBehaviour
{
    [SerializeField] private MaterialController _materialController;

    // 衣装色を変更（FabricSatin/FabricCotton両対応）
    public void SetClothingColor(Color color)
    {
        _materialController.SetClothColor(color);
    }

    // サテンのハイライト色を変更
    public void SetSatinHighlight(Color highlightColor)
    {
        Material[] materials = GetComponentsInChildren<Renderer>()
            .SelectMany(r => r.materials)
            .Where(m => m.shader.name == "ShaderOp/Cloth/FabricSatin")
            .ToArray();

        foreach (Material mat in materials)
        {
            mat.SetColor("_SatinColor", highlightColor);
        }
    }

    // 光沢度を変更
    public void SetGlossiness(float glossiness)
    {
        Material[] materials = GetComponentsInChildren<Renderer>()
            .SelectMany(r => r.materials)
            .Where(m => m.shader.name == "ShaderOp/Cloth/FabricSatin")
            .ToArray();

        foreach (Material mat in materials)
        {
            mat.SetFloat("_Glossiness", glossiness);
        }
    }
}
```

### マテリアル直接操作

```csharp
using UnityEngine;

public class DirectMaterialControl : MonoBehaviour
{
    public Material satinMaterial;
    public Material cottonMaterial;

    void Start()
    {
        // サテンマテリアル設定
        if (satinMaterial != null)
        {
            satinMaterial.SetColor("_BaseColor", new Color(0.8f, 0.2f, 0.3f));
            satinMaterial.SetColor("_SatinColor", Color.white);
            satinMaterial.SetFloat("_Anisotropy", 0.7f);
            satinMaterial.SetFloat("_Glossiness", 0.85f);
        }

        // コットンマテリアル設定
        if (cottonMaterial != null)
        {
            cottonMaterial.SetColor("_BaseColor", new Color(0.2f, 0.3f, 0.5f));
            cottonMaterial.SetFloat("_FabricStrength", 0.5f);
            cottonMaterial.SetFloat("_Roughness", 0.9f);
        }
    }
}
```

---

## Performance Specs

### FabricSatin
- **命令数**: 約22-25命令/pass
- **テクスチャサンプリング**: 1-2回（法線マップ使用時）
- **レンダリングパス**: 2 passes (ForwardLit + ShadowCaster)
- **ターゲットFPS**: 60fps @ 10キャラクター

### FabricCotton
- **命令数**: 約18-20命令/pass
- **テクスチャサンプリング**: 2回
- **レンダリングパス**: 2 passes (ForwardLit + ShadowCaster)
- **ターゲットFPS**: 60fps @ 15キャラクター

---

## Material Creation

### Unityエディター内で作成

1. **Project** ウィンドウで右クリック
2. **Create > Material**
3. Shaderドロップダウンから選択:
   - `ShaderOp/Cloth/FabricSatin`
   - `ShaderOp/Cloth/FabricCotton`

### コードから作成

```csharp
using UnityEngine;

public class MaterialFactory
{
    public static Material CreateSatinMaterial(Color baseColor)
    {
        Shader shader = Shader.Find("ShaderOp/Cloth/FabricSatin");
        Material mat = new Material(shader);
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_SatinColor", Color.white);
        mat.SetFloat("_Anisotropy", 0.7f);
        mat.SetFloat("_Glossiness", 0.8f);
        return mat;
    }

    public static Material CreateCottonMaterial(Color baseColor)
    {
        Shader shader = Shader.Find("ShaderOp/Cloth/FabricCotton");
        Material mat = new Material(shader);
        mat.SetColor("_BaseColor", baseColor);
        mat.SetFloat("_FabricStrength", 0.3f);
        mat.SetFloat("_Roughness", 0.85f);
        return mat;
    }
}
```

---

## Optimization Tips

### モバイル最適化
1. **法線マップは必要な時のみ使用**
   - `_UseNormalMap = 0` でオフ
   - パフォーマンス向上: 約15%

2. **ファブリックテクスチャは小さく**
   - 推奨サイズ: 256x256 以下
   - タイリング設定で対応

3. **SRP Batcherを有効化**
   - Edit > Project Settings > Graphics > SRP Batcher

### 品質設定
- **Low**: 法線マップオフ、ファブリックテクスチャオフ
- **Medium**: 法線マップオン、ファブリックテクスチャ低解像度
- **High**: すべて有効

---

## Troubleshooting

### ハイライトが表示されない（FabricSatin）
- `_Anisotropy` が 0 になっていないか確認
- `_Glossiness` を 0.5 以上に設定
- ライトの角度を調整

### 織り目が見えない（FabricCotton）
- `_FabricStrength` を 0.3-0.6 に設定
- `_FabricTex` にテクスチャを割り当て
- UV tiling を 10-20 に設定

### 影が濃すぎる/薄すぎる
- `_ShadowThreshold` を 0.4-0.6 の範囲で調整
- `_ShadowSmoothness` で境界をぼかす

---

## Version

**作成日**: 2026-02-28
**バージョン**: 1.0.0
**Unity**: 6000.3.9f1
**URP**: 17.3.0

---

**Happy Customizing!** ✨
