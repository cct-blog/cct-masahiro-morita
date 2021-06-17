using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Shared.Models
{
    public record CreateRoom
    {
        public string RoomName { get; init; }

        public List<string> UserIds { get; init; }
    }
}