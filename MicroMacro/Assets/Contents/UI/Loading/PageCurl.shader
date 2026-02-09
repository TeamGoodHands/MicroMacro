Shader "Hagemi/PageCurlDiagonal"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        _CurlAmount ("Curl Amount", Range(0, 1.5)) = 0.0
        _Radius ("Curl Radius", Float) = 0.5
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.2
        _Angle ("Curl Angle", Range(-180, 180)) = 30.0

        [Header(Mask Dissolve)]
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskThreshold ("Mask Threshold", Range(0, 1)) = 0.0
        [Toggle] _UseMask ("Use Mask", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        Cull Off

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode"="DepthNormals"
            }

            Cull Off

            HLSLPROGRAM
            #pragma vertex   dn_vert
            #pragma fragment dn_frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MaskTex_ST;
                float _CurlAmount;
                float _MaskThreshold;
                float _UseMask;
            CBUFFER_END

            struct A
            {
                float4 positionOS: POSITION;
                float3 normalOS: NORMAL;
                float4 tangentOS: TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct V
            {
                float4 positionHCS: SV_POSITION;
                float3 normalWS: TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            V dn_vert(A v)
            {
                V o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.positionHCS = p.positionCS;
                o.normalWS = n.normalWS;
                o.uv = v.uv;
                return o;
            }

            half4 dn_frag(V i) : SV_Target
            {
                if (_UseMask > 0.5)
                {
                    half maskValue = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv).r;
                    // しきい値より大きい部分をクリップ（白い部分から消える）
                    // maskValue > _MaskThreshold の場合、結果が負になりclipされる
                    clip(_MaskThreshold - maskValue);
                }

                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ScreenSpaceHatchingCutoutPrepass"
            Tags
            {
                "LightMode"="ScreenSpaceHatchingCutoutPrepass"
            }
            ZWrite On Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MaskTex_ST;
                float _CurlAmount;
                float _MaskThreshold;
                float _UseMask;
            CBUFFER_END

            struct A
            {
                float4 positionOS: POSITION;
                float3 normalOS: NORMAL;
                float4 tangentOS: TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct V
            {
                float4 positionHCS: SV_POSITION;
                float3 normalWS: TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            V vert(A v)
            {
                V o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.positionHCS = p.positionCS;
                o.normalWS = n.normalWS;
                o.uv = v.uv;
                return o;
            }

            half4 frag(V i) : SV_Target
            {
                if (_UseMask > 0.5)
                {
                    half maskValue = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv).r;
                    // しきい値より大きい部分をクリップ（白い部分から消える）
                    // maskValue > _MaskThreshold の場合、結果が負になりclipされる
                    clip(_MaskThreshold - maskValue);
                }

                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "HandwriteOutlineCutoutPrepass"
            Tags
            {
                "LightMode" = "HandwriteOutlineCutoutPrepass"
            }

            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Ensure PI is defined if not already included by Core.hlsl (it usually is, but just in case)
            #ifndef PI
            #define PI 3.14159265359
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MaskTex_ST;
                float _CurlAmount;
                float _Radius;
                float _ShadowIntensity;
                float _Angle;
                float _MaskThreshold;
                float _UseMask;
            CBUFFER_END

            // 回転行列を適用するヘルパー関数
            float2 Rotate(float2 p, float angleDegrees)
            {
                float rad = angleDegrees * PI / 180.0;
                float s, c;
                sincos(rad, s, c);
                return float2(
                    p.x * c - p.y * s,
                    p.x * s + p.y * c
                );
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 pos = IN.positionOS.xyz;

                float2 rotatedPos = Rotate(pos.xy, _Angle);

                float xNormalized = rotatedPos.x + 0.5;
                float pivot = 1.0 - _CurlAmount * 2.0;
                float angle = (xNormalized - pivot) / _Radius;

                if (xNormalized > pivot)
                {
                    float3 curledPos;
                    curledPos.x = pivot + _Radius * sin(angle);
                    curledPos.y = rotatedPos.y;
                    curledPos.z = -_Radius * (1.0 - cos(angle));

                    curledPos.x -= 0.5;

                    float2 originalAnglePos = Rotate(curledPos.xy, -_Angle);

                    pos.x = originalAnglePos.x;
                    pos.y = originalAnglePos.y;
                    pos.z = curledPos.z;
                }

                OUT.positionHCS = TransformObjectToHClip(pos);
                return OUT;
            }

            half4 frag() : SV_Target
            {
                return float4(0, 0, 0, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "PageCurlPass"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float shadow : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MaskTex_ST;
                float _CurlAmount;
                float _Radius;
                float _ShadowIntensity;
                float _Angle;
                float _MaskThreshold;
                float _UseMask;
            CBUFFER_END

            // 回転行列を適用するヘルパー関数
            float2 Rotate(float2 p, float angleDegrees)
            {
                float rad = angleDegrees * PI / 180.0;
                float s, c;
                sincos(rad, s, c);
                return float2(
                    p.x * c - p.y * s,
                    p.x * s + p.y * c
                );
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.shadow = 0.0;
                float3 pos = input.positionOS.xyz;

                float2 rotatedPos = Rotate(pos.xy, _Angle);

                float xNormalized = rotatedPos.x + 0.5;
                float pivot = 1.0 - _CurlAmount * 2.0;
                float angle = (xNormalized - pivot) / _Radius;

                if (xNormalized > pivot)
                {
                    float3 curledPos;
                    curledPos.x = pivot + _Radius * sin(angle);
                    curledPos.y = rotatedPos.y;
                    curledPos.z = -_Radius * (1.0 - cos(angle));

                    curledPos.x -= 0.5;

                    float2 originalAnglePos = Rotate(curledPos.xy, -_Angle);

                    pos.x = originalAnglePos.x;
                    pos.y = originalAnglePos.y;
                    pos.z = curledPos.z;

                    output.shadow = _ShadowIntensity * (1.0 - cos(angle));
                }

                output.positionCS = TransformObjectToHClip(pos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // マスクによるクリッピング
                if (_UseMask > 0.5)
                {
                    half maskValue = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv).r;
                    // しきい値より大きい部分をクリップ（白い部分から消える）
                    clip(_MaskThreshold - maskValue);
                }

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                col.rgb *= 1 - input.shadow * 1.3;
                return col;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}