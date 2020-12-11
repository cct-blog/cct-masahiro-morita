using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorTest.Shared.Models
{
    public record ChatPostPostRequest
    {
        public Guid roomId { get; init; }

        public DateTime needMessageTailDate { get; init; }

        public int MessageCount { get; init; }
    }
}