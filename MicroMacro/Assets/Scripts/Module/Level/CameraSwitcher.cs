using System;
using Constants;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

namespace Module.Level
{
    public class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera targetCamera;
        [SerializeField] private int activePriority = 1;
        [SerializeField] private int inactivePriority = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tag.Handle.Player))
            {
                targetCamera.Priority = activePriority;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tag.Handle.Player))
            {
                targetCamera.Priority = inactivePriority;
            }
        }

        private BoxCollider boxCollider;

        private void OnDrawGizmosSelected()
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
            }

            Gizmos.color = new Color(0.09f, 1f, 0.14f, 0.09f);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            
            Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            Gizmos.matrix = oldMatrix;
        }
    }
}