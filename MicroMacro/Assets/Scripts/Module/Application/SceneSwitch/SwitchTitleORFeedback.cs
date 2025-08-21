using System;
using UnityEngine;

namespace Module.Application.SceneSwitch
{
    public class SwitchTitleORFeedback : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;

        private void SwitchTitle()
        {
            if (sceneManager != null)
            {
                sceneManager.StartTransition("Title");
            }
        }

        private void SwitchFeedback()
        {
            if (sceneManager != null)
            {
                sceneManager.StartTransition("Feedback");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                SwitchTitle();
            }
            else if (Input.GetKeyDown(KeyCode.F12))
            {
                SwitchFeedback();
            }
        }
    }
}