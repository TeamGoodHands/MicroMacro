Shader "FX/FBM_DomainWarp_SemicircleHeat"
{
    Properties
    {
        _CoreColor   ("Core Color (RGBA)", Color) = (0.9, 0.4, 0.1, 0.7)
        _HeatColor   ("Heat Rim (RGB)",   Color) = (1.0, 0.8, 0.2, 1)

        _Radius      ("Radius (0..0.5)", Range(0.05, 0.8)) = 0.35
        _EdgeWidth   ("Edge Width", Range(0.001, 0.3)) = 0.08
        _SemiFeather ("Semi Feather", Range(0.0, 0.3)) = 0.03

        _FBMScale    ("FBM Scale", Range(0.5, 10)) = 3.0
        _FBMAmp      ("FBM Amp", Range(0, 0.5)) = 0.12

        _WarpScale   ("Warp Scale", Range(0.5, 10)) = 3.0
        _WarpAmp     ("Warp Amp (UV)", Range(0, 0.25)) = 0.06
        _WarpSpeed   ("Warp Speed", Range(0, 6)) = 1.8

        _Dir         ("Direction (xy)", Vector) = (1,0,0,0)
        _BowOffset   ("Bow Offset (+forward)", Range(-0.5, 0.5)) = 0.1
        _Stretch     ("Forward Stretch", Range(0, 1.5)) = 0.35

        _ShockWidth  ("Shock Rim Width", Range(0.001, 0.3)) = 0.06
        _ShockStrength ("Shock Strength", Range(0, 2)) = 1.0

        _FillAlpha   ("Fill Alpha", Range(0,1)) = 1
        _EdgeAlpha   ("Edge Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes{
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings{
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _CoreColor;
                float4 _HeatColor;
                float  _Radius, _EdgeWidth, _SemiFeather;
                float  _FBMScale, _FBMAmp;
                float  _WarpScale, _WarpAmp, _WarpSpeed;
                float4 _Dir; // xy使用
                float  _BowOffset, _Stretch;
                float  _ShockWidth, _ShockStrength;
                float  _FillAlpha, _EdgeAlpha;
            CBUFFER_END

            // ---- ノイズ & FBM ----
            float hash21(float2 p){
                p = frac(p*float2(123.34,456.21));
                p += dot(p, p+45.32);
                return frac(p.x*p.y);
            }
            float noise(float2 p){
                float2 i=floor(p), f=frac(p);
                float a=hash21(i);
                float b=hash21(i+float2(1,0));
                float c=hash21(i+float2(0,1));
                float d=hash21(i+float2(1,1));
                float2 u=f*f*(3-2*f);
                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }
            float fbm4(float2 p){
                float a=0, w=0.5;
                [unroll] for(int i=0;i<4;i++){
                    a += noise(p)*w;
                    p = p*2.015 + 17.13;
                    w *= 0.5;
                }
                return a;
            }

            Varyings vert(Attributes IN){
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // 交差SDF: 半平面(進行方向側) ∩ 円
            // dCircle: 円の外側で正、内側で負
            // dHalf  : 半平面の外側で正、内側で負
            // 交差は max(dCircle, dHalf)
            half4 frag(Varyings IN) : SV_Target
            {
                // UV中心化
                float2 uv = IN.uv*2.0 - 1.0;

                // ドメインワープ（揺らぎ）
                float t = _Time.y*_WarpSpeed;
                float2 w; 
                w.x = fbm4(uv*_WarpScale + float2( 1.7, 9.2) + t);
                w.y = fbm4(uv*_WarpScale + float2(-4.3, 2.6) + t);
                uv += (w-0.5)*2.0*_WarpAmp;

                // 進行方向座標系へ（dir軸 x、垂直軸 y）
                float2 dir = normalize(_Dir.xy + 1e-6);
                float2 perp = float2(-dir.y, dir.x);
                float2 p;
                p.x = dot(uv, dir);
                p.y = dot(uv, perp);

                // 前方へ伸ばす（流線方向の押しつぶし/引き伸ばし）
                p.x /= (1.0 + _Stretch);

                // ボウショックの張り出し: 円心を前方へ
                float2 center = float2(_BowOffset, 0);
                float  dCircle = length(p - center) - _Radius;

                // 半平面（進行方向側 = p.x >= 0）
                float dHalf = -p.x; // p.x>=0で負 → 内側（OK）

                // 交差SDF
                float d = max(dCircle, dHalf);

                // FBMで半径いびつ化（角度依存）
                float ang = atan2(p.y, p.x);
                float2 angv = float2(cos(ang), sin(ang));
                float irregular = (fbm4(angv*_FBMScale + t*0.5)-0.5)*2.0*_FBMAmp;

                d -= irregular; // 負側（内部）を広げたり狭めたり

                // ソフト境界
                float edge = _EdgeWidth;
                float fillMask = 1.0 - smoothstep(0.0, edge, d);
                float edgeMask = smoothstep(-edge, 0.0, d) * (1.0 - smoothstep(0.0, edge, d));

                // 前縁の圧縮熱リム（円の“風上側”に寄るほど強い）
                // 目安として、dCircle≈0 かつ 進行方向正側（p.x大きい）を強調
                float rim = 1.0 - smoothstep(0.0, _ShockWidth, abs(dCircle));
                float facing = saturate( (p.x - center.x) / (_Radius + 1e-4) ); // 0..1（前縁寄りほど1）
                float shock = rim * facing * _ShockStrength;

                // 色合成
                float3 baseCol = _CoreColor.rgb;
                float3 heatCol = _HeatColor.rgb;
                float3 col = lerp(baseCol, heatCol, saturate(edgeMask*0.7 + shock));

                // アルファ合成（中身＋縁＋ショック）
                float aFill = fillMask * _FillAlpha * _CoreColor.a;
                float aEdge = edgeMask * _EdgeAlpha;
                float aShock = shock * 0.9; // リムは発光寄りに薄く（ポストのブルームで伸ばす）
                float alpha = saturate(max(aFill, max(aEdge, aShock)));

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
