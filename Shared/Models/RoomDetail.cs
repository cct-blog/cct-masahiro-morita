using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Shared.Models
{
    public record RoomDetail
    {
        public Guid RoomId { get; init; }

        public string RoomName { get; init; }

        public DateTime CreateDate { get; init; }

        public DateTime UpdateDate { get; init; }

        public DateTime LastAccessedDate { get; init; }

        public List<UserInformation> Users { get; init; }
    }
}