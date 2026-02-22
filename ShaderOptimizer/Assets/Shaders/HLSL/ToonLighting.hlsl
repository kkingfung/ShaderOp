//=============================================================================
// ToonLighting.hlsl
// トゥーンライティング計算用HLSL Custom Functions
//
// プロジェクト: ShaderOp - Unified Stylized Shader Library
// 作成日: 2026-02-22
// バージョン: 0.1.0 (スタブ実装)
//
// 参考実装: Unity-Chan Toon Shader 2.0.6
// - UCTS_DoubleShadeWithFeather.cginc の Half-Lambert + Step + Feather
//=============================================================================

#ifndef TOON_LIGHTING_INCLUDED
#define TOON_LIGHTING_INCLUDED

//-----------------------------------------------------------------------------
// 基本Half-Lambert計算
// 用途: セルシェーディングの基礎ライティング計算
// 範囲: [0, 1]
//-----------------------------------------------------------------------------
/// <summary>
/// Half-Lambert計算（法線とライト方向から0-1の値を返す）
/// </summary>
/// <param name="Normal">ワールド空間法線（正規化済み）</param>
/// <param name="LightDirection">ワールド空間ライト方向（正規化済み）</param>
/// <returns>Half-Lambert値 (0-1範囲)</returns>
void CalculateHalfLambert_float(
    float3 Normal,
    float3 LightDirection,
    out float HalfLambert
)
{
    // Half-Lambert: 0.5 * dot(N, L) + 0.5
    // 通常のLambertは dot(N, L) で範囲[-1, 1]
    // Half-Lambertは範囲を[0, 1]にシフト
    HalfLambert = 0.5 * dot(Normal, LightDirection) + 0.5;
}

//-----------------------------------------------------------------------------
// 2段階トゥーンシェーディング（Base → 1st Shade）
// 用途: セルシェーディングのシャドウ境界計算（Step + Feather）
//-----------------------------------------------------------------------------
/// <summary>
/// トゥーンシェーディングマスク計算（Step + Feather対応）
/// </summary>
/// <param name="HalfLambert">Half-Lambert値 (0-1)</param>
/// <param name="ShadowStep">シャドウ境界の閾値 (0-1, default: 0.5)</param>
/// <param name="ShadowFeather">境界のぼかし幅 (0-1, default: 0.05)</param>
/// <returns>シャドウマスク (0: フル影, 1: ライト領域)</returns>
void CalculateToonShadowMask_float(
    float HalfLambert,
    float ShadowStep,
    float ShadowFeather,
    out float ShadowMask
)
{
    // smoothstep でStep + Feather を実現
    // min = ShadowStep - ShadowFeather (影側境界)
    // max = ShadowStep                (ライト側境界)
    // result: min以下=0, max以上=1, 中間=グラデーション
    ShadowMask = smoothstep(ShadowStep - ShadowFeather, ShadowStep, HalfLambert);
}

//-----------------------------------------------------------------------------
// 3段階トゥーンシェーディング（Base → 1st Shade → 2nd Shade）
// 用途: より詳細なセルシェーディング（Week 2で実装予定）
//-----------------------------------------------------------------------------
/// <summary>
/// 2段階シャドウマスク計算（1st Shade → 2nd Shade）
/// </summary>
/// <param name="HalfLambert">Half-Lambert値 (0-1)</param>
/// <param name="FirstShadeStep">1st Shade境界の閾値</param>
/// <param name="SecondShadeStep">2nd Shade境界の閾値</param>
/// <param name="Feather1st">1st Shade境界のぼかし幅</param>
/// <param name="Feather2nd">2nd Shade境界のぼかし幅</param>
/// <param name="ShadowMask1st">1st Shadeマスク (0-1)</param>
/// <param name="ShadowMask2nd">2nd Shadeマスク (0-1)</param>
void CalculateTwoStageShadow_float(
    float HalfLambert,
    float FirstShadeStep,
    float SecondShadeStep,
    float Feather1st,
    float Feather2nd,
    out float ShadowMask1st,
    out float ShadowMask2nd
)
{
    // Week 2で実装予定
    // 現在はスタブとして基本計算のみ
    ShadowMask1st = smoothstep(FirstShadeStep - Feather1st, FirstShadeStep, HalfLambert);
    ShadowMask2nd = smoothstep(SecondShadeStep - Feather2nd, SecondShadeStep, HalfLambert);
}

//-----------------------------------------------------------------------------
// リムライト計算（Week 2で実装予定）
//-----------------------------------------------------------------------------
/// <summary>
/// リムライト強度計算（フレネル効果ベース）
/// </summary>
/// <param name="Normal">ワールド空間法線</param>
/// <param name="ViewDirection">ワールド空間視線方向</param>
/// <param name="RimPower">リムライトの鋭さ (0-1)</param>
/// <param name="RimIntensity">リムライト強度出力 (0-1)</param>
void CalculateRimLight_float(
    float3 Normal,
    float3 ViewDirection,
    float RimPower,
    out float RimIntensity
)
{
    // Week 2で実装予定
    // 現在はスタブとして0を返す
    RimIntensity = 0.0;
}

//-----------------------------------------------------------------------------
// 使用例（Shader Graph Custom Functionノードに記載）
//-----------------------------------------------------------------------------
/*
=== 基本セルシェーディング（2トーン）の実装例 ===

1. CalculateHalfLambert_float
   入力: Normal (Vector3), LightDirection (Vector3)
   出力: HalfLambert (Float)

2. CalculateToonShadowMask_float
   入力: HalfLambert (Float), ShadowStep (Float: 0.5), ShadowFeather (Float: 0.05)
   出力: ShadowMask (Float)

3. Lerp ノード
   入力: ShadeColor (Color), BaseColor (Color), ShadowMask (Float)
   出力: FinalColor (Color)

推奨パラメータ:
- ShadowStep: 0.5 (標準), 0.6 (ライト寄り), 0.4 (シャドウ寄り)
- ShadowFeather: 0.05 (標準), 0.0 (ハードエッジ), 0.1 (ソフトエッジ)
*/

#endif // TOON_LIGHTING_INCLUDED
