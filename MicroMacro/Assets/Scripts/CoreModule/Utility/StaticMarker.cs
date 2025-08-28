using UnityEngine;

namespace CoreModule.Utility
{
    [ExecuteInEditMode]
    public class StaticMarker : MonoBehaviour
    {
        [SerializeField] private bool isStaticObject;
        public bool IsStaticObject => isStaticObject;

#if UNITY_EDITOR
        private void Update()
        {
            isStaticObject = gameObject.isStatic;
        }
#endif
    }
}