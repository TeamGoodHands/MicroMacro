
float Gaussian(float standardDeviation, float offset)
{
    float a = 1.0 / (sqrt(TWO_PI) * standardDeviation);
    
    float offsetSq = offset * offset;
    float standardDeviationSq = standardDeviation * standardDeviation;
    float b = exp(-(offsetSq / (2.0 * standardDeviationSq)));
    
    return a * b;
}

float GaussianBlur(float2 uv, float2 dir, int kernelRadius, float standardDeviation, TEXTURE2D_X(textureToBlur), SAMPLER(sampler_TextureToBlur), float2 textureToBlurTexelSizeXy)
{
    float2 texelSizeTimesDir = textureToBlurTexelSizeXy * dir;
    float result = 0.0;

    UNITY_LOOP
    for (int i = -kernelRadius; i <= kernelRadius; ++i)
    {
        float2 uvOffset = (float)i * texelSizeTimesDir;
        float2 uvSample = uv + uvOffset;
        float t = SAMPLE_TEXTURE2D_X(textureToBlur, sampler_TextureToBlur, uvSample).r;
        result += t * Gaussian(standardDeviation, (float)i);
    }

    return result;
}