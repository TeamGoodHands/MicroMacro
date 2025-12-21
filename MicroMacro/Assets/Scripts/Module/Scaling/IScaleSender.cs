using Cysharp.Threading.Tasks;

namespace Module.Scaling
{
    public interface IScaleSender
    {
        UniTaskVoid Scale(int additionalStep, bool forceScale = false);
    }
}
