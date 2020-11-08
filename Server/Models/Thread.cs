using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Models
{
    public class Thread
    {
        public Guid Id { get; set; }
        public string ApplicationUserId { get; set; }
        public Guid PostId { get; set; }

        [MaxLength(200)]
        public string Text { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateDate { get; set; }
    }
}