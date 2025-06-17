using UnityEngine;

namespace Module.Enemy
{
    public class PushPumpHead : MonoBehaviour
    {
        [SerializeField] private Transform bodyTransform;

        private void Update()
        {
            transform.localPosition = bodyTransform.localPosition + bodyTransform.up * (bodyTransform.localScale.y * 0.5f);
        }
    }
}