Shader "WriteDepthNormal"
{
    SubShader
    {

        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode"="DepthNormals"
            }
            ZWrite On Cull Back

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
                float3 n = normalize(i.normalWS);
                return half4(n * 0.5 + 0.5, 1);
            }
            ENDHLSL
        }
    }
}