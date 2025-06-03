using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreModule.Input
{
    /// <summary>
    ///     登録解除不要なInputActionを提供するクラス
    /// </summary>
    public static class InputProvider
    {
        private static List<InputEvent> inputEvents = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            inputEvents = new List<InputEvent>();

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    OnDispose();
                }
            };
#else
            Application.quitting += OnDispose;
#endif
        }

        private static void OnDispose()
        {
            foreach (var inputEvent in inputEvents)
            {
                inputEvent.Clear();
            }
        }

        public static InputEvent CreateEvent(Guid guid)
        {
            var inputAction = InputSystem.actions.FindAction(guid);
            Debug.Assert(inputAction != null);

            var inputEvent = new InputEvent(inputAction);
            inputEvents.Add(inputEvent);

            return inputEvent;
        }

        public static InputActionMap GetActionMap(Guid guid)
        {
            return InputSystem.actions.FindActionMap(guid);
        }
        
        public static void ClearEvents()
        {
            foreach (var inputEvent in inputEvents)
            {
                inputEvent.Clear();
            }

            inputEvents.Clear();
        }
    }
}