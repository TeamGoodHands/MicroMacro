Shader "Sketch/ScreenSpaceOutline"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        ZWrite Off ZTest Always Cull Off

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            // Blit の入力（URPが供給）
            SAMPLER(sampler_BlitTexture);

            // 紙テク
            TEXTURE2D(_PaperTex);
            SAMPLER(sampler_PaperTex);

            float4 _EdgeColor;
            float _EdgeWidth;
            float _EdgeThreshold;
            float _WobbleAmplitude;
            float _WobbleFrequency;
            float _NoiseScale;
            float _QuantizeStepSeconds;

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float noise2d(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                float a = hash21(i), b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1)), d = hash21(i + float2(1, 1));
                float2 u = f * f * (3 - 2 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float luma(float3 c) { return dot(c, float3(0.299, 0.587, 0.114)); }

            float sobelLuma(float2 uv, float2 texel)
            {
                float tl = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2(-1,-1)));
                float l = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2(-1, 0)));
                float bl = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2(-1, 1)));
                float t = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2( 0,-1)));
                float c = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv));
                float b = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2( 0, 1)));
                float tr = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2( 1,-1)));
                float r = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2( 1, 0)));
                float br = luma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + texel*float2( 1, 1)));
                float gx = (tr + 2 * r + br) - (tl + 2 * l + bl);
                float gy = (bl + 2 * b + br) - (tl + 2 * t + tr);
                return sqrt(gx * gx + gy * gy);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 texel = _BlitTexture_TexelSize.xy * max(_EdgeWidth, 1);

                // コマ撮り風の時間量子化
                float t = _Time.y;
                if (_QuantizeStepSeconds > 0) t = floor(t / _QuantizeStepSeconds) * _QuantizeStepSeconds;

                // UVを少し“手ぶれ”
                float n = noise2d(uv * _NoiseScale + t * _WobbleFrequency);
                float2 jitter = (n - 0.5) * _WobbleAmplitude * texel * 2.0;

                float edge = sobelLuma(uv + jitter, texel);
                float m = saturate((edge - _EdgeThreshold) * 4.0);

                float4 src = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                float paper = SAMPLE_TEXTURE2D(_PaperTex, sampler_PaperTex, uv).r;
                float inkMul = lerp(0.75, 1.0, paper);

                float4 ink = float4(_EdgeColor.rgb * inkMul, _EdgeColor.a * m);

                return src;
                // アウトラインを“上描き”
                return lerp(src, float4(ink.rgb, 1), ink.a);
            }
            ENDHLSL
        }
    }
}