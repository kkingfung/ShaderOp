Shader "ShaderOp/Cloth/FabricCotton"
{
    Properties
    {
        // メインカラー
        [Header(Base Properties)]
        _BaseColor ("Base Color", Color) = (0.9, 0.9, 0.95, 1.0)
        _MainTex ("Main Texture", 2D) = "white" {}

        // シャドウ設定
        [Header(Toon Shading)]
        _ShadowColor ("Shadow Color", Color) = (0.6, 0.6, 0.65, 1.0)
        _ShadowStep ("Shadow Step", Range(0.0, 1.0)) = 0.5
        _ShadowSmoothness ("Shadow Smoothness", Range(0.0, 0.2)) = 0.05

        // 法線マップ
        [Header(Normal Mapping)]
        [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0.0, 2.0)) = 1.0

        // マテリアル設定（モバイル最適化のため固定値推奨）
        [Header(Material)]
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.2
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

            // Unity URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            // モバイル最適化: 精度をhalf（16bit）に変更
            #define HALF_PRECISION

            // プロパティ宣言
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTex_ST;
                half4 _ShadowColor;
                half _ShadowStep;
                half _ShadowSmoothness;
                half _NormalStrength;
                half _Smoothness;
            CBUFFER_END

            // 頂点シェーダー入力
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // フラグメントシェーダー入力
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                float fogFactor : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 頂点シェーダー
            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // 座標変換
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // 法線、接線、従法線をワールド空間に変換
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                // ビュー方向
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

                // フォグ
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            // トゥーンシェーディング計算
            half CalculateToonShading(half NdotL)
            {
                // Smoothstep でトゥーン境界をぼかす
                half edge1 = _ShadowStep - _ShadowSmoothness * 0.5;
                half edge2 = _ShadowStep + _ShadowSmoothness * 0.5;
                return smoothstep(edge1, edge2, NdotL);
            }

            // フラグメントシェーダー
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // テクスチャサンプリング
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalStrength);

                // ベースカラー
                half4 albedo = baseMap * _BaseColor;

                // タンジェント空間からワールド空間への法線変換
                half3 normalWS = TransformTangentToWorld(normalTS,
                    half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                normalWS = normalize(normalWS);

                // メインライト取得
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);

                // Lambert拡散反射
                half NdotL = saturate(dot(normalWS, lightDir));

                // トゥーンシェーディング
                half toonFactor = CalculateToonShading(NdotL);

                // シャドウカラーとライトカラーをブレンド
                half3 finalColor = lerp(_ShadowColor.rgb * albedo.rgb,
                                        albedo.rgb * mainLight.color,
                                        toonFactor);

                // アンビエントライト追加
                half3 ambient = SampleSH(normalWS) * albedo.rgb * 0.3;
                finalColor += ambient;

                // フォグ適用
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        // ShadowCaster Pass（影生成用）
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                Light mainLight = GetMainLight();
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, mainLight.direction));

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // DepthOnly Pass（深度バッファ用）
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

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
