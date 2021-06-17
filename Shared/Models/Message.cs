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
}