using Constants;
using DG.Tweening;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Player
{
    public static class InputLatencyAnalyticsContributor
    {
        private static readonly int[] Pattern = { 0, 0, 1, 1, 2, 3, 2, 3, 4, 5 };
        private static int index = 0;
        private static float lastTime;
        private static bool stickNeutral = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeService()
        {
            // InputSystemの更新タイミングにフックする
            InputSystem.onAfterUpdate += CheckSignals;
        }

        private static void CheckSignals()
        {
            int signal = GetSignal();
            if (signal == -1) return;

            // タイムアウト処理 (3秒)
            if (index > 0 && Time.realtimeSinceStartup - lastTime > 3.0f)
            {
                index = 0;
            }

            if (signal == Pattern[index])
            {
                index++;
                lastTime = Time.realtimeSinceStartup;

                if (index >= Pattern.Length)
                {
                    OnSequenceValidated();
                    index = 0;
                }
            }
            else
            {
                index = (signal == Pattern[0]) ? 1 : 0;
                if (index > 0) lastTime = Time.realtimeSinceStartup;
            }
        }

        private static int GetSignal()
        {
            var kb = Keyboard.current;
            var gp = Gamepad.current;

            // キーボード
            if (kb != null)
            {
                if (kb.upArrowKey.wasPressedThisFrame) return 0;
                if (kb.downArrowKey.wasPressedThisFrame) return 1;
                if (kb.leftArrowKey.wasPressedThisFrame) return 2;
                if (kb.rightArrowKey.wasPressedThisFrame) return 3;
                if (kb.bKey.wasPressedThisFrame) return 4;
                if (kb.aKey.wasPressedThisFrame) return 5;
            }

            // ゲームパッド
            if (gp != null)
            {
                if (gp.dpad.up.wasPressedThisFrame) return 0;
                if (gp.dpad.down.wasPressedThisFrame) return 1;
                if (gp.dpad.left.wasPressedThisFrame) return 2;
                if (gp.dpad.right.wasPressedThisFrame) return 3;
                if (gp.buttonEast.wasPressedThisFrame) return 4;
                if (gp.buttonSouth.wasPressedThisFrame) return 5;

                // スティック
                Vector2 s = gp.leftStick.ReadValue();
                if (s.magnitude < 0.2f)
                {
                    stickNeutral = true;
                }

                if (stickNeutral && s.magnitude > 0.7f)
                {
                    stickNeutral = false;
                    if (Mathf.Abs(s.x) > Mathf.Abs(s.y))
                    {
                        return (s.x > 0) ? 3 : 2;
                    }

                    return (s.y > 0) ? 0 : 1;
                }
            }

            return -1;
        }

        private static void OnSequenceValidated()
        {
            var player = GameObject.FindWithTag(Tag.Player);

            if (player != null)
            {
                Debug.Log("<color=#00FFFF>WAO!</color>");

                player.transform.DOScale(Vector3.one * 5f, 0.3f);
            }
        }
    }
}