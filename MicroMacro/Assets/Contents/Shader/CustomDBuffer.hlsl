#ifndef CUSTOM_DBUFFER_INCLUDED
#define CUSTOM_DBUFFER_INCLUDED

// 先に DBuffer の定義を読み込む（順序依存を避けるためここでも include）
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

// 画面UVを作る（clip space → [0,1]）
float2 GetScreenUV(float4 positionHCS)
{
    float2 uv = positionHCS.xy / positionHCS.w;
    uv = uv * 0.5f + 0.5f;
    #if defined(UNITY_UV_STARTS_AT_TOP)
        uv.y = 1.0 - uv.y;
    #endif
    return uv;
}

// RNM（Reoriented Normal Mapping）風ブレンド（単純 lerp より歪みが少ない）
float3 BlendNormalWS(float3 baseN, float3 decalN, float w)
{
    decalN = normalize(decalN);
    baseN  = normalize(baseN);
    float3 t = normalize(float3(1,1,1) + baseN * float3(-1,-1,1));
    float3 u = float3(decalN.xy, decalN.z);
    float3 r = t * dot(t, u) - u;
    return normalize(lerp(baseN, normalize(r), saturate(w)));
}

struct Surf // 手元のサーフェス集約
{
    float3 baseColor;
    float3 normalWS;
    float3 emission;
};

// DBuffer をサンプルして Surf に合成
void ApplyDBuffer_URP17(float2 screenUV, inout Surf s)
{
#if defined(_RECEIVE_DECALS) && (defined(_DBUFFER_MRT1) || defined(_DBUFFER_MRT2) || defined(_DBUFFER_MRT3))
    // --- URP 17 実装：DBufferData と SampleDBuffer の実名を確認 ---
    // DBuffer.hlsl 内で以下のような形を探してそのまま使う：
    //   struct DBufferData { float3 baseColor; float3 normalWS; float metallic; float smoothness; float occlusion; float mask; };
    //   DBufferData SampleDBuffer(float2 uv);
    DBufferData db = SampleDBuffer(screenUV);   // ← 実名が違ったら置換
    float w = saturate(db.mask);                // ← “影響度/アルファ” 的マスク

    // BaseColor は通常 lerp
    s.baseColor = lerp(s.baseColor, db.baseColor, w);

    // Normal は WS 前提で RNM 合成
    s.normalWS  = BlendNormalWS(s.normalWS, db.normalWS, w);

    // Emission を Decal 側で使っているなら加算など（実装にあれば）
    // s.emission += db.emission * w;
#endif
}

#endif // CUSTOM_DBUFFER_INCLUDED