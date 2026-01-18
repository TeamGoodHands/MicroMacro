using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Contents.ScreenSpaceHatching
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/SSHatching")]
    public class ScreenSpaceHatchingVolume : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter OcclusionLength = new ClampedFloatParameter(1f, 0.01f, 5f);
        public ClampedFloatParameter MinDistance = new ClampedFloatParameter(0f, 0f, 5f);
        public ClampedFloatParameter MaxDistance = new ClampedFloatParameter(5f, 0f, 150f);
        public ClampedFloatParameter OcclusionBias = new ClampedFloatParameter(0.001f, 0f, 1f);
        public ClampedFloatParameter Strength = new ClampedFloatParameter(1f, 0f, 4f);
        public ClampedFloatParameter OcclusionPower = new ClampedFloatParameter(1f, 0.1f, 4f);
        public ClampedFloatParameter OcclusionThreshold = new ClampedFloatParameter(20f, 0.1f, 100f);
        public ClampedFloatParameter BlendStep = new ClampedFloatParameter(0.5f, 0.1f, 1f);
        public ClampedFloatParameter BlendPower = new ClampedFloatParameter(1f, 0.1f, 5f);
        public ClampedFloatParameter HatchScale = new ClampedFloatParameter(1f, 0.1f, 10f);
        public ClampedFloatParameter FrontOffset = new ClampedFloatParameter(1f, -1f, 1f);
        public ClampedFloatParameter BackOffset = new ClampedFloatParameter(1f, -1f, 1f);
        public ClampedFloatParameter OffsetBorder = new ClampedFloatParameter(0.5f, 0f, 0.1f);
        public ClampedIntParameter BlurRadius = new ClampedIntParameter(6, 2, 32);
        public TextureParameter CrossPattern = new TextureParameter(null);

        public bool IsActive()
        {
            if (active == false)
                return false;

            return Strength.value > 0f;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}