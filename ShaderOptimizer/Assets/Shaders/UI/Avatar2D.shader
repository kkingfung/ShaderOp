Shader "ShaderOp/UI/Avatar2D"
{
    Properties
    {
        // メインテクスチャ
        [Header(Base Properties)]
        _MainTex ("Avatar Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)

        // カスタマイズカラー
        [Header(Customization)]
        _SkinTone ("Skin Tone", Color) = (1.0, 0.85, 0.75, 1.0)
        _HairColor ("Hair Color", Color) = (0.3, 0.2, 0.1, 1.0)
        _EyeColor ("Eye Color", Color) = (0.2, 0.4, 0.6, 1.0)
        _ClothingColor ("Clothing Color", Color) = (0.8, 0.3, 0.3, 1.0)

        // カラーマスク（R=Skin, G=Hair, B=Eyes, A=Clothing）
        [Header(Color Mask)]
        [NoScaleOffset] _ColorMask ("Color Mask (RGBA)", 2D) = "black" {}

        // アウトライン
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.015

        // シャドウ/ハイライト
        [Header(Shading)]
        _ShadowColor ("Shadow Color", Color) = (0.0, 0.0, 0.0, 0.3)
        _ShadowIntensity ("Shadow Intensity", Range(0.0, 1.0)) = 0.5
        _HighlightColor ("Highlight Color", Color) = (1.0, 1.0, 1.0, 0.5)
        _HighlightIntensity ("Highlight Intensity", Range(0.0, 1.0)) = 0.3

        // ステータスエフェクト
        [Header(Status Effects)]
        _StatusColor ("Status Color", Color) = (0.0, 0.0, 0.0, 0.0)
        _StatusIntensity ("Status Intensity", Range(0.0, 1.0)) = 0.0

        // ステンシル設定（UI用）
        [Header(Stencil)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        // Main Pass
        Pass
        {
            Name "Default"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_ColorMask);
            SAMPLER(sampler_ColorMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _SkinTone;
                half4 _HairColor;
                half4 _EyeColor;
                half4 _ClothingColor;
                half4 _OutlineColor;
                half _OutlineWidth;
                half4 _ShadowColor;
                half _ShadowIntensity;
                half4 _HighlightColor;
                half _HighlightIntensity;
                half4 _StatusColor;
                half _StatusIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = vertexInput.positionCS;
                output.color = input.color;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            // カラーマスクからカスタマイズカラーを適用
            half3 ApplyCustomColors(half3 baseColor, half4 mask)
            {
                half3 result = baseColor;

                // スキンカラー（Rチャンネル）
                result = lerp(result, result * _SkinTone.rgb, mask.r);

                // ヘアカラー（Gチャンネル）
                result = lerp(result, result * _HairColor.rgb, mask.g);

                // アイカラー（Bチャンネル）
                result = lerp(result, result * _EyeColor.rgb, mask.b);

                // クロージングカラー（Aチャンネル）
                result = lerp(result, result * _ClothingColor.rgb, mask.a);

                return result;
            }

            // シェーディング計算（簡易トゥーン）
            half3 ApplyShading(half3 color, float2 uv)
            {
                // UVベースの簡易シェーディング（上部がハイライト、下部がシャドウ）
                half shadeFactor = uv.y;

                // シャドウ適用
                half3 shadowed = lerp(color * _ShadowColor.rgb, color, shadeFactor);
                color = lerp(color, shadowed, _ShadowIntensity);

                // ハイライト適用
                half3 highlighted = color + _HighlightColor.rgb * (1.0 - shadeFactor);
                color = lerp(color, highlighted, _HighlightIntensity);

                return color;
            }

            // エッジ検出（アウトライン用）
            half DetectEdge(float2 uv, half alpha)
            {
                // アルファ値から境界を検出
                half edgeFactor = 1.0 - smoothstep(0.3, 0.7, alpha);
                return edgeFactor * step(0.1, alpha);
            }

            half4 frag(Varyings input) : SV_Target
            {
                // テクスチャサンプリング
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 maskColor = SAMPLE_TEXTURE2D(_ColorMask, sampler_ColorMask, input.uv);

                // ベースカラー
                half3 baseColor = texColor.rgb * _Color.rgb * input.color.rgb;

                // カスタマイズカラー適用
                baseColor = ApplyCustomColors(baseColor, maskColor);

                // シェーディング適用
                baseColor = ApplyShading(baseColor, input.uv);

                // ステータスエフェクト（オンライン/オフライン等）
                if (_StatusIntensity > 0.01)
                {
                    baseColor = lerp(baseColor, _StatusColor.rgb, _StatusIntensity);
                }

                // アウトライン
                half edge = DetectEdge(input.uv, texColor.a);
                if (edge > 0.5 && _OutlineWidth > 0.001)
                {
                    baseColor = lerp(baseColor, _OutlineColor.rgb, edge * 0.9);
                }

                // 最終カラー
                half4 finalColor = half4(baseColor, texColor.a * _Color.a * input.color.a);

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
