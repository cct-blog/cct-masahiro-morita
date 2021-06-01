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
    /// <summary>
    /// Call InitializeHubConnection method after instantiate
    /// </summary>
    public class ChatModels
    {
        private readonly HubUtility _hubUtility;

        private readonly HttpClient _httpClient;

        private HubConnection _hubConnection { get; set; }

        public ChatModels(HttpClient httpClient, HubUtility hubUtility)
        {
            _httpClient = httpClient;
            _hubUtility = hubUtility;
        }
        /// <summary>
        /// 現在のルールのID
        /// </summary>
        public Guid RoomId { get; set; }

        /// <summary>
        /// ルームに投稿されているメッセージのリスト
        /// </summary>
        public List<Message> Messages { get; set; } = new();

        /// <summary>
        /// ルームに参加しているユーザーのリスト
        /// </summary>
        public List<UserInformation> UserInformations { get; set; } = new();

        public async Task InitializeHubConnection()
        {
            _hubConnection = _hubUtility.CreateHubConnection();

            _hubConnection.On<Message>(SignalRMehod.ReceiveMessage, (message) =>
            {
                if (message.RoomId != RoomId)
                    return;

                Messages.Add(message);
            });

            await _hubConnection.StartAsync();
        }

        public async Task GetAllParticipatedUsersAsync()
        {
            var roomDetail = await _httpClient.GetFromJsonAsync<RoomDetail>($"room/{RoomId}");

            UserInformations = roomDetail.Users;
        }

        public async Task AddUsersAsync(List<string> userEmails)
        {
            var response = await RoomUtility.AddUsersToRoom(RoomId, userEmails, _httpClient);
            UserInformations = response.Users;
        }

        public async Task DeleteUsersAsync(List<string> userEmails)
        {
            var response = await RoomUtility.DeleteUserFromRoom(RoomId, userEmails, _httpClient);
            UserInformations = response.Users;
        }

        public async Task GetMessages(DateTime needMessageTailDate)
        {
            var request = new ChatPostPostRequest()
            {
                RoomId = RoomId,
                NeedMessageTailDate = needMessageTailDate
            };

            var response = await _httpClient.PostAsJsonAsync("Post", request);

            Messages = await response.Content.ReadFromJsonAsync<List<Message>>();
        }

        public async Task SendMessage(Message message)
            => await _hubConnection.SendAsync(SignalRMehod.SendMessage, message);
    }

    /// <summary>
    /// Call InitializeHubConnection method after instantiate
    /// </summary>
    public class ThreadModels
    {
        private readonly HubUtility _hubUtility;

        private readonly HttpClient _httpClient;

        private HubConnection _hubConnection { get; set; }

        public ThreadModels(HttpClient httpClient, HubUtility hubUtility)
        {
            _httpClient = httpClient;
            _hubUtility = hubUtility;
        }

        /// <summary>
        /// スレッドの開始元の投稿のID
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// スレッドに投稿されたメッセージのリスト
        /// </summary>
        public List<ThreadMessage> ThreadMessages { get; set; } = new();

        public async Task InitializeHubConnection()
        {
            _hubConnection = _hubUtility.CreateHubConnection();

            _hubConnection.On<ThreadMessage>(SignalRMehod.SendThreadMessage, (message) =>
            {
                if (PostId != message.PostId) return;

                ThreadMessages.Add(message);
            });

            await _hubConnection.StartAsync();
        }

        public async Task GetThreads()
            => ThreadMessages = await _httpClient.GetFromJsonAsync<List<ThreadMessage>>("Thread/Post/" + PostId.ToString());

        public async Task SendThread(ThreadMessage threadMessage)
            => await _httpClient.PostAsJsonAsync("Thread", threadMessage);
    }
}
