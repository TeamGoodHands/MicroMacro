using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class LockOnEffect : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float initialScale;
        [SerializeField] private float removeDelay;

        private Vector3 defaultScale;

        private void Awake()
        {
            target.gameObject.SetActive(false);
            defaultScale = target.localScale;
        }

        public void LockOn()
        {
            target.gameObject.SetActive(true);

            // target.localScale = new Vector3(defaultScale.x, initialScale, defaultScale.z);
        }
        
        public void LockOff()
        {
            target.gameObject.SetActive(false);
        }
    }
}