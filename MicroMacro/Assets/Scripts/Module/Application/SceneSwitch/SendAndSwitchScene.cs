using System;
using UnityEngine;

namespace Module.Application.SceneSwitch
{
    public class SendAndSwitchScene : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;
        [SerializeField] private FeedbackSender sender;

        private void Start()
        {
            if (sender != null)
            {
                sender.OnSend += OnSend;
            }
        }

        private void OnDestroy()
        {
            if (sender != null)
            {
                sender.OnSend -= OnSend;
            }
        }

        private void OnSend()
        {
            if (sceneManager != null)
            {
                sceneManager.StartTransition();
            }
        }
    }
}