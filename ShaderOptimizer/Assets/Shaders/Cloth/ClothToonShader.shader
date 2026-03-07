/// <summary>
/// 布地専用トゥーンシェーダー
/// サテン・ベルベット・メタリック対応・モバイル最適化
/// </summary>
Shader "ShaderOp/Cloth/ClothToon"
{
    Properties
    {
        [Header(Base Color)]
        _BaseColor("Cloth Color", Color) = (0.8, 0.2, 0.2, 1)
        _MainTex("Cloth Texture", 2D) = "white" {}
        _ColorMaskTex("Color Mask (R=Primary G=Secondary B=Accent A=Pattern)", 2D) = "black" {}

        [Header(Customizable Colors)]
        _PrimaryColor("Primary Color (R Channel)", Color) = (0.8, 0.2, 0.2, 1)
        _SecondaryColor("Secondary Color (G Channel)", Color) = (0.2, 0.2, 0.8, 1)
        _AccentColor("Accent Color (B Channel)", Color) = (1, 0.8, 0.2, 1)
        _PatternColor("Pattern Color (A Channel)", Color) = (1, 1, 1, 1)

        [Header(Shading)]
        _ShadeColor("Shade Color", Color) = (0.6, 0.15, 0.15, 1)
        _BaseColorStep("Base Color Step", Range(0, 1)) = 0.5
        _BaseShadeFeather("Base Shade Feather", Range(0, 0.5)) = 0.05

        [Header(Fabric Type)]
        [KeywordEnum(Cotton, Satin, Velvet, Metallic)] _FabricType("Fabric Type", Float) = 0

        [Header(Satin Properties)]
        _SatinIntensity("Satin Highlight Intensity", Range(0, 2)) = 0.8
        _SatinPower("Satin Highlight Sharpness", Range(1, 200)) = 50
        _SatinColor("Satin Highlight Color", Color) = (1, 1, 1, 1)

        [Header(Velvet Properties)]
        _VelvetColor("Velvet Rim Color", Color) = (0.6, 0.4, 0.4, 1)
        _VelvetPower("Velvet Rim Power", Range(0.5, 5.0)) = 2.0
        _VelvetIntensity("Velvet Rim Intensity", Range(0, 2)) = 1.2

        [Header(Metallic Properties)]
        _Metallic("Metallic", Range(0, 1)) = 0.9
        _MetallicColor("Metallic Tint", Color) = (1, 0.85, 0.6, 1)
        _Roughness("Roughness", Range(0, 1)) = 0.2

        [Header(Rim Light)]
        [Toggle(_RIMLIGHT_ON)] _UseRimLight("Use Rim Light", Float) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.5
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.4

        [Header(Advanced)]
        _LightColorIntensity("Light Color Intensity", Range(0, 1)) = 0.85
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

            // ファブリックタイプ
            #pragma multi_compile _FABRICTYPE_COTTON _FABRICTYPE_SATIN _FABRICTYPE_VELVET _FABRICTYPE_METALLIC

            // シェーダーフィーチャー
            #pragma shader_feature_local _RIMLIGHT_ON

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
                half4 _BaseColor;
                half4 _MainTex_ST;
                half4 _ColorMaskTex_ST;

                // カスタムカラー
                half4 _PrimaryColor;
                half4 _SecondaryColor;
                half4 _AccentColor;
                half4 _PatternColor;

                // シェーディング
                half4 _ShadeColor;
                half _BaseColorStep;
                half _BaseShadeFeather;

                // サテン
                half _SatinIntensity;
                half _SatinPower;
                half4 _SatinColor;

                // ベルベット
                half4 _VelvetColor;
                half _VelvetPower;
                half _VelvetIntensity;

                // メタリック
                half _Metallic;
                half4 _MetallicColor;
                half _Roughness;

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
            /// カラーマスク適用
            /// </summary>
            half3 ApplyColorMask(half4 colorMask, half3 baseColor)
            {
                half3 primaryContrib = _PrimaryColor.rgb * colorMask.r;
                half3 secondaryContrib = _SecondaryColor.rgb * colorMask.g;
                half3 accentContrib = _AccentColor.rgb * colorMask.b;
                half3 patternContrib = _PatternColor.rgb * colorMask.a;

                half totalMask = saturate(colorMask.r + colorMask.g + colorMask.b + colorMask.a);
                half3 maskedColor = primaryContrib + secondaryContrib + accentContrib + patternContrib;

                return lerp(baseColor, maskedColor * baseColor, totalMask);
            }

            /// <summary>
            /// サテンハイライト計算（アニソトロピック風）
            /// </summary>
            half CalculateSatinHighlight(half3 normal, half3 viewDir, half3 lightDir, half power)
            {
                half3 halfVec = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normal, halfVec));
                return pow(NdotH, power);
            }

            /// <summary>
            /// ベルベットリム計算（逆フレネル）
            /// </summary>
            half3 CalculateVelvetRim(half3 normal, half3 viewDir, half power, half3 color, half intensity)
            {
                // フレネル効果（視線と法線の関係）
                half fresnel = saturate(dot(normal, viewDir));

                // ベルベットは中間角度で最大になる
                half velvetRim = pow(1.0 - abs(fresnel - 0.5) * 2.0, power);

                return color * velvetRim * intensity;
            }

            /// <summary>
            /// メタリック反射計算（簡易）
            /// </summary>
            half3 CalculateMetallicReflection(half3 normal, half3 viewDir, half3 lightDir, half metallic, half roughness, half3 tintColor)
            {
                half3 reflectDir = reflect(-viewDir, normal);
                half NdotV = saturate(dot(normal, viewDir));
                half NdotL = saturate(dot(normal, lightDir));

                // 簡易フレネル
                half fresnel = pow(1.0 - NdotV, 5.0);
                fresnel = lerp(0.04, 1.0, fresnel);

                // スペキュラー
                half3 halfVec = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normal, halfVec));
                half specPower = lerp(1, 200, 1.0 - roughness);
                half spec = pow(NdotH, specPower);

                return tintColor * spec * metallic * fresnel;
            }

            /// <summary>
            /// 頂点シェーダー
            /// </summary>
            Varyings vert(Attributes input)
            {
                Varyings output;

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
                Light mainLight = GetMainLight(input.shadowCoord);

                half3 normalWS = normalize(input.normalWS);
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

                // テクスチャサンプリング
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 colorMask = SAMPLE_TEXTURE2D(_ColorMaskTex, sampler_ColorMaskTex, input.uv);

                // カラーマスク適用
                half3 baseColor = ApplyColorMask(colorMask, mainTexColor.rgb);

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

                // ファブリックタイプ別処理
                #if defined(_FABRICTYPE_SATIN)
                    // サテンハイライト
                    half satinHighlight = CalculateSatinHighlight(normalWS, viewDir, lightDir, _SatinPower);
                    finalColor += _SatinColor.rgb * satinHighlight * _SatinIntensity * mainLight.color;

                #elif defined(_FABRICTYPE_VELVET)
                    // ベルベットリム
                    half3 velvetContrib = CalculateVelvetRim(normalWS, viewDir, _VelvetPower, _VelvetColor.rgb, _VelvetIntensity);
                    finalColor += velvetContrib;

                #elif defined(_FABRICTYPE_METALLIC)
                    // メタリック反射
                    half3 metallicReflection = CalculateMetallicReflection(
                        normalWS, viewDir, lightDir, _Metallic, _Roughness, _MetallicColor.rgb
                    );
                    finalColor += metallicReflection * mainLight.color;
                #endif

                // リムライト
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
    }

    FallBack "Universal Render Pipeline/Lit"
}
