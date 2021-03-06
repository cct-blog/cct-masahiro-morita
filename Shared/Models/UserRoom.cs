using System;

namespace ChatApp.Shared.Models
{
    public record UserRoom
    {
        public Guid Id { get; init; }

        public string Name { get; init; }

        public DateTime LastAccessDate { get; init; }
    }
}