# ToonLightingCore.hlsl 使用ガイド

## 概要
モバイル最適化されたトゥーンシェーディング用のコア関数ライブラリ。
URP対応で、キャラクター・環境・布などあらゆるトゥーン調シェーダーで使用可能。

## インクルード方法
```hlsl
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Includes/ToonLightingCore.hlsl"
```

## 提供される関数

### 1. CalculateHalfLambert
**用途**: 柔らかいライティング計算（Half-Lambert）

**シグネチャ**:
```hlsl
half CalculateHalfLambert(half3 normal, half3 lightDir)
```

**パラメータ**:
- `normal`: 正規化されたワールド空間法線
- `lightDir`: 正規化された光源方向

**戻り値**: 0-1の範囲のライティング値

**使用例**:
```hlsl
half3 normalWS = normalize(input.normalWS);
half3 lightDir = normalize(mainLight.direction);
half halfLambert = CalculateHalfLambert(normalWS, lightDir);
```

---

### 2. CalculateToonShadowMask
**用途**: トゥーン調のシャドウマスク生成（2段階: Base ⇔ Shade）

**シグネチャ**:
```hlsl
half CalculateToonShadowMask(
    half halfLambert,
    half baseStep,
    half feather,
    half shadowAttenuation
)
```

**パラメータ**:
- `halfLambert`: Half-Lambert値（0-1）
- `baseStep`: 影の境界線閾値（0-1、推奨: 0.5）
- `feather`: 境界のぼかし量（0-0.5、推奨: 0.05）
- `shadowAttenuation`: URPシャドウ減衰（`mainLight.shadowAttenuation`）

**戻り値**: 0=完全な影、1=完全な光

**使用例**:
```hlsl
half shadowMask = CalculateToonShadowMask(
    halfLambert,
    _BaseColorStep,        // プロパティ: 0.5
    _BaseShadeFeather,     // プロパティ: 0.05
    mainLight.shadowAttenuation
);

// シェードカラーとベースカラーをブレンド
half3 shadeColor = baseColor * _ShadeColor.rgb;
half3 finalColor = lerp(shadeColor, baseColor, shadowMask);
```

**推奨パラメータ**:
- **ハードエッジ**: `baseStep=0.5`, `feather=0.0`
- **柔らかい**: `baseStep=0.5`, `feather=0.1`
- **アニメ風**: `baseStep=0.6`, `feather=0.02`

---

### 3. CalculateRimLight
**用途**: フレネル効果によるリムライト（エッジ光）

**シグネチャ**:
```hlsl
half CalculateRimLight(half3 normal, half3 viewDir, half power)
```

**パラメータ**:
- `normal`: 正規化されたワールド空間法線
- `viewDir`: 正規化された視線方向
- `power`: リムライトの鋭さ（0.5-8.0推奨）

**戻り値**: 0-1のリムライト強度

**使用例**:
```hlsl
half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
half rimLight = CalculateRimLight(normalWS, viewDir, _RimPower);

// リムライトを加算
half3 rimContribution = _RimColor.rgb * rimLight * _RimIntensity;
finalColor += rimContribution;
```

**推奨パラメータ**:
- **細く鋭い**: `power=5.0`
- **広く柔らかい**: `power=2.0`
- **アニメ風**: `power=4.0`

---

### 4. CalculateToonShading3Levels
**用途**: 3段階トゥーンシェーディング（Base → 1st Shade → 2nd Shade）

**シグネチャ**:
```hlsl
ToonShadingResult CalculateToonShading3Levels(
    half halfLambert,
    half baseStep,
    half baseFeather,
    half shadeStep,
    half shadeFeather,
    half shadowAttenuation
)
```

**パラメータ**:
- `halfLambert`: Half-Lambert値
- `baseStep`: Base → 1st Shade境界（推奨: 0.6）
- `baseFeather`: Base境界のぼかし（推奨: 0.05）
- `shadeStep`: 1st → 2nd Shade境界（推奨: 0.3）
- `shadeFeather`: Shade境界のぼかし（推奨: 0.05）
- `shadowAttenuation`: URPシャドウ減衰

**戻り値**: `ToonShadingResult` 構造体
```hlsl
struct ToonShadingResult
{
    half baseMask;      // Base色マスク
    half firstMask;     // 1st Shadeマスク
    half secondMask;    // 2nd Shadeマスク
};
```

**使用例**:
```hlsl
ToonShadingResult shadingResult = CalculateToonShading3Levels(
    halfLambert,
    _BaseColorStep,     // 0.6
    _BaseShadeFeather,  // 0.05
    _ShadeColorStep,    // 0.3
    _ShadeShadeFeather, // 0.05
    shadowAttenuation
);

// 3段階の色をブレンド
half3 baseColor = albedo;
half3 firstShade = albedo * _1stShadeColor.rgb;
half3 secondShade = albedo * _2ndShadeColor.rgb;

half3 finalColor =
    baseColor * shadingResult.baseMask +
    firstShade * shadingResult.firstMask +
    secondShade * shadingResult.secondMask;
```

---

### 5. CalculateAnisotropicHighlight (未実装)
**用途**: 髪の毛用アニソトロピックハイライト

**ステータス**: Week 4実装予定（現在は0を返すプレースホルダー）

---

## 完全な使用例

### シンプルなトゥーンシェーダー
```hlsl
half4 frag(Varyings input) : SV_Target
{
    // ライトとベクトル取得
    Light mainLight = GetMainLight(input.shadowCoord);
    half3 normalWS = normalize(input.normalWS);
    half3 lightDir = normalize(mainLight.direction);
    half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

    // テクスチャサンプリング
    half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

    // Half-Lambert計算
    half halfLambert = CalculateHalfLambert(normalWS, lightDir);

    // トゥーンシャドウマスク
    half shadowMask = CalculateToonShadowMask(
        halfLambert,
        _BaseColorStep,
        _BaseShadeFeather,
        mainLight.shadowAttenuation
    );

    // シェーディング
    half3 shadeColor = albedo.rgb * _ShadeColor.rgb;
    half3 litColor = albedo.rgb * mainLight.color;
    half3 finalColor = lerp(shadeColor, litColor, shadowMask);

    // リムライト
    #if defined(_RIMLIGHT_ON)
        half rimLight = CalculateRimLight(normalWS, viewDir, _RimPower);
        finalColor += _RimColor.rgb * rimLight * _RimIntensity;
    #endif

    return half4(finalColor, albedo.a);
}
```

---

## パフォーマンス

### モバイル最適化
- ✅ テクスチャサンプリング: 0回（計算のみ）
- ✅ 分岐なし（shader feature で制御）
- ✅ ALU操作: 最小限
  - `CalculateHalfLambert`: 1 dot, 2 mad
  - `CalculateToonShadowMask`: 1 smoothstep, 1 mul
  - `CalculateRimLight`: 1 dot, 1 pow

### 推奨設定
- ターゲット: `#pragma target 3.0`
- リムライト: shader feature で切り替え
- 追加ライト: モバイルでは無効化推奨

---

## プロパティ定義例

```hlsl
Properties
{
    [Header(Shading)]
    _ShadeColor("Shade Color Tint", Color) = (0.7, 0.7, 0.7, 1)
    _BaseColorStep("Base Color Step", Range(0, 1)) = 0.5
    _BaseShadeFeather("Base Shade Feather", Range(0, 0.5)) = 0.05

    [Header(Rim Light)]
    [Toggle(_RIMLIGHT_ON)] _UseRimLight("Use Rim Light", Float) = 1
    _RimColor("Rim Color", Color) = (1, 1, 1, 1)
    _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
    _RimIntensity("Rim Intensity", Range(0, 1)) = 0.3
}

CBUFFER_START(UnityPerMaterial)
    half4 _ShadeColor;
    half _BaseColorStep;
    half _BaseShadeFeather;

    half4 _RimColor;
    half _RimPower;
    half _RimIntensity;
CBUFFER_END
```

---

## トラブルシューティング

### 影が薄すぎる
- `_BaseColorStep` を上げる（0.5 → 0.6）
- `_ShadeColor` を暗くする

### 影が濃すぎる
- `_BaseColorStep` を下げる（0.5 → 0.4）
- `_ShadeColor` を明るくする

### 境界がガタガタ
- `_BaseShadeFeather` を増やす（0.0 → 0.05）

### リムライトが見えない
- `_RimPower` を下げる（5.0 → 3.0）
- `_RimIntensity` を上げる（0.3 → 0.5）

---

## 参考
- Unity-Chan Toon Shader: https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
- URP Shader Library: `Packages/com.unity.render-pipelines.universal/ShaderLibrary/`

---

最終更新: 2026-03-01
