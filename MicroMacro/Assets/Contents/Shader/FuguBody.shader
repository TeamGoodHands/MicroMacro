Shader"Custom/FuguBody"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0,0.5,1,1)
        [MainTexture] _BaseTexture("Base Texture", 2D) = "white" {}
        _RimColor("Rim Color", Color) = (0,1,1,1)
        _RimPower("Rim Power", Range(0,100)) = 0.4
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        Blend SrcAlpha OneMinusSrcAlpha

        SubShader
        {
            Pass
            {
                ColorMask 0
            }

            Pass
            {
                Tags
                {
                    "LightMode" = "UniversalForward"
                }

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                //Core機能をまとめたhlslを参照可能にする。いろんな便利関数や事前定義された値が利用可能となる。
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


                float4 _TintColor;
                float4 _RimColor;
                float _RimPower;


                TEXTURE2D(_BaseTexture);
                SAMPLER(sampler_BaseTexture);


                CBUFFER_START(UnityPerMaterial)
                    float4 _BaseTexture_ST;
                CBUFFER_END

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 world_pos : TEXCOORD0;
                    float3 normalDir : TEXCOORD1;
                    float2 uv : TEXCOORD2;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;

                    o.vertex = TransformObjectToHClip(v.vertex);
                    o.world_pos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.normalDir = TransformObjectToWorldNormal(v.normal);
                    o.uv = TRANSFORM_TEX(v.uv, _BaseTexture); // Apply tiling and offset
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    //カメラのベクトルを計算
                    float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.world_pos.xyz);
                    //法線とカメラのベクトルの内積を計算し、補間値を算出
                    half rim = 1.0 - saturate(dot(viewDirection, i.normalDir));

                    float4 color = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, i.uv);

                    color.rgb *= _TintColor;
                    color.a += pow(rim, _RimPower);

                    //補間値で塗分け
                    return color;
                }
                ENDHLSL
            }
        }
    }
}