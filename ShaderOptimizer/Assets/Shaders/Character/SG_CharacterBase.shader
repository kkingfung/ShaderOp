Shader "ShaderOp/Character/CharacterBase"
{
    Properties
    {
        // ベース設定
        [Header(Base Settings)]
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Main Texture", 2D) = "white" {}

        // カスタマイズカラー（4ゾーン）
        [Header(Customization Colors)]
        _HairColor("Hair Color", Color) = (0.2, 0.1, 0.05, 1)
        _SkinColor("Skin Color", Color) = (1.0, 0.87, 0.78, 1)
        _EyeColor("Eye Color", Color) = (0.3, 0.5, 1.0, 1)
        _ClothColor("Cloth Color", Color) = (0.8, 0.2, 0.2, 1)

        // トゥーンシェーディング設定
        [Header(Toon Shading)]
        _ShadowColor("Shadow Color", Color) = (0.7, 0.7, 0.7, 1)
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0, 1)) = 0.05

        // リムライト設定
        [Header(Rim Lighting)]
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0, 10)) = 3.0
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.5

        // レンダリング設定
        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        LOD 300
        Cull [_Cull]

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP必須キーワード
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // モバイル最適化
            #pragma target 4.5
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // プロパティ
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _HairColor;
                half4 _SkinColor;
                half4 _EyeColor;
                half4 _ClothColor;
                half4 _ShadowColor;
                half4 _RimColor;
                half _ShadowThreshold;
                half _ShadowSmoothness;
                half _RimPower;
                half _RimIntensity;
                float4 _MainTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // 入力構造体
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // 出力構造体
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half3 viewDirWS : TEXCOORD3;
            };

            /// <summary>
            /// 頂点シェーダー
            /// </summary>
            Varyings vert(Attributes input)
            {
                Varyings output;

                // ワールド空間への変換
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

                return output;
            }

            /// <summary>
            /// Half-Lambert計算（0〜1の範囲にリマップ）
            /// </summary>
            half CalculateHalfLambert(half3 normal, half3 lightDir)
            {
                half NdotL = dot(normal, lightDir);
                return NdotL * 0.5h + 0.5h;
            }

            /// <summary>
            /// 2段階トゥーンシェーディング計算
            /// </summary>
            half CalculateToonShading(half halfLambert, half threshold, half smoothness)
            {
                // スムーズステップでエッジをぼかす
                return smoothstep(threshold - smoothness, threshold + smoothness, halfLambert);
            }

            /// <summary>
            /// リムライト計算（フレネル効果）
            /// </summary>
            half3 CalculateRimLight(half3 normal, half3 viewDir, half power, half intensity, half3 rimColor)
            {
                half NdotV = saturate(dot(normal, viewDir));
                half rim = 1.0h - NdotV;
                rim = pow(rim, power);
                return rim * intensity * rimColor;
            }

            /// <summary>
            /// フラグメントシェーダー
            /// </summary>
            half4 frag(Varyings input) : SV_Target
            {
                // テクスチャサンプリング
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // ベースカラー（将来的にカラーマスクテクスチャでゾーン分け可能）
                // 現在は_BaseColorをベースに、カスタマイズカラーを乗算
                half3 baseColor = _BaseColor.rgb * mainTex.rgb;

                // 法線とビュー方向の正規化
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);

                // メインライト取得
                Light mainLight = GetMainLight();
                half3 lightDir = mainLight.direction;
                half3 lightColor = mainLight.color;

                // Half-Lambert計算
                half halfLambert = CalculateHalfLambert(normalWS, lightDir);

                // トゥーンシェーディングマスク計算
                half toonMask = CalculateToonShading(halfLambert, _ShadowThreshold, _ShadowSmoothness);

                // ライトカラーとシャドウカラーのブレンド
                half3 shadedColor = lerp(_ShadowColor.rgb * baseColor, baseColor, toonMask);

                // メインライトカラーを適用
                shadedColor *= lightColor;

                // アンビエントライト追加（環境光）
                half3 ambient = SampleSH(normalWS) * baseColor * 0.5h;
                shadedColor += ambient;

                // リムライト計算
                half3 rimLight = CalculateRimLight(normalWS, viewDirWS, _RimPower, _RimIntensity, _RimColor.rgb);

                // 最終カラー
                half3 finalColor = shadedColor + rimLight;

                return half4(finalColor, 1.0h);
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
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // デプスパス（深度書き込み）
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma target 4.5

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
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
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
