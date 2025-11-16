Shader "Hidden/ChameleonOutline/Composite"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Overlay"
        }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "OutlineNearestInit"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // R16G16_UNorm なので 0〜1 しか入らない
            // → (0,0) を invalid 専用値として予約する
            static const float2 INVALID_UV = float2(0.0, 0.0);

            // [0,1] の UV を [0.01,1.0] に押し込む（0 を予約しておく）
            float2 EncodeNearestUv(float2 uv)
            {
                return uv * 0.99 + 0.01;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // Prepass（アウトラインの色＋太さなどが入っている）
                float4 prepass = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                // a > 0 → アウトライン対象
                float isInside = step(0.00001, prepass.a);

                // 有効なピクセルなら encode した UV、何も無ければ (0,0)
                float2 encodedUv = EncodeNearestUv(uv);
                float2 nearestUvEncoded = lerp(INVALID_UV, encodedUv, isInside);

                // RG = 最近傍 UV（エンコード済み）, BA は未使用
                return float4(nearestUvEncoded, 0.0, 0.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "OutlineNearestJump"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float2 _Step; // uv 空間でのジャンプ幅 (dx, dy)

            static const float2 INVALID_UV = float2(0.0, 0.0);

            // 0,0 なら invalid
            bool IsValidNearest(float2 encodedUv)
            {
                return any(encodedUv != INVALID_UV);
            }

            // Encode/Decode は Composite と対になるように揃えておく
            float2 EncodeNearestUv(float2 uv)
            {
                return uv * 0.99 + 0.01;
            }

            float2 DecodeNearestUv(float2 encodedUv)
            {
                // 呼ぶ前に invalid チェックしている前提
                return (encodedUv - 0.01) / 0.99;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // 自分の現在候補（エンコード済み UV）
                float2 encodedBest = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv).rg;

                // 距離比較用
                float bestDist = 1e9;
                float2 bestNearestUv = uv; // デコード後の UV（距離計算用）

                if (IsValidNearest(encodedBest))
                {
                    bestNearestUv = DecodeNearestUv(encodedBest);
                    float2 delta = bestNearestUv - uv;
                    bestDist = length(delta);
                }

                // 3x3 近傍（ジャンプ幅 _Step を使う）
                [unroll]
                for (int dy = -1; dy <= 1; dy++)
                {
                    [unroll]
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        float2 sampleUv = uv + float2(dx, dy) * _Step;

                        float2 encodedNeighbor =
                            SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, sampleUv).rg;

                        if (!IsValidNearest(encodedNeighbor))
                        {
                            continue;
                        }

                        float2 neighborNearestUv = DecodeNearestUv(encodedNeighbor);
                        float2 delta = neighborNearestUv - uv;
                        float dist = length(delta);

                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestNearestUv = neighborNearestUv;
                            encodedBest = encodedNeighbor; // 書き戻すのは encode 側
                        }
                    }
                }

                // RG に「エンコード済みの最近傍 UV」を書き戻す
                return float4(encodedBest, 0.0, 0.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "OutlineComposite"
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_OutlinePrepassTexture);
            SAMPLER(sampler_OutlinePrepassTexture);

            TEXTURE2D_X(_NearestTexture);
            SAMPLER(sampler_NearestTexture);

            float2 DecodeNearestUv(float2 encodedUv)
            {
                // invalid（0,0）はそのまま返して早期リターン
                if (all(encodedUv == float2(0.0, 0.0)))
                {
                    return encodedUv;
                }

                // [0.01,1.0] → [0,1] に戻す
                return (encodedUv - 0.01) / 0.99;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                float4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                // 最近傍 UV を取得（0,0 は invalid とする）
                float2 encodedNearestUv = SAMPLE_TEXTURE2D_X(_NearestTexture, sampler_NearestTexture, uv).rg;

                if (all(encodedNearestUv == float2(0.0, 0.0)))
                {
                    return sceneColor;
                }

                float2 nearestUv = DecodeNearestUv(encodedNearestUv);

                // 最近傍の内側ピクセルの Prepass 情報（色＋太さ）
                float4 prepassNearest = SAMPLE_TEXTURE2D_X(_OutlinePrepassTexture, sampler_OutlinePrepassTexture,
                                                           nearestUv);

                float3 outlineColor = prepassNearest.rgb;
                float width01 = prepassNearest.a;
                float widthPixels = width01 * 255.0;

                // 現在ピクセルの Prepass（元オブジェクトに重なってるかどうか用）
                float4 preAtUv = SAMPLE_TEXTURE2D_X(_OutlinePrepassTexture, sampler_OutlinePrepassTexture, uv);
                float isInsideOriginal = step(0.0001, preAtUv.a);

                // 距離（ピクセル）
                float2 deltaPixels = (nearestUv - uv) / _BlitTexture_TexelSize.xy;
                float distPixels =length(deltaPixels)*0.01 ;

                // return float4(distPixels, distPixels, distPixels, 1.0);

                // 幅 widthPixels の線にする
                float inner = widthPixels - 0.5;
                float outer = widthPixels + 0.5;

                float outlineMask = 1.0 - smoothstep(inner, outer, distPixels);

                // 元メッシュの内側は消す → シルエットの外側だけに線を出す
                outlineMask *= (1.0 - isInsideOriginal);
                outlineMask = saturate(outlineMask);

                // return float4(outlineMask, outlineMask, outlineMask, 1);

                // 合成
                float3 finalColor = lerp(sceneColor.rgb, outlineColor, outlineMask);

                return float4(finalColor, sceneColor.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}