using Unity.Cinemachine;
using UnityEditor;

namespace Module.Level
{
    public static class CinemachineInitialization
    {
        private static bool isEnabled;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            isEnabled = false;
        }

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