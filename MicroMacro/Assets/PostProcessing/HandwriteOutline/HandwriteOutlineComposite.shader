Shader "Hidden/HandwriteOutline/Composite"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Overlay"
        }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "OutlineComposite"
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_OutlinePrepassTexture);
            SAMPLER(sampler_OutlinePrepassTexture);

            float _CrayonIntensityPixels;
            float _CrayonScale;
            float _CrayonTimeSpeed;
            float _CrayonTimeStepSize;

            float4 _Stroke1NoiseOffset;
            float4 _Stroke2NoiseOffset;
            float _Stroke2PerpendicularOffsetPixels;

            float _JitterIntensityPixels;
            float _JitterFrequency;

            float _Stroke2Weight;
            float4 _Stroke2ColorTint;

            float _CoreThreshold;
            float _OuterAlphaMultiplier;

            // ------------------ ノイズ関数 ------------------

            float2 Hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
      dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453123);
            }

            float Noise2D(float2 uv)
            {
                float2 integerPart = floor(uv);
                float2 fractionalPart = frac(uv);

                float2 smooth = fractionalPart * fractionalPart * (3.0 - 2.0 * fractionalPart);

                float2 hash00 = Hash2(integerPart + float2(0.0, 0.0));
                float2 hash10 = Hash2(integerPart + float2(1.0, 0.0));
                float2 hash01 = Hash2(integerPart + float2(0.0, 1.0));
                float2 hash11 = Hash2(integerPart + float2(1.0, 1.0));

                float v00 = hash00.x;
                float v10 = hash10.x;
                float v01 = hash01.x;
                float v11 = hash11.x;

                float vX0 = lerp(v00, v10, smooth.x);
                float vX1 = lerp(v01, v11, smooth.x);
                float value = lerp(vX0, vX1, smooth.y);

                return value;
            }

            float FBM(float2 uv, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                [unroll]
                for (int index = 0; index < 8; index++)
                {
                    if (index >= octaves)
                    {
                        break;
                    }

                    value += Noise2D(uv * frequency) * amplitude;
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            float2 Rotate90(float2 v)
            {
                return float2(-v.y, v.x);
            }

            float2 ComputeStrokeOffset(float2 uv, float2 noiseOffset)
            {
                // 離散更新の周期（秒単位）
                float timeValue = floor(_TimeParameters.x / _CrayonTimeStepSize) * _CrayonTimeStepSize * _CrayonTimeSpeed;
                float2 baseUv = uv * _CrayonScale + noiseOffset + timeValue;

                const int octaves = 3; // 固定値（ここだけは許して）

                float baseX = FBM(baseUv, octaves);
                float baseY = FBM(baseUv + float2(37.2, 17.7), octaves);

                float2 baseNoise = float2(baseX, baseY);
                baseNoise = baseNoise * 2.0 - 1.0;

                float2 pixelSize = 1.0 / _ScreenParams.xy;
                float2 baseOffsetPixels = baseNoise * _CrayonIntensityPixels * pixelSize;

                float2 jitterUv = uv * _JitterFrequency + noiseOffset * 1.3 + timeValue * 3.17;
                float jitterX = Noise2D(jitterUv * 1.3);
                float jitterY = Noise2D(jitterUv * 2.1);
                float2 jitterNoise = float2(jitterX, jitterY);
                jitterNoise = jitterNoise * 2.0 - 1.0;
                jitterNoise = sign(jitterNoise);

                float2 jitterOffsetPixels = jitterNoise * _JitterIntensityPixels * pixelSize;

                float2 finalOffset = baseOffsetPixels + jitterOffsetPixels;
                return finalOffset;
            }


            float4 Frag(Varyings inputData) : SV_Target
            {
                float2 uv = inputData.texcoord;

                float4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                // Stroke 1
                float2 stroke1Offset = ComputeStrokeOffset(uv, _Stroke1NoiseOffset.xy);
                float2 stroke1Uv = saturate(uv + stroke1Offset);
                float4 stroke1Sample = SAMPLE_TEXTURE2D_X(_OutlinePrepassTexture, sampler_OutlinePrepassTexture,
      stroke1Uv);

                // Stroke 2
                float2 stroke2OffsetBase = ComputeStrokeOffset(uv, _Stroke2NoiseOffset.xy);

                float2 perpendicularDirection = Rotate90(stroke1Offset);
                float lengthValue = max(length(perpendicularDirection), 1e-5);
                perpendicularDirection /= lengthValue;

                float2 pixelSize = 1.0 / _ScreenParams.xy;
                float2 perpendicularOffsetPixels = perpendicularDirection * _Stroke2PerpendicularOffsetPixels *
                    pixelSize;

                float2 stroke2Offset = stroke2OffsetBase + perpendicularOffsetPixels;

                float2 stroke2Uv = saturate(uv + stroke2Offset);
                float4 stroke2Sample = SAMPLE_TEXTURE2D_X(_OutlinePrepassTexture, sampler_OutlinePrepassTexture,
                                stroke2Uv);

                // 色ブレンド
                float3 stroke1Color = stroke1Sample.rgb * stroke1Sample.a;
                float3 stroke2Tint = _Stroke2ColorTint.rgb;
                float3 stroke2Color = stroke2Sample.rgb * stroke2Tint * stroke2Sample.a * _Stroke2Weight;

                float3 strokeColor = stroke1Color + stroke2Color;

                // 元のアルファ（線のカバー率）
                float alpha1 = stroke1Sample.a;
                float alpha2 = stroke2Sample.a * _Stroke2Weight;
                float rawAlpha = saturate(alpha1 + alpha2 - alpha1 * alpha2);

                // Core / Rim でアルファ整形
                float coreMask = smoothstep(_CoreThreshold, 1.0, rawAlpha);
                float rimMask = saturate(rawAlpha - coreMask);
                float shapedAlpha = coreMask + rimMask * _OuterAlphaMultiplier;

                float3 finalColor = lerp(sceneColor.rgb, strokeColor, shapedAlpha);

                return float4(finalColor, sceneColor.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}