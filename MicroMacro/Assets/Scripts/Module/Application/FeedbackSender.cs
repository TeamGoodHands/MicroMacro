using System;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Module.Application
{

    [System.Serializable]
    public class FormQuestion
    {
        public string entryID; // 質問項目に対応するID  例:entry.123456
        public QuestionType type;

        // 対応するUIコンポーネント 使うものだけInspectorで設定すればOK
        public TMP_InputField inputField;
        public TMP_Dropdown dropdown;
        [FormerlySerializedAs("toggleGroup")] public ToggleGroup radioToggleGroup; // 単一選択用
        public Toggle[] checkToggles; // 複数選択チェックボックス用

        [Header("「その他」選択肢用の設定")] public Toggle otherToggle; // 「その他」のトグル
        public TMP_InputField otherInputField; // 「その他」の入力欄
        public string otherOptionEntryID; //  その他」入力欄用特別なEntryID
    }

    // GoogleFormの質問タイプ (ToggleGroupはラジオボタン)
    public enum QuestionType
    {
        InputField,
        Dropdown,
        RadioButtons,
        Checkboxes
    }

    public class FeedbackSender : MonoBehaviour
    {
        [Header("共通設定")] [SerializeField] private string formActionURL; // 末尾の/viewformを/formResponseに書き換えた送信先URL 

        [Header("質問リスト")] [SerializeField] private FormQuestion[] questions; // 構造体の配列 

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

            isSending = true;
            // 一度ボタンを無効化
            if (sendButton != null)
                sendButton.interactable = false;

            await PostAsync();

            isSending = false;
            if (sendButton != null)
                sendButton.interactable = true;
        }

        private async UniTask PostAsync()
        {
            // 未設定チェック
            if (questions == null || questions.Length == 0 ||
                string.IsNullOrWhiteSpace(formActionURL))
            {
                Debug.LogWarning("FeedbackSenderの設定が不十分です。");
                return;
            }

            // WWWFormを使って送信するデータを作成->複数選択だと微妙だから自力で作成
            var formFields = new List<string>();

            foreach (var q in questions)
            {
                string value = "";
                // ドロップダウンもラジオボタンも、フォーム側が持っている情報は質問項目の番号ではなく原文なので、Unity側でもtextを取得する
                switch (q.type)
                {
                    case QuestionType.InputField:
                        value = q.inputField.text;
                        if (!string.IsNullOrEmpty(value))
                            formFields.Add(CreateField(q.entryID, value));
                        break;

                    case QuestionType.Dropdown:
                        value = q.dropdown.options[q.dropdown.value].text;
                        if (!string.IsNullOrEmpty(value))
                            formFields.Add(CreateField(q.entryID, value));
                        break;

                    // 選択されている項目を取得->text取得
                    case QuestionType.RadioButtons:
                        Toggle activeToggle = q.radioToggleGroup.GetFirstActiveToggle();
                        if (activeToggle != null)
                        {
                            value = GetToggleText(activeToggle);
                            if (!string.IsNullOrEmpty(value))
                                formFields.Add(CreateField(q.entryID, value));
                        }

                        break;

                    case QuestionType.Checkboxes:
                        // ONになっているトグルをループ
                        foreach (var toggle in q.checkToggles)
                        {
                            if (toggle == q.otherToggle)
                            {
                                Debug.Log("「その他」トグルは下の専用箇所にアタッチしてください。");
                                continue; 
                            }
                            
                            if (toggle.isOn)
                            {
                                value = GetToggleText(toggle);
                                if (!string.IsNullOrEmpty(value))
                                {
                                    // 同じEntryIDで複数追加する
                                    formFields.Add(CreateField(q.entryID, value));
                                }
                            }
                        }

                        // 「その他」の処理
                        if (q.otherToggle != null && q.otherToggle.isOn)
                        {
                            // 「その他」が選ばれたことを示すデータを追加
                            formFields.Add(CreateField(q.entryID, "__other_option__"));

                            // 「その他」の入力内容を専用IDで追加
                            value = q.otherInputField.text;
                            if (!string.IsNullOrEmpty(value))
                            {
                                formFields.Add(CreateField(q.otherOptionEntryID, value));
                            }
                            else
                            {
                                Debug.LogError("その他が選択されましたが文章が空欄です。");
                            }
                        }
                        break;
                }
            }

            string postData = string.Join("&", formFields);
            Debug.Log($"<color=cyan>送信データ: {postData}</color>");

            using (UnityWebRequest www = new UnityWebRequest(formActionURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("フィードバックが正常に送信されました");
                    ResetAllFields();
                    OnSend?.Invoke();
                }
                else
                {
                    Debug.LogError("フィードバックの送信に失敗しました: " + www.error);
                }
            }
        }

        /// <summary>
        /// 項目(entryID)とその回答をフォームに追加
        /// </summary>
        private string CreateField(string entryID, string value)
        {
            return $"{UnityWebRequest.EscapeURL(entryID)}={UnityWebRequest.EscapeURL(value)}";
        }

        /// <summary>
        /// // Toggleからテキストを取得するヘルパー関数（TextMeshPro/レガシーUI両対応）
        /// </summary>
        private string GetToggleText(Toggle toggle)
        {
            var tempText = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (tempText != null)
                return tempText.text;

            var legacyText = toggle.GetComponentInChildren<Text>();
            if (legacyText != null)
                return legacyText.text;
            
            Debug.LogError("textの取得に失敗しました。");
            return "";
        }
    
        /// <summary>
        /// 全パターンの入力項目リセット
        /// </summary>
        private void ResetAllFields()
        {
            foreach (var q in questions)
            {
                if (q.inputField != null)  q.inputField.text = "";
                if (q.dropdown != null)    q.dropdown.value = 0;
                if (q.checkToggles != null)
                {
                    foreach (var t in q.checkToggles)
                            t.isOn = false;
                }
                if (q.otherToggle != null)     q.otherToggle.isOn = false;
                if (q.otherInputField != null) q.otherInputField.text = "";
            }
        }

    }
}