using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Application.SceneSwitch
{
    public class SwitchTitleORFeedback : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;

        private void SwitchTitle()
        {
            if (sceneManager != null)
            {
                sceneManager.StartNormalTransition("Title");
            }
        }

        private void SwitchFeedback()
        {
            if (sceneManager != null)
            {
                sceneManager.StartNormalTransition("Feedback");
            }
        }

        private void Update()
        {
            if (Keyboard.current[Key.F1].wasPressedThisFrame)
            {
                SwitchTitle();
            }
            else if (Keyboard.current[Key.F12].wasPressedThisFrame)
            {
                SwitchFeedback();
            }
        }
    }
}