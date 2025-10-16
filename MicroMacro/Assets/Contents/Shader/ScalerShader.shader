Shader "ScalerShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white"{}
        _NoiseMap("NoiseMap Map", 2D) = "white"{}
        [HDR]_BaseColor("Base Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Float) = 0
        [HDR]_OutlineColor("Outline Color", Color) = (0,0,0,1)
        _FresnelPower("Fresnel Power", Float) = 0.2
        [HDR]_FresnelColor("Fresnel Color", Color) = (0,0,0,0)
        [HDR]_AdditionalColor("Additional Color", Color) = (0,0,0,0)
        _WavePower("Wave Power", Float) = 0.05
        _WaveSpeed("Wave Speed", Float) = 7
        [Toggle(_RECEIVE_DECALS)] _ReceiveDecals("Receive Decals", Float) = 1
    }

    SubShader
    {

        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode"="DepthNormals"
            }
            ZWrite On Cull Back

            HLSLPROGRAM
            #pragma vertex   dn_vert
            #pragma fragment dn_frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct A
            {
                float4 positionOS: POSITION;
                float3 normalOS: NORMAL;
                float4 tangentOS: TANGENT;
            };

            struct V
            {
                float4 positionHCS: SV_POSITION;
                float3 normalWS: TEXCOORD0;
            };

            V dn_vert(A v)
            {
                V o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.positionHCS = p.positionCS;
                o.normalWS = n.normalWS;
                return o;
            }

            half4 dn_frag(V i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                return half4(n * 0.5 + 0.5, 1);
            }
            ENDHLSL
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

            #pragma shader_feature_local _RECEIVE_DECALS
            #pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ _DECAL_LAYERS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include  "SimpleNoise.hlsl"

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
                float _OutlineWidth;
                float4 _OutlineColor;
                float _FresnelPower;
                float4 _FresnelColor;
                float4 _AdditionalColor;
                float _WavePower;
                float _WaveSpeed;
            CBUFFER_END


            float FresnelEffect(float3 normal, float3 viewDir, float power)
            {
                return pow(1.0 - saturate(dot(normalize(normal), normalize(viewDir))), power);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.normal = IN.normal;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                // 頂点をノイズでユラユラさせる
                float2 vertexUv = IN.positionOS + float2(_Time.x, _Time.x) * _WaveSpeed;
                float3 vertexOffset = IN.normal * SimpleNoise(vertexUv, 10) * _WavePower;

                // z座標には動かさない
                vertexOffset.z = 0;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS + vertexOffset);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);


                #if defined(_RECEIVE_DECALS) && (defined(_DBUFFER_MRT1) || defined(_DBUFFER_MRT2) || defined(_DBUFFER_MRT3))
                half3 decal = half3(color.r, color.g, color.b);
                ApplyDecalToBaseColor(IN.positionHCS, decal);
                color.r = decal.r;
                color.g = decal.g;
                color.b = decal.b;
                #endif


                color *= _BaseColor;

                // フレネルエフェクトを掛ける
                float fresnel = FresnelEffect(IN.normal, UNITY_MATRIX_V[2].xyz, _FresnelPower);

                // フレネルにノイズを重ねる
                fresnel *= SimpleNoise(IN.screenPos, 10);

                fresnel *= _FresnelColor.a;

                color = lerp(color, _FresnelColor, fresnel);

                // // 影係数を計算
                // float4 shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
                // Light mainLight = GetMainLight(shadowCoord);
                // half shadowAttention = mainLight.shadowAttenuation;
                //
                // color *= shadowAttention;

                color += _AdditionalColor;

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Stencil
            {
                Ref 1
                Comp NotEqual // 本体ピクセルはスキップ
                Pass Keep
            }

            Cull Front
            ZTest Always
            ZWrite On
            AlphaToMask On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _OutlineWidth;
                float4 _OutlineColor;
                float _FresnelPower;
                float4 _FresnelColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                IN.positionOS.xyz += IN.normal * _OutlineWidth;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 color = _OutlineColor;
                color.a *= 1.0 - step(_OutlineWidth, 0);
                return color;
            }
            ENDHLSL
        }
    }
}