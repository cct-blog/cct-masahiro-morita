using System;

namespace blazorTest.Shared.Models
{
    public record Message : MessageBase
    {
        /// <summary>
        /// PostId
        /// </summary>
        public Guid Id { get; init; }
    }

    public record ThreadMessage : MessageBase
    {
        public Guid ThreadId { get; init; }

        public Guid PostId { get; init; }
    }

    public record MessageBase
    {
        public string UserEmail { get; init; }

        public string HandleName { get; init; }

        public string MessageContext { get; init; }

        public DateTime CreateDate { get; init; }

        public Guid RoomId { get; init; }
    }
}