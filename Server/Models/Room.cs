using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Models
{
    public class Room
    {
        public Guid Id { get; set; }
        [MaxLength(20)]
        public string Name { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateDate { get; set; }
        public List<Post> Posts { get; set; }
        public List<UserInfoInRoom> UserInfoInRooms { get; set; }
    }
}
