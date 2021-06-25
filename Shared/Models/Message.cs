using System;

namespace ChatApp.Shared.Models
{
    public record Message : MessageBase
    {
        /// <summary>
        /// PostId
        /// </summary>
        public Guid Id { get; init; }
    }
}