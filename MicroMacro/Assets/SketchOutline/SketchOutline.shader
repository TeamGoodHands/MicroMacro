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
            float _DepthLo;
            float _DepthHi;
            float _NormalLo;
            float _NormalHi;

            float SobelFloat(
                float s00, float s10, float s20,
                float s01, float s21,
                float s02, float s12, float s22)
            {
                float gx = (s20 + 2.0 * s21 + s22) - (s00 + 2.0 * s01 + s02);
                float gy = (s02 + 2.0 * s12 + s22) - (s00 + 2.0 * s10 + s20);
                
                return sqrt(gx * gx + gy * gy);
            }

            float SobelFloat3(
                float3 s00, float3 s10, float3 s20,
                float3 s01,  float3 s21,
                float3 s02, float3 s12, float3 s22)
            {
                float3 gx = (s20 + 2.0 * s21 + s22) - (s00 + 2.0 * s01 + s02);
                float3 gy = (s02 + 2.0 * s12 + s22) - (s00 + 2.0 * s10 + s20);
                
                // ベクトル勾配の大きさ
                return length(float2(length(gx), length(gy)));
            }

            float3 SampleSceneNormalsRemapped(float2 uv)
            {
                return SampleSceneNormals(uv) * 0.5 + 0.5;
            }

            // しきい値の幅を持たせてスムース化
            float SmoothStep01(float x, float lo, float hi)
            {
                float t = saturate((x - lo) / max(1e-6, (hi - lo)));
                return t; // 0..1
            }

            half4 Frag(Varyings IN) : SV_TARGET
            {
                float2 uv = IN.texcoord;
                float2 texel = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                // _Thickness をピクセル単位ステップに丸める（1以上）
                int stepPx = max(1, (int)round(_Thickness));
                float2 o = texel * stepPx;

                // 3×3 の 8 方向 （8サンプル）
                float2 uv00 = uv + float2(-o.x, -o.y);
                float2 uv10 = uv + float2(0.0, -o.y);
                float2 uv20 = uv + float2(o.x, -o.y);

                float2 uv01 = uv + float2(-o.x, 0.0);
                float2 uv21 = uv + float2(o.x, 0.0);

                float2 uv02 = uv + float2(-o.x, o.y);
                float2 uv12 = uv + float2(0.0, o.y);
                float2 uv22 = uv + float2(o.x, o.y);

                // 深度サンプル
                float d00 = SampleSceneDepth(uv00);
                float d10 = SampleSceneDepth(uv10);
                float d20 = SampleSceneDepth(uv20);
                float d01 = SampleSceneDepth(uv01);
                float d21 = SampleSceneDepth(uv21);
                float d02 = SampleSceneDepth(uv02);
                float d12 = SampleSceneDepth(uv12);
                float d22 = SampleSceneDepth(uv22);

                // 法線サンプル（0..1 に再マップ）
                float3 n00 = SampleSceneNormalsRemapped(uv00);
                float3 n10 = SampleSceneNormalsRemapped(uv10);
                float3 n20 = SampleSceneNormalsRemapped(uv20);
                float3 n01 = SampleSceneNormalsRemapped(uv01);
                float3 n21 = SampleSceneNormalsRemapped(uv21);
                float3 n02 = SampleSceneNormalsRemapped(uv02);
                float3 n12 = SampleSceneNormalsRemapped(uv12);
                float3 n22 = SampleSceneNormalsRemapped(uv22);

                // Sobel（8方向勾配）でエッジ強度算出
                float edge_depth = SobelFloat(d00, d10, d20, d01, d21, d02, d12, d22);
                float edge_normal = SobelFloat3(n00, n10, n20, n01, n21, n02, n12, n22);

                float d = SmoothStep01(edge_depth, _DepthLo, _DepthHi);
                float n = SmoothStep01(edge_normal, _NormalLo, _NormalHi);

                float edge = saturate(max(d, n));
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

            float4 _OutlineColor;
            float _JitterAmpPixels; // ピクセル単位の振幅
            float _JitterScale; // ノイズ空間スケール
            float _JitterSpeed; // 時間スケール
            float _TimeStepSize; // ノイズを更新する感覚
            float _Blend; // 輪郭の合成量 0..1

            TEXTURE2D_X(_EdgeTexture);
            SAMPLER(sampler_BlitTexture);

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

                float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0); // quintic

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
                float sum = 0.0;
                float amp = 0.5;
                float freq = 1.0;

                [unroll] for (int o = 0; o < 4; o++)
                {
                    sum += amp * noise2D(p * freq);
                    freq *= 2.0;
                    amp *= 0.5;
                }
                return sum; // ≈0..1
            }

            float4 Frag(Varyings i) : SV_Target
            {
                // 元画像
                float4 src = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.texcoord);

                // ピクセルサイズ（解像度非依存の揺れ幅にする）
                float2 px = float2(_ScreenParams.z, _ScreenParams.w);

                // 離散更新の周期（秒単位）
                float t = floor(_TimeParameters.x / _TimeStepSize) * _TimeStepSize * _JitterSpeed;

                // fBM で角度場を生成 → 方向ベクトル
                float2 p = i.texcoord * _JitterScale + float2(0.7, -1.1) * t;
                float angle = fbm2D(p) * 6.2831853; // 2π
                float2 dir = float2(cos(angle), sin(angle));

                // UVをピクセル単位でワープ（0なら無効）
                float2 uvJitter = i.texcoord + dir * (_JitterAmpPixels * px);

                // エッジを揺らいだUVで読む
                float edgeA = SAMPLE_TEXTURE2D_X(_EdgeTexture, sampler_BlitTexture, uvJitter).x;

                float2 p2 = p + float2(40, 40);
                float angle2 = fbm2D(p2) * 6.2831853;
                float2 dir2 = float2(cos(angle2), sin(angle2));
                float2 uvJitter2 = i.texcoord + dir2 * (_JitterAmpPixels * px);
                float edgeB = SAMPLE_TEXTURE2D(_EdgeTexture, sampler_LinearClamp, uvJitter2).x;

                float edge = max(edgeA, edgeB);

                // 合成
                float alpha = saturate(_Blend);
                return lerp(src, _OutlineColor, edge * alpha);
            }
            ENDHLSL
        }
    }
}