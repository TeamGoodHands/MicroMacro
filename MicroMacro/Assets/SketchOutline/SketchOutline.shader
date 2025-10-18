Shader "Hidden/Custom/EdgeDetectionOutline"
{
    Properties {}

    SubShader
    {
        Pass
        {
            Name "EdgeDetection"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float4 _OutlineColor;
            float _Thickness;
            float _DepthThreshold;
            float _Blend;

            float2 PixelStep()
            {
                float px = max(1.0, _Thickness);
                return float2(_BlitTexture_TexelSize.x * px, _BlitTexture_TexelSize.y * px);
            }

            float LinearEyeDepthAt(float2 uv)
            {
                float raw = SampleSceneDepth(uv);
                return LinearEyeDepth(raw, _ZBufferParams);
            }

            float EdgeFromDepthSobel(float2 uv)
            {
                float2 stepUV = PixelStep();

                float d00 = LinearEyeDepthAt(uv + float2(-stepUV.x, -stepUV.y));
                float d01 = LinearEyeDepthAt(uv + float2(0, -stepUV.y));
                float d02 = LinearEyeDepthAt(uv + float2(stepUV.x, -stepUV.y));

                float d10 = LinearEyeDepthAt(uv + float2(-stepUV.x, 0));
                float d11 = LinearEyeDepthAt(uv);
                float d12 = LinearEyeDepthAt(uv + float2(stepUV.x, 0));

                float d20 = LinearEyeDepthAt(uv + float2(-stepUV.x, stepUV.y));
                float d21 = LinearEyeDepthAt(uv + float2(0, stepUV.y));
                float d22 = LinearEyeDepthAt(uv + float2(stepUV.x, stepUV.y));

                float gx = (d02 + 2.0 * d12 + d22) - (d00 + 2.0 * d10 + d20);
                float gy = (d20 + 2.0 * d21 + d22) - (d00 + 2.0 * d01 + d02);

                float g = sqrt(gx * gx + gy * gy);
                float edge = smoothstep(_DepthThreshold * 0.5, _DepthThreshold, g);

                return saturate(edge);
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float edge = EdgeFromDepthSobel(i.texcoord);
                return edge * _Blend;
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
            float _Thickness;
            float _DepthThreshold;
            float _Blend;

            TEXTURE2D_X(_EdgeTexture);

            SAMPLER(sampler_BlitTexture);

            float4 Frag(Varyings i) : SV_Target
            {
                float4 src = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.texcoord);
                float edge = SAMPLE_TEXTURE2D(_EdgeTexture, sampler_LinearClamp, i.texcoord);

                return lerp(src, _OutlineColor, edge);
            }
            ENDHLSL
        }
    }
}