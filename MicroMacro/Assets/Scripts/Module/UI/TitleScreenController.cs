using UnityEngine;
using UnityEngine.UI;
using Module.Application.Data;
using Module.Application.SceneSwitch;

namespace Module.UI
{
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private FadeAndSceneTransition sceneTransition;
        
        [Header("遷移先")]
        [SerializeField] private string firstStageSceneName = "StageSelect"; 

        private void Start()
        {
            // ボタンのリスナー登録
            newGameButton.onClick.AddListener(OnNewGameClicked);
            continueButton.onClick.AddListener(OnContinueClicked);

            // セーブデータがなければ「続きから」を押せなくする（または非表示）
            if (SaveManager.Instance != null)
            {
                continueButton.interactable = SaveManager.Instance.HasSaveData();
            }
        }

        private void OnNewGameClicked()
        {
            // データを消して新規開始
            SaveManager.Instance.DeleteSave();
            sceneTransition.StartPageFlipTransition("Opening");
        }

        private void OnContinueClicked()
        {
            // データはそのままで遷移
            sceneTransition.StartPageFlipTransition(firstStageSceneName);
        }
    }
}