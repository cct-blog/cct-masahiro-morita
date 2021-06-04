using blazorTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace blazorTest.Client.Models
{

    public delegate void ChangeThreadMessageEventHandler();
    
    public class ThreadModel
    {

        public string UserEmail { get; set; }

        public string HandleName { get; set; }
        
        public Guid ThreadId { get; set; }

        public string MessageContext { get; set; }

        public DateTime CreateDate { get; set; }

        public event ChangeThreadMessageEventHandler ChangeMessage;

        public async Task ChangeThreadMessage(ThreadMessage threadMessage, HttpClient httpClient)
        {
            await httpClient.PutAsJsonAsync("Thread", threadMessage);

            ChangeMessage();
        }
    }
}
