Shader "ScalerShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white"{}
        _NoiseMap("NoiseMap Map", 2D) = "white"{}
        [HDR]_BaseColor("Base Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Float) = 0
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
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

                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                float3 worldNormal = TransformObjectToWorldNormal(IN.normal);

                // ノイズ用UV（ローカル座標 + 時間）
                float2 baseUv = IN.positionOS.xy;
                float2 timeOffset = _Time.xx * _WaveSpeed;
                float2 vertexUv = baseUv + timeOffset;

                // カメラ基底（ワールド空間）
                float3 cameraRight = UNITY_MATRIX_V[0].xyz;
                float3 cameraUp = UNITY_MATRIX_V[1].xyz;

                // カメラ方向 & 正面度
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float facing = 1 - saturate(dot(viewDir, worldNormal)); // 正面で1, 斜めで0〜

                // ノイズを -1〜1 に
                float noiseX = SimpleNoise(vertexUv, 10);
                float noiseY = SimpleNoise(vertexUv + float2(19.3, 7.1), 10);

                noiseX = noiseX * 2.0 - 1.0;
                noiseY = noiseY * 2.0 - 1.0;

                float2 offsetXY = float2(noiseX, noiseY) * _WavePower * facing;

                // カメラから見た画面XY方向だけ揺らす（Zは変えない）
                float3 vertexOffsetWS = cameraRight * offsetXY.x + cameraUp * offsetXY.y;

                worldPos += vertexOffsetWS;

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.worldPos = worldPos;

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
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            ZWrite On
            ZTest Always

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            ColorMask 0
        }


        //        Pass
        //        {
        //
        //            Name "HandwriteOutlineCutoutPrepass"
        //            Tags
        //            {
        //                "LightMode" = "HandwriteOutlineCutoutPrepass"
        //            }
        //            
        //            ZTest Always
        //
        //            HLSLPROGRAM
        //            #pragma vertex vert
        //            #pragma fragment frag
        //
        //            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //
        //
        //            struct Attributes
        //            {
        //                float4 positionOS : POSITION;
        //            };
        //
        //            struct Varyings
        //            {
        //                float4 positionHCS : SV_POSITION;
        //            };
        //
        //            Varyings vert(Attributes IN)
        //            {
        //                Varyings OUT;
        //                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        //                return OUT;
        //            }
        //
        //            half4 frag() : SV_Target
        //            {
        //                return float4(0, 0, 0, 0);
        //            }
        //            ENDHLSL
        //        }

        Pass
        {
            Name "HandwriteOutlinePrepass"
            Tags
            {
                "LightMode" = "HandwriteOutlinePrepass"
            }

            Stencil
            {
                Ref 1
                Comp NotEqual // 本体ピクセルはスキップ
                Pass Keep
            }

            Cull Front
            ZWrite On
            ZTest Always
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
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);

                float3 normal = TransformObjectToWorldDir(IN.normal);
                normal = TransformWorldToHClipDir(normal);

                // オブジェクト空間で normal 方向に押し出す
                OUT.positionHCS.xy += normal.xy * _OutlineWidth / unity_CameraProjection._m11;

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}