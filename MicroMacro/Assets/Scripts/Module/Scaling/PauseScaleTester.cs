using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Scaling
{
    public class PauseScaleTester : MonoBehaviour
    {
        private Scaler scaler;

        private void Start()
        {
            scaler = GetComponent<Scaler>();
            scaler.OnScaleCompleted += _ => Debug.Log("Scale Completed");
            scaler.OnScaleStarted += _ => Debug.Log("Scale Started");
            scaler.OnScaleResumed += (_, _) => Debug.Log("Scale Resumed");
            scaler.OnScalePaused += () => Debug.Log("Scale Paused");
        }

        private void Update()
        {
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                scaler.Pause();
            }
        }
    }
}