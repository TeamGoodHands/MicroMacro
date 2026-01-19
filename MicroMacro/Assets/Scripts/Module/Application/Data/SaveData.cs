using System;
using System.Collections.Generic;

namespace Module.Application.Data
{
    [Serializable]
    public class SaveData
    {
        // クリア済みのステージIDリスト
        // List<string>にすることで "1-1", "Boss", "Extra-1" など自由に増やせる
        public List<string> clearedStageIds = new List<string>();

        // 最後に遊んだ日時など（将来用）
        public string lastPlayedDate;
    }
}