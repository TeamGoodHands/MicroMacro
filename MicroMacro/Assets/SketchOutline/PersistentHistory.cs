using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SketchOutline
{
    public sealed class PersistentHistory
    {
        // ====== Public Factory / Registry ======

        /// <summary>
        /// カメラとタグで一意化された PersistentHistory を取得（無ければ生成）
        /// </summary>
        public static PersistentHistory GetOrCreate(Camera camera, string tag,
            GraphicsFormat graphicsFormat = GraphicsFormat.R8_UNorm,
            FilterMode filterMode = FilterMode.Bilinear,
            TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            int key = MakeKey(camera, tag);

            if (registry.TryGetValue(key, out PersistentHistory history))
            {
                return history;
            }

            history = new PersistentHistory(camera, tag, graphicsFormat, filterMode, wrapMode);
            registry.Add(key, history);
            return history;
        }

        /// <summary>
        /// すべての履歴を解放（シーン終了やディスポーズ時などに）
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var kv in registry)
            {
                kv.Value.Release();
            }
            registry.Clear();
        }

        // ====== Public Instance API ======

        /// <summary>
        /// Camera のターゲット記述子に追従して確保（解像度変化に対応）
        /// </summary>
        public void EnsureAllocated(in RenderTextureDescriptor baseDescriptor)
        {
            // 標準的な履歴設定（軽量1ch）
            RenderTextureDescriptor desc = baseDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.graphicsFormat = graphicsFormat;
            desc.bindMS = false;
            desc.enableRandomWrite = false;

            // ReAllocateIfNeeded はサイズが同じならコストゼロに近い
      
            // ここを ReAllocateHandleIfNeeded に差し替え
            RenderingUtils.ReAllocateHandleIfNeeded(
                ref historyA,
                desc,
                filterMode: filterMode,
                wrapMode: wrapMode,
                anisoLevel: 1,
                mipMapBias: 0,
                name: historyNameA
            );

            RenderingUtils.ReAllocateHandleIfNeeded(
                ref historyB,
                desc,
                filterMode: filterMode,
                wrapMode: wrapMode,
                anisoLevel: 1,
                mipMapBias: 0,
                name: historyNameB
            );
        }

        /// <summary>
        /// RenderGraph へ読み取り面を Import
        /// </summary>
        public TextureHandle ImportRead(RenderGraph renderGraph)
        {
            if (historyA == null || historyB == null)
            {
                // Ensure が未呼び出しなら何もできない
                return TextureHandle.nullHandle;
            }

            RTHandle read = isFlip ? historyB : historyA;
            return renderGraph.ImportTexture(read);
        }

        /// <summary>
        /// RenderGraph へ書き込み面を Import（MRT の SV_Target1 などに割り当て）
        /// </summary>
        public TextureHandle ImportWrite(RenderGraph renderGraph)
        {
            if (historyA == null || historyB == null)
            {
                return TextureHandle.nullHandle;
            }

            RTHandle write = isFlip ? historyA : historyB;
            return renderGraph.ImportTexture(write);
        }

        /// <summary>
        /// フレーム終端で呼び出し（次フレームの読み書きを入れ替え）
        /// </summary>
        public void Swap()
        {
            isFlip = !isFlip;
        }

        /// <summary>
        /// 任意：値で初期化したい場合にクリア（最初のフレームなど）
        /// </summary>
        public void Clear(CommandBuffer commandBuffer, Color value)
        {
            if (historyA == null || historyB == null)
            {
                return;
            }

            CoreUtils.SetRenderTarget(commandBuffer, historyA);
            CoreUtils.ClearRenderTarget(commandBuffer, ClearFlag.Color, value);

            CoreUtils.SetRenderTarget(commandBuffer, historyB);
            CoreUtils.ClearRenderTarget(commandBuffer, ClearFlag.Color, value);
        }

        /// <summary>
        /// 個別解放
        /// </summary>
        public void Release()
        {
            if (historyA != null) RTHandles.Release(historyA);
            if (historyB != null) RTHandles.Release(historyB);

            historyA = null;
            historyB = null;
            isFlip = false;
        }

        // ====== Private ======

        private static readonly Dictionary<int, PersistentHistory> registry = new Dictionary<int, PersistentHistory>();

        private readonly string tag;
        private readonly int cameraId;

        private readonly string historyNameA;
        private readonly string historyNameB;

        private readonly GraphicsFormat graphicsFormat;
        private readonly FilterMode filterMode;
        private readonly TextureWrapMode wrapMode;

        private RTHandle historyA;
        private RTHandle historyB;
        private bool isFlip;

        private PersistentHistory(Camera camera, string tag,
            GraphicsFormat graphicsFormat, FilterMode filterMode, TextureWrapMode wrapMode)
        {
            this.tag = tag;
            this.cameraId = camera != null ? camera.GetInstanceID() : 0;
            this.graphicsFormat = graphicsFormat;
            this.filterMode = filterMode;
            this.wrapMode = wrapMode;

            // 名前はデバッグで見やすいように
            string baseName = $"_Hist_{(camera != null ? camera.name : "NullCam")}_{tag}";
            this.historyNameA = baseName + "_A";
            this.historyNameB = baseName + "_B";
        }

        private static int MakeKey(Camera camera, string tag)
        {
            int camId = camera != null ? camera.GetInstanceID() : 0;
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + camId;
                hash = hash * 31 + (tag != null ? tag.GetHashCode() : 0);
                return hash;
            }
        }
    }
}