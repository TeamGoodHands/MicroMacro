using System;
using Constants;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class HoseWater : MonoBehaviour
    {
        [SerializeField] private HoseParameter parameter;
        [SerializeField] private Scaler scaler;

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out Rigidbody rigidBody))
            {
                float scaleMultiplier = parameter.ScaleMultiplier * (scaler.CurrentStep - scaler.MinStep);
                rigidBody.AddForce(transform.up * parameter.WaterPower * scaleMultiplier);

                Vector2 playerPosition = rigidBody.position - transform.position;
                Vector2 sideForce = GetPerpendicularTowardPoint(transform.up, playerPosition);
                rigidBody.AddForce(sideForce * parameter.WaterPower * parameter.SideForceMultiplier);
            }
        }

        private Vector2 GetPerpendicularTowardPoint(Vector2 a, Vector2 p)
        {
            Vector2 n = new Vector2(-a.y, a.x).normalized;
            float s = Mathf.Sign(Vector2.Dot(n, p));
            return n * s;
        }
    }
}