using System;
using UnityEngine;

namespace CoreModule.Utility
{
    [ExecuteAlways]
    public class TransformSyncer : MonoBehaviour
    {
        [SerializeField] private Transform target;
        public bool syncPosition;
        public bool syncRotation;

        public void UpdateManual()
        {
            if (target == null)
                return;

            if (syncPosition)
            {
                transform.position = target.position;
            }

            if (syncRotation)
            {
                transform.rotation = target.rotation;
            }
        }

        private void Update()
        {
            UpdateManual();
        }
    }
}