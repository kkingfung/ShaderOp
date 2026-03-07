Shader "ShaderOp/Cloth/FabricSatin"
{
    Properties
    {
        // ベースカラー
        [Header(Base Settings)]
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Main Texture", 2D) = "white" {}

        // サテン設定
        [Header(Satin Settings)]
        _SatinColor("Satin Highlight Color", Color) = (1, 0.95, 0.9, 1)
        _Anisotropy("Anisotropy", Range(0, 1)) = 0.7
        _Glossiness("Glossiness", Range(0, 1)) = 0.8
        _AnisotropicDirection("Anisotropic Direction", Vector) = (0, 1, 0, 0)

        // トゥーンシェーディング
        [Header(Toon Shading)]
        _ShadowColor("Shadow Color", Color) = (0.5, 0.5, 0.6, 1)
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0, 0.5)) = 0.05

        // オプション
        [Header(Optional)]
        [Toggle] _UseNormalMap("Use Normal Map", Float) = 0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0, 2)) = 1.0
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

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float fogFactor : TEXCOORD5;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _SatinColor;
                half4 _ShadowColor;
                half _Anisotropy;
                half _Glossiness;
                half _ShadowThreshold;
                half _ShadowSmoothness;
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                half _BumpScale;
                half _UseNormalMap;
                half4 _AnisotropicDirection;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 座標変換
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;

                // 法線・接線変換
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                // UV
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // フォグ
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // ベーステクスチャサンプリング
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;

                // 法線マップ（オプション）
                half3 normalWS = input.normalWS;
                if (_UseNormalMap > 0.5)
                {
                    half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                    normalWS = normalize(mul(normalTS, tangentToWorld));
                }

                // ライティング計算
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 halfDir = normalize(lightDir + viewDir);

                // ディフューズ（Half-Lambert）
                half NdotL = dot(normalWS, lightDir);
                half halfLambert = NdotL * 0.5 + 0.5;

                // トゥーンステップ
                half toonStep = smoothstep(_ShadowThreshold - _ShadowSmoothness,
                                          _ShadowThreshold + _ShadowSmoothness,
                                          halfLambert);
                half3 diffuse = lerp(_ShadowColor.rgb, half3(1, 1, 1), toonStep);

                // 異方性ハイライト計算
                half3 tangent = normalize(input.tangentWS);
                half3 bitangent = normalize(input.bitangentWS);

                // 異方性方向（デフォルトはbitangent方向）
                half3 anisoDir = normalize(bitangent * _AnisotropicDirection.y + tangent * _AnisotropicDirection.x);

                // 異方性スペキュラー
                half TdotH = dot(anisoDir, halfDir);
                half anisoSpec = sqrt(1.0 - TdotH * TdotH);
                anisoSpec = pow(anisoSpec, lerp(1.0, 128.0, _Glossiness));
                anisoSpec *= _Anisotropy;

                // スペキュラーカラー
                half3 specular = _SatinColor.rgb * anisoSpec * toonStep;

                // 最終カラー合成
                half3 finalColor = albedo.rgb * diffuse * mainLight.color;
                finalColor += specular;

                // 環境光
                finalColor += albedo.rgb * half3(0.2, 0.2, 0.25);

                // フォグ
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        // Shadow caster pass
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
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
