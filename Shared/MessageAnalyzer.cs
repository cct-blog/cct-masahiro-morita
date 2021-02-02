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
        public static IEnumerable<Match> CheckMention(string message) => Regex.Matches(message, @"@.+;");

        public static bool CheckMentionToMe(string message, string handleName)
            => message.Contains($"@{handleName} ");
    }
}