using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

namespace CoreModule.Input
{
    /// <summary>
    /// ゲームパッドのバイブレーションを管理するクラス
    /// </summary>
    public class GamepadVibrator
    {
        private bool enabled;
        private IDualMotorRumble motorRumble;
        private CancellationTokenSource cTokenSource;

        public GamepadVibrator()
        {
            // デバイスの接続状況の変化を監視
            InputSystem.onDeviceChange += (device, change) =>
            {
                bool useDevice = change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected || change == InputDeviceChange.Enabled;

                if (device is IDualMotorRumble dualMotorRumble)
                {
                    // 新たなGamePadが接続された場合、それを使用する
                    if (useDevice)
                    {
                        motorRumble = dualMotorRumble;
                        enabled = true;
                    }
                    // GamePadが外された場合、他に接続されているGamePadを使用する
                    else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected || change == InputDeviceChange.Disabled)
                    {
                        motorRumble = Gamepad.current;
                        if (motorRumble == null)
                        {
                            enabled = false;
                        }
                    }
                }
            };
        }

        /// <summary>
        /// バイブレーションを有効化します
        /// </summary>
        public void Enable()
        {
            if (enabled || Gamepad.current == null)
            {
                return;
            }

            motorRumble = Gamepad.current;
            enabled = true;
        }

        /// <summary>
        /// バイブレーションを無効化します
        /// </summary>
        public void Disable()
        {
            if (!enabled)
            {
                return;
            }

            Reset();
            motorRumble = null;
            enabled = false;
        }

        /// <summary>
        /// 指定した秒数バイブレーションを行います
        /// </summary>
        /// <param name="right">右側の強さ (0 ~ 1)</param>
        /// <param name="left">左側の強さ (0 ~ 1)</param>
        /// <param name="duration">バイブレーションを行う秒数</param>
        public void Vibrate(float right, float left, float duration)
        {
            if (!enabled)
            {
                return;
            }

            VibrateTask(right, left, duration);
        }

        private async void VibrateTask(float right, float left, float duration)
        {
            SetMotorSpeed(right, left);
            await Task.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cTokenSource.Token);
            Reset();
        }

        /// <summary>
        /// バイブレーションの強さを設定します
        /// </summary>
        /// <param name="right">右側の強さ (0 ~ 1)</param>
        /// <param name="left">左側の強さ (0 ~ 1)</param>
        public void SetMotorSpeed(float right, float left)
        {
            if (!enabled)
            {
                return;
            }

            Reset();
            cTokenSource = new CancellationTokenSource();
            motorRumble.SetMotorSpeeds(left, right);
        }

        /// <summary>
        /// バイブレーションの強さをリセットします
        /// </summary>
        public void Reset()
        {
            if (!enabled)
            {
                return;
            }

            cTokenSource?.Cancel();
            cTokenSource?.Dispose();
            cTokenSource = null;
            motorRumble.ResetHaptics();
        }

        /// <summary>
        /// バイブレーションを一時停止します
        /// </summary>
        public void Pause()
        {
            if (!enabled)
            {
                return;
            }

            motorRumble.PauseHaptics();
        }

        /// <summary>
        /// バイブレーションを再開します
        /// </summary>
        public void Resume()
        {
            if (!enabled)
            {
                return;
            }

            motorRumble.ResumeHaptics();
        }
    }
}