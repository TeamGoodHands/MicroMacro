using Unity.Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Module.Level
{
    public static class CinemachineInitialization
    {
        private static bool isEnabled;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            isEnabled = false;
        }
#endif

        public static void CheckInitialization(CinemachineCamera targetCamera)
        {
            if (!IsFirstEnable())
            {
                CinemachineCore.SoloCamera = targetCamera;
                SetEnabled();
            }
            else
            {
                CinemachineCore.SoloCamera = null;
            }
        }

        private static bool IsFirstEnable()
        {
            return isEnabled;
        }

        private static void SetEnabled()
        {
            isEnabled = true;
        }
    }
}