using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace Module.Application
{
    public class FeedbackSender : MonoBehaviour
    {
        [Header("共通設定")]
        [SerializeField] private string formActionURL; // 末尾の/viewformを/formResponseに書き換えた送信先URL 
        
        [Header("感想テキスト欄")]
        [SerializeField] private string textFieldEntryID;       // 質問項目に対応するID  例:entry.123456
        [SerializeField] private TMP_InputField feedbackInputField; // HierarchyからInputFieldをアタッチ

        [Header("選択肢 (プルダウン)")] 
        [SerializeField] private string dropdownEntryID;
        [SerializeField] private TMP_Dropdown questionDropdown;

        [SerializeField] private Button sendButton;

        private bool isSending = false;
        
        // UnitaskはOnClickじゃ呼び出せない
        public void OnClickSendButton()
        {
            SendFeedbackAsync().Forget();
        }
        
        // ボタンのOnClickイベントから直接呼び出す非同期関数はUniTaskVoid型にするのが定石らしい
        private async UniTaskVoid SendFeedbackAsync()
        {
            // 連打対策
            if (isSending)
                return;
            
            // InputFieldが空でなければ送信処理を開始
            if (string.IsNullOrEmpty(feedbackInputField.text))
            {
                Debug.Log("空欄で送信することはできません！");
                return;
            }
            
            // 一度ボタンを無効化
            sendButton.enabled = false;
            isSending = true;

            await PostAsync();
            
            sendButton.enabled = true;
        }

        private async UniTask PostAsync()
        {
            // テキスト取得
            string feedbackText = feedbackInputField.text;

            string selectedOptionText = questionDropdown.options[questionDropdown.value].text;
            
            // WWWFormを使って送信するデータを作成
            WWWForm form = new WWWForm();
            
            // EntryIDに感想のテキストをセット
            form.AddField(textFieldEntryID, feedbackText);  
            form.AddField(dropdownEntryID, selectedOptionText);
           
            // PostでformActionURLにデータを送信と待機
            UnityWebRequest www = UnityWebRequest.Post(formActionURL, form);
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("フィードバックが正常に送信されました！");
                //TODO 送信完了メッセージ表示
                feedbackInputField.text = "";    // 送信後空に
                questionDropdown.value = 0;
            }
            else
            {
                Debug.LogError("フィードバックの送信に失敗しました: " + www.error);
            }
        }
    }
}