using OBSWebsocketDotNet;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;

public static class RecordController
{
    private static readonly OBSWebsocket websocket = new();
    private static bool isInitialize;
    private static bool isRecordingInternal;
    private static string host;
    private static string port;
    private static string password;
    private static CancellationTokenSource cancellationTokenSource = new();
    
    // エディタ上で終了するとstatic変数はリセットされないため、開始前にリセット
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // websocketはreadonly、その他はInitializeで上書きされるため、
        // isInitializeのリセットだけで十分。
        isInitialize = false;
        isRecordingInternal = false;
    }

    public static bool Initialize(string host, string port, string password)
    {
        if (isInitialize)
        {
            Debug.LogWarning("すでに初期化済みです。");
            return false;
        }
        
        RecordController.host = host;
        RecordController.port = port;
        RecordController.password = password;
        isInitialize = true;
        return true;
    }

    public static string Host
    {
        get { return host; }
    }
    public static string Port
    {
        get { return port; }
    }
    public static string Password
    {
        get { return password; }
    }

    public static async UniTask OBSConnect(CancellationToken token)
    {
        if (!isInitialize)
            return;
       
        // タイムアウトした際再度接続できるようCTS(ストップボタン的な物)をリセット
        cancellationTokenSource = new CancellationTokenSource();
        
        websocket.ConnectAsync("ws://" + Host + ":" + Port, Password);
        var source = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationTokenSource.Token);
        var ct = source.Token;
        Timeout(ct);
        var canceled = await UniTask.WaitUntil(() => websocket.IsConnected, PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
        if (canceled)
            return;
    }

    public static void OBSDisconnect()
    {
        if (websocket.IsConnected)
            websocket.Disconnect();
    }

    public static bool IsConnected()
    {
        return websocket.IsConnected;
    }

    public static void RecordStart(string uniqueId = null)
    {
        if (!websocket.IsConnected || isRecordingInternal)
        {
            Debug.Log("接続がされていないか、すでに録画中です。");
            return;
        }
        
        try
        {
            if (!string.IsNullOrEmpty(uniqueId))
            {
                // OBSのプロファイル設定「Output」カテゴリの「FilenameFormatting」を書き換え
                string newFormat = $"%CCYY-%MM-%DD %hh-%mm-%ss_{uniqueId}";
                websocket.SetProfileParameter("Output", "FilenameFormatting", newFormat);
                
                Debug.Log($"OBSファイル名設定を変更: {newFormat}");
            }
            websocket.StartRecord();
            isRecordingInternal = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"録画開始時にエラーが発生しました: {e.Message}");
        }
    }

    public static string RecordStop()
    {
        if (!isRecordingInternal)
            return null;
        
        try
        {
            if (websocket.IsConnected)
            {
                string path = websocket.StopRecord();
                isRecordingInternal = false;
                return path;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"録画停止時にエラーが発生しました: {e.Message}");
        }
        // 何らかのエラーでも状態falseに
        isRecordingInternal = false;
        return null;
    }

    public static bool IsRecording()
    {
        return isRecordingInternal;
    }

    private static async void Timeout(CancellationToken token)
    {
        var canceled = await UniTask.Delay(TimeSpan.FromSeconds(10), true, PlayerLoopTiming.Update, token).SuppressCancellationThrow();
        if (canceled)
            return;
        cancellationTokenSource.Cancel();
    }
    
    // 必要に応じてフォーマットを元に戻す関数
    public static void ResetFileNameFormat()
    {
        if (!websocket.IsConnected)
            return;
        try
        {
            websocket.SetProfileParameter("Output", "FilenameFormatting", "%CCYY-%MM-%DD %hh-%mm-%ss");
        } 
        catch {}
    }
}