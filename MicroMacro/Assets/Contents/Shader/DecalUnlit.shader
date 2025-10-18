// このシェーダーはコード内に事前定義されている色でメッシュ形状を塗りつぶします。
Shader "Example/URPUnlitShaderBasic"
{
    // Unity シェーダーのプロパティブロック。この例では出力の色がフラグメントシェーダーの
    // コード内に事前定義されているため、このブロックは空です。
    Properties
    {
        [Toggle(_RECEIVE_DECALS)] _ReceiveDecals("Receive Decals", Float) = 1
    }

    // シェーダーのコードが含まれる SubShader ブロック。
    SubShader
    {
        // SubShader Tags では SubShader ブロックまたはパスが実行されるタイミングと条件を
        // 定義します。
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        // ====== DepthNormals（Angle Fade/法線影響を使う時は推奨）======
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

            // URPのDepthNormalsはWS法線をそのまま書き出す実装に依存するため、
            // ここでは簡易的に0..1へエンコード（URP内部の実装差があっても「何かは出る」）
            half4 dn_frag(V i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                return half4(n * 0.5 + 0.5, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "UnlitDBufferProbe"
            // Forwardパスとして動かす
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            ZWrite On
            ZTest LEqual
            Cull Back
            Blend One Zero



            // HLSL コードブロック。Unity SRP では HLSL 言語を使用します。
            HLSLPROGRAM
            #pragma shader_feature_local _RECEIVE_DECALS
            #pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ _DECAL_LAYERS

            // この行では頂点シェーダーの名前を定義します。
            #pragma vertex vert
            // この行ではフラグメントシェーダーの名前を定義します。
            #pragma fragment frag

            // Core.hlsl ファイルには、よく使用される HLSL マクロおよび関数の
            // 定義が含まれ、その他の HLSL ファイル (Common.hlsl、
            // SpaceTransforms.hlsl など) への #include 参照も含まれています。
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

            // この構造体定義では構造体に含まれる変数を定義します。
            // この例では Attributes 構造体を頂点シェーダーの入力構造体として
            // 使用しています。
            struct Attributes
            {
                // positionOS 変数にはオブジェクト空間内での頂点位置が
                // 含まれます。
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                // この構造体内の位置には SV_POSITION セマンティクスが必要です。
                float4 positionHCS : SV_POSITION;
            };

            // Varyings 構造体内に定義されたプロパティを含む頂点シェーダーの
            // 定義。vert 関数の型は戻り値の型 (構造体) に一致させる
            // 必要があります。
            Varyings vert(Attributes IN)
            {
                // Varyings 構造体での出力オブジェクト (OUT) の宣言。
                Varyings OUT;
                // TransformObjectToHClip 関数は頂点位置をオブジェクト空間から
                // 同種のクリップスペースに変換します。
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // 出力を返します。
                return OUT;
            }

            // フラグメントシェーダーの定義。
            half4 frag(Varyings OUT) : SV_Target
            {
                // 色変数を定義して返します。
                half3 customColor = half3(0, 0, 0);
                ApplyDecalToBaseColor(OUT.positionHCS, customColor);

                return half4(customColor.r, customColor.g, customColor.b, 1);
            }
            ENDHLSL
        }
    }
}