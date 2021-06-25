using System;

namespace ChatApp.Shared.Models
{
    public record ThreadMessage : MessageBase
    {
        public Guid ThreadId { get; init; }

        public Guid PostId { get; init; }
    }
}
