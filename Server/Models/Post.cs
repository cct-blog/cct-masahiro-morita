using ChatApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ChatApp.Server.Models
{
    public class Post : ICreateAndUpdateDate
    {
        public Guid Id { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public Guid RoomId { get; set; }

        [MaxLength(200)]
        public string Text { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public List<Thread> Threads { get; set; }
    }
}