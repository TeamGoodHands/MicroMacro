using UnityEngine;

namespace CoreModule.Utility
{
    public class ChildScaleLocker : MonoBehaviour
    {
        public Vector3 defaultScale = Vector3.zero;

        private void Start()
        {
            defaultScale = transform.lossyScale;
        }

        private void Update()
        {
            Vector3 lossScale = transform.lossyScale;
            Vector3 localScale = transform.localScale;

            transform.localScale = new Vector3(
                localScale.x / lossScale.x * defaultScale.x,
                localScale.y / lossScale.y * defaultScale.y,
                localScale.z / lossScale.z * defaultScale.z
            );
        }
    }
}