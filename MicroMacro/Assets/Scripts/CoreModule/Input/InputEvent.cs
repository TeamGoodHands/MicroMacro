using System;
using UnityEngine.InputSystem;

namespace CoreModule.Input
{
    /// <summary>
    ///     InputActionのラッパークラス
    /// </summary>
    public class InputEvent
    {
        private readonly InputAction inputAction;

        public InputEvent(InputAction inputAction)
        {
            this.inputAction = inputAction;

            inputAction.started += OnStarted;
            inputAction.performed += OnPerformed;
            inputAction.canceled += OnCanceled;
        }

        public event Action<InputAction.CallbackContext> Started;
        public event Action<InputAction.CallbackContext> Performed;
        public event Action<InputAction.CallbackContext> Canceled;

        private void OnStarted(InputAction.CallbackContext ctx)
        {
            Started?.Invoke(ctx);
        }

        private void OnPerformed(InputAction.CallbackContext ctx)
        {
            Performed?.Invoke(ctx);
        }

        private void OnCanceled(InputAction.CallbackContext ctx)
        {
            Canceled?.Invoke(ctx);
        }

        public TValue ReadValue<TValue>() where TValue : struct
        {
            return inputAction.ReadValue<TValue>();
        }

        public void Clear()
        {
            inputAction.started -= OnStarted;
            inputAction.performed -= OnPerformed;
            inputAction.canceled -= OnCanceled;

            Started = null;
            Performed = null;
            Canceled = null;
        }

        public InputAction GetInputAction()
        {
            return inputAction;
        }
    }
}