using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Models
{
    public class UserInfoInRoom : ICreateAndUpdateDate
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string ApplicationUserId { get; set; }
        public DateTime LatestAccessDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}