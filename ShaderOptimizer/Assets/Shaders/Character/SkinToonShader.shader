/// <summary>
/// 肌専用トゥーンシェーダー
/// サブサーフェススキャッタリング近似・柔らかいシェーディング・モバイル最適化
/// </summary>
Shader "ShaderOp/Character/SkinToon"
{
    Properties
    {
        [Header(Base Color)]
        _BaseColor("Skin Color", Color) = (1, 0.85, 0.75, 1)
        _MainTex("Skin Texture", 2D) = "white" {}

        [Header(Shading)]
        _ShadeColor("Shade Color", Color) = (0.9, 0.7, 0.6, 1)
        _BaseColorStep("Base Color Step", Range(0, 1)) = 0.55
        _BaseShadeFeather("Base Shade Feather", Range(0, 0.5)) = 0.15

        [Header(Subsurface Scattering)]
        [Toggle(_SSS_ON)] _UseSSS("Use SSS Approximation", Float) = 1
        _SSSColor("SSS Color (赤み)", Color) = (1, 0.4, 0.3, 1)
        _SSSIntensity("SSS Intensity", Range(0, 2)) = 0.6
        _SSSPower("SSS Power", Range(1, 10)) = 3.0
        _SSSDistortion("SSS Distortion", Range(0, 1)) = 0.5

        [Header(Detail)]
        _DetailTex("Detail Map (Normal/Roughness)", 2D) = "bump" {}
        _DetailNormalIntensity("Detail Normal Intensity", Range(0, 2)) = 0.3
        _Smoothness("Smoothness", Range(0, 1)) = 0.8

        [Header(Rim Light)]
        [Toggle(_RIMLIGHT_ON)] _UseRimLight("Use Rim Light", Float) = 1
        _RimColor("Rim Color", Color) = (1, 0.9, 0.8, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.5
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.3

        [Header(Advanced)]
        _LightColorIntensity("Light Color Intensity", Range(0, 1)) = 0.9
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
            #pragma shader_feature_local _SSS_ON
            #pragma shader_feature_local _RIMLIGHT_ON

            // モバイル最適化
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Includes/ToonLightingCore.hlsl"

            // テクスチャとサンプラー
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DetailTex);
            SAMPLER(sampler_DetailTex);

            // マテリアルプロパティ
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadeColor;
                half4 _MainTex_ST;
                half _BaseColorStep;
                half _BaseShadeFeather;

                // SSS
                half4 _SSSColor;
                half _SSSIntensity;
                half _SSSPower;
                half _SSSDistortion;

                // ディテール
                half4 _DetailTex_ST;
                half _DetailNormalIntensity;
                half _Smoothness;

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
            /// サブサーフェススキャッタリング近似
            /// 光が肌の内部を透過する効果を簡易的にシミュレート
            /// </summary>
            half3 CalculateSimpleSSS(
                half3 normal,
                half3 lightDir,
                half3 viewDir,
                half distortion,
                half power,
                half3 sssColor,
                half intensity)
            {
                // ライトを法線方向にディストーション（歪み）
                half3 distortedLightDir = normalize(lightDir + normal * distortion);

                // バックライト計算（光が裏から透ける）
                half backLight = saturate(dot(viewDir, -distortedLightDir));

                // パワーカーブで調整
                backLight = pow(backLight, power);

                // SSS色と強度を適用
                return sssColor * backLight * intensity;
            }

            /// <summary>
            /// 簡易ノーマルマップ適用
            /// </summary>
            half3 ApplyDetailNormal(half3 baseNormal, half3 tangent, half3 bitangent, float2 uv, half intensity)
            {
                // ディテールノーマルマップサンプリング
                half3 detailNormal = UnpackNormal(SAMPLE_TEXTURE2D(_DetailTex, sampler_DetailTex, uv));
                detailNormal.xy *= intensity;

                // タンジェント空間→ワールド空間変換
                half3x3 TBN = half3x3(tangent, bitangent, baseNormal);
                half3 worldDetailNormal = normalize(mul(detailNormal, TBN));

                return worldDetailNormal;
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

                // ディテールノーマル適用
                normalWS = ApplyDetailNormal(normalWS, tangentWS, bitangentWS, input.uv * 5.0, _DetailNormalIntensity);

                // テクスチャサンプリング
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Half-Lambert計算（肌用に柔らかめ）
                half halfLambert = CalculateHalfLambert(normalWS, lightDir);

                // トゥーンシャドウマスク計算（肌用に広めのフェザリング）
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

                // サブサーフェススキャッタリング（肌の透け感）
                #if defined(_SSS_ON)
                    half3 sssContribution = CalculateSimpleSSS(
                        normalWS,
                        lightDir,
                        viewDir,
                        _SSSDistortion,
                        _SSSPower,
                        _SSSColor.rgb,
                        _SSSIntensity
                    );

                    // シャドウ部分にSSSを加算（光が透けて見える）
                    finalColor += sssContribution * mainLight.color * (1.0 - shadowMask);
                #endif

                // リムライト追加（肌の輪郭を柔らかく）
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
