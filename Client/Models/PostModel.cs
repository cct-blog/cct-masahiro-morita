using ChatApp.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ChatApp.Client.Models
{
    public class PostModel : ICreateAndUpdateDate
    {
        public List<ThreadModel> ThreadModels { get; set; }

        public string UserEmail { get; set; }

        public string HandleName { get; set; }

        public Guid PostId { get; init; }

        public string MessageContext { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}