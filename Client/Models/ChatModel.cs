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
    public delegate void InOutUserEventHandler();

    public delegate void ChangePostCountEventHandler();

    public class ChatModel
    {
        public List<PostModel> PostModels { get; set; }

        // 入室中のユーザー情報
        public List<UserInformation> RoomParticipants { get; set; }

        // すべてのユーザー
        public List<UserInformation> AllUser { get; set; }

        public Guid RoomId { get; private set; }

        public static event InOutUserEventHandler InOutUser;

        public static event ChangePostCountEventHandler ChangePost;

        public async Task InitializeChatRoom(Guid roomId, HttpClient httpClient)
        {
            RoomId = roomId;
            await InitializeChatRoom(httpClient);
        }

        private async Task InitializeChatRoom(HttpClient httpClient)
        {
            AllUser = await httpClient.GetFromJsonAsync<List<UserInformation>>("User");

            var roomDetail = await httpClient.GetFromJsonAsync<RoomDetail>($"Room/{RoomId}");

            RoomParticipants = roomDetail.Users;

            var request = new ChatPostPostRequest()
            {
                RoomId = RoomId,
                NeedMessageTailDate = DateTime.Now
            };

            var response = await httpClient.PostAsJsonAsync("Post", request);
            var messages = await response.Content.ReadFromJsonAsync<List<Message>>();

            var postModelTask = await Task.WhenAll(messages.Select(async message =>
            {
                var threadMessages = await httpClient.GetFromJsonAsync<List<ThreadMessage>>($"Thread/Post/{message.Id}");

                return new PostModel()
                {
                    UserEmail = message.UserEmail,
                    HandleName = message.HandleName,
                    PostId = message.Id,
                    MessageContext = message.MessageContext,
                    CreateDate = message.CreateDate,
                    UpdateDate = message.UpdateDate,
                    ThreadModels = threadMessages
                        .Select(threadMessage => new ThreadModel()
                        {
                            UserEmail = threadMessage.UserEmail,
                            HandleName = threadMessage.HandleName,
                            ThreadId = threadMessage.ThreadId,
                            MessageContext = threadMessage.MessageContext,
                            CreateDate = threadMessage.CreateDate,
                            UpdateDate = threadMessage.UpdateDate
                        }).ToList()
                };
            }));

            PostModels = postModelTask.ToList();
        }

        public async Task AddUsersAsync(List<string> userEmails, HttpClient httpClient)
        {
            var response = await RoomUtility.AddUsersToRoom(RoomId, userEmails, httpClient);
            RoomParticipants = response.Users;

            InOutUser();
        }

        public async Task DeleteUsersAsync(List<string> userEmails, HttpClient httpClient)
        {
            var response = await RoomUtility.DeleteUserFromRoom(RoomId, userEmails, httpClient);
            RoomParticipants = response.Users;

            InOutUser();
        }

        public async Task SendMessage(Message message, HubConnection hubConnection)
        {
            await hubConnection.SendAsync(SignalRMehod.SendMessage, message);

            ChangePost();
        }

        public async Task DeleteMessage(Guid postId, HttpClient httpClient)
        {
            await httpClient.DeleteAsync($"Post/{postId}");

            var deletedMessage = PostModels.FirstOrDefault(postModel => postModel.PostId == postId);

            PostModels.Remove(deletedMessage);

            ChangePost();
        }
    }
}
