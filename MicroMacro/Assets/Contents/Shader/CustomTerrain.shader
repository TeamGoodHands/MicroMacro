Shader "Custom/Terrain"
{
    Properties
    {
        [HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
        
        _ToonSteps("Toon Steps", Range(1, 5)) = 3
        _ShadowColor("Shadow Tint", Color) = (0.5, 0.5, 0.6, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_Control); SAMPLER(sampler_Control);
            TEXTURE2D(_Splat0); SAMPLER(sampler_Splat0);
            TEXTURE2D(_Splat1); SAMPLER(sampler_Splat1);
            TEXTURE2D(_Splat2); SAMPLER(sampler_Splat2);
            TEXTURE2D(_Splat3); SAMPLER(sampler_Splat3);

            float _ToonSteps;
            float4 _ShadowColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 1. スプラットマップ（重み）のサンプリング
                half4 control = SAMPLE_TEXTURE2D(_Control, sampler_Control, input.uv);
                
                // 2. 各レイヤーのカラーをブレンド (UVは適宜タイリング調整が必要)
                float2 terrainUV = input.worldPos.xz * 0.1; // 簡易的なワールド空間タイリング
                half4 col0 = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, terrainUV);
                half4 col1 = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat1, terrainUV);
                half4 col2 = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat2, terrainUV);
                half4 col3 = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat3, terrainUV);

                half4 finalCol = col0 * control.r + col1 * control.g + col2 * control.b + col3 * control.a;

                // 3. スタイライズド・ライティング (Toon)
                Light mainLight = GetMainLight();
                half d = dot(half3(0,1,0), mainLight.direction); // 簡易的に真上法線を使用
                half toonDiffuse = floor(saturate(d) * _ToonSteps) / _ToonSteps;
                
                half3 lightColor = lerp(_ShadowColor.rgb, mainLight.color, toonDiffuse);
                
                return half4(finalCol.rgb * lightColor, 1.0);
            }
            ENDHLSL
        }
    }
}