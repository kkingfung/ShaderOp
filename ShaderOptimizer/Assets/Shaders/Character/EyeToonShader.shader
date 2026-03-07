/// <summary>
/// 目専用トゥーンシェーダー
/// パララックス虹彩・複数ハイライト・瞳孔拡張対応
/// </summary>
Shader "ShaderOp/Character/EyeToon"
{
    Properties
    {
        [Header(Eye Textures)]
        _IrisTex("Iris Texture (RGB)", 2D) = "white" {}
        _ScleraTex("Sclera Texture (白目)", 2D) = "white" {}
        _IrisColor("Iris Tint Color", Color) = (0.2, 0.4, 0.6, 1)
        _ScleraColor("Sclera Color", Color) = (1, 1, 1, 1)

        [Header(Iris Parallax)]
        _IrisDepth("Iris Depth", Range(0, 0.5)) = 0.15
        _PupilSize("Pupil Size", Range(0, 1)) = 0.3
        _IrisMask("Iris Mask (R=Iris Area)", 2D) = "white" {}

        [Header(Highlights)]
        [Toggle(_HIGHLIGHT_ON)] _UseHighlight("Use Highlights", Float) = 1
        _Highlight1Pos("Highlight 1 Position", Vector) = (0.3, 0.3, 0, 0)
        _Highlight1Size("Highlight 1 Size", Range(0, 1)) = 0.15
        _Highlight1Color("Highlight 1 Color", Color) = (1, 1, 1, 1)

        _Highlight2Pos("Highlight 2 Position", Vector) = (-0.2, 0.4, 0, 0)
        _Highlight2Size("Highlight 2 Size", Range(0, 1)) = 0.08
        _Highlight2Color("Highlight 2 Color", Color) = (0.8, 0.8, 1, 1)

        [Header(Rim Light)]
        [Toggle(_RIMLIGHT_ON)] _UseRimLight("Use Rim Light", Float) = 1
        _RimColor("Rim Color", Color) = (1, 0.9, 0.8, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 2.5
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.6

        [Header(Shading)]
        _ShadowColor("Shadow Color", Color) = (0.8, 0.8, 0.85, 1)
        _ShadowStep("Shadow Step", Range(0, 1)) = 0.4
        _ShadowFeather("Shadow Feather", Range(0, 0.5)) = 0.1

        [Header(Advanced)]
        _LightColorIntensity("Light Color Intensity", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP機能
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            // シェーダーフィーチャー
            #pragma shader_feature_local _HIGHLIGHT_ON
            #pragma shader_feature_local _RIMLIGHT_ON

            // モバイル最適化
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Includes/ToonLightingCore.hlsl"

            // テクスチャとサンプラー
            TEXTURE2D(_IrisTex);
            SAMPLER(sampler_IrisTex);
            TEXTURE2D(_ScleraTex);
            SAMPLER(sampler_ScleraTex);
            TEXTURE2D(_IrisMask);
            SAMPLER(sampler_IrisMask);

            // マテリアルプロパティ
            CBUFFER_START(UnityPerMaterial)
                half4 _IrisTex_ST;
                half4 _ScleraTex_ST;
                half4 _IrisColor;
                half4 _ScleraColor;

                half _IrisDepth;
                half _PupilSize;

                // ハイライト
                half4 _Highlight1Pos;
                half _Highlight1Size;
                half4 _Highlight1Color;
                half4 _Highlight2Pos;
                half _Highlight2Size;
                half4 _Highlight2Color;

                // リムライト
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;

                // シェーディング
                half4 _ShadowColor;
                half _ShadowStep;
                half _ShadowFeather;

                half _LightColorIntensity;
            CBUFFER_END

            // 頂点シェーダー入力
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // フラグメントシェーダー入力
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirTS : TEXCOORD3; // タンジェント空間の視線ベクトル
                float4 shadowCoord : TEXCOORD4;
            };

            /// <summary>
            /// パララックス虹彩UV計算
            /// 視線方向に応じて虹彩の位置をオフセット
            /// </summary>
            float2 CalculateParallaxIrisUV(float2 uv, float3 viewDirTS, half depth)
            {
                // 視線ベクトルの正規化（Z成分で除算）
                half2 parallaxOffset = viewDirTS.xy * depth / max(viewDirTS.z, 0.1);

                // UV中心からのオフセット
                float2 centeredUV = uv - 0.5;
                float2 parallaxUV = centeredUV + parallaxOffset;
                parallaxUV = parallaxUV + 0.5;

                return parallaxUV;
            }

            /// <summary>
            /// ハイライト計算（円形グラデーション）
            /// </summary>
            half CalculateEyeHighlight(float2 uv, half2 position, half size)
            {
                // UV中心化
                float2 centeredUV = uv - 0.5;

                // ハイライト位置からの距離
                half dist = length(centeredUV - position.xy);

                // スムーズステップで円形グラデーション
                half highlight = 1.0 - smoothstep(0, size, dist);

                return pow(highlight, 2.5); // シャープ化
            }

            /// <summary>
            /// 瞳孔マスク計算
            /// </summary>
            half CalculatePupilMask(float2 uv, half pupilSize)
            {
                float2 centeredUV = uv - 0.5;
                half dist = length(centeredUV);
                return 1.0 - smoothstep(pupilSize * 0.8, pupilSize, dist);
            }

            /// <summary>
            /// 頂点シェーダー
            /// </summary>
            Varyings vert(Attributes input)
            {
                Varyings output;

                // 座標変換
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;

                // 法線
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;

                // UV
                output.uv = TRANSFORM_TEX(input.uv, _ScleraTex);

                // タンジェント空間の視線ベクトル（パララックス用）
                half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

                // 簡易的なタンジェント空間変換（視線→ローカル）
                half3 viewDirOS = mul((float3x3)unity_WorldToObject, viewDirWS);
                output.viewDirTS = viewDirOS;

                // シャドウ座標
                output.shadowCoord = GetShadowCoord(vertexInput);

                return output;
            }

            /// <summary>
            /// フラグメントシェーダー
            /// </summary>
            half4 frag(Varyings input) : SV_Target
            {
                // メインライト取得
                Light mainLight = GetMainLight(input.shadowCoord);

                // 正規化
                half3 normalWS = normalize(input.normalWS);
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

                // 虹彩マスク取得
                half irisMask = SAMPLE_TEXTURE2D(_IrisMask, sampler_IrisMask, input.uv).r;

                // パララックス虹彩UV計算
                float2 irisUV = CalculateParallaxIrisUV(input.uv, normalize(input.viewDirTS), _IrisDepth);

                // テクスチャサンプリング
                half4 irisColor = SAMPLE_TEXTURE2D(_IrisTex, sampler_IrisTex, irisUV);
                half4 scleraColor = SAMPLE_TEXTURE2D(_ScleraTex, sampler_ScleraTex, input.uv);

                // 虹彩と白目のブレンド
                half3 eyeColor = lerp(
                    scleraColor.rgb * _ScleraColor.rgb,
                    irisColor.rgb * _IrisColor.rgb,
                    irisMask
                );

                // 瞳孔（黒い部分）
                half pupilMask = CalculatePupilMask(irisUV, _PupilSize);
                eyeColor = lerp(eyeColor, half3(0, 0, 0), pupilMask * irisMask);

                // Half-Lambert計算
                half halfLambert = CalculateHalfLambert(normalWS, lightDir);

                // トゥーンシャドウマスク
                half shadowMask = CalculateToonShadowMask(
                    halfLambert,
                    _ShadowStep,
                    _ShadowFeather,
                    mainLight.shadowAttenuation
                );

                // シャドウ適用
                half3 shadedColor = lerp(eyeColor * _ShadowColor.rgb, eyeColor, shadowMask);

                // ライトカラー反映
                half3 lightColor = lerp(half3(1, 1, 1), mainLight.color, _LightColorIntensity);
                half3 finalColor = shadedColor * lightColor;

                // ハイライト追加（虹彩のみ）
                #if defined(_HIGHLIGHT_ON)
                    half highlight1 = CalculateEyeHighlight(irisUV, _Highlight1Pos.xy, _Highlight1Size);
                    half highlight2 = CalculateEyeHighlight(irisUV, _Highlight2Pos.xy, _Highlight2Size);

                    half3 highlightColor = _Highlight1Color.rgb * highlight1 + _Highlight2Color.rgb * highlight2;
                    finalColor += highlightColor * irisMask;
                #endif

                // リムライト（眼球全体）
                #if defined(_RIMLIGHT_ON)
                    half rimLight = CalculateRimLight(normalWS, viewDir, _RimPower);
                    half3 rimContribution = _RimColor.rgb * rimLight * _RimIntensity;
                    finalColor += rimContribution;
                #endif

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // シャドウキャスターパス
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                positionWS = ApplyShadowBias(positionWS, normalWS, _LightDirection);
                output.positionCS = TransformWorldToHClip(positionWS);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // デプスオンリーパス
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
