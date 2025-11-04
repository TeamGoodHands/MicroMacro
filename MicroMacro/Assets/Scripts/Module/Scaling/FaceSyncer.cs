using UnityEngine;

namespace Module.Scaling
{
    [ExecuteAlways]
    public class FaceSyncer : MonoBehaviour
    {
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private bool swapXY = false;

        private void Update()
        {
            Transform parentTransform = transform.parent;

            if (parentTransform == null)
            {
                return;
            }

            Vector3 parentScale = parentTransform.lossyScale;

            // x, y の小さい方を採用
            float minParentScale = Mathf.Min(parentScale.x, parentScale.y);

            float targetWorldScale = baseScale * minParentScale;

            float childScaleX = targetWorldScale / Mathf.Max(parentScale.x, 0.0001f);
            float childScaleY = targetWorldScale / Mathf.Max(parentScale.y, 0.0001f);
            float childScaleZ = targetWorldScale / Mathf.Max(parentScale.z, 0.0001f);

            Vector3 localScale = transform.localScale;

            if (swapXY)
            {
                localScale.x = childScaleY;
                localScale.y = childScaleX;
            }
            else
            {
                localScale.x = childScaleX;
                localScale.y = childScaleY;
            }

            localScale.z = childScaleZ;
            transform.localScale = localScale;
        }
    }
}