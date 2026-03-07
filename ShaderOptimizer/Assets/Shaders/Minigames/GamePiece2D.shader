Shader "ShaderOp/Minigames/GamePiece2D"
{
    Properties
    {
        // メインテクスチャ
        [Header(Base Properties)]
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)

        // プレイヤーカラー
        [Header(Player Colors)]
        _Player1Color ("Player 1 Color", Color) = (1.0, 0.3, 0.3, 1.0)
        _Player2Color ("Player 2 Color", Color) = (0.3, 0.3, 1.0, 1.0)
        _NeutralColor ("Neutral Color", Color) = (0.8, 0.8, 0.8, 1.0)

        // プレイヤー設定（0=Neutral, 1=Player1, 2=Player2）
        [Header(Player Control)]
        _PlayerOwner ("Player Owner", Float) = 0.0

        // グローエフェクト
        [Header(Glow Effect)]
        _GlowColor ("Glow Color", Color) = (1.0, 1.0, 0.5, 1.0)
        _GlowIntensity ("Glow Intensity", Range(0.0, 2.0)) = 0.0
        _GlowSpeed ("Glow Speed", Range(0.0, 5.0)) = 2.0

        // アウトライン
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.01

        // シャドウ
        [Header(Shadow)]
        _ShadowColor ("Shadow Color", Color) = (0.0, 0.0, 0.0, 0.5)
        _ShadowOffset ("Shadow Offset", Vector) = (0.02, -0.02, 0.0, 0.0)

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
            "CanUseSpriteAtlas" = "True"
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

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _Player1Color;
                half4 _Player2Color;
                half4 _NeutralColor;
                float _PlayerOwner;
                half4 _GlowColor;
                half _GlowIntensity;
                half _GlowSpeed;
                half4 _OutlineColor;
                half _OutlineWidth;
                half4 _ShadowColor;
                float4 _ShadowOffset;
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

            // プレイヤーカラー取得
            half4 GetPlayerColor()
            {
                int owner = (int)_PlayerOwner;

                if (owner == 1) return _Player1Color;
                if (owner == 2) return _Player2Color;

                return _NeutralColor;
            }

            // グロー計算
            half CalculateGlow()
            {
                float time = _Time.y * _GlowSpeed;
                return (sin(time) * 0.5 + 0.5) * _GlowIntensity;
            }

            // エッジ検出（アウトライン用）
            half DetectEdge(float2 uv, half alpha)
            {
                // アルファ値から境界を検出
                half edgeFactor = 1.0 - smoothstep(0.4, 0.6, alpha);
                return edgeFactor * step(0.1, alpha);
            }

            half4 frag(Varyings input) : SV_Target
            {
                // テクスチャサンプリング
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // プレイヤーカラー取得
                half4 playerColor = GetPlayerColor();

                // カラー合成
                half4 finalColor = texColor * _Color * input.color * playerColor;

                // グロー効果
                if (_GlowIntensity > 0.01)
                {
                    half glow = CalculateGlow();
                    finalColor.rgb += _GlowColor.rgb * glow * finalColor.a;
                }

                // エッジ検出（アウトライン）
                half edge = DetectEdge(input.uv, texColor.a);
                if (edge > 0.5 && _OutlineWidth > 0.001)
                {
                    finalColor.rgb = lerp(finalColor.rgb, _OutlineColor.rgb, edge * 0.8);
                }

                // アルファ値維持
                finalColor.a *= texColor.a;

                return finalColor;
            }
            ENDHLSL
        }

        // Shadow Pass（シャドウ描画用）
        Pass
        {
            Name "Shadow"

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _ShadowColor;
                float4 _ShadowOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vertShadow(Attributes input)
            {
                Varyings output;

                // シャドウオフセット適用
                float3 offsetPos = input.positionOS.xyz;
                offsetPos.xy += _ShadowOffset.xy;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(offsetPos);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 fragShadow(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 shadowColor = _ShadowColor;
                shadowColor.a *= texColor.a;

                return shadowColor;
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
