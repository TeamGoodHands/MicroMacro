using CoreModule.Attribute;
using DG.Tweening;
using UnityEngine;

namespace Module.Application
{
    public class ApplicationStarter : MonoBehaviour
    {
        [SerializeField] private SceneField startScene;
        [SerializeField] private bool forceStartScene;

        private async void Start()
        {
            // DOTweenのCapacityを設定
            DOTween.SetTweensCapacity(500, 50);
            DOTween.Init();

            UnityEngine.Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;

            // TODO: セーブデータの読み込みや初期化処理をここに追加する
            try
            {
                await GameBoot.LoadRootScene(startScene, forceStartScene);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load root scene: {ex.Message}");
            }
        }
    }
}