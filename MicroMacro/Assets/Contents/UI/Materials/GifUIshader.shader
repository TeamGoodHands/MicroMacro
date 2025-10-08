Shader "Unlit/GifUIshader"
// Self-contained unlit shader that animates between two textures (like a GIF)
// and applies a pseudo-random jitter, all without needing an external script.
// VERSION 2.0 - NOW SUPPORTS ALPHA TRANSPARENCY

{
    Properties
    {
        _Texture1 ("Texture A (PNG)", 2D) = "white" {}
        _Texture2 ("Texture B (PNG)", 2D) = "white" {}

        [Header(Animation Settings)]
        _DisplayTime ("Display Time per Frame (s)", Range(0.01, 5.0)) = 0.5

        [Header(Jitter Settings)]
        _JitterStrength ("Jitter Strength", Range(0, 0.1)) = 0.01
        _JitterFrequency ("Jitter Frequency (Hz)", Range(1, 60)) = 10.0
    }
    SubShader
    {
        // 1. Tags - 已修改为支持透明
        // "RenderType"="Transparent" 告诉Unity这是一个透明着色器。
        // "Queue"="Transparent" 确保它在所有不透明物体之后被渲染。
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            // 2. Blend - 这是开启透明效果的关键
            // 这行代码告诉GPU如何将当前像素颜色(Source)与背景像素颜色(Destination)混合。
            // SrcAlpha OneMinusSrcAlpha 是标准的Alpha混合模式。
            Blend SrcAlpha OneMinusSrcAlpha
            
            // 关闭深度写入，以避免透明物体遮挡其后面的其他透明物体
            ZWrite Off
            // 关闭背面剔除，如果你希望贴图的两面都可见
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Texture1;
            sampler2D _Texture2;
            float _DisplayTime;
            float _JitterStrength;
            float _JitterFrequency;

            float random(float seed)
            {
                return frac(sin(seed) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                float2 jitterOffset = float2(0, 0);
                if (_JitterStrength > 0)
                {
                    float timeSeed = floor(_Time.y * _JitterFrequency);
                    float offsetX = (random(timeSeed) - 0.5) * 2.0;
                    float offsetY = (random(timeSeed * 1.234) - 0.5) * 2.0;
                    jitterOffset = float2(offsetX, offsetY) * _JitterStrength;
                }
                
                o.uv = v.uv + jitterOffset;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float cycleDuration = _DisplayTime * 2.0;
                float timeInCycle = fmod(_Time.y, cycleDuration);
                float activeIndex = step(_DisplayTime, timeInCycle);

                fixed4 colA = tex2D(_Texture1, i.uv);
                fixed4 colB = tex2D(_Texture2, i.uv);
                
                fixed4 finalColor = lerp(colA, colB, activeIndex);
                
                // 为了避免透明度极低的像素仍然被渲染（可能出现白边），
                // 我们可以加一个阈值判断。
                // 如果最终颜色的alpha值非常小，就直接丢弃这个像素。
                clip(finalColor.a - 0.01);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}