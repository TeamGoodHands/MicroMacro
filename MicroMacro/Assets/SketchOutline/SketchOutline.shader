Shader "Hidden/Custom/EdgeDetectionOutline"
{
    Properties {}

    SubShader
    {
        Pass
        {
            Name "EdgeDetection"
            Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float4 _OutlineColor;
            float _Thickness;

            float RobertsCross(float3 samples[4])
            {
                const float3 d1 = samples[1] - samples[2];
                const float3 d2 = samples[0] - samples[3];
                return sqrt(dot(d1, d1) + dot(d2, d2));
            }

            float RobertsCross(float samples[4])
            {
                const float d1 = samples[1] - samples[2];
                const float d2 = samples[0] - samples[3];
                return sqrt(d1 * d1 + d2 * d2);
            }

            float3 SampleSceneNormalsRemapped(float2 uv)
            {
                return SampleSceneNormals(uv) * 0.5 + 0.5;
            }

            half4 Frag(Varyings IN) : SV_TARGET
            {
                float2 uv = IN.texcoord;
                float2 texel_size = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                const float half_width_f = floor(_Thickness * 0.5);
                const float half_width_c = ceil(_Thickness * 0.5);

                float2 uvs[4];
                uvs[0] = uv + texel_size * float2(half_width_f, half_width_c) * float2(-1, 1);
                uvs[1] = uv + texel_size * float2(half_width_c, half_width_c) * float2(1, 1);
                uvs[2] = uv + texel_size * float2(half_width_f, half_width_f) * float2(-1, -1);
                uvs[3] = uv + texel_size * float2(half_width_c, half_width_f) * float2(1, -1);

                float3 normal_samples[4];
                float depth_samples[4];

                UNITY_UNROLL
                for (int i = 0; i < 4; i++)
                {
                    depth_samples[i] = SampleSceneDepth(uvs[i]);
                    normal_samples[i] = SampleSceneNormalsRemapped(uvs[i]);
                }

                float edge_depth = RobertsCross(depth_samples);
                float edge_normal = RobertsCross(normal_samples);

                const float depth_threshold = 1.0 / 200.0;
                const float normal_threshold = 1.0 / 4.0;

                edge_depth = edge_depth > depth_threshold ? 1 : 0;
                edge_normal = edge_normal > normal_threshold ? 1 : 0;

                float edge = max(edge_depth, edge_normal);
                return edge;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Composite"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            // ★ 追加: _RcpScreenSize を自前定義（URP 17 は自動で来ない事がある）
            #define _RcpScreenSize float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y)

            float4 _OutlineColor;
            float _JitterAmpPixels; // ピクセル単位の振幅
            float _JitterScale; // ノイズ空間スケール
            float _JitterSpeed; // 時間スケール
            float _CrayonGrainStrength; // 粒子の乗算強度 0..1
            float _CrayonGrainScale; // 粒子サイズ（大きいほど細かい）
            float _CrayonGrainThreshold; // 粒子のしきい値 0..1
            float _Blend; // 輪郭の合成量 0..1

            TEXTURE2D_X(_EdgeTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D_X(_MotionVectorTexture);
            SAMPLER(sampler_MotionVectorTexture);

            TEXTURE2D_X(_SketchHistory);
            SAMPLER(sampler_SketchHistory);

            // --- 低コストノイズたち ---
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
                float a = hash21(i + float2(0, 0));
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float x1 = lerp(a, b, u.x);
                float x2 = lerp(c, d, u.x);
                return lerp(x1, x2, u.y);
            }

            float fbm2D(float2 p)
            {
                float sum = 0.0, amp = 0.5, freq = 1.0;
                [unroll] for (int o = 0; o < 4; o++)
                {
                    sum += amp * noise2D(p * freq);
                    freq *= 2.0;
                    amp *= 0.5;
                }
                return sum;
            }

            float grain2D(float2 p)
            {
                const float2x2 R = float2x2(0.8660254, -0.5, 0.5, 0.8660254);
                p = mul(R, p);
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
                float h00 = frac(sin(dot(i + float2(0, 0), float2(127.1, 311.7))) * 43758.5453);
                float h10 = frac(sin(dot(i + float2(1, 0), float2(127.1, 311.7))) * 43758.5453);
                float h01 = frac(sin(dot(i + float2(0, 1), float2(127.1, 311.7))) * 43758.5453);
                float h11 = frac(sin(dot(i + float2(1, 1), float2(127.1, 311.7))) * 43758.5453);
                float x1 = lerp(h00, h10, u.x);
                float x2 = lerp(h01, h11, u.x);
                return lerp(x1, x2, u.y);
            }

            struct FragOut
            {
                float4 color : SV_Target0;
                float4 hist : SV_Target1;
            };

            FragOut Frag(Varyings i)
            {
                FragOut o;

                // === Motion Vector を UV に換算して逆流し ===
                float2 motionPix = SAMPLE_TEXTURE2D_X(_MotionVectorTexture, sampler_MotionVectorTexture, i.texcoord).xy;
                float2 motionUV = motionPix * _RcpScreenSize; // ★ ピクセル → UV(0..1)
                float2 uvPrev = i.texcoord - motionUV; // ★ 逆投影

                // 前フレーム履歴（R: 粒, G: 揺らぎ）を“貼り付け座標”で読む
                float2 historyRG = SAMPLE_TEXTURE2D_X(_SketchHistory, sampler_SketchHistory, uvPrev).rg;
                float grainPrev = historyRG.r;
                float jitterPrev = historyRG.g;

                // 元フレーム色
                float4 src = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.texcoord);

                // 時間
                float t = _TimeParameters.x * _JitterSpeed;

                // 現フレームの新規ノイズ
                float2 p = i.texcoord * _JitterScale + float2(t * 0.7, -t * 1.1);
                float angle = fbm2D(p) * 6.2831853; // 2π
                float2 dir = float2(cos(angle), sin(angle));
                float grainC = grain2D(i.texcoord * _CrayonGrainScale);
                float jitterC = noise2D(i.texcoord * _JitterScale + t);

                // 履歴ブレンド（貼り付き安定化）
                const float historyWeight = 0.85;
                float grain = lerp(grainC, grainPrev, historyWeight);
                float jitter = lerp(jitterC, jitterPrev, historyWeight);

                // ピクセル基準のオフセットを UV に変換してサンプル座標を揺らす
                float2 jitterUV = dir * ((jitter - 0.5) * _JitterAmpPixels) * _RcpScreenSize;
                // ★ 修正: _RcpScreenSize.xy を使用

                // エッジを“貼り付いたまま”揺らぎUVで読む
                float edge = SAMPLE_TEXTURE2D_X(_EdgeTexture, sampler_LinearClamp, i.texcoord + jitterUV).x;

                // 粒マスクを乗算
                float mask = step(_CrayonGrainThreshold, grain);
                edge *= lerp(1.0, mask, _CrayonGrainStrength);
                edge *= _Blend;

                o.color = lerp(src, _OutlineColor, edge);

                // ★ SV_Target1 に次フレーム用履歴を書き出し（R=粒, G=揺らぎ）
                o.hist = float4(grain, jitter, 0, 1);
                return o;
            }
            ENDHLSL
        }
    }
}