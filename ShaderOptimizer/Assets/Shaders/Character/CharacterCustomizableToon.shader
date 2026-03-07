/// <summary>
/// カスタマイズ可能トゥーンキャラクターシェーダー
/// カラーマスクで複数の色を個別制御可能（衣装・髪・肌など）
/// モバイル最適化・URP対応
/// </summary>
Shader "ShaderOp/Character/CharacterCustomizableToon"
{
    Properties
    {
        [Header(Main Textures)]
        _MainTex("Main Texture (Albedo)", 2D) = "white" {}
        _ColorMaskTex("Color Mask (R=Primary G=Secondary B=Accent A=Trim)", 2D) = "black" {}

        [Header(Customizable Colors)]
        _PrimaryColor("Primary Color (R Channel)", Color) = (1, 0.5, 0.5, 1)
        _SecondaryColor("Secondary Color (G Channel)", Color) = (0.5, 1, 0.5, 1)
        _AccentColor("Accent Color (B Channel)", Color) = (0.5, 0.5, 1, 1)
        _TrimColor("Trim Color (A Channel)", Color) = (1, 1, 1, 1)

        [Header(Shading)]
        _ShadeColor("Shade Color Tint", Color) = (0.7, 0.7, 0.7, 1)
        _BaseColorStep("Base Color Step", Range(0, 1)) = 0.5
        _BaseShadeFeather("Base Shade Feather", Range(0, 0.5)) = 0.05
        _ShadePower("Shade Power", Range(0, 1)) = 1.0

        [Header(Rim Light)]
        [Toggle(_RIMLIGHT_ON)] _UseRimLight("Use Rim Light", Float) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.3

        [Header(Advanced)]
        [Toggle(_RECEIVE_SHADOWS_OFF)] _ReceiveShadowsOff("Receive Shadows Off", Float) = 0
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
            #pragma shader_feature_local _RIMLIGHT_ON
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // モバイル最適化
            #pragma target 3.0

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
                half4 _MainTex_ST;
                half4 _ColorMaskTex_ST;

                // カスタマイズ可能カラー
                half4 _PrimaryColor;
                half4 _SecondaryColor;
                half4 _AccentColor;
                half4 _TrimColor;

                // シェーディング
                half4 _ShadeColor;
                half _BaseColorStep;
                half _BaseShadeFeather;
                half _ShadePower;

                // リムライト
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;

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
                float4 shadowCoord : TEXCOORD3;
            };

            /// <summary>
            /// カラーマスクを使用してカスタムカラーを適用
            /// R=Primary, G=Secondary, B=Accent, A=Trim
            /// </summary>
            half3 ApplyColorMask(half4 colorMask, half3 baseColor)
            {
                // 各チャンネルの色を混合
                half3 primaryContrib = _PrimaryColor.rgb * colorMask.r;
                half3 secondaryContrib = _SecondaryColor.rgb * colorMask.g;
                half3 accentContrib = _AccentColor.rgb * colorMask.b;
                half3 trimContrib = _TrimColor.rgb * colorMask.a;

                // マスクされていない部分はベーステクスチャの色を使用
                half totalMask = saturate(colorMask.r + colorMask.g + colorMask.b + colorMask.a);
                half3 maskedColor = primaryContrib + secondaryContrib + accentContrib + trimContrib;

                // マスク部分とベース部分をブレンド
                return lerp(baseColor, maskedColor * baseColor, totalMask);
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

                // 法線をワールド空間に変換
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;

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
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

                // テクスチャサンプリング
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 colorMask = SAMPLE_TEXTURE2D(_ColorMaskTex, sampler_ColorMaskTex, input.uv);

                // カラーマスク適用
                half3 baseColor = ApplyColorMask(colorMask, mainTexColor.rgb);

                // Half-Lambert計算
                half halfLambert = CalculateHalfLambert(normalWS, lightDir);

                // シャドウ減衰
                #if defined(_RECEIVE_SHADOWS_OFF)
                    half shadowAttenuation = 1.0;
                #else
                    half shadowAttenuation = mainLight.shadowAttenuation;
                #endif

                // トゥーンシャドウマスク計算
                half shadowMask = CalculateToonShadowMask(
                    halfLambert,
                    _BaseColorStep,
                    _BaseShadeFeather,
                    shadowAttenuation
                );

                // シェードカラー計算
                half3 shadeColor = baseColor * _ShadeColor.rgb;
                half3 finalColor = lerp(shadeColor, baseColor, shadowMask);

                // ライトカラー反映
                half3 lightColor = lerp(half3(1, 1, 1), mainLight.color, _LightColorIntensity);
                finalColor *= lightColor;

                // Shade Powerで影の強さ調整
                finalColor = lerp(baseColor * lightColor, finalColor, _ShadePower);

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

        // シャドウキャスターパス（影を落とす）
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

                // シャドウバイアス適用
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
