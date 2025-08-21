using System;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;

namespace Module.Application
{
    
    [System.Serializable]
    public class FormQuestion
    {
        public string entryID;   // 質問項目に対応するID  例:entry.123456
        public QuestionType type;
        
        // 対応するUIコンポーネント 使うものだけInspectorで設定
        public TMP_InputField inputField;
        public TMP_Dropdown   dropdown;
        public ToggleGroup    toggleGroup;
    }

    // GoogleFormの質問タイプ (ToggleGroupはラジオボタン)
    public enum QuestionType
    {
        InputField,
        Dropdown,
        ToggleGroup
    }
    
    public class FeedbackSender : MonoBehaviour
    {
        [Header("共通設定")]
        [SerializeField] private string formActionURL; // 末尾の/viewformを/formResponseに書き換えた送信先URL 

        [Header("質問リスト")] 
        [SerializeField] private FormQuestion[] questions;  // 構造体の配列 
        
        [SerializeField] private Button sendButton;
        private bool isSending = false;

        public event Action OnSend;
        
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
            FormQuestion firstInput = questions.FirstOrDefault(q => q.type == QuestionType.InputField);
            
            if (firstInput != null && string.IsNullOrEmpty(firstInput.inputField.text))
            {
                Debug.Log("必須項目が空欄です！");
                return;
            }
            
            // 一度ボタンを無効化
            if (sendButton != null)
                sendButton.interactable = false;
            isSending = true;
            
            await PostAsync(); 
           
            isSending = false;
            if (sendButton != null)
                sendButton.interactable = true;
            
        }

        private async UniTask PostAsync()
        {
            // 未設定チェック
            if (questions == null || questions.Length == 0)
            {
                Debug.LogWarning("questionsが未設定または空です。");
                return;
            }
            if (string.IsNullOrWhiteSpace(formActionURL))
            {
                Debug.LogError("formActionURLが未設定です。");
                return;
            }
            
            // WWWFormを使って送信するデータを作成
            WWWForm form = new WWWForm();

            foreach (var q in questions)
            {
                string value = "";
                
                // ドロップダウンもラジオボタンも、フォーム側が持っている情報は質問項目の番号ではなく原文なので、Unity側でもtextを取得する
                switch (q.type)
                {
                    case QuestionType.InputField:
                        value = q.inputField.text;
                        break;
                    
                    case QuestionType.Dropdown:
                        value = q.dropdown.options[q.dropdown.value].text;
                        break;
                    
                    // 選択されている項目を取得->text取得
                    case QuestionType.ToggleGroup:
                        Toggle activeToggle = q.toggleGroup.GetFirstActiveToggle();
                        if (activeToggle != null)
                        {
                            value = activeToggle.GetComponentInChildren<Text>().text;
                        }
                        break;
                }

                // 項目(entryID)とその回答をフォームに追加
                if (!string.IsNullOrEmpty(value))
                {
                    form.AddField(q.entryID, value);
                }
            }
           
            
            // PostでformActionURLにデータを送信と待機
            UnityWebRequest www = UnityWebRequest.Post(formActionURL, form);
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("フィードバックが正常に送信されました！");
                //TODO 送信完了メッセージ表示
                foreach (var q in questions)
                {
                    if (q.inputField != null)
                        q.inputField.text = "";
                    
                    if (q.dropdown != null)
                        q.dropdown.value = 0;
                    
                    if (q.toggleGroup != null)
                        q.toggleGroup.SetAllTogglesOff();
                }
                
                OnSend?.Invoke();
            }
            else
            {
                Debug.LogError("フィードバックの送信に失敗しました: " + www.error);
            }
        }
    }
}