Shader "Unlit/GifUIshader"
{
    
    Properties
    {
        _Texture1 ("Texture A", 2D) = "white" {}
        _Texture2 ("Texture B", 2D) = "white" {}

        [Header(Animation Settings)]
        _DisplayTime ("Display Time per Frame (s)", Range(0.01, 5.0)) = 0.5

        [Header(Jitter Settings)]
        _JitterStrength ("Jitter Strength", Range(0, 0.1)) = 0.01
        _JitterFrequency ("Jitter Frequency (Hz)", Range(1, 60)) = 10.0
    }
    SubShader
    {
        // For transparent images, change "Opaque" to "Transparent" and "Queue" to "Transparent".
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            // For transparent images, add this line:
            // Blend SrcAlpha OneMinusSrcAlpha

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

            // Link to properties defined above
            sampler2D _Texture1;
            sampler2D _Texture2;
            float _DisplayTime;
            float _JitterStrength;
            float _JitterFrequency;

            // A simple hash function to generate a pseudo-random number from a seed.
            // This is the core of our "randomness".
            float random(float seed)
            {
                return frac(sin(seed) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                // --- Jitter Calculation (moved to vertex shader for efficiency) ---
                float2 jitterOffset = float2(0, 0);
                if (_JitterStrength > 0)
                {
                    // Create a "seed" that changes over time based on frequency.
                    // floor() makes the value step, creating a jerky motion instead of a smooth one.
                    float timeSeed = floor(_Time.y * _JitterFrequency);
                    
                    // Generate two different pseudo-random numbers for X and Y.
                    // We map the 0..1 range to -1..1.
                    float offsetX = (random(timeSeed) - 0.5) * 2.0;
                    float offsetY = (random(timeSeed * 1.234) - 0.5) * 2.0; // Use a slightly different seed for Y
                    
                    jitterOffset = float2(offsetX, offsetY) * _JitterStrength;
                }
                
                o.uv = v.uv + jitterOffset;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // --- Animation Timing Calculation ---
                
                // Total duration for one full cycle (Texture A -> Texture B)
                float cycleDuration = _DisplayTime * 2.0;
                
                // Find the current time within the cycle using the modulo operator (fmod)
                float timeInCycle = fmod(_Time.y, cycleDuration);
                
                // Determine which texture to show.
                // step(edge, x) returns 0 if x < edge, and 1 if x >= edge.
                // This is a very efficient way to do an if/else on the GPU.
                float activeIndex = step(_DisplayTime, timeInCycle);

                // --- Texture Sampling and Selection ---
                
                // Sample both textures
                fixed4 colA = tex2D(_Texture1, i.uv);
                fixed4 colB = tex2D(_Texture2, i.uv);
                
                // Use lerp to select the active texture's color
                fixed4 finalColor = lerp(colA, colB, activeIndex);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}