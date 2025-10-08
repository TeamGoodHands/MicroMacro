// Assets/Scripts/Rendering/CounterMotionFeature.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class CounterMotionFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Range(0.1f, 50f)] public float planeDistance = 5f; // ノイズを貼る仮想平面距離
        public float strength = 1.0f; // オフセット倍率
        public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
        public string globalParamName = "_UVOffset"; // SG側の参照名
    }

    class CounterMotionPass : ScriptableRenderPass
    {
        private readonly Settings settings;
        private readonly Dictionary<int, Matrix4x4> prevView = new();
        private readonly Dictionary<int, Matrix4x4> prevProj = new();
        private readonly int uvId;

        private Vector4 currentUv;

        public CounterMotionPass(Settings s)
        {
            settings = s;
            renderPassEvent = s.injectionPoint;
            uvId = UnityEngine.Shader.PropertyToID(string.IsNullOrEmpty(s.globalParamName) ? "_UVOffset" : s.globalParamName);
        }

        // RenderGraph有効時はこちらが呼ばれる
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var camData = frameData.Get<UniversalCameraData>();
            var cam = camData.camera;
            int id = cam.GetInstanceID();

            // 現フレーム行列
            Matrix4x4 view = cam.worldToCameraMatrix;
            Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);

            // 基準点：カメラ前方の一定距離
            Vector3 anchorWorld = cam.transform.position + cam.transform.forward * settings.planeDistance;

            // 今フレームのビューポート座標
            Vector2 uvCurr = WorldToViewport(anchorWorld, view, proj);

            Vector2 uvOffset = Vector2.zero;
            if (prevView.TryGetValue(id, out var pv) && prevProj.TryGetValue(id, out var pp))
            {
                Vector2 uvPrev = WorldToViewport(anchorWorld, pv, pp);
                uvOffset = (uvPrev - uvCurr) * settings.strength; // 逆向き相殺
            }


            // ---- ここから RenderGraph パスの正しい書き方 ----
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CounterMotion_SetGlobal_RG", out var passData))
            {
                passData.uv = new Vector4(uvOffset.x, uvOffset.y, 0, 0);
                passData.uvId = uvId;

                builder.AllowGlobalStateModification(true);

                // このパスはリソースを読まない/書かないので、不要カリングされがち。安全に無効化。
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    currentUv += data.uv;
                    ctx.cmd.SetGlobalVector(data.uvId, currentUv);
                });
                // ---- ここまで ----

                // 次フレーム用に保存
                prevView[id] = view;
                prevProj[id] = proj;
            }
        }

        class PassData
        {
            public Vector4 uv;
            public int uvId;
        }

        static Vector2 WorldToViewport(Vector3 p, Matrix4x4 view, Matrix4x4 proj)
        {
            Vector4 clip = proj * view * new Vector4(p.x, p.y, p.z, 1f);
            clip /= clip.w;
            return new Vector2(clip.x * 0.5f + 0.5f, clip.y * 0.5f + 0.5f);
        }
    }

    public Settings settings = new Settings();
    CounterMotionPass pass;

    public override void Create()
    {
        pass = new CounterMotionPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}