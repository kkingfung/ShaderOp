/// <summary>
/// トゥーンアウトラインシェーダー
/// 法線方向押し出し・距離フェード・モバイル最適化
/// URP Renderer Feature または マルチパスで使用
/// </summary>
Shader "ShaderOp/Outline/ToonOutline"
{
    Properties
    {
        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.02)) = 0.003
        _OutlineWidthTex("Outline Width Texture (R)", 2D) = "white" {}

        [Header(Distance Fade)]
        _NearDistance("Near Distance", Float) = 2.0
        _FarDistance("Far Distance", Float) = 10.0

        [Header(Advanced)]
        [Toggle(_USE_BAKED_NORMAL)] _UseBakedNormal("Use Baked Normal", Float) = 0
        _BakedNormalTex("Baked Normal Texture", 2D) = "bump" {}
        _OffsetZ("Z Offset", Range(-1, 1)) = 0.0
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
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _USE_BAKED_NORMAL

            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // テクスチャとサンプラー
            TEXTURE2D(_OutlineWidthTex);
            SAMPLER(sampler_OutlineWidthTex);
            TEXTURE2D(_BakedNormalTex);
            SAMPLER(sampler_BakedNormalTex);

            // マテリアルプロパティ
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _OutlineWidth;
                half4 _OutlineWidthTex_ST;
                half _NearDistance;
                half _FarDistance;
                half4 _BakedNormalTex_ST;
                half _OffsetZ;
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
                half4 color : COLOR;
            };

            /// <summary>
            /// 頂点シェーダー - 法線方向に押し出し
            /// </summary>
            Varyings vert(Attributes input)
            {
                Varyings output;

                // ワールド座標取得
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // 法線（Baked Normalまたは頂点法線）
                #if defined(_USE_BAKED_NORMAL)
                    // Baked Normalテクスチャからサンプリング
                    float2 uv = TRANSFORM_TEX(input.uv, _BakedNormalTex);
                    half3 bakedNormal = UnpackNormal(SAMPLE_TEXTURE2D_LOD(_BakedNormalTex, sampler_BakedNormalTex, uv, 0));
                    float3 normalOS = bakedNormal;
                #else
                    float3 normalOS = input.normalOS;
                #endif

                // ワールド空間法線
                float3 normalWS = TransformObjectToWorldNormal(normalOS);

                // カメラ距離計算
                float3 viewPos = GetCameraPositionWS();
                half distance = length(positionWS - viewPos);

                // 距離フェード（遠くなるほど細く）
                half distanceFade = smoothstep(_FarDistance, _NearDistance, distance);

                // アウトライン幅テクスチャ
                float2 widthUV = TRANSFORM_TEX(input.uv, _OutlineWidthTex);
                half widthMask = SAMPLE_TEXTURE2D_LOD(_OutlineWidthTex, sampler_OutlineWidthTex, widthUV, 0).r;

                // 最終アウトライン幅
                half finalWidth = _OutlineWidth * widthMask * distanceFade;

                // 法線方向に押し出し
                float3 outlinePos = positionWS + normalWS * finalWidth;

                // クリップ空間に変換
                output.positionCS = TransformWorldToHClip(outlinePos);

                // Z-Offset（カメラ方向にオフセット、埋もれ防止）
                #if UNITY_REVERSED_Z
                    output.positionCS.z -= _OffsetZ * 0.001;
                #else
                    output.positionCS.z += _OffsetZ * 0.001;
                #endif

                // カラー
                output.color = _OutlineColor;

                return output;
            }

            /// <summary>
            /// フラグメントシェーダー - 単色
            /// </summary>
            half4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
