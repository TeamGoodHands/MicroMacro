using System;
using Constants;
using UnityEngine;

namespace Module.Management
{
    public class AreaSoundManager : MonoBehaviour
    {
        [SerializeField] private string areaSoundName;
        [SerializeField, Range(0.1f, 50f)] private float areaRadius = 5f;
        [SerializeField] private bool isPlaying;
        [SerializeField] private float startDelay = 1f;

        public float Volume { get; set; }

        private Transform playerTransform;
        private AudioSource audioSource;

        private void Start()
        {
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;
        }

        private void Update()
        {
            if (Time.timeSinceLevelLoad < startDelay)
                return;

            if (!isPlaying && IsPlayerInArea())
            {
                audioSource = SoundManager.instance.PlayAtPoint(areaSoundName, transform.position);
                isPlaying = true;
            }
            else if (isPlaying && !IsPlayerInArea())
            {
                if (audioSource != null)
                {
                    SoundManager.instance.Stop(audioSource);
                    audioSource = null;
                }

                isPlaying = false;
            }

            if (isPlaying)
            {
                audioSource.volume = Volume;
            }
        }

        private bool IsPlayerInArea()
        {
            float distance = Vector3.SqrMagnitude(playerTransform.position - transform.position);
            return distance <= areaRadius * areaRadius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.46f);
            Gizmos.DrawSphere(transform.position, areaRadius);
        }
    }
}