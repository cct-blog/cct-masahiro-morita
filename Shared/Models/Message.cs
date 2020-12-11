using System;

namespace blazorTest.Shared.Models
{
    public record Message
    {
        public string userEmail { get; init; }

        public string handleName { get; init; }

        public string messageContext { get; init; }

        public Guid roomId { get; init; }
    }
}