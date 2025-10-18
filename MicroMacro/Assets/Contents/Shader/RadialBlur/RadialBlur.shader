
Shader "Hidden/PostProcessing/RadialBlur"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Radial Blur"
            
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local_fragment _ USE_DITHER
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            //X: サンプリング回数。
            //Y: サンプリング回数の逆数。
            //Z: サンプリング回数-1の逆数。
            half3 _SampleCountParams;
            
            half _Intensity;
            half2 _RadialCenter;
            
            half4 Frag (Varyings input) : SV_Target
            {
                half4 output = 0;
                const float2 uv = input.texcoord - _RadialCenter;
                const half sampleCount = _SampleCountParams.x;
                const half rcpSampleCount = _SampleCountParams.y;

                #if defined(USE_DITHER)//ディザリングっぽくぼかす場合。
                half dither = InterleavedGradientNoise(input.positionCS.xy, 0);
                #else
                const half rcpSampleCountMinusOne = _SampleCountParams.z;
                #endif
                
                for (int i = 0; i < sampleCount; i++)
                {
                    #if defined(USE_DITHER)
                    float t = (i + dither) * rcpSampleCount;//i枚目とi+1枚目をディザリング。
                    #else
                    float t =  i * rcpSampleCountMinusOne;
                    #endif
                    
                    output += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv * lerp(1, 1 - _Intensity, t) + _RadialCenter);
                }
                output *= rcpSampleCount;
                
                return output;
            }

            ENDHLSL
        }
    }
}
