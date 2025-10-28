using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

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
               DontDestroyOnLoad(gameObject);
               InitializeDatabase();
           }
           else
           {
               Destroy(gameObject);
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
                   // 同じ名前のセリフが登録されていたら警告
                   if (database.ContainsKey(item.EntryName))
                   {
                       Debug.LogWarning($"[DialogueDatabase] 重複するキーが見つかりました: {item.EntryName} 上書きされます。");
                   }

                   database[item.EntryName] = item;
               }
           }
           Debug.Log($"[DialogueDatabase] {database.Count} アイテムが初期化されました。");
          
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