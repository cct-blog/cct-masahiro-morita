using System;

namespace blazorTest.Shared.Models
{
    public class UserRoom
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}