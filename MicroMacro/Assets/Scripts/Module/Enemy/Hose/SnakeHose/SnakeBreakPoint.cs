using System;
using UnityEngine;


namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeBreakPoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<WaterPusher>(out _))
            {
                Destroy(gameObject);
            }
        }
    }
}