Shader "Hidden/Custom/ScreenSpaceHatching"
{
    Properties {}

    SubShader
    {
        Pass
        {
            Name "ScreenSpaceHatching Compute"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _SamplingRotations[12];
            float _SamplingDistances[12];
            float _OcclusionSampleLength;
            float _OcclusionMinDistance;
            float _OcclusionMaxDistance;
            float _OcclusionBias;
            float _OcclusionStrength;
            float _OcclusionPower;
            float _OcclusionDifferenceThreshold;

            float3 ReconstructViewPositionFromDepth(float2 screenUV, float rawDepth)
            {
                // TODO: depthはgraphicsAPIを考慮している必要があるはず
                float4 clipPos = float4(screenUV * 2.0 - 1.0, rawDepth, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                clipPos.y = -clipPos.y;
                #endif
                float4 viewPos = mul(UNITY_MATRIX_I_P, clipPos);
                return viewPos.xyz / viewPos.w;
            }

            float SampleRawDepth(float2 uv)
            {
                float rawDepth = SAMPLE_DEPTH_TEXTURE_LOD(
                    _CameraDepthTexture,
                    sampler_CameraDepthTexture,
                    UnityStereoTransformScreenSpaceTex(uv),
                    0
                );
                return rawDepth;
            }

            float2x2 GetRotationMatrix(float rad)
            {
                float c = cos(rad);
                float s = sin(rad);
                return float2x2(c, -s, s, c);
            }

            float SampleRawDepthByViewPosition(float3 viewPosition, float3 offset)
            {
                float4 offsetViewPosition = float4(viewPosition + offset, 1.);
                float4 offsetClipPosition = mul(UNITY_MATRIX_P, offsetViewPosition);

                #if UNITY_UV_STARTS_AT_TOP
                offsetClipPosition.y = -offsetClipPosition.y;
                #endif

                // TODO: reverse zを考慮してあるべき？
                float2 samplingCoord = (offsetClipPosition.xy / offsetClipPosition.w) * 0.5 + 0.5;
                float samplingRawDepth = SampleRawDepth(samplingCoord);

                return samplingRawDepth;
            }

            float Frag(Varyings i) : SV_Target
            {
                // 1. depth を depth texture から参照する場合
                // return float4(i.texcoord.r, i.texcoord.g, 1.0, 1.0);
                float rawDepth = SampleRawDepth(i.texcoord);
                float depth = Linear01Depth(rawDepth, _ZBufferParams);
                //return float4(depth, depth, depth, 1.);

                float3 viewPosition = ReconstructViewPositionFromDepth(i.texcoord, rawDepth);

                //return float4(viewPosition, 1.);
                //return float4(rawDepth, rawDepth, rawDepth, 1.);
                //return float4(depth, depth, depth, 1.);

                float eps = .0001;

                // mask exists depth
                if (depth > 1. - eps)
                {
                    return float4(1, 1, 1, 1);
                }

                float occludedAcc = 0.;
                int samplingCount = 12;

                for (int j = 0; j < samplingCount; j++)
                {
                    float2x2 rot = GetRotationMatrix(_SamplingRotations[j]);
                    float offsetLen = _SamplingDistances[j] * _OcclusionSampleLength;
                    float3 offsetA = float3(mul(rot, float2(1, 0)), 0.) * offsetLen;
                    float3 offsetB = -offsetA;

                    float rawDepthA = SampleRawDepthByViewPosition(viewPosition, offsetA);
                    float rawDepthB = SampleRawDepthByViewPosition(viewPosition, offsetB);

                    float3 viewPositionA = ReconstructViewPositionFromDepth(i.texcoord, rawDepthA);
                    float3 viewPositionB = ReconstructViewPositionFromDepth(i.texcoord, rawDepthB);

                    float distA = distance(viewPositionA, viewPosition);
                    float distB = distance(viewPositionB, viewPosition);

                    if (distA < _OcclusionMinDistance || _OcclusionMaxDistance < distA)
                    {
                        continue;
                    }
                    if (distB < _OcclusionMinDistance || _OcclusionMaxDistance < distB)
                    {
                        continue;
                    }

                    if (abs(distA - distB) > _OcclusionDifferenceThreshold)
                    {
                        continue;
                    }

                    // compare with surface to camera
                    float3 surfaceToCameraDir = -normalize(viewPosition);
                    float dotA = dot(normalize(viewPositionA - viewPosition), surfaceToCameraDir);
                    float dotB = dot(normalize(viewPositionB - viewPosition), surfaceToCameraDir);
                    float ao = (dotA + dotB) * .5;

                    occludedAcc += ao;
                }

                float aoRate = occludedAcc / (float)samplingCount;

                float ao = 1 - saturate(pow(saturate(aoRate), _OcclusionPower) * _OcclusionStrength);

                return ao;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Down And Up Sampling"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings IN) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Gaussian Horizontal"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "./GaussianBlur.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            int _BlurKernelRadius;
            float _BlurStandardDeviation;

            float Frag(Varyings i) : SV_Target
            {
                float scale = ((float)_ScreenParams.y / 1440.0);

                return GaussianBlur(i.texcoord, float2(1.0, 0.0), _BlurKernelRadius, _BlurStandardDeviation,
                                    _BlitTexture, sampler_LinearClamp, _BlitTexture_TexelSize.xy * scale);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Gaussian Vertical"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "./GaussianBlur.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            int _BlurKernelRadius;
            float _BlurStandardDeviation;

            float Frag(Varyings i) : SV_Target
            {
                float scale = ((float)_ScreenParams.y / 1440.0);

                return GaussianBlur(i.texcoord, float2(0.0, 1.0), _BlurKernelRadius, _BlurStandardDeviation,
                                    _BlitTexture, sampler_LinearClamp, _BlitTexture_TexelSize.xy * scale);
            }
            ENDHLSL
        }


        Pass
        {
            Name "SSAO Composite"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _BlendStep;
            float _BlendPower;
            float _HatchScale;
            float _FrontHatchOffset;
            float _BackHatchOffset;
            float _HatchOffsetBorder;

            TEXTURE2D_X(_CrossHatchPatternTexture);
            TEXTURE2D_X(_BlurResultTexture);

            SAMPLER(sampler_BlitTexture);

            float4 Frag(Varyings i) : SV_Target
            {
                float depth = SampleSceneDepth(i.texcoord);
                float offset = depth > _HatchOffsetBorder ? _FrontHatchOffset : _BackHatchOffset;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 hatchUv = (i.texcoord - _WorldSpaceCameraPos.xy * offset) * _HatchScale;
                hatchUv.x = (hatchUv.x - 0.5) * aspect + 0.5;

                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.texcoord);
                float blur = SAMPLE_TEXTURE2D(_BlurResultTexture, sampler_LinearClamp, i.texcoord).r;
                float4 hatch = SAMPLE_TEXTURE2D(_CrossHatchPatternTexture, sampler_LinearRepeat, hatchUv);

                float4 stepBlur = step(blur, _BlendStep);
                hatch.r *= stepBlur.r;
                color -= hatch.r * pow(1 - blur.r, _BlendPower);

                return color;
            }
            ENDHLSL

        }
    }
}