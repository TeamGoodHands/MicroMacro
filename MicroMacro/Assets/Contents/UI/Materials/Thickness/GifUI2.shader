Shader "Unlit/GifUI2"

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
        
        [Header(Thickness Settings)]
        [Toggle(_THICKNESS_ON)] _ThicknessToggle("Enable Thickness", Float) = 1
        _ThicknessSize ("Thickness Size", Range(0.0, 0.1)) = 0.03
        _ThicknessDirection ("Thickness Direction", Vector) = (0.5, -0.5, 0, 0)
        _ThicknessColor ("Thickness Tint", Color) = (0.5, 0.5, 0.5, 1) // 默认颜色调得更暗一些
        _ThicknessSteps ("Thickness Steps (Quality)", Range(4, 32)) = 16 // 新增：厚度渲染质量
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma shader_feature _THICKNESS_ON
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _Texture1, _Texture2;
            float _DisplayTime, _JitterStrength, _JitterFrequency;
            float _ThicknessSize;
            float4 _ThicknessDirection, _ThicknessColor;
            int _ThicknessSteps; // 接收步数

            float random(float seed) { return frac(sin(seed) * 43758.5453123); }

            v2f vert(appdata v)
            {
                v2f o;
                float2 jitterOffset = 0;
                if (_JitterStrength > 0) {
                    float timeSeed = floor(_Time.y * _JitterFrequency);
                    float offsetX = (random(timeSeed) - 0.5) * 2.0;
                    float offsetY = (random(timeSeed * 1.234) - 0.5) * 2.0;
                    jitterOffset = float2(offsetX, offsetY) * _JitterStrength;
                }
                o.uv = v.uv + jitterOffset;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float cycleDuration = _DisplayTime * 2.0;
                float timeInCycle = fmod(_Time.y, cycleDuration);
                float activeIndex = step(_DisplayTime, timeInCycle);

                fixed4 mainColorA = tex2D(_Texture1, i.uv);
                fixed4 mainColorB = tex2D(_Texture2, i.uv);
                fixed4 mainColor = lerp(mainColorA, mainColorB, activeIndex);

            #ifdef _THICKNESS_ON
                // --- [ Solid Thickness Raymarching Logic ] ---

                // 1. 计算单步偏移量
                float2 direction = normalize(_ThicknessDirection.xy);
                float2 step = direction * _ThicknessSize / _ThicknessSteps;
                
                fixed4 thicknessLayerColor = fixed4(0, 0, 0, 0);

                // 2. 循环步进
                // 我们从当前像素(i.uv)向厚度反方向步进，寻找实体像素
                [loop]
                for (int j = 1; j <= _ThicknessSteps; j++)
                {
                    float2 sampleUV = i.uv - j * step;
                    
                    fixed4 sampleColorA = tex2D(_Texture1, sampleUV);
                    fixed4 sampleColorB = tex2D(_Texture2, sampleUV);
                    fixed4 sampledColor = lerp(sampleColorA, sampleColorB, activeIndex);
                    
                    // 3. 找到实体像素
                    // 如果在步进过程中找到了一个非透明像素...
                    if (sampledColor.a > 0.5)
                    {
                        // ...说明当前像素(i.uv)位于厚度区域内。
                        // 用厚度颜色乘以采样到的实体颜色，模拟光照反射
                        thicknessLayerColor.rgb = _ThicknessColor.rgb * sampledColor.rgb;
                        // 透明度设为1，表示这是一个坚实的侧面
                        thicknessLayerColor.a = 1.0; 
                        // 找到后立即跳出循环，因为我们只需要最近的表面
                        break; 
                    }
                }

                // 4. 混合顶面和侧面
                // 使用主贴图的alpha通道来混合。
                // 如果主贴图当前像素是透明的(mainColor.a=0)，则完全显示厚度颜色。
                // 如果主贴图当前像素是不透明的(mainColor.a=1)，则完全显示主贴图颜色。
                // 这会将主贴图"盖"在厚度层之上。
                fixed4 finalColor = lerp(thicknessLayerColor, mainColor, mainColor.a);
                
                // 剔除所有完全透明的区域
                clip(finalColor.a - 0.01);
                return finalColor;

            #else
                // 如果厚度关闭，直接返回主颜色
                clip(mainColor.a - 0.01);
                return mainColor;
            #endif
            }
            ENDCG
        }
    }
}
