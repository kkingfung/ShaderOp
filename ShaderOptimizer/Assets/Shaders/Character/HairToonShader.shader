/// <summary>
/// 髪の毛専用トゥーンシェーダー
/// アニソトロピックハイライト・透明度対応・モバイル最適化
/// </summary>
Shader "ShaderOp/Character/HairToon"
{
    Properties
    {
        [Header(Base Color)]
        _BaseColor("Hair Color", Color) = (0.3, 0.2, 0.15, 1)
        _MainTex("Hair Texture", 2D) = "white" {}

        [Header(Shading)]
        _ShadeColor("Shade Color", Color) = (0.2, 0.15, 0.1, 1)
        _BaseColorStep("Base Color Step", Range(0, 1)) = 0.6
        _BaseShadeFeather("Base Shade Feather", Range(0, 0.5)) = 0.08

        [Header(Anisotropic Highlight)]
        [Toggle(_ANISOTROPIC_ON)] _UseAnisotropic("Use Anisotropic Highlight", Float) = 1
        _HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
        _SpecularShift("Specular Shift", Range(-1, 1)) = 0.0
        _SpecularPower("Specular Power", Range(1, 200)) = 80
        _SpecularIntensity("Specular Intensity", Range(0, 2)) = 0.8
        _ShiftTex("Shift Texture (R)", 2D) = "gray" {}

        [Header(Secondary Highlight)]
        [Toggle(_SECONDARY_HIGHLIGHT_ON)] _UseSecondaryHighlight("Use Secondary Highlight", Float) = 1
        _SecondaryHighlightColor("Secondary Color", Color) = (0.8, 0.8, 1, 1)
        _SecondaryShift("Secondary Shift", Range(-1, 1)) = 0.1
        _SecondaryPower("Secondary Power", Range(1, 200)) = 120
        _SecondaryIntensity("Secondary Intensity", Range(0, 2)) = 0.5

        [Header(Rim Light)]
        [Toggle(_RIMLIGHT_ON)] _UseRimLight("Use Rim Light", Float) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 4.0
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.4

        [Header(Alpha)]
        _AlphaClip("Alpha Clip Threshold", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 0

        [Header(Advanced)]
        _LightColorIntensity("Light Color Intensity", Range(0, 1)) = 0.9
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }
        LOD 200
        Cull [_Cull]

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
            #pragma shader_feature_local _ANISOTROPIC_ON
            #pragma shader_feature_local _SECONDARY_HIGHLIGHT_ON
            #pragma shader_feature_local _RIMLIGHT_ON

            // モバイル最適化
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Includes/ToonLightingCore.hlsl"

            // テクスチャとサンプラー
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ShiftTex);
            SAMPLER(sampler_ShiftTex);

            // マテリアルプロパティ
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadeColor;
                half4 _MainTex_ST;
                half _BaseColorStep;
                half _BaseShadeFeather;

                // アニソトロピックハイライト
                half4 _HighlightColor;
                half _SpecularShift;
                half _SpecularPower;
                half _SpecularIntensity;
                half4 _ShiftTex_ST;

                // セカンダリハイライト
                half4 _SecondaryHighlightColor;
                half _SecondaryShift;
                half _SecondaryPower;
                half _SecondaryIntensity;

                // リムライト
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;

                half _AlphaClip;
                half _LightColorIntensity;
            CBUFFER_END

            // 頂点シェーダー入力
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            // フラグメントシェーダー入力
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
            };

            /// <summary>
            /// Kajiya-Kayアニソトロピックハイライト計算
            /// 髪の毛のような繊維状のハイライトを生成
            /// </summary>
            half CalculateKajiyaKaySpecular(
                half3 tangent,
                half3 viewDir,
                half3 lightDir,
                half shift,
                half power,
                half shiftMask)
            {
                // シフトテクスチャでタンジェント方向を調整
                half3 shiftedTangent = normalize(tangent + shift * shiftMask);

                // タンジェント方向のハイライト計算
                half3 halfVec = normalize(lightDir + viewDir);
                half tangentDotH = dot(shiftedTangent, halfVec);

                // sin^2を使って繊維状のハイライト
                half sinTH = sqrt(1.0 - tangentDotH * tangentDotH);
                half dirAtten = smoothstep(-1.0, 0.0, tangentDotH);

                return dirAtten * pow(sinTH, power);
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

                // 法線・タンジェント・バイタンジェント
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                // UV変換
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // シャドウ座標計算
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
                half3 tangentWS = normalize(input.tangentWS);
                half3 bitangentWS = normalize(input.bitangentWS);
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

                // テクスチャサンプリング
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half shiftMask = SAMPLE_TEXTURE2D(_ShiftTex, sampler_ShiftTex, input.uv).r;

                // アルファクリッピング
                clip(mainTexColor.a - _AlphaClip);

                // Half-Lambert計算
                half halfLambert = CalculateHalfLambert(normalWS, lightDir);

                // トゥーンシャドウマスク計算
                half shadowMask = CalculateToonShadowMask(
                    halfLambert,
                    _BaseColorStep,
                    _BaseShadeFeather,
                    mainLight.shadowAttenuation
                );

                // Base色とShade色の合成
                half3 baseColor = _BaseColor.rgb * mainTexColor.rgb;
                half3 shadeColor = _ShadeColor.rgb * mainTexColor.rgb;
                half3 finalColor = lerp(shadeColor, baseColor, shadowMask);

                // ライトカラー反映
                half3 lightColor = lerp(half3(1, 1, 1), mainLight.color, _LightColorIntensity);
                finalColor *= lightColor;

                // アニソトロピックハイライト（プライマリ）
                #if defined(_ANISOTROPIC_ON)
                    half specular = CalculateKajiyaKaySpecular(
                        tangentWS,
                        viewDir,
                        lightDir,
                        _SpecularShift,
                        _SpecularPower,
                        shiftMask
                    );
                    half3 specularColor = _HighlightColor.rgb * specular * _SpecularIntensity;
                    finalColor += specularColor * mainLight.color;
                #endif

                // セカンダリハイライト（より細く鋭い）
                #if defined(_SECONDARY_HIGHLIGHT_ON)
                    half secondarySpec = CalculateKajiyaKaySpecular(
                        tangentWS,
                        viewDir,
                        lightDir,
                        _SecondaryShift,
                        _SecondaryPower,
                        shiftMask
                    );
                    half3 secondaryColor = _SecondaryHighlightColor.rgb * secondarySpec * _SecondaryIntensity;
                    finalColor += secondaryColor * mainLight.color;
                #endif

                // リムライト追加
                #if defined(_RIMLIGHT_ON)
                    half rimLight = CalculateRimLight(normalWS, viewDir, _RimPower);
                    half3 rimContribution = _RimColor.rgb * rimLight * _RimIntensity;
                    finalColor += rimContribution;
                #endif

                return half4(finalColor, mainTexColor.a);
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
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _MainTex_ST;
                half _AlphaClip;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                positionWS = ApplyShadowBias(positionWS, normalWS, _LightDirection);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _AlphaClip);
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
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _MainTex_ST;
                half _AlphaClip;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _AlphaClip);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
