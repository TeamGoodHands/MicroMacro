
Shader "FX/Shadertoy_FBM_DomainWarp_URP"
{
    Properties
    {
        _Color1     ("Mix Color 1", Color) = (0.8, 0.35, 0.12, 1)
        _Color2     ("Mix Color 2", Color) = (0.3, 0.75, 0.69, 1)

        _Scale      ("UV Scale", Range(0.1, 10)) = 1.0
        _QScale     ("q Scale (fbm strength)", Range(0.0, 8.0)) = 1.0
        _RScale     ("r Scale (domain warp)", Range(0.0, 8.0)) = 4.0

        _SpeedQ     ("q Time Speed", Range(0.0, 3.0)) = 0.0
        _SpeedRX    ("r.x Time Speed", Range(0.0, 3.0)) = 0.15
        _SpeedRY    ("r.y Time Speed", Range(0.0, 3.0)) = 0.12

        _CoefA      ("f^3 coef", Range(0, 2)) = 1.0
        _CoefB      ("f^2 coef", Range(0, 2)) = 0.6
        _CoefC      ("f coef",   Range(0, 2)) = 0.5
        _Intensity  ("Final Intensity", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color1;
                float4 _Color2;

                float  _Scale;
                float  _QScale;
                float  _RScale;

                float  _SpeedQ;
                float  _SpeedRX;
                float  _SpeedRY;

                float  _CoefA;
                float  _CoefB;
                float  _CoefC;
                float  _Intensity;
            CBUFFER_END

            // ====== Shadertoy -> HLSL 変換関数群 ======
            // random(st) 相当
            float random(float2 st)
            {
                // GLSL: fract(sin(dot(st, vec2(12.9898,78.233)))*43758.5453123)
                return frac(sin(dot(st, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // noise(st) 相当（Value Noise）
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                float a = random(i + float2(0.0, 0.0));
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                // u = f*f*(3-2f)
                float2 u = f * f * (3.0 - 2.0 * f);

                // mix(a,b,u.x) + (c-a)*u.y*(1-u.x) + (d-b)*u.x*u.y
                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            // fbm(st) 相当（NUM_OCTAVES=5）
            float fbm(float2 st)
            {
                float v = 0.0;
                float a = 0.5;
                [unroll] for (int i = 0; i < 5; i++)
                {
                    v += a * noise(st);
                    st *= 2.0;
                    a *= 0.5;
                }
                return v;
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
                // Shadertoy: st = fragCoord / iResolution
                // UnityのQuad UV(0..1)をそのまま使う（任意にスケール可）
                float2 st = IN.uv * _Scale;

                // 時間
                float t  = _Time.y;
                float2 tq = t * _SpeedQ * float2(1,1);
                float2 trx = t * _SpeedRX * float2(1,1);
                float2 try_ = t * _SpeedRY * float2(1,1);

                // q
                float2 q = float2(0.0, 0.0);
                q.x = fbm(st + float2(0.0, 0.0) + tq) * _QScale;
                q.y = fbm(st + float2(1.0, 0.0) + tq) * _QScale;

                // r（domain warp の元）
                float2 r = float2(0.0, 0.0);
                r.x = fbm(st + (4.0 * q) + float2(1.7, 9.2) + trx) * _RScale;
                r.y = fbm(st + (4.0 * q) + float2(8.3, 2.8) + try_) * _RScale;

                // 色ブレンド
                float3 color = float3(0,0,0);
                color = lerp(color, _Color1.rgb, saturate(length(q))); // clamp -> saturate
                color = lerp(color, _Color2.rgb, saturate(length(r)));

                // f = fbm(st + 4.0 * r)
                float f = fbm(st + 4.0 * r);

                // coef = f^3 + 0.6 f^2 + 0.5 f（係数は可変化）
                float coef = (_CoefA * f * f * f) + (_CoefB * f * f) + (_CoefC * f);

                color *= coef * _Intensity;

                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
