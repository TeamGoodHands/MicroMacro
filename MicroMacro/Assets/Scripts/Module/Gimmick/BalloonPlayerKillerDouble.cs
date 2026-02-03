using System;
using Constants;
using Module.UI;
using UnityEngine;

namespace Module.Gimmick
{
    public class BalloonPlayerKillerDouble : MonoBehaviour
    {
        [SerializeField] private DoubleBalloon balloon;
        [SerializeField] private VehicleRider vehicleRider;

        private int balloonHealth;
        private bool balloonIsDead;
        private bool wasRidden;
        private HealthStatus playerHealthStatus;

        private void Start()
        {
            balloon.OnReset += OnBalloonReset;
            playerHealthStatus = GameObject.FindWithTag(Tag.Player).GetComponent<HealthStatus>();
            playerHealthStatus.OnDeath += OnPlayerDeath;
            vehicleRider.OnRide += OnRide;
        }

        private void OnRide()
        {
            wasRidden = true;
        }

        private void OnPlayerDeath()
        {
            if (balloonIsDead || !wasRidden)
                return;

            KillBalloon();
        }

        private void OnBalloonReset()
        {
            balloonIsDead = false;
        }

        private void Update()
        {
            if (balloon.HealthPoint > 0 || balloonIsDead)
                return;

            if (Physics.Raycast(transform.position, Vector3.down, 1.5f, -1, QueryTriggerInteraction.Ignore))
            {
                KillBalloon();
            }
        }

        private void KillBalloon()
        {
            balloon.KillBalloon().Forget();
            balloonIsDead = true;
        }
    }
}