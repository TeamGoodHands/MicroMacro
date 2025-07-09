using System;
using CoreModule.Attribute;
using CoreModule.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Module.Management
{
    public class LevelUnloader : MonoBehaviour
    {
        [SerializeField] private SceneField stageSelectScene;
        private InputEvent exitLevelEvent;
        
        private void Start()
        {
            exitLevelEvent = InputProvider.CreateEvent(ActionGuid.Player.ExitLevel);
            exitLevelEvent.Started += OnExitLevel;
        }

        private void OnExitLevel(InputAction.CallbackContext ctx)
        {
            SceneManager.LoadScene(stageSelectScene, LoadSceneMode.Single);
        }
    }
}