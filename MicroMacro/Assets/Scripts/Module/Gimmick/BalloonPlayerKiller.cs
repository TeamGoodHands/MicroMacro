using System;
using Constants;
using Module.UI;
using UnityEngine;

namespace Module.Gimmick
{
    public class BalloonPlayerKiller : MonoBehaviour
    {
        [SerializeField] private SingleBalloon balloon;

        private int balloonHealth;
        private bool balloonIsDead;

        private void Update()
        {
            if (balloon.HealthPoint > 0)
                return;

            if (Physics.Raycast(transform.position, Vector3.down, 1.5f, -1, QueryTriggerInteraction.Ignore))
            {
                balloon.KillBalloon().Forget();
            }
        }
    }
}