using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ラディアルブラーのシェーダーで用いるパラメータ。
/// </summary>
[Serializable]
public class RadialBlurParams
{
    [Range(0, 1), Tooltip("ブラーの強さ")] public float Intensity = 0.4f;
    [Min(1), Tooltip("サンプリング回数")] public int SampleCount = 3;
    [Tooltip("エフェクトの中心")] public Vector2 RadialCenter = new Vector2(0.5f, 0.5f);
    [Tooltip("ディザリングを利用する")] public bool UseDither = true;
    [Tooltip("ラディアルブラーのシェーダー")] public Shader Shader;
}

public class RadialBlurFeature : ScriptableRendererFeature
{
    [SerializeField] private RadialBlurParams parameters;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    private RadialBlurPass pass;

    public RadialBlurParams GetParams()
    {
        return parameters;
    }

    public override void Create()
    {
        pass = new RadialBlurPass(parameters)
        {
            renderPassEvent = renderPassEvent,
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass != null) renderer.EnqueuePass(pass);
    }

    public void OnDestroy() => pass?.Dispose();
}

public class RadialBlurPass : ScriptableRenderPass
{
    private readonly Material material;
    private readonly RadialBlurParams parameters;
    private readonly LocalKeyword keywordUseDither;

    private static readonly int idIntensity = Shader.PropertyToID("_Intensity");
    private static readonly int idSampleCountParams = Shader.PropertyToID("_SampleCountParams");
    private static readonly int idRadialCenter = Shader.PropertyToID("_RadialCenter");

    public RadialBlurPass(RadialBlurParams parameters)
    {
        this.parameters = parameters;
        if (!this.parameters.Shader)
        {
            Debug.LogError("ラディアルブラーのシェーダーをセットしてください。");
            return;
        }

        //シェーダーの取得、マテリアルとキーワードの生成。
        material = CoreUtils.CreateEngineMaterial(parameters.Shader);
        keywordUseDither = new LocalKeyword(parameters.Shader, "USE_DITHER");
    }

    private readonly ProfilingSampler _sampler = new("RadialBlur");

    private class PassData
    {
        public TextureHandle CameraTexture;
        public Material Material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (!material) return;

        //リソース関係のデータ（カメラのテクスチャなど）を取得する。
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        //ラディアルブラーのパラメータを設定。
        material.SetFloat(idIntensity, parameters.Intensity);
        material.SetVector(idSampleCountParams,
            new Vector3(
                parameters.SampleCount,
                1f / parameters.SampleCount,
                2 <= parameters.SampleCount ? 1f / (parameters.SampleCount - 1) : 1));
        material.SetVector(idRadialCenter, parameters.RadialCenter);
        material.SetKeyword(keywordUseDither, parameters.UseDither);

        //カメラに映すテクスチャの取得。
        TextureHandle cameraTexture = resourceData.activeColorTexture;

        //一時的なテクスチャの性質を決めるDescriptorを取得。
        TextureDesc tempDesc = renderGraph.GetTextureDesc(cameraTexture);
        tempDesc.name = "_TempTexture";
        //一時的なテクスチャの取得。
        TextureHandle tempTexture = renderGraph.CreateTexture(tempDesc);

        //RasterPassの追加。
        using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(_sampler.name, out PassData passData, _sampler))
        {
            passData.CameraTexture = cameraTexture;
            passData.Material = material;
            builder.UseTexture(cameraTexture, AccessFlags.Read);
            builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);
            builder.SetRenderFunc<PassData>(static (passData, context) =>
            {
                Blitter.BlitTexture(context.cmd, passData.CameraTexture, Vector2.one, passData.Material, 0);
            });
        }

        //以後URPで用いるテクスチャを差し替える。
        resourceData.cameraColor = tempTexture;
    }


    public void Dispose()
    {
        CoreUtils.Destroy(material);
    }
}