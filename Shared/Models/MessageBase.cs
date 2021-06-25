using System;

namespace ChatApp.Shared.Models
{
    public record MessageBase : IReadonlyCreateAndUpdateDate

    {
        public string UserEmail { get; init; }

        public string HandleName { get; init; }

        public string MessageContext { get; init; }

        public DateTime CreateDate { get; init; }

        public DateTime UpdateDate { get; init; }

        public Guid RoomId { get; init; }
    }
}
