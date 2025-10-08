using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CoreModule.Utility
{
    [RequireComponent(typeof(DecalProjector))]
    public class DecalFader : MonoBehaviour
    {
        [SerializeField] private float decalDistance;
        [SerializeField] private LayerFlag conflictLayer;

        private DecalProjector decalProjector;

        private void Start()
        {
            decalProjector = GetComponent<DecalProjector>();
        }

        private void Update()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, decalDistance, conflictLayer))
            {
                float ratio = hit.distance / decalDistance;
                decalProjector.fadeFactor = 1 - ratio;
            }
            else
            {
                decalProjector.fadeFactor = 0f;
            }
        }
    }
}