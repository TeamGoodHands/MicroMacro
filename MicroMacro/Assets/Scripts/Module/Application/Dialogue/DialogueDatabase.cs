using UnityEngine;
using System.Collections.Generic;

namespace Module.Application.Dialogue
{
    /// <summary>
    /// シーンに配置してセリフデータを一元管理するクラス
    /// </summary>
    public class DialogueDatabase : MonoBehaviour
    {
       public static DialogueDatabase Instance { get; private set; }
       // <"登録名", DialogueItem本体> の一意で登録、保持する辞書
       private readonly Dictionary<string, DialogueItem> database = new Dictionary<string, DialogueItem>();
       
       [SerializeField] private DialogueCollection[] dialogueCollections;

       private void Awake()
       {
           if (Instance == null)
           {
               Instance = this;
               DontDestroyOnLoad(gameObject);   // 何かの子にしてしまうと効かないので注意
               InitializeDatabase();
           }
           else
           {
               Destroy(this);
           }
       }

       private void InitializeDatabase()
       {
           foreach (var collection in dialogueCollections)
           {
               if (collection == null)
                   continue;
               
               foreach (var item in collection.items)
               {
                   if (CheckBlank(item) == false)   // 設定ミスがないか各アイテムチェック
                       continue;
                   
                   // 同じ名前のセリフが登録されていたら警告
                   if (database.ContainsKey(item.EntryName))
                   {
                       Debug.LogWarning($"[DialogueDatabase] 重複するキーが見つかりました: {item.EntryName} 上書きされます。");
                   }

                   database[item.EntryName] = item;
               }
           }
       }

       private bool CheckBlank(DialogueItem item)
       {
           if (string.IsNullOrWhiteSpace(item.EntryName))
           {
               Debug.LogError("名前が空欄です。");
               return false;
           }

           if (string.IsNullOrWhiteSpace(item.Text))
           {
               Debug.LogError("セリフが空欄です。");
               return false;
           }

           if (item.DisplayTime <= 1f)
           {
               Debug.LogWarning("表示時間が一秒以下です。Name: " + item.EntryName); 
           }

           return true;
       }

       /// <summary>
       /// 登録名からセリフデータを取得
       /// </summary>
       public DialogueItem GetItem(string name)
       {
           if (database.TryGetValue(name, out DialogueItem item))
           {
               return item;
           }
           
           Debug.LogError($"[DialogueDatabase] Dialogue itemが見つかりません。name: {name}");
           return null;
       }
    }
}