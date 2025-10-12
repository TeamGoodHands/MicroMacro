Shader "WaterCircleShaderBack"
{
    Properties
    {
        [HDR]_BaseColor ("Base Color (RGBA)", Color) = (0.2, 0.6, 1.0, 0.8)
        [HDR]_EdgeColor ("Edge Color (RGB)", Color) = (0.05, 0.12, 0.20, 1)

        _Radius ("Radius (0..0.5)", Range(0.0, 0.6)) = 0.35
        _EdgeWidth ("Edge Width", Range(0.001, 0.3)) = 0.08

        _FBMScale ("FBM Scale", Range(0.5, 10)) = 3.0
        _FBMAmp ("FBM Amp (Irregularity)", Range(0, 0.5)) = 0.15

        _WarpScale ("Warp Scale", Range(0.5, 10)) = 3.0
        _WarpAmp ("Warp Amp (UV)", Range(0, 0.25)) = 0.06
        _WarpSpeed ("Warp Speed", Range(0, 6)) = 1.5

        _UpdateInterval ("Noise Update Interval (sec)", Range(0.05, 2.0)) = 0.5

        _FillAlpha ("Fill Alpha", Range(0,1)) = 1
        _EdgeAlpha ("Edge Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            ZWrite Off
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EdgeColor;
                float _Radius;
                float _EdgeWidth;

                float _FBMScale;
                float _FBMAmp;

                float _WarpScale;
                float _WarpAmp;
                float _WarpSpeed;

                float _UpdateInterval;

                float _FillAlpha;
                float _EdgeAlpha;
            CBUFFER_END

            // ---- 低コスト疑似ノイズとFBM ----
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                // 2D value noise (bilinear interpolate 4 hashed corners)
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // 固定4オクターブFBM（軽量）
            float fbm4(float2 p)
            {
                float a = 0.0;
                float w = 0.5;
                a += noise(p) * w;
                p = p * 2.01 + 37.2;
                w *= 0.5;
                a += noise(p) * w;
                p = p * 2.02 + 11.7;
                w *= 0.5;
                a += noise(p) * w;
                p = p * 2.03 + 19.3;
                w *= 0.5;
                a += noise(p) * w;
                return a; // 0..1
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // UV中心化（-1..1）
                float2 uv = IN.uv * 2.0 - 1.0;

                // --- ドメインワープ（UVを時間で揺らす） ---
                float t = floor(_Time.y / _UpdateInterval) * _UpdateInterval;

                float2 warp;
                warp.x = fbm4(uv * _WarpScale + float2(1.7, 9.2) + t * _WarpSpeed);
                warp.y = fbm4(uv * _WarpScale + float2(-4.3, 2.6) + t * _WarpSpeed);
                warp = (warp - 0.5) * 2.0 * _WarpAmp;
                uv += warp;

                // --- 角度依存FBMで半径をいびつに ---
                float ang = atan2(uv.y, uv.x);
                float2 dir = float2(cos(ang), sin(ang));
                float irregular = fbm4(dir * _FBMScale + t * 0.5); // 0..1
                irregular = (irregular - 0.5) * 2.0 * _FBMAmp; // -_FBMAmp.._FBMAmp
                float r = _Radius + irregular;

                // --- SDF的に円の内外を決める ---
                float dist = length(uv);
                float edge = _EdgeWidth;
                float fillMask = 1.0 - smoothstep(r, r + edge, dist);
                float edgeMask = smoothstep(r - edge, r, dist) * (1.0 - smoothstep(r, r + edge, dist));

                // 色合成：縁にEdgeColor、内部にBaseColor
                float3 col = lerp(_BaseColor.rgb, _EdgeColor.rgb, edgeMask);

                // 透過：内部はFillAlpha、縁はEdgeAlpha（最大を採用）
                float aFill = saturate(fillMask) * _FillAlpha * _BaseColor.a;
                float aEdge = saturate(edgeMask) * _EdgeAlpha * _EdgeColor.a;
                float alpha = saturate(max(aFill, aEdge));

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}