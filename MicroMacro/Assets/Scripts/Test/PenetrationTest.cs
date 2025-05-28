using System;
using UnityEngine;

namespace Test
{
    public class PenetrationTest : MonoBehaviour
    {
        [SerializeField] private Collider colliderA;
        [SerializeField] private Collider colliderB;

        private void FixedUpdate()
        {
            bool isPenetrate = Physics.ComputePenetration(colliderA, colliderA.transform.position, colliderA.transform.rotation,
                colliderB, colliderB.transform.position, colliderB.transform.rotation,
                out Vector3 direction, out float distance);

 //           Debug.Log($"Is Penetrating: {isPenetrate}, Direction: {direction}, Distance: {distance}");
        }

    }
}