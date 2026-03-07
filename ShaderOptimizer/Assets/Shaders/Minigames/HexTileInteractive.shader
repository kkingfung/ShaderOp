Shader "ShaderOp/Minigames/HexTileInteractive"
{
    Properties
    {
        // ベース設定
        [Header(Base Properties)]
        _BaseColor ("Base Color", Color) = (0.9, 0.9, 0.9, 1.0)
        _MainTex ("Main Texture", 2D) = "white" {}

        // ステートカラー（ゲーム状態に応じた色）
        [Header(State Colors)]
        _IdleColor ("Idle Color", Color) = (0.8, 0.8, 0.8, 1.0)
        _HoverColor ("Hover Color", Color) = (1.0, 0.9, 0.5, 1.0)
        _SelectedColor ("Selected Color", Color) = (0.5, 1.0, 0.5, 1.0)
        _ValidMoveColor ("Valid Move Color", Color) = (0.5, 0.8, 1.0, 1.0)
        _InvalidMoveColor ("Invalid Move Color", Color) = (1.0, 0.5, 0.5, 1.0)

        // ステート制御（0=Idle, 1=Hover, 2=Selected, 3=ValidMove, 4=InvalidMove）
        [Header(State Control)]
        _TileState ("Tile State", Float) = 0.0

        // グローエフェクト
        [Header(Glow Effect)]
        _GlowIntensity ("Glow Intensity", Range(0.0, 2.0)) = 0.5
        _GlowSpeed ("Glow Speed", Range(0.0, 5.0)) = 1.0

        // アウトライン
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.2, 0.2, 0.2, 1.0)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.02

        // トゥーンシェーディング
        [Header(Toon Shading)]
        _ShadowStep ("Shadow Step", Range(0.0, 1.0)) = 0.5
        _ShadowSmoothness ("Shadow Smoothness", Range(0.0, 0.2)) = 0.05
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

        // Main Pass
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTex_ST;
                half4 _IdleColor;
                half4 _HoverColor;
                half4 _SelectedColor;
                half4 _ValidMoveColor;
                half4 _InvalidMoveColor;
                float _TileState;
                half _GlowIntensity;
                half _GlowSpeed;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _ShadowStep;
                half _ShadowSmoothness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = normalInput.normalWS;
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

            // ステートカラー取得
            half4 GetStateColor()
            {
                int state = (int)_TileState;

                if (state == 1) return _HoverColor;
                if (state == 2) return _SelectedColor;
                if (state == 3) return _ValidMoveColor;
                if (state == 4) return _InvalidMoveColor;

                return _IdleColor;
            }

            // グロー計算
            half CalculateGlow()
            {
                float time = _Time.y * _GlowSpeed;
                return (sin(time) * 0.5 + 0.5) * _GlowIntensity;
            }

            // エッジ検出（アウトライン用）
            half DetectEdge(float2 uv)
            {
                // UV座標から中心までの距離
                float2 center = float2(0.5, 0.5);
                float dist = distance(uv, center);

                // ヘックス形状の近似（円形）
                float edgeThreshold = 0.5 - _OutlineWidth;
                return step(edgeThreshold, dist);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // テクスチャサンプリング
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // ステートカラー取得
                half4 stateColor = GetStateColor();

                // ベースカラー計算
                half4 albedo = baseMap * _BaseColor * stateColor;

                // メインライト取得
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 normalWS = normalize(input.normalWS);

                // Lambert拡散反射
                half NdotL = saturate(dot(normalWS, lightDir));

                // トゥーンシェーディング
                half toonFactor = CalculateToonShading(NdotL);

                // ライティング適用
                half3 finalColor = albedo.rgb * mainLight.color * toonFactor;

                // アンビエントライト追加
                half3 ambient = SampleSH(normalWS) * albedo.rgb * 0.5;
                finalColor += ambient;

                // グロー効果（Selected/ValidMove状態のみ）
                int state = (int)_TileState;
                if (state == 2 || state == 3)
                {
                    half glow = CalculateGlow();
                    finalColor += stateColor.rgb * glow;
                }

                // アウトライン
                half edge = DetectEdge(input.uv);
                finalColor = lerp(finalColor, _OutlineColor.rgb, edge);

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
