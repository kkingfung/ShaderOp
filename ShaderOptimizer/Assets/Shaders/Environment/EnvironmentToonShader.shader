/// <summary>
/// 環境オブジェクト用トゥーンシェーダー
/// 背景・小道具・建築物向け・バッチング最適化・モバイル最適化
/// </summary>
Shader "ShaderOp/Environment/EnvironmentToon"
{
    Properties
    {
        [Header(Base Color)]
        _BaseColor("Environment Color", Color) = (1, 1, 1, 1)
        _MainTex("Environment Texture", 2D) = "white" {}

        [Header(Color Customization)]
        [Toggle(_USE_COLOR_MASK)] _UseColorMask("Use Color Mask", Float) = 0
        _ColorMaskTex("Color Mask (R=Primary G=Secondary B=Accent)", 2D) = "black" {}
        _PrimaryColor("Primary Color (R Channel)", Color) = (1, 1, 1, 1)
        _SecondaryColor("Secondary Color (G Channel)", Color) = (1, 1, 1, 1)
        _AccentColor("Accent Color (B Channel)", Color) = (1, 1, 1, 1)

        [Header(Shading)]
        _ShadeColor("Shade Color", Color) = (0.7, 0.7, 0.7, 1)
        _BaseColorStep("Base Color Step", Range(0, 1)) = 0.5
        _BaseShadeFeather("Base Shade Feather", Range(0, 0.5)) = 0.05

        [Header(Rim Light)]
        [Toggle(_RIMLIGHT_ON)] _UseRimLight("Use Rim Light", Float) = 0
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.3

        [Header(Alpha)]
        [Enum(Off,0,On,1)] _AlphaClip("Alpha Clip", Float) = 0
        _AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2

        [Header(Advanced)]
        _LightColorIntensity("Light Color Intensity", Range(0, 1)) = 0.8
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
            #pragma shader_feature_local _USE_COLOR_MASK
            #pragma shader_feature_local _RIMLIGHT_ON
            #pragma shader_feature_local _ _ALPHACLIP_ON

            // モバイル最適化
            #pragma target 3.0

            // GPUインスタンシング対応
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Includes/ToonLightingCore.hlsl"

            // テクスチャとサンプラー
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ColorMaskTex);
            SAMPLER(sampler_ColorMaskTex);

            // マテリアルプロパティ
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _MainTex_ST;
                half4 _ColorMaskTex_ST;

                // カスタムカラー
                half4 _PrimaryColor;
                half4 _SecondaryColor;
                half4 _AccentColor;

                // シェーディング
                half4 _ShadeColor;
                half _BaseColorStep;
                half _BaseShadeFeather;

                // リムライト
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;

                half _AlphaClipThreshold;
                half _LightColorIntensity;
            CBUFFER_END

            // 頂点シェーダー入力
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // フラグメントシェーダー入力
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            /// <summary>
            /// カラーマスク適用（環境オブジェクト用3チャンネル）
            /// </summary>
            half3 ApplyColorMask(half4 colorMask, half3 baseColor)
            {
                half3 primaryContrib = _PrimaryColor.rgb * colorMask.r;
                half3 secondaryContrib = _SecondaryColor.rgb * colorMask.g;
                half3 accentContrib = _AccentColor.rgb * colorMask.b;

                half totalMask = saturate(colorMask.r + colorMask.g + colorMask.b);
                half3 maskedColor = primaryContrib + secondaryContrib + accentContrib;

                return lerp(baseColor, maskedColor * baseColor, totalMask);
            }

            /// <summary>
            /// 頂点シェーダー
            /// </summary>
            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;

                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.shadowCoord = GetShadowCoord(vertexInput);

                return output;
            }

            /// <summary>
            /// フラグメントシェーダー
            /// </summary>
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                Light mainLight = GetMainLight(input.shadowCoord);

                half3 normalWS = normalize(input.normalWS);
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

                // テクスチャサンプリング
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                #if defined(_ALPHACLIP_ON)
                    clip(mainTexColor.a - _AlphaClipThreshold);
                #endif

                // カラーマスク適用
                half3 baseColor = mainTexColor.rgb * _BaseColor.rgb;
                #if defined(_USE_COLOR_MASK)
                    half4 colorMask = SAMPLE_TEXTURE2D(_ColorMaskTex, sampler_ColorMaskTex, input.uv);
                    baseColor = ApplyColorMask(colorMask, baseColor);
                #endif

                // Half-Lambert
                half halfLambert = CalculateHalfLambert(normalWS, lightDir);

                // トゥーンシャドウマスク
                half shadowMask = CalculateToonShadowMask(
                    halfLambert,
                    _BaseColorStep,
                    _BaseShadeFeather,
                    mainLight.shadowAttenuation
                );

                // シェード色合成
                half3 shadeColor = baseColor * _ShadeColor.rgb;
                half3 finalColor = lerp(shadeColor, baseColor, shadowMask);

                // ライトカラー
                half3 lightColor = lerp(half3(1, 1, 1), mainLight.color, _LightColorIntensity);
                finalColor *= lightColor;

                // リムライト
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

            #pragma shader_feature_local _ _ALPHACLIP_ON
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _MainTex_ST;
                half _AlphaClipThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                positionWS = ApplyShadowBias(positionWS, normalWS, _LightDirection);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                #if defined(_ALPHACLIP_ON)
                    half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                    clip(alpha - _AlphaClipThreshold);
                #endif

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

            #pragma shader_feature_local _ _ALPHACLIP_ON
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _MainTex_ST;
                half _AlphaClipThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                #if defined(_ALPHACLIP_ON)
                    half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                    clip(alpha - _AlphaClipThreshold);
                #endif

                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
