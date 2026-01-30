Shader "Custom/URP_GodRay_Raymarching_Robust_BottomFade"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1, 1, 0.8, 1)
        _Density("Fog Density", Range(0, 50)) = 20.0
        
        [Header(Shape)]
        _TopScale("Top Scale", Range(0.0, 1.5)) = 0.2
        
        [Header(Quality)]
        [IntRange] _Steps("Step Count", Range(4, 100)) = 30
        
        [Header(Fading)]
        _EdgeSoftness("Edge Softness", Range(0.01, 1.0)) = 0.2
        _Softness("Soft Particle Factor", Range(0.01, 5.0)) = 1.0
        _CamFadeDist("Camera Near Fade", Range(0.0, 1.0)) = 0.2
        
        // ★変更：底面（下側）のフェードの高さ
        _BottomFadeHeight("Bottom Fade Height", Range(0.0, 1.0)) = 0.2
        
        // 合成モード設定
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 10 // OneMinusSrcAlpha
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 1 
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
        }

        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        Cull [_Cull]

        Pass
        {
            Name "RaymarchingRobustPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float4 screenPos  : TEXCOORD1;
                float3 positionOS : TEXCOORD5;
                float3 cameraPosOS : TEXCOORD6;
                float2 uv         : TEXCOORD7;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Density;
                float _TopScale;
                int _Steps;
                float _EdgeSoftness;
                float _Softness;
                float _CamFadeDist;
                float _Cull;
                float _BottomFadeHeight; // ★変更
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // --- メッシュ変形 ---
                float3 pos = input.positionOS.xyz;
                float h = pos.y + 0.5; // 0.0 ~ 1.0
                float scale = lerp(1.0, _TopScale, h);
                pos.x *= scale;
                pos.z *= scale;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(pos);
                output.positionCS = vertexInput.positionCS;
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                output.positionOS = pos;
                output.color = input.color;
                output.uv = input.uv;

                float3 worldCameraPos = GetCameraPositionWS();
                output.cameraPosOS = TransformWorldToObject(worldCameraPos);
                
                return output;
            }

            float2 IntersectAABB(float3 ro, float3 rd, float3 boxMin, float3 boxMax)
            {
                float3 tMin = (boxMin - ro) / (rd + 1e-5);
                float3 tMax = (boxMax - ro) / (rd + 1e-5);
                float3 t1 = min(tMin, tMax);
                float3 t2 = max(tMin, tMax);
                float tNear = max(max(t1.x, t1.y), t1.z);
                float tFar = min(min(t2.x, t2.y), t2.z);
                return float2(tNear, tFar);
            }

            float GetDensity(float3 p)
            {
                if (abs(p.y) > 0.5) return 0.0;

                float h = p.y + 0.5;
                float currentScale = lerp(1.0, _TopScale, h);
                float width = 0.5 * currentScale;

                float distX = abs(p.x);
                float distZ = abs(p.z);
                
                if (distX > width || distZ > width) return 0.0;

                float fadeX = smoothstep(0.0, _EdgeSoftness * width, width - distX);
                float fadeZ = smoothstep(0.0, _EdgeSoftness * width, width - distZ);

                return fadeX * fadeZ;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 ro = input.cameraPosOS;
                float3 endPos = input.positionOS;
                float3 rayVec = endPos - ro;
                float fullLen = length(rayVec);
                float3 rd = rayVec / fullLen;

                float2 tBox = IntersectAABB(ro, rd, float3(-0.5,-0.5,-0.5), float3(0.5,0.5,0.5));
                float tStart = max(0.0, tBox.x);
                float tEnd = min(fullLen, tBox.y);

                if (tStart >= tEnd) discard;

                float marchDist = tEnd - tStart;
                float stepSize = marchDist / float(_Steps);
                float3 currentPos = ro + rd * tStart;
                
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float noise = frac(sin(dot(screenUV, float2(12.9898, 78.233))) * 43758.5453);
                currentPos += rd * stepSize * noise; 

                float accumulate = 0.0;

                for(int i = 0; i < _Steps; i++)
                {
                    accumulate += GetDensity(currentPos);
                    currentPos += rd * stepSize;
                }

                float alpha = accumulate * stepSize * _Density;

                // --- 深度・カメラ距離フェード ---
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneLinearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float pixelLinearDepth = input.positionCS.w;
                float depthFade = saturate((sceneLinearDepth - pixelLinearDepth) / _Softness);
                alpha *= depthFade;

                float cameraFade = saturate(pixelLinearDepth - _CamFadeDist);
                alpha *= cameraFade;

                // ★変更：UVのBottom（Y=0付近）のフェード処理
                // uv.y が 0.0（底面）から _BottomFadeHeight にかけて
                // 0.0（透明） -> 1.0（不透明） に変化する
                float bottomFade = smoothstep(0.0, _BottomFadeHeight, input.uv.y);
                alpha *= bottomFade;

                half3 color = _BaseColor.rgb * input.color.rgb;
                
                return half4(color, saturate(alpha));
            }
            ENDHLSL
        }
    }
}