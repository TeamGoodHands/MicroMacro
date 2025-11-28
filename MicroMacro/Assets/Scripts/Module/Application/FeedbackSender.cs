using System;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.IO;
using Module.Application.Record;

namespace Module.Application
{

    [System.Serializable]
    public class FeedbackEntry
    {
        public string submissionId; // 削除時の識別に利用
        public string postData;     // 送信するURLのエンコード済みデータ

        public FeedbackEntry(string data)
        {
            submissionId = System.Guid.NewGuid().ToString();
            postData = data;
        }
    }

    /// <summary>
    /// JsonUtilityでListをシリアライズするためのラッパークラス
    /// </summary>
    [System.Serializable]
    public class FeedbackQueue
    {
        public List<FeedbackEntry> pendingSubmissions = new List<FeedbackEntry>();
    }

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
        public string otherOptionEntryID;      //  その他」入力欄用特別なEntryID
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
        [Header("ID連携設定")] [SerializeField] private string testPlayIdEntryID;
        [SerializeField] private Button sendButton;
        
        private bool isSending = false;
        public event Action OnSend;

        private static bool isSyncing = false; // 複数の送信処理が同時に動くのを防ぐ
        private static string FilePath =>
            Path.Combine(UnityEngine.Application.persistentDataPath, "pending_feedback.json");
        
        private void Start()
        {
            // 起動時に保留中のフィードバック送信を試みる
            TryToSendQueueAsync().Forget();
        }

        // インスペクターから呼び出すための同期メソッド
        public void OnClickSendButton()
        {
            // 非同期処理を完了待たずに実行（Forget）
            SendFeedbackLocallyAsync().Forget();
        }

        /// <summary>
        /// データをローカルに保存し、UIをリセット
        /// その後キューの送信を試みる
        /// </summary>
        private async UniTask SendFeedbackLocallyAsync()
        {
            // 連打対策
            if (isSending)
                return;

            if (!TryBuildPostData(out string postData))
            {
                Debug.LogWarning("設定が不十分かなにも入力していないため送信に失敗しました。");
                return;
            }
            
            isSending = true;
            // 一度ボタンを無効化
            if (sendButton != null)
                sendButton.interactable = false;

            var newEntry = new FeedbackEntry(postData);

            // 既存のキューをロードし、新しいEntryを追加して保存
            var queue = LoadQueue();
            queue.pendingSubmissions.Add(newEntry);
            SaveQueue(queue);
            
            Debug.Log($"<color=green>フィードバックをローカルに保存しました: {newEntry.submissionId}</color>");
            ResetAllFields();
            
            // オフラインならすぐ発火、オンラインなら全部送信してからイベント発火
            bool allSent =　await TryToSendQueueAsync();
            if (allSent)
            {
                OnSend?.Invoke();
            }
            
            isSending = false;
            if (sendButton != null)
                sendButton.interactable = true;
        }

        /// <summary>
        /// 保留中のフィードバックキューを非同期で送信する
        /// </summary>
        private async UniTask<bool> TryToSendQueueAsync()
        {
            if (isSyncing)
            {
                Debug.Log("既に別の同期処理が実行中です。");
                return false; // 既に別の同期処理が実行中
            }

            if (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("オフラインのため、送信をスキップします。");
                return false;
            }

            isSyncing = true;
            try
            {
                var queue = LoadQueue();
                if (queue.pendingSubmissions.Count == 0)
                {
                    // 送信待ちのデータなし = 送信済みとみなす
                    return true;             
                }
            
                // 送信の開始
                Debug.Log($"<color=yellow>保留中のフィードバック{queue.pendingSubmissions.Count}件の送信を開始します...</color>");
            
                // リストをコピーしてイテレート（ループ中に元のリストを変更するため）
                List<FeedbackEntry> entriesToSend = new List<FeedbackEntry>(queue.pendingSubmissions);
                bool queueWasModified = false;

                foreach (var entry in entriesToSend)
                {
                    bool success = await SendDataAsync(entry.postData);
                    if (success)
                    {
                        // 送信成功: 元のキューから削除
                        queue.pendingSubmissions.Remove(entry);
                        queueWasModified = true;
                    }
                    else
                    {
                        // 送信失敗: おそらくネットワークが切断された
                        // この後のキューの送信を中止し、後で再試行する
                        Debug.LogWarning($"フィードバック {entry.submissionId} の送信に失敗。後で再試行します。");
                        
                        // ここでも変更があれば保存
                        if (queueWasModified)
                        {
                            SaveQueue(queue);
                        }
                        return false; // 送信失敗
                    }
                }

                // キューに変更があった場合（＝送信成功した項目があった場合）のみファイルに保存
                if (queueWasModified)
                {
                    SaveQueue(queue);
                }
                
                // 全ての送信が成功した場合はtrue
                return queue.pendingSubmissions.Count == 0;
            }
            finally
            {
                isSyncing = false;
            }
        }

        /// <summary>
        /// 実際のWebリクエスト(送信)部分
        /// </summary>
        /// <param name="postData"></param>
        private async UniTask<bool> SendDataAsync(string postData)
        {
            if (string.IsNullOrWhiteSpace(formActionURL))
            {
                Debug.LogWarning("FeedbackSenderのURLが設定されていません。");
                return false;
            }
            
            using (UnityWebRequest www = new UnityWebRequest(formActionURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("<color=green>フィードバックが正常に送信されました</color>");
                    return true;
                }
                else
                {
                    Debug.LogError("フィードバックの送信に失敗しました: " + www.error);
                    return false;
                }
            }
        }

        /// <summary>
        /// UIからデータを読み取り、送信用の文字列を構築する
        /// </summary>
        /// <param name="postData"></param>
        private bool TryBuildPostData(out string postData)
        {
            postData = null;

            if (questions == null || questions.Length == 0)
            {
                Debug.LogWarning("FeedbackSenderの質問リストが設定されていません。");
                return false;
            }

            // 送信するデータの作成
            var formFields = new List<string>();

            if (!string.IsNullOrEmpty(testPlayIdEntryID) && RecordManager.Instance != null)
            {
                string currentId = RecordManager.Instance.CurrentTestPlayID;

                if (!string.IsNullOrEmpty(currentId))
                {
                    formFields.Add(CreateField(testPlayIdEntryID, currentId));
                    Debug.Log($"フォームにID紐づけ: {currentId}");
                }
            }

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
            
            if (formFields.Count == 0)
            {
                Debug.LogWarning("送信するデータがありません。");
                return false;
            }
            
            postData = string.Join("&", formFields);
            Debug.Log($"<color=cyan>送信データを作成: {postData}</color>");
            return true;
        }

        /// <summary>
        /// ローカルファイルからキューを読み込む
        /// </summary>
        private FeedbackQueue LoadQueue()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonUtility.FromJson<FeedbackQueue>(json) ?? new FeedbackQueue();
                }
                catch (Exception e)
                {
                    Debug.LogError($"フィードバックキューの読み込みに失敗: {e.Message}");
                    return new FeedbackQueue();
                }
            }
            return new FeedbackQueue();
        }

        /// <summary>
        /// キューをローカルファイルに保存する
        /// </summary>
        private void SaveQueue(FeedbackQueue queue)
        {
            try
            {
                string json = JsonUtility.ToJson(queue, true); // trueで整形
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"フィードバックキューの保存に失敗: {e.Message}");
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