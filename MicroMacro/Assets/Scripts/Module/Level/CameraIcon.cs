using UnityEngine;

namespace Module.Level
{
#if UNITY_EDITOR
    [ExecuteAlways]
    public class CameraIcon : MonoBehaviour
    {
        [SerializeField] private BoxCollider boxCollider;

        private void Update()
        {
            Vector3 pos = boxCollider.transform.TransformPoint(boxCollider.center);
            pos.z = -10f;
            transform.position = pos;
        }
    }
#endif
}