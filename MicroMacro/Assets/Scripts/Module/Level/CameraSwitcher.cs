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

        private CinemachineBrain cinemachineBrain;

        private void Start()
        {
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (targetCamera.Priority == activePriority)
                return;

            if (!other.CompareTag(Tag.Handle.Player))
                return;

            // ゲーム起動直後の初期化では、Cinemachine側にカメラを設定する。
            // Soloモード設定した状態で起動すると、CinemachineBrainのカメラがnullになってしまう対策
            CinemachineInitialization.CheckInitialization(targetCamera);

            targetCamera.Priority = activePriority;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tag.Handle.Player))
                return;

            // ゲーム起動直後の初期化では、Cinemachine側にカメラを設定する。
            // Soloモード設定した状態で起動すると、CinemachineBrainのカメラがnullになってしまう対策
            CinemachineInitialization.CheckInitialization(targetCamera);

            targetCamera.Priority = activePriority;
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