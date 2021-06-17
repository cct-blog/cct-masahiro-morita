using ChatApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Server.Models
{
    public class Room : ICreateAndUpdateDate
    {
        public Guid Id { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<Post> Posts { get; set; }
        public List<UserInfoInRoom> UserInfoInRooms { get; set; }
    }
}