using System;
using Constants;
using UnityEngine;

namespace Module.Application.Dialogue
{
    public class SectionGate : MonoBehaviour
    {
        public event Action OnPlayerExit;
        
        private void Awake()
        {
            if (TryGetComponent<Renderer>(out var renderer))
            {
                renderer.enabled = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                OnPlayerExit?.Invoke();
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var cache = Gizmos.matrix;
            Gizmos.color = new Color(0.27f, 0.88f, 0.88f, 0.5f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = cache;
        }
#endif
    }
}