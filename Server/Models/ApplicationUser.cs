using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Models
{
    public class ApplicationUser : IdentityUser, ICreateAndUpdateDate
    {
        public int CreatedRoomCount { get; set; }

        [MaxLength(20)]
        public string HandleName { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<Post> Posts { get; set; }
        public List<Thread> Threads { get; set; }
        public List<UserInfoInRoom> UserInfoInRooms { get; set; }
    }
}