using blazorTest.Client.Services;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace blazorTest.Client.Models
{
    public class ChatModel
    {
        public List<PostModel> PostModels { get; set; }

        // 入室中のユーザー情報
        public List<UserInformation> UserInformations { get; set; }

        // すべてのユーザー
        public List<UserInformation> AllUser { get; set; }

        public Guid RoomId { get; private set; }

        public async Task InitializeChatRoom(Guid roomId, HttpClient httpClient)
        {
            RoomId = roomId;
            await InitializeChatRoom(httpClient);
        }

        private async Task InitializeChatRoom(HttpClient httpClient)
        {
            AllUser = await httpClient.GetFromJsonAsync<List<UserInformation>>("User");

            var roomDetail = await httpClient.GetFromJsonAsync<RoomDetail>($"Room/{RoomId}");

            UserInformations = roomDetail.Users;

            var request = new ChatPostPostRequest()
            {
                RoomId = RoomId,
                NeedMessageTailDate = DateTime.Now
            };

            var response = await httpClient.PostAsJsonAsync("Post", request);
            var messages = await response.Content.ReadFromJsonAsync<List<Message>>();

            PostModels = messages.Select(message => new PostModel()
            {
                UserEmail = message.UserEmail,
                HandleName = message.HandleName,
                PostId = message.Id,
                MessageContext = message.MessageContext,
                CreateDate = message.CreateDate
            }).ToList();
        }

        public async Task AddUsersAsync(List<string> userEmails, HttpClient httpClient)
        {
            var response = await RoomUtility.AddUsersToRoom(RoomId, userEmails, httpClient);
            UserInformations = response.Users;
        }

        public async Task DeleteUsersAsync(List<string> userEmails, HttpClient httpClient)
        {
            var response = await RoomUtility.DeleteUserFromRoom(RoomId, userEmails, httpClient);
            UserInformations = response.Users;
        }
    }
}
