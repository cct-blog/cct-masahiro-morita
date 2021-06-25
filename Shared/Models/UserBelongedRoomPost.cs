using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Shared.Models
{
    public record UserBelongedRoomPost
    {
        public Guid RoomId { get; init; }

        public IEnumerable<string> Texts { get; init; }
    }
}