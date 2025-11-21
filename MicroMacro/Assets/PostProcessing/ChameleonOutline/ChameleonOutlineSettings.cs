using PostProcessing.ChameleonOutline;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ChameleonOutline
{
    [System.Serializable]
    public class ChameleonOutlineSettings
    {
        public RenderPassEvent PrepassEvent = RenderPassEvent.AfterRenderingPrePasses;
     
        public int JumpIterations = 6;
    }
}