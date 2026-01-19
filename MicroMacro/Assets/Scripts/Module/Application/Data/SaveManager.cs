using UnityEngine;
using System.IO;

namespace Module.Application.Data
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_KEY = "MicroMacro_SaveData";
        private SaveData currentData;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Load(); // 起動時にロード
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// データをロード（なければ新規作成）
        /// </summary>
        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                currentData = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                currentData = new SaveData();
            }
        }

        /// <summary>
        /// データを保存
        /// </summary>
        public void Save()
        {
            currentData.lastPlayedDate = System.DateTime.Now.ToString();
            string json = JsonUtility.ToJson(currentData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// "初めから" 用：データを消去してリセット
        /// </summary>
        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            currentData = new SaveData();
        }

        /// <summary>
        /// セーブデータが存在するか（"続きから"ボタンの活性化用）
        /// </summary>
        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }

        /// <summary>
        /// 指定したステージがクリア済みか判定
        /// </summary>
        public bool IsStageCleared(string stageId)
        {
            if (currentData == null) return false;
            return currentData.clearedStageIds.Contains(stageId);
        }

        /// <summary>
        /// ステージクリアを記録する
        /// </summary>
        public void SetStageCleared(string stageId)
        {
            if (currentData == null) currentData = new SaveData();

            if (!currentData.clearedStageIds.Contains(stageId))
            {
                currentData.clearedStageIds.Add(stageId);
                Save(); // 即時保存
                Debug.Log($"Stage {stageId} Cleared & Saved.");
            }
        }
    }
}