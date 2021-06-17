using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Shared.Models
{
    public record ChatPostPostRequest
    {
        public Guid RoomId { get; init; }

        public DateTime NeedMessageTailDate { get; init; }

        public int MessageCount { get; init; }
    }
}