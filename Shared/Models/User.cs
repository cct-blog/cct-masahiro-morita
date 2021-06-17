using System;

namespace ChatApp.Shared.Models
{
    /// <summary>
    /// 画面上でユーザーを扱うために必要な情報
    /// </summary>
    public class User
    {
        public string Id { get; set; }

        public string HandleName { get; set; }

        public DateTime LastAccess { get; set; }
    }
}
