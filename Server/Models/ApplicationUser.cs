using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int CreatedRoomCount { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateDate { get; set; }
        public List<Post> Posts { get; set; }
        public List<Thread> Threads { get; set; }
        public List<UserInfoInRoom> UserInfoInRooms { get; set; }
        
    }

}
