using blazorTest.Shared;
using blazorTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Client.Services
{
    public class PostUtility
    {
        //TODO WIP
        public static async Task PushPost(Guid roomId, string messageContent)
        {
            if (messageContent.Length > 240) throw new Exception("Message Over, under 240 chracters can be posted");

            //TODO ユーザーがルームに所属しているかをチェック

            var message = new Message()
            {
                Id = Guid.Empty,
                UserEmail = "",
                HandleName = "",
                MessageContext = messageContent,
                RoomId = roomId
            };

            //await _hubConnection.SendAsync(SignalRMehod.SendMessage, message);
        }
    }
}