using System;
using UnityEngine;

namespace CoreModule.Utility
{
    [ExecuteAlways]
    public class TransformSyncer : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private bool hasOffset;
        public bool syncPosition;
        public bool syncRotation;

        private Vector3 positionOffset;
        private Quaternion rotationOffset;

        private void Start()
        {
            positionOffset = transform.position - target.position;
            rotationOffset = Quaternion.Inverse(target.rotation) * transform.rotation;
        }

        public void UpdateManual()
        {
            if (target == null || hasOffset && !Application.isPlaying)
                return;

            if (syncPosition)
            {
                transform.position = target.position + positionOffset;
            }

            if (syncRotation)
            {
                transform.rotation = rotationOffset * target.rotation;
            }
        }

        private void Update()
        {
            UpdateManual();
        }
    }
}