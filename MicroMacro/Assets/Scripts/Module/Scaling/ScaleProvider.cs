using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Module.Scaling
{
    public class ScaleProvider : MonoBehaviour,IScaleSender
    {
        [SerializeField] private Scaler scaler;
        
        public UniTaskVoid Scale(int additionalStep, bool forceScale = false)
        {
            return scaler.Scale(additionalStep, forceScale);
        }
    }
}
