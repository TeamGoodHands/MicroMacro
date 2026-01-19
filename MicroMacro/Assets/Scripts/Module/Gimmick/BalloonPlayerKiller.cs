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
        private HealthStatus playerHealthStatus;

        private void Start()
        {
            balloon.OnReset += OnBalloonReset;
            playerHealthStatus = GameObject.FindWithTag(Tag.Player).GetComponent<HealthStatus>();
            playerHealthStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            if (balloonIsDead)
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