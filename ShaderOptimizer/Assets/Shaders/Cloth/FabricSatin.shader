Shader "ShaderOp/Cloth/FabricSatin"
{
    Properties
    {
        // メインカラー
        [Header(Base Properties)]
        _BaseColor ("Base Color", Color) = (0.95, 0.92, 0.88, 1.0)
        _MainTex ("Main Texture", 2D) = "white" {}

        // マテリアル設定
        [Header(Material)]
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.3
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.85

        // 異方性反射
        [Header(Anisotropy)]
        _Anisotropy ("Anisotropy", Range(-1.0, 1.0)) = 0.5
        _AnisotropyMap ("Anisotropy Map (RG)", 2D) = "white" {}

        // リムライティング
        [Header(Rim Lighting)]
        _RimColor ("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.0, 10.0)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0.0, 2.0)) = 0.5

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

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            TEXTURE2D(_AnisotropyMap);
            SAMPLER(sampler_AnisotropyMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTex_ST;
                half _Metallic;
                half _Smoothness;
                half _Anisotropy;
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;
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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 viewDirWS : TEXCOORD5;
                float fogFactor : TEXCOORD6;
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
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            // 異方性スペキュラー計算
            half CalculateAnisotropicSpecular(half3 H, half3 T, half3 B, half3 N, half anisotropy)
            {
                // Kajiya-Kay モデル（簡易版）
                half TdotH = dot(T, H);
                half BdotH = dot(B, H);

                half sinTH = sqrt(1.0 - TdotH * TdotH);
                half sinBH = sqrt(1.0 - BdotH * BdotH);

                half dirAtten = smoothstep(-1.0, 0.0, TdotH);
                return dirAtten * pow(sinTH, _Smoothness * 128.0) * anisotropy;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // テクスチャサンプリング
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalStrength);
                half2 anisotropyMap = SAMPLE_TEXTURE2D(_AnisotropyMap, sampler_AnisotropyMap, input.uv).rg;

                // ベースカラー
                half4 albedo = baseMap * _BaseColor;

                // 法線
                half3 normalWS = TransformTangentToWorld(normalTS,
                    half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                normalWS = normalize(normalWS);

                // ライティング
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(input.viewDirWS);
                half3 halfDir = normalize(lightDir + viewDir);

                // 拡散反射
                half NdotL = saturate(dot(normalWS, lightDir));
                half3 diffuse = albedo.rgb * mainLight.color * NdotL;

                // スペキュラー反射（標準）
                half NdotH = saturate(dot(normalWS, halfDir));
                half specularPower = exp2(_Smoothness * 10.0 + 1.0);
                half3 specular = pow(NdotH, specularPower) * mainLight.color * _Metallic;

                // 異方性スペキュラー
                half anisoSpec = CalculateAnisotropicSpecular(halfDir, input.tangentWS, input.bitangentWS, normalWS, _Anisotropy * anisotropyMap.r);
                specular += anisoSpec * mainLight.color * 0.5;

                // リムライティング（Fresnel効果）
                half NdotV = saturate(dot(normalWS, viewDir));
                half rimFactor = pow(1.0 - NdotV, _RimPower);
                half3 rim = rimFactor * _RimColor.rgb * _RimIntensity;

                // 最終カラー
                half3 finalColor = diffuse + specular + rim;

                // アンビエント
                half3 ambient = SampleSH(normalWS) * albedo.rgb * 0.2;
                finalColor += ambient;

                // フォグ
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
