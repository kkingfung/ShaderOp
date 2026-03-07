Shader "ShaderOp/Cloth/FabricCotton"
{
    Properties
    {
        // ベースカラー
        [Header(Base Settings)]
        _BaseColor("Base Color", Color) = (0.8, 0.8, 0.9, 1)
        _MainTex("Main Texture", 2D) = "white" {}

        // ファブリック設定
        [Header(Fabric Settings)]
        _FabricTex("Fabric Weave Texture", 2D) = "white" {}
        _FabricStrength("Fabric Texture Strength", Range(0, 1)) = 0.3
        _Roughness("Roughness", Range(0, 1)) = 0.8

        // トゥーンシェーディング
        [Header(Toon Shading)]
        _ShadowColor("Shadow Color", Color) = (0.4, 0.4, 0.5, 1)
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0, 0.5)) = 0.08

        // リムライト
        [Header(Rim Light)]
        _RimColor("Rim Color", Color) = (0.9, 0.9, 1, 1)
        _RimPower("Rim Power", Range(0, 10)) = 2.5
        _RimIntensity("Rim Intensity", Range(0, 1)) = 0.3
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_FabricTex);
            SAMPLER(sampler_FabricTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _RimColor;
                half _ShadowThreshold;
                half _ShadowSmoothness;
                half _Roughness;
                half _FabricStrength;
                half _RimPower;
                half _RimIntensity;
                float4 _MainTex_ST;
                float4 _FabricTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 座標変換
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;

                // 法線変換
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                // UV
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // フォグ
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // ベーステクスチャ
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;

                // ファブリックテクスチャ（織り目模様）
                half3 fabricPattern = SAMPLE_TEXTURE2D(_FabricTex, sampler_FabricTex, input.uv * 10.0).rgb;
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * fabricPattern, _FabricStrength);

                // 法線
                half3 normalWS = normalize(input.normalWS);

                // ライティング
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

                // ディフューズ（Half-Lambert）
                half NdotL = dot(normalWS, lightDir);
                half halfLambert = NdotL * 0.5 + 0.5;

                // ラフネスを考慮したディフューズ調整
                halfLambert = lerp(halfLambert, halfLambert * halfLambert, _Roughness);

                // トゥーンステップ
                half toonStep = smoothstep(_ShadowThreshold - _ShadowSmoothness,
                                          _ShadowThreshold + _ShadowSmoothness,
                                          halfLambert);
                half3 diffuse = lerp(_ShadowColor.rgb, half3(1, 1, 1), toonStep);

                // リムライト（フレネル）
                half NdotV = saturate(dot(normalWS, viewDir));
                half fresnel = pow(1.0 - NdotV, _RimPower);
                half3 rim = _RimColor.rgb * fresnel * _RimIntensity;

                // 最終カラー合成
                half3 finalColor = albedo.rgb * diffuse * mainLight.color;
                finalColor += rim;

                // 環境光（マットな質感）
                finalColor += albedo.rgb * half3(0.25, 0.25, 0.28);

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

    FallBack "Universal Render Pipeline/Simple Lit"
}
