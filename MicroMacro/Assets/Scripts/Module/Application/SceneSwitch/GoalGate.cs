using System;
using UnityEngine;

namespace Module.Application.SceneSwitch
{
    public class GoalGate : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition fadeAndSceneTransition;

        private void Start()
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (fadeAndSceneTransition != null)
                fadeAndSceneTransition.StartTransition();
        }
    }
}