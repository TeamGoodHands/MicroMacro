using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class SketchOutlineFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader outlineShader;
    private Material material;

    class RGPass : ScriptableRenderPass
    {
        static readonly string kTag = "SketchOutlineRG";
        readonly Material _mat;

        // RenderGraph に渡すデータ入れ物
        sealed class PassData
        {
            public Material mat;
            public TextureHandle src;
            public TextureHandle dst;
        }

        public RGPass(Material mat)
        {
            _mat = mat;

            // 他PPと干渉しにくい位置（線をハッキリ残したいならこのまま）
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
        {
            if (_mat == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var desc = cameraData.cameraTargetDescriptor;

            // 入力（現在のカラーバッファ）
            var src = resourceData.activeColorTexture;

            // 出力（同サイズの一時テクスチャ）
            var dst = rg.CreateTexture(new TextureDesc(desc.width, desc.height)
            {
                colorFormat = desc.graphicsFormat,
                depthBufferBits = 0,
                name = "_SketchTmpRG"
            });

            using var builder = rg.AddRenderPass<PassData>(kTag, out var passData);
            passData.mat = _mat;
            passData.src = builder.ReadTexture(src);
            passData.dst = builder.WriteTexture(dst);

            // グローバルは禁止。Blitter×Materialだけで完結させる。
            builder.SetRenderFunc((PassData data, RenderGraphContext ctx) =>
            {
                // src -> dst へ 1パス描画（マテリアルのPass 0を使用）
                Blitter.BlitTexture(ctx.cmd, data.src, Vector4.one, data.mat, 0);
            });

            // 以降のパスが参照するアクティブカラーを差し替える
            resourceData.cameraColor = dst;
        }
    }

    private RGPass pass;

    public override void Create()
    {
        if (outlineShader != null)
        {
            material = CoreUtils.CreateEngineMaterial(outlineShader);
        }
        else
        {
            Debug.LogWarning("ScreenSpaceHatching shader not assigned in ScreenSpaceHatchingFeature.");
        }

        pass = new RGPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (outlineShader == null)
            return;
        
        renderer.EnqueuePass(pass);
    }
}