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
            
            // TODO: セーブデータの読み込みや初期化処理をここに追加する
            await GameBoot.LoadRootScene(startScene, forceStartScene);
        }
    }
}