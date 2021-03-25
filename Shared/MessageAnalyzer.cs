using blazorTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace blazorTest.Shared
{
    public static class MessageAnalyzer
    {
        /// <summary>
        /// メッセージでメンションされたユーザーを重複を削除して返却します。
        /// </summary>
        /// <param name="message">投稿メッセージ</param>
        /// <returns>メンションリスト</returns>
        public static IEnumerable<string> GetMentionedUser(string message)
        {
            var mentions = CheckMention(message);

            var mentionedUser = mentions.Select(mention =>
                mention.Value.Substring(1, mention.Length - 2));

            return mentionedUser.Distinct();
        }

        /// <summary>
        /// メッセージ内のメンションをチェックします。
        /// メンションされた位置が必要な場合はこちらを使用してください。
        /// </summary>
        /// <param name="message">投稿メッセージ</param>
        /// <returns>メンションリスト</returns>
        public static IEnumerable<Match> CheckMention(string message)
            => Regex.Matches(message, @"@.+;");

        /// <summary>
        /// 対象へのメンションが存在するかを確認
        /// </summary>
        /// <param name="message">投稿メッセージ</param>
        /// <param name="handleName">メンションが存在するか確認したい対象ユーザー</param>
        /// <returns></returns>
        public static bool CheckMentionToMe(string message, string handleName)
            => message.Contains($"@{handleName} ");
    }
}