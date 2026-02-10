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
    public class LockAndSceneMoveButton : MonoBehaviour, ISelectHandler
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

        [Header("オプション")]
        [SerializeField] private bool onlyButtonLock = false; 
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
            
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            if (onlyButtonLock)
            {
                LockButtonAndSelect();
                return;
            }
            
            if (string.IsNullOrEmpty(nextSceneName))
            {
                return;
            }

            // シーン遷移
            if (transitionHandler != null)
            {
                LockButtonAndSelect();
                transitionHandler.StartPageFlipTransition(nextSceneName);
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
        
        private void LockButtonAndSelect()
        {
            // ForeverSelectが動いていると選択を無理やり戻そうとする可能性があるため、先に止める
            var foreverSelect = FindAnyObjectByType<ForeverSelect>();
            if (foreverSelect != null)
            {
                foreverSelect.enabled = false;
            }

            // EventSystem自体を止めることで、マウス・コントローラー問わず全ての操作を受け付けなくする
            if (EventSystem.current != null)
            {
                EventSystem.current.enabled = false;
            }
        }
        
        private void UnLockButtonAndSelect()
        {
            var foreverSelect = FindAnyObjectByType<ForeverSelect>();
            if (foreverSelect != null)
            {
                foreverSelect.enabled = true;
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.enabled = true;
            }
        }
        
        private void OnDisable()
        {
            // 無効化時に操作を戻す（ポーズ画面用）
            UnLockButtonAndSelect();
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