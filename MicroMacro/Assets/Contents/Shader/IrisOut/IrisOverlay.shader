Shader "UI/IrisOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayColor ("Overlay Color", Color) = (0,0,0,1)

        // 中心：スクリーン正規化(0..1)。左下(0,0) 右上(1,1)
        _Center01 ("Center (Normalized Screen 0-1)", Vector) = (0.5, 0.5, 0, 0)

        // 半径
        _Radius ("Radius", Range(-1,4)) = 1.0

        _EdgeWidth ("Edge Width", Range(0.001, 0.3)) = 0.08

        _FBMScale ("FBM Scale", Range(0.5, 10)) = 3.0
        _FBMAmp ("FBM Amp (Irregularity)", Range(0, 0.5)) = 0.15

        _WarpScale ("Warp Scale", Range(0.5, 10)) = 3.0
        _WarpAmp ("Warp Amp (UV)", Range(0, 0.25)) = 0.06
        _WaveSpeed ("WaveSpeed ", Range(0, 5)) = 1
        _UpdateInterval ("Update Interval (Frame)", Range(0, 60)) = 24
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"
        }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            Name "UI"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _OverlayTex;
            float4 _OverlayTex_ST;

            fixed4 _OverlayColor;
            float4 _Center01;
            float _Radius;

            float _EdgeWidth;

            float _FBMScale;
            float _FBMAmp;

            float _WarpScale;
            float _WarpAmp;
            float _WaveSpeed;
            float _UpdateInterval;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // UI の 0..1 UV
                o.color = v.color;
                return o;
            }


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


            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenSize = _ScreenParams.xy;
                float2 delta = i.uv - _Center01.xy; // 中心からの差（UV）
                float minDim = min(screenSize.x, screenSize.y);
                float2 uv = float2(delta.x * screenSize.x, delta.y * screenSize.y) / minDim * 2.0;

                float frameTime = 1.0 / _UpdateInterval;
                // --- ドメインワープ（UVを時間で揺らす） ---
                float t = (floor(_Time.y / frameTime) * frameTime) * _WaveSpeed;

                float2 warp;
                warp.x = fbm4(uv * _WarpScale + float2(1.7, 9.2));
                warp.y = fbm4(uv * _WarpScale + float2(-4.3, 2.6));
                warp = (warp - 0.5) * 2.0 * _WarpAmp;
                float2 warpedUv = uv + warp;

                // --- 角度依存FBMで半径をいびつに ---
                float ang = atan2(warpedUv.y, warpedUv.x);
                float2 dir = float2(cos(ang), sin(ang));
                float irregular = fbm4(dir * _FBMScale + t * 0.5); // 0..1
                irregular = (irregular - 0.5) * 2.0 * _FBMAmp; // -_FBMAmp.._FBMAmp
                float r = _Radius + irregular;

                // --- SDF的に円の内外を決める ---
                float dist = length(warpedUv);
                float edge = _EdgeWidth;
                float fillMask = smoothstep(r, r + edge, dist);

                float4 color = _OverlayColor;
                color.a = fillMask;

                color *= saturate(tex2D(_OverlayTex, uv * 0.2));

                return color;
            }
            ENDHLSL
        }
    }
}