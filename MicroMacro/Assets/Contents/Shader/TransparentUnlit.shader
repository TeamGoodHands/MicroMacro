Shader "TransparentUnlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white"{}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode"="SketchOutlineTransparentPrepass"
            }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex   dn_vert
            #pragma fragment dn_frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct A
            {
                float4 positionOS: POSITION;
                float3 normalOS: NORMAL;
                float4 tangentOS: TANGENT;
            };

            struct V
            {
                float4 positionHCS: SV_POSITION;
                float3 normalWS: TEXCOORD0;
            };

            V dn_vert(A v)
            {
                V o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.positionHCS = p.positionCS;
                o.normalWS = n.normalWS;
                return o;
            }

            half4 dn_frag(V i) : SV_Target
            {
                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }


        Pass
        {
            ZWrite On
            ColorMask 0
        }

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                return color * _BaseColor;
            }
            ENDHLSL
        }
    }
}