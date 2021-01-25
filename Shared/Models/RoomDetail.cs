using System;
using System.Collections.Generic;

namespace blazorTest.Shared.Models
{
    public record RoomDetail
    {
        public Guid RoomId { get; init; }

        public string RoomName { get; init; }

        public DateTime CreateDate { get; init; }

        public DateTime UpdateDate { get; init; }

        public DateTime LastAccessedDate { get; init; }

        public List<User> Users { get; init; }
    }
}