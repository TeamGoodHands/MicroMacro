using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Module.Application.SceneSwitch
{
    public class GoalGate : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;

        private void Start()
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (sceneManager != null)
                sceneManager.StartTransition();
        }
    }
}