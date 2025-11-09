using UnityEngine;
using System.IO; 
using System.Text; 

public class ErrorLogger : MonoBehaviour
{
    public static ErrorLogger Instance { get; private set; }

    // trueにするとエラーと例外のみ、falseにすると全ログを記録
    [SerializeField]
    private bool logErrorsOnly = true; 

    private string logFilePath;
    private StringBuilder logBuilder = new StringBuilder();

    void Awake()
    {
        // シンプルなシングルトン設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ログファイルのパスを決定
            logFilePath = Path.Combine(Application.persistentDataPath, "game_log.txt");
            
            // 起動時にログファイルの場所をコンソールに出しておく
            Debug.Log($"ログファイル保存先: {logFilePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // イベントにメソッドを登録
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // オブジェクトが破棄される際にイベントから登録解除
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

        // ファイルに追記
        // (try-catchで囲むと、ファイルI/Oエラーで無限ループするのを防げる)
        try
        {
            File.AppendAllText(logFilePath, logBuilder.ToString());
        }
        catch (IOException ex)
        {
            // ここでのDebug.Logは無限ループするので避ける
            Debug.LogWarning($"ログの書き込みに失敗: {ex.Message}");
        }
    }
}