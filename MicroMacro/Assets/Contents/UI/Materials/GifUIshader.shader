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
        // 1. Tags - ���޸�Ϊ֧��͸��
        // "RenderType"="Transparent" ����Unity����һ��͸����ɫ����
        // "Queue"="Transparent" ȷ���������в�͸������֮����Ⱦ��
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            // 2. Blend - ���ǿ���͸��Ч���Ĺؼ�
            // ���д������GPU��ν���ǰ������ɫ(Source)�뱳��������ɫ(Destination)��ϡ�
            // SrcAlpha OneMinusSrcAlpha �Ǳ�׼��Alpha���ģʽ��
            Blend SrcAlpha OneMinusSrcAlpha
            
            // �ر����д�룬�Ա���͸�������ڵ�����������͸������
            ZWrite On
            // �رձ����޳��������ϣ����ͼ�����涼�ɼ�
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
                
                // Ϊ�˱���͸���ȼ��͵�������Ȼ����Ⱦ�����ܳ��ְױߣ���
                // ���ǿ��Լ�һ����ֵ�жϡ�
                // ���������ɫ��alphaֵ�ǳ�С����ֱ�Ӷ���������ء�
                clip(finalColor.a - 0.01);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}