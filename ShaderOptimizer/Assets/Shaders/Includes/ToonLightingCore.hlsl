/// <summary>
/// トゥーンライティング計算のコア関数
/// Unity-Chan Toon Shaderの手法を参考にモバイル最適化
/// </summary>

#ifndef TOON_LIGHTING_CORE_INCLUDED
#define TOON_LIGHTING_CORE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

/// <summary>
/// Half-Lambert計算
/// 通常のLambertより柔らかいライティング（範囲 0〜1）
/// </summary>
/// <param name="normal">正規化された法線ベクトル</param>
/// <param name="lightDir">正規化された光源方向ベクトル</param>
/// <returns>Half-Lambert値（0〜1）</returns>
half CalculateHalfLambert(half3 normal, half3 lightDir)
{
    return saturate(dot(normal, lightDir) * 0.5 + 0.5);
}

/// <summary>
/// 2段階トゥーンシェーディング計算（Base → 1st Shade）
/// </summary>
/// <param name="halfLambert">Half-Lambert値</param>
/// <param name="baseStep">Base色の閾値（0〜1）</param>
/// <param name="feather">フェザリング幅（グラデーション幅）</param>
/// <param name="shadowAttenuation">影の減衰値（UnityのShadowマスク）</param>
/// <returns>シャドウマスク（0=完全シャドウ、1=完全ライト）</returns>
half CalculateToonShadowMask(half halfLambert, half baseStep, half feather, half shadowAttenuation)
{
    // トゥーンステップ関数（両側フェザー）
    // feather = 0 でハードエッジ、feather > 0 で境界がぼける
    half toonStep = smoothstep(baseStep - feather, baseStep + feather, halfLambert);

    // URPシャドウ減衰を乗算
    // shadowAttenuation: 0 = 完全な影、1 = 光の中
    half shadowMask = toonStep * shadowAttenuation;

    return shadowMask;
}

/// <summary>
/// 3段階トゥーンシェーディング計算（Base → 1st Shade → 2nd Shade）
/// </summary>
struct ToonShadingResult
{
    half baseMask;      // Base色マスク（1=Base表示）
    half firstMask;     // 1st Shadeマスク（1=1st表示）
    half secondMask;    // 2nd Shadeマスク（1=2nd表示）
};

ToonShadingResult CalculateToonShading3Levels(
    half halfLambert,
    half baseStep,
    half baseFeather,
    half shadeStep,
    half shadeFeather,
    half shadowAttenuation)
{
    ToonShadingResult result;

    // システムシャドウ統合
    half systemShadowLevel = saturate(shadowAttenuation * 0.5 + 0.5);
    half adjustedLambert = halfLambert * systemShadowLevel;

    // Base → 1st Shade境界
    half shadowMask1st = smoothstep(baseStep - baseFeather, baseStep, adjustedLambert);

    // 1st Shade → 2nd Shade境界
    half shadowMask2nd = smoothstep(shadeStep - shadeFeather, shadeStep, adjustedLambert);

    // 各レベルのマスク計算
    result.baseMask = shadowMask1st;
    result.firstMask = (1.0 - shadowMask1st) * shadowMask2nd;
    result.secondMask = 1.0 - shadowMask2nd;

    return result;
}

/// <summary>
/// リムライト計算（フレネル効果）
/// </summary>
/// <param name="normal">正規化された法線ベクトル</param>
/// <param name="viewDir">正規化された視線方向ベクトル</param>
/// <param name="power">リムライトの鋭さ（1〜10推奨）</param>
/// <returns>リムライト強度（0〜1）</returns>
half CalculateRimLight(half3 normal, half3 viewDir, half power)
{
    half rimValue = 1.0 - saturate(dot(normal, viewDir));
    return pow(rimValue, power);
}

/// <summary>
/// アニソトロピックハイライト計算（髪の毛用）
/// Week 4実装予定のプレースホルダー
/// </summary>
half CalculateAnisotropicHighlight(half3 tangent, half3 viewDir, half3 lightDir, half shift, half exponent)
{
    // TODO: Week 4で実装
    return 0.0;
}

#endif // TOON_LIGHTING_CORE_INCLUDED
