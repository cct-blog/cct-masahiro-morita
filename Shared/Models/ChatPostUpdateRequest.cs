using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Shared.Models
{
    public record ChatPostUpdateRequest
    {
        /// <summary>
        /// 更新後のテキスト
        /// </summary>
        public string Text { get; set; }
    }
}
