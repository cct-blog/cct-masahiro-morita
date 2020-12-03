using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorTest.Shared.Models
{
    public class ChatPostPostRequest
    {
        public Guid roomId { get; set; }

        public DateTime needMessageTailDate { get; set; }

        public int MessageCount { get; set; }
    }
}