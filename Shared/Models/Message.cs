using System;

namespace blazorTest.Shared.Models
{
    public record Message
    {
        /// <summary>
        /// PostId
        /// </summary>
        public Guid Id { get; set; }

        public string UserEmail { get; init; }

        public string HandleName { get; init; }

        public string MessageContext { get; init; }

        public Guid RoomId { get; init; }
    }
}