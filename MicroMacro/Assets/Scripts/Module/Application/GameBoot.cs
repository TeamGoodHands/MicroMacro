using CoreModule.Attribute;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Module.Application
{
    /// <summary>
    /// 起動時の初期シーンのロードクラス
    /// </summary>
    public static class GameBoot
    {
        public static bool IsBooted { get; private set; } = false;

        private static string defaultScene;
        private const string BootSceneName = "Root";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            Reset();

            defaultScene = SceneManager.GetActiveScene().name;

            // 強制的に初期シーンに遷移する
            if (defaultScene != BootSceneName)
            {
                SceneManager.LoadScene(BootSceneName);
            }
        }

        private static void Reset()
        {
            IsBooted = false;
            defaultScene = string.Empty;
        }

        /// <summary>
        /// メインシーンをロードします
        /// </summary>
        /// <param name="startScene"></param>
        /// <param name="forceStartScene"></param>
        public static async UniTask LoadRootScene(SceneField startScene, bool forceStartScene)
        {
            IsBooted = true;

            string sceneName = forceStartScene ? startScene : defaultScene;

            try
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load scene '{sceneName}': {ex.Message}");
                throw;
            }
        }
    }
}