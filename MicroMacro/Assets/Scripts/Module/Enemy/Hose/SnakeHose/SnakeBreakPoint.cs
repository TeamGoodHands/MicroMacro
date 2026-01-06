using System;
using Module.Scaling;
using UnityEngine;


namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeBreakPoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out WaterPusher waterPusher))
                return;

            SnakeHoseTapBehaviour snakeHoseTap = waterPusher.GetComponentInParent<SnakeHoseTapBehaviour>();

            if (snakeHoseTap.IsRapid)
            {
                Destroy(gameObject);
            }
        }
    }
}