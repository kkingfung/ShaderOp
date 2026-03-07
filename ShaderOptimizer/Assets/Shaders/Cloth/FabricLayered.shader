Shader "ShaderOp/Cloth/FabricLayered"
{
    Properties
    {
        // ベースレイヤー
        [Header(Base Layer)]
        _BaseColor ("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex ("Base Texture", 2D) = "white" {}

        // パターンレイヤー1
        [Header(Pattern Layer 1)]
        _Pattern1Color ("Pattern 1 Color", Color) = (0.8, 0.2, 0.2, 1.0)
        _Pattern1Tex ("Pattern 1 Texture", 2D) = "white" {}
        _Pattern1Blend ("Pattern 1 Blend", Range(0.0, 1.0)) = 0.5

        // パターンレイヤー2
        [Header(Pattern Layer 2)]
        _Pattern2Color ("Pattern 2 Color", Color) = (0.2, 0.2, 0.8, 1.0)
        _Pattern2Tex ("Pattern 2 Texture", 2D) = "white" {}
        _Pattern2Blend ("Pattern 2 Blend", Range(0.0, 1.0)) = 0.3

        // デカールレイヤー
        [Header(Decal Layer)]
        _DecalTex ("Decal Texture (RGBA)", 2D) = "white" {}
        _DecalBlend ("Decal Blend", Range(0.0, 1.0)) = 1.0

        // トゥーンシェーディング
        [Header(Toon Shading)]
        _ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _ShadowStep ("Shadow Step", Range(0.0, 1.0)) = 0.5
        _ShadowSmoothness ("Shadow Smoothness", Range(0.0, 0.2)) = 0.05

        // 法線マップ
        [Header(Normal Mapping)]
        [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0.0, 2.0)) = 1.0
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            // テクスチャ
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Pattern1Tex);
            SAMPLER(sampler_Pattern1Tex);

            TEXTURE2D(_Pattern2Tex);
            SAMPLER(sampler_Pattern2Tex);

            TEXTURE2D(_DecalTex);
            SAMPLER(sampler_DecalTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTex_ST;
                half4 _Pattern1Color;
                float4 _Pattern1Tex_ST;
                half _Pattern1Blend;
                half4 _Pattern2Color;
                float4 _Pattern2Tex_ST;
                half _Pattern2Blend;
                float4 _DecalTex_ST;
                half _DecalBlend;
                half4 _ShadowColor;
                half _ShadowStep;
                half _ShadowSmoothness;
                half _NormalStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            // トゥーンシェーディング計算
            half CalculateToonShading(half NdotL)
            {
                half edge1 = _ShadowStep - _ShadowSmoothness * 0.5;
                half edge2 = _ShadowStep + _ShadowSmoothness * 0.5;
                return smoothstep(edge1, edge2, NdotL);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // テクスチャサンプリング
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalStrength);

                // パターンレイヤー
                float2 pattern1UV = TRANSFORM_TEX(input.uv, _Pattern1Tex);
                half4 pattern1Map = SAMPLE_TEXTURE2D(_Pattern1Tex, sampler_Pattern1Tex, pattern1UV);

                float2 pattern2UV = TRANSFORM_TEX(input.uv, _Pattern2Tex);
                half4 pattern2Map = SAMPLE_TEXTURE2D(_Pattern2Tex, sampler_Pattern2Tex, pattern2UV);

                // デカールレイヤー
                float2 decalUV = TRANSFORM_TEX(input.uv, _DecalTex);
                half4 decalMap = SAMPLE_TEXTURE2D(_DecalTex, sampler_DecalTex, decalUV);

                // ベースカラー計算
                half4 albedo = baseMap * _BaseColor;

                // パターン1ブレンド（乗算）
                half3 pattern1 = pattern1Map.rgb * _Pattern1Color.rgb;
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * pattern1, _Pattern1Blend * pattern1Map.a);

                // パターン2ブレンド（加算）
                half3 pattern2 = pattern2Map.rgb * _Pattern2Color.rgb;
                albedo.rgb = lerp(albedo.rgb, albedo.rgb + pattern2 * 0.5, _Pattern2Blend * pattern2Map.a);

                // デカールブレンド（アルファブレンド）
                albedo.rgb = lerp(albedo.rgb, decalMap.rgb, _DecalBlend * decalMap.a);

                // 法線
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

        // ShadowCaster Pass
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

        // DepthOnly Pass
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
