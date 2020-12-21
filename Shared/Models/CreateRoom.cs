using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorTest.Shared.Models
{
    public record CreateRoom
    {
        public string roomName { get; init; }

        public List<string> UserIds { get; init; }
    }
}