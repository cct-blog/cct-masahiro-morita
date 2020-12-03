using System;

namespace blazorTest.Shared.Models
{
    public class Message
    {
        public string userEmail { get; set; }

        public string handleName { get; set; }

        public string messageContext { get; set; }

        public Guid roomId { get; set; }
    }
}