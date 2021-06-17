using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Client.Models
{
    public class RoomModel
    {
        public Guid RoomId { get; set; }

        public string RoomName { get; set; }

        public DateTime LastAccessDate { get; set; }
    }
}
