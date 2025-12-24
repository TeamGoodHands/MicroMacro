using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SketchOutline
{
    [Serializable]
    public class SketchOutlineSettings
    {
        public Color outlineColor = Color.black;
        [Range(1, 6)] public int thickness = 1;

        [Header("Jitter (Sketchy Outline)")]
        public bool enableJitter = true;

        [Range(0f, 1f)] public float blend = 1.0f;
        [Range(0f, 0.01f)] public float jitterAmpPixels = 0.75f;
        [Range(8f, 512f)] public float jitterScale = 160f;
        [Range(0f, 2f)] public float jitterSpeed = 1.0f;
        [Range(0.01f, 1f)] public float timeStepSize = 1f;

        [Range(0f, 0.01f)] public float depthLow = 1f / 220f;
        [Range(0f, 0.01f)] public float depthHigh = 1f / 180f;
        [Range(0f, 1f)] public float normalLow = 1f / 4.5f;
        [Range(0f, 1f)] public float normalHigh = 1f / 3.5f;

        public RenderPassEvent injectEvent = RenderPassEvent.AfterRenderingTransparents;
    }
}