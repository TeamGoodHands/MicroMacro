using UnityEngine;

namespace Module.Enemy.Boomerang
{
    public class BoomerangAnimationEventProvider : MonoBehaviour
    {
        [SerializeField] private Boomerang boomerang;

        private void ThrowCap()
        {
            boomerang.Throw().Forget();
        }

        private void CatchCap()
        {
            boomerang.Catch();
        }
    }
}