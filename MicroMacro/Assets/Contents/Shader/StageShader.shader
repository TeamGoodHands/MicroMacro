Shader "StageShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white"{}
        _NoiseMap("NoiseMap Map", 2D) = "white"{}
        [HDR]_BaseColor("Base Color", Color) = (0,0,0,1)
        _NoiseScale("Noise Scale",Float) = 1
        _NoisePower("Noise Power",Range(0,1)) = 0
        _ShadowColor("Shadow Color", Color) = (0,0,0,1)
        _Cull("__cull", Float) = 2.0
    }

    SubShader
    {

        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        // LitシェーダーのShaderCasterPass
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 screenPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _NoiseMap_ST;
                float4 _BaseColor;
                float4 _ShadowColor;
                float _NoiseScale;
                float _NoisePower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.normal = IN.normal;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);

                return OUT;
            }

            float SamplePaperNoise(float3 worldPos)
            {
                // アンカー（オブジェクト原点）
                float3 anchorWorld = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1)).xyz;

                // 差分ベクトルをスクリーンに投影
                float4 clipPixel = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                float4 clipAnchor = mul(UNITY_MATRIX_VP, float4(anchorWorld, 1.0));

                float2 screenPixel = (clipPixel.xy / clipPixel.w) * 0.5 + 0.5;
                float2 screenAnchor = (clipAnchor.xy / clipAnchor.w) * 0.5 + 0.5;

                // 差分UV
                float2 uv = screenPixel - screenAnchor; //　ペーパーノイズをサンプリング

                return SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv * _NoiseScale).r;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                color *= _BaseColor;

                // ピクセルのワールド座標
                float3 pixelWorld = IN.worldPos;
                float noise = SamplePaperNoise(pixelWorld);

                // 影係数を計算
                float4 shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                half shadowAttention = mainLight.shadowAttenuation;

                color *= shadowAttention;
                float3 shadowColor = lerp(color.xyz, _ShadowColor.xyz, _ShadowColor.a);
                color.xyz = lerp(color.xyz, shadowColor, 1 - shadowAttention) * saturate(noise + _NoisePower);

                return color;
            }
            ENDHLSL
        }
    }
}