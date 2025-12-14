using System;
using UnityEngine;
using System.IO; 
using System.Text; 
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Module.Application.Record;

public class ErrorLogger : MonoBehaviour
{
    public static ErrorLogger Instance { get; private set; }

    // trueにするとエラーと例外のみ、falseにすると全ログを記録
    [SerializeField]
    private bool logErrorsOnly = true;

    // ドライブに送信する用GASのURL
    [SerializeField] private string gasWebAppUrl =
        "https://script.google.com/macros/s/AKfycby-_KHHNDwTfiuUwysIeUm6JpSveyf5pHEwFkVjOptdhYfJI5hyfxvBGhfEYClcFZJh/exec";

    private string logFilePath;
    // StringBuilder : 文字列の編集ができるstring
    private StringBuilder logBuilder = new StringBuilder();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ログファイルのパスを決定
            logFilePath = Path.Combine(Application.persistentDataPath, "game_log.txt");
            Debug.Log($"ログファイル保存先: {logFilePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // ログタイプでフィルタリング
        if (logErrorsOnly && type != LogType.Error && type != LogType.Exception)
        {
            return;
        }

        // ログのフォーマットを作成
        logBuilder.Clear();
        logBuilder.AppendLine("--------------------");
        logBuilder.AppendLine($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] - [{type}]");
        logBuilder.AppendLine(logString);
        logBuilder.AppendLine(stackTrace);
        logBuilder.AppendLine("--------------------");

        string finalLog = logBuilder.ToString();
        
        // ローカル保存
        try
        {
            File.AppendAllText(logFilePath, logBuilder.ToString());
        }
        catch (IOException ex)
        {
            // ここでのDebug.Logは無限ループするので避ける
            Debug.LogWarning($"ログの書き込みに失敗: {ex.Message}");
        }
        
        // Driveに送信
        UploadLogToDriveAsync(finalLog).Forget();
    }

    private async UniTaskVoid UploadLogToDriveAsync(string logContent)
    {
        // 回線繋がってない場合弾く
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return;

        // ID取得
        string playId = "unknown_id";
        if (RecordManager.Instance != null && !string.IsNullOrEmpty(RecordManager.Instance.CurrentTestPlayID))
        {
            playId = RecordManager.Instance.CurrentTestPlayID;
        }

        // ファイル名にIDを含める (GAS側での紐づけ用)
        string fileName = $"ErrorLog_{playId}_{System.DateTime.Now:yyyyMMMMdd_HHmmss}.txt";
        
        // Jsonデータ作成
        LogData data = new LogData
        {
            fileName = fileName,
            content = logContent
        };
        string json = JsonUtility.ToJson(data);

        // POST送信
        using (var www = new UnityWebRequest(gasWebAppUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"ログアップロード失敗: {www.error}");
                }
                else
                {
                    // 成功ログ出したって良い
                    Debug.Log("ログアップロード完了");
                }
            }
            catch (Exception ex)
            {
                // キャンセルやその他の通信エラー
                Debug.LogWarning($"ログ送信例外: {ex.Message}");
            }
        }

    }

    [Serializable]
    private class LogData
    {
        public string fileName;
        public string content;
    }
}