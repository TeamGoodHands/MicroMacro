using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Module.Management;              
using Module.Application.SceneSwitch; 
namespace Module.UI
{
    
    /// <summary>
    /// ボタン決定とセレクトのサウンドのみ行うことも可能なシーン遷移ボタン
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SceneTransitionButton : MonoBehaviour, ISelectHandler
    {
        [Header("遷移設定")]
        [Tooltip("遷移先のシーン名")]
        [SerializeField] private string nextSceneName;

        [Tooltip("シーン遷移マネージャー（空欄の場合、自動で探します）")]
        [SerializeField] private FadeAndSceneTransition transitionHandler;

        [Header("サウンド設定")]
        [Tooltip("決定時（クリック時）のSE名")]
        [SerializeField] private string submitSeName = ""; // 初期値は空

        [Tooltip("選択時（フォーカス時）のSE名")]
        [SerializeField] private string selectSeName = ""; // 初期値は空

        private Button button;
        private bool isFirstSelect = true;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnSubmit);

            // 遷移ハンドラーが未設定ならシーン内から探す
            if (transitionHandler == null)
            {
                transitionHandler = FindAnyObjectByType<FadeAndSceneTransition>();
            }
            
            isFirstSelect = false;
        }
        
        private void OnSubmit()
        {
            // 決定音
            PlaySound(submitSeName);
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log("SceneTransitionButton: nextSceneNameが設定されていません。");
                return;
            }

            // シーン遷移
            if (transitionHandler != null)
            {
                transitionHandler.StartTransition(nextSceneName);
            }
            else
            {
                Debug.LogError("FadeAndSceneTransitionが見つかりません。シーンに配置されているか確認してください。");
            }
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            // 最初の選択時（シーン切り替わった瞬間）は音を鳴らさない
            if (isFirstSelect)
                return;
            
            // 選択音再生
            PlaySound(selectSeName);
        }
       
        private void PlaySound(string seName)
        {
            if (string.IsNullOrEmpty(seName)) return;

            if (SoundManager.instance != null)
            {
                SoundManager.instance.Play(seName);
            }
        }
    }
}