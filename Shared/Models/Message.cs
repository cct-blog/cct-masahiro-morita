using System;

namespace blazorTest.Shared.Models
{
    public record Message
    {
        public string UserEmail { get; init; }

        public string HandleName { get; init; }

        public string MessageContext { get; init; }

        public Guid RoomId { get; init; }
    }
}