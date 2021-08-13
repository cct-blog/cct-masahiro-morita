using ChatApp.Shared.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ChatApp.Client.Models
{
    public class ThreadModel : ICreateAndUpdateDate
    {
        public string UserEmail { get; set; }

        public string HandleName { get; set; }

        public Guid ThreadId { get; set; }

        public string MessageContext { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}