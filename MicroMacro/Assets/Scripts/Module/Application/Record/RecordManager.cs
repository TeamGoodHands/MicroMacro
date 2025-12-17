using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Module.Application.Record
{
    /// <summary>
    /// シングルトンでシーンの動きを監視し、ゲームの開始と終了に合わせて録画、停止を行うクラス
    /// </summary>
    public class RecordManager : MonoBehaviour
    {
        public static RecordManager Instance { get; private set; }
        
        // 現在のセッションIDを外部（FeedbackSender）から取れるようにする
        public string CurrentTestPlayID { get; private set; }
        [SerializeField] private string host;
        [SerializeField] private string port;
        private string password;
        
        [SerializeField] private string[] recordStartSceneNames;
        [SerializeField] private string[] recordStopSceneNames;

        public async UniTask SetPasswordAsync(string value)
        {
            password = value;
                
            if (!RecordController.IsConnected())
                await Initialize();
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private async UniTask Initialize()
        {
            if (!RecordController.Initialize(host, port, password))
                return;

            // GameObject破棄時にキャンセルされるtoken取得
            CancellationToken token = this.GetCancellationTokenOnDestroy();

            try
            {
                await RecordController.OBSConnect(token);

                if (RecordController.IsConnected())
                {
                    Debug.Log("OBSへの接続に成功しました。");
                    SceneManager.sceneLoaded -= OnSceneLoaded;  // 多重登録防ぐ
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("OBSへの接続がキャンセルされました。");
            }
            catch (Exception e)
            {
                Debug.LogError($"OBS接続中にエラーが発生しました: {e}");
            }
        }
        
        private void OnDestroy()
        {
            // 破棄されるのがInstanceじゃない(=シーンロード時の複製)ならreturn
            if (Instance != this)
                return;
            
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (RecordController.IsRecording())
            {
                RecordController.RecordStop();
            }
            
            RecordController.ResetFileNameFormat();
            RecordController.OBSDisconnect();
        }

        private void OnApplicationQuit()
        {
            if (Instance == this)
            {
                RecordController.ResetFileNameFormat();
            }
        }

        // ボタンからも呼び出し可能
        // TODO ステージセレクトのボタン周りを改善しボタンで録画開始できるよう変更
        /*public void RecordStart()
        {
            RecordController.RecordStart();
        }*/

        // ステージセレクト画面のボタン周りがよく分からなかったのでひとまずシーン名から録画開始に。
        private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!RecordController.IsConnected())
            {
                Debug.Log("接続されていません。録画開始に失敗しました。");
                return;
            }
            
            // 録画開始対象のシーンかチェック
            if (!RecordController.IsRecording())
            {
                foreach (var name in recordStartSceneNames)
                {
                    if (scene.name == name)
                    {
                        // GUID:「世界中で重複しない番号」を作る仕組み
                        // ファイル名に使いやすいよう、短めのIDを生成
                        CurrentTestPlayID = "Play_" + Guid.NewGuid().ToString().Substring(0, 8);
                        
                        Debug.Log($"テストプレイ開始 ID: {CurrentTestPlayID}");
                        
                        RecordController.RecordStart(CurrentTestPlayID);
                        return;
                    }
                }
            }
            
            foreach (var name in recordStopSceneNames)
            {
                if (scene.name == name)
                {
                    RecordController.RecordStop();

                    if (ErrorLogger.Instance != null)
                    {
                        Debug.Log("ゲーム終了 : エラーログの送信を開始します。");
                        await ErrorLogger.Instance.SendLogExternalAsync();
                    }
                    return;
                }
            }
        }
    }
}