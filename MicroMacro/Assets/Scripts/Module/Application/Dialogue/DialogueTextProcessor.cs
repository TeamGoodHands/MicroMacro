using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Module.Application.Dialogue
{
    public class DialogueTextProcessor
    {
        // ルビタグを検知する正規表現
        // <r=...> または <r="..."> に対応し、タグの中身（漢字）をグループ2で取得
        private static readonly Regex RubyTagRegex = new Regex(@"<r=""?([^"">]+)""?>([^<]+)</r>", RegexOptions.Compiled);

        /// <summary>
        /// テキストを指定された文字数でページ分割する（ルビタグ対応版）
        /// </summary>
        public List<string> SplitTextToPages(string text, int maxCharsPerPage, int maxOverrunChars)
        {
            var pages = new List<string>();
            if (string.IsNullOrEmpty(text)) return pages;

            StringBuilder currentPage = new StringBuilder();
            int currentVisibleCount = 0; // 画面に見えている文字数だけのカウント

            int i = 0;
            while (i < text.Length)
            {
                // 現在位置からルビタグが始まっているかチェック
                var match = RubyTagRegex.Match(text, i);
                
                // マッチし、かつマッチした場所が現在のインデックス(i)と一致する場合（＝ここからタグが始まる）
                if (match.Success && match.Index == i)
                {
                    string fullTag = match.Value;             // <r=よみ>漢字</r> 全体
                    string kanjiPart = match.Groups[2].Value; // "漢字" の部分のみ
                    
                    int kanjiLength = kanjiPart.Length;

                    // このタグを入れるとページあふれするかチェック
                    if (currentVisibleCount + kanjiLength > maxCharsPerPage + maxOverrunChars)
                    {
                        // ページ終了
                        pages.Add(currentPage.ToString());
                        currentPage.Clear();
                        currentVisibleCount = 0;
                    }

                    // タグ全体をページに追加するが、カウントは「漢字の文字数」だけ増やす
                    currentPage.Append(fullTag);
                    currentVisibleCount += kanjiLength;

                    // インデックスをタグの文字数分進める
                    i += fullTag.Length;
                }
                else
                {
                    // 通常の文字の場合
                    char c = text[i];
                    
                    // ページあふれチェック
                    if (currentVisibleCount + 1 > maxCharsPerPage + maxOverrunChars)
                    {
                        pages.Add(currentPage.ToString());
                        currentPage.Clear();
                        currentVisibleCount = 0;
                    }

                    currentPage.Append(c);
                    currentVisibleCount++;
                    i++;
                }
            }

            // 最後のページを追加
            if (currentPage.Length > 0)
            {
                pages.Add(currentPage.ToString());
            }

            return pages;
        }

        /// <summary>
        /// 句読点判定
        /// </summary>
        public bool IsPunctuation(char character)
        {
            return character == '、' || character == '。' || character == '！' || character == '？' ||
                   character == '!' || character == '?';
        }
    }
}