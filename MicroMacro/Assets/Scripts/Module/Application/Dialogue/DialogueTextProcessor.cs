using System.Collections.Generic;

namespace Module.Application.Dialogue
{
    /// <summary>
    /// テキストの解析、ページ分割などのロジックのみを担当するクラス
    /// </summary>
    public class DialogueTextProcessor
    {
        /// <summary>
        /// 文字列をページ分割する。
        /// 区切りがいい（句読点がある）なら多少超過しても許容するように。
        /// </summary>
        /// <param name="maxChars">1ページあたりの基本最大文字数</param>
        /// <param name="maxOverrunChars">ページ分割時に許容する最大超過文字数</param>
        public List<string> SplitTextToPages(string text, int maxChars, int maxOverrunChars)
        {
            var list = new List<string>();
            int currentPos = 0;

            while (currentPos < text.Length)
            {
                // 残りの文字数がmaxChars以下なら、すべて追加して終了
                if (text.Length - currentPos <= maxChars)
                {
                    list.Add(text.Substring(currentPos));
                    break;
                }

                // 基本の分割位置
                int splitLength = maxChars;
                
                // 超過を許容して区切り文字（句読点など）を探す
                // maxCharsの位置から、maxOverrunChars分だけ先をチェック
                bool foundSplitChar = false;
                for (int offset = 0; offset <= maxOverrunChars; offset++)
                {
                    int checkIndex = currentPos + maxChars + offset;

                    // テキストの範囲外ならループ終了
                    if (checkIndex >= text.Length) break;

                    // 区切り文字が見つかったら、そこで切る（その文字を含めるため +1）
                    if (IsSplitPosition(text[checkIndex]))
                    {
                        splitLength = maxChars + offset + 1;
                        foundSplitChar = true;
                        break;
                    }
                }
                
                // ここで句読点の直前に改ページみたいな、「手前」を探す処理を入れても良い。

                list.Add(text.Substring(currentPos, splitLength));
                currentPos += splitLength;
            }

            return list;
        }

        /// <summary>
        /// 句読点の判定
        /// </summary>
        public bool IsPunctuation(char c)
        {
            return "、。！？!?,.".IndexOf(c) >= 0;
        }
        
        /// <summary>
        /// 区切りが良い文字かどうか判定（ページ切り替え時の判断）
        /// </summary>
        private bool IsSplitPosition(char c)
        {
            // 句読点、感嘆符、スペース、閉じ括弧などを区切りとみなす
            return "、。！？!?,. 　」』)".IndexOf(c) >= 0;
        }
    }
}