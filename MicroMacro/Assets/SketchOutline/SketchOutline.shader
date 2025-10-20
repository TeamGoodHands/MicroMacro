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

            float4 _OutlineColor;
            float _JitterAmpPixels; // ピクセル単位の振幅
            float _JitterScale; // ノイズ空間スケール
            float _JitterSpeed; // 時間スケール
            float _Blend; // 輪郭の合成量 0..1

            TEXTURE2D_X(_EdgeTexture);
            SAMPLER(sampler_BlitTexture);

            // --- 追加：軽量ノイズ & fBM（4オクターブ） ---
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

                // 時間
                float t = _TimeParameters.x * _JitterSpeed;

                // fBM で角度場を生成 → 方向ベクトル
                float2 p = i.texcoord * _JitterScale + float2(0.7, -1.1) * t;
                float angle = fbm2D(p) * 6.2831853; // 2π
                float2 dir = float2(cos(angle), sin(angle));

                // UVをピクセル単位でワープ（0なら無効）
                float2 uvJitter = i.texcoord + dir * (_JitterAmpPixels * px);

                // エッジを“揺らいだUV”で読む
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