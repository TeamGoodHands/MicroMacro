using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Module.Application
{
    public class FeedbackSender : MonoBehaviour
    {
        [SerializeField] private string formActionURL; // 末尾の/viewformを/formResponseに書き換えた送信先URL 
        [SerializeField] private string entryID;       // 質問項目に対応するID  例:entry.123456
        
        [SerializeField] private TMP_InputField feedbackInputField; // HierarchyからInputFieldをアタッチ

        // UnitaskはOnClickじゃ呼び出せない
        public void OnClickSendButton()
        {
            SendFeedbackAsync().Forget();
        }
        
        // ボタンのOnClickイベントから直接呼び出す非同期関数はUniTaskVoid型にするのが定石らしい
        private async UniTaskVoid SendFeedbackAsync()
        {
            // InputFieldが空でなければ送信処理を開始
            if (string.IsNullOrEmpty(feedbackInputField.text))
            {
                Debug.Log("空欄で送信することはできません！");
                return;
            }
            
            // 一度ボタンを無効化

            await PostAsync(feedbackInputField.text);
            
            // 失敗ならボタンを再度有効化
        }

        private async UniTask PostAsync(string feedbackText)
        {
           // WWWFormを使って送信するデータを作成
           WWWForm form = new WWWForm();
           form.AddField(entryID, feedbackText);  // EntryIDに感想のテキストをセット
           
           // PostでformActionURLにデータを送信
           UnityWebRequest www = UnityWebRequest.Post(formActionURL, form);

           // リクエストが完了まで待機
           await www.SendWebRequest();

           if (www.result == UnityWebRequest.Result.Success)
           {
               Debug.Log("フィードバックが正常に送信されました！");
               //TODO 送信完了メッセージ表示と連続送信の禁止
               feedbackInputField.text = "";    // 送信後空に
           }
           else
           {
               Debug.LogError("フィードバックの送信に失敗しました: " + www.error);
           }

        }
    }
}