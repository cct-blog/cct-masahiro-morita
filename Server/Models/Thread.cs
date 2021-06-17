using blazorTest.Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace blazorTest.Server.Models
{
    public class Thread : ICreateAndUpdateDate
    {
        public Guid Id { get; set; }
        public string ApplicationUserId { get; set; }
        public Guid PostId { get; set; }

        [MaxLength(200)]
        public string Text { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public Post Post { get; set; }
    }
}