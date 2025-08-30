using System.Collections.Generic;
using CoreModule.Helper;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallObjectsSequencer : MonoBehaviour
    {
        [SerializeField] private List<ProjectileShooter> objects;
        [SerializeField] private List<Transform> fallTargets;
        [SerializeField] private float fallInterval = 2f;
        [SerializeField] private float firstFlightTime = 4f;

        public void DoSequence()
        {
            float flightTime = firstFlightTime;

            foreach (ProjectileShooter shooter in objects)
            {
                Vector3 targetPosition = fallTargets[Random.Range(0, fallTargets.Count)].position;
                shooter.Shoot(targetPosition, flightTime);
                
                flightTime += fallInterval;
            }
        }
    }
}