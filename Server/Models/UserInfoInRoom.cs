using System;
using blazorTest.Shared.Models;

namespace blazorTest.Server.Models
{
    public class UserInfoInRoom : ICreateAndUpdateDate
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }

        public Room Room { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime LatestAccessDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}