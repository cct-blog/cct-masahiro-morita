using ChatApp.Shared.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ChatApp.Client.Models
{
    public class ThreadModel : ICreateAndUpdateDate
    {
        public ThreadModel() {}
        public ThreadModel(ThreadMessage thread)
        {
            UserEmail = thread.UserEmail;
            HandleName = thread.HandleName;
            ThreadId = thread.ThreadId;
            PostId = thread.PostId;
            MessageContext = thread.MessageContext;
            CreateDate = thread.CreateDate;
            UpdateDate = thread.UpdateDate;
        }
        public string UserEmail { get; set; }

        public string HandleName { get; set; }

        public Guid ThreadId { get; set; }

        public Guid PostId { get; set; }

        public string MessageContext { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}