using System;

namespace blazorTest.Shared.Models
{
    public record UserRoom
    {
        public Guid Id { get; init; }

        public string Name { get; init; }
    }
}