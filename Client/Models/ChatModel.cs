using ChatApp.Client.Services;
using ChatApp.Shared;
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
    public class ChatModel
    {
        public List<PostModel> PostModels { get; set; }

        // 入室中のユーザー情報
        public List<UserInformation> RoomParticipants { get; set; }

        // すべてのユーザー
        public List<UserInformation> AllUser { get; set; }

        public Guid RoomId { get; private set; }

        public event EventHandler<UserInformation[]> RoomParticipantsChanged;

        public event EventHandler<PostModel[]> PostsChanged;

        public event EventHandler<ThreadModel[]> ThreadsChanged;

        private readonly HttpClient _httpClient;

        private readonly HubConnection _hubConnection;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="hubConnection"></param>
        /// <param name="roomId"></param>
        public ChatModel(HttpClient httpClient, HubConnection hubConnection, Guid roomId)
        {
            _httpClient = httpClient;
            _hubConnection = hubConnection;
            RoomId = roomId;

            Task.Run(async () =>
            {
                await GetAllUserAsync();
                await GetAllRoomParticipantsAsync();
                await GetRoomPostsAsync();
            });

            _hubConnection.On<Message>(SignalRMehod.ReceiveMessage, async (message) =>
            {
                if (message.RoomId != RoomId)
                    return;

                var threadMessages = await _httpClient.GetFromJsonAsync<List<ThreadMessage>>($"Thread/Post/{message.Id}");

                var postModel = new PostModel()
                {
                    PostId = message.Id,
                    UserEmail = message.UserEmail,
                    HandleName = message.HandleName,
                    MessageContext = message.MessageContext,
                    CreateDate = message.CreateDate,
                    UpdateDate = message.UpdateDate,
                    ThreadModels = threadMessages
                        .Select(threadMessage => new ThreadModel()
                        {
                            UserEmail = threadMessage.UserEmail,
                            HandleName = threadMessage.HandleName,
                            ThreadId = threadMessage.ThreadId,
                            PostId = threadMessage.PostId,
                            MessageContext = threadMessage.MessageContext,
                            CreateDate = threadMessage.CreateDate,
                            UpdateDate = threadMessage.UpdateDate
                        }).ToList()
                };
                await hubConnection.StartAsync();

                PostModels.Add(postModel);

                PostsChanged(this, PostModels.ToArray());
            });

            _hubConnection.On<ThreadMessage>(SignalRMehod.SendThreadMessage, (message) =>
            {
                var threads = PostModels.FirstOrDefault(post => post.PostId == message.PostId).ThreadModels;
                if (threads == null)
                    return;

                var threadMessage = threads.FirstOrDefault(each => each.ThreadId == message.ThreadId);

                var threadModel = new ThreadModel()
                {
                    ThreadId = message.ThreadId,
                    PostId = message.PostId,
                    UserEmail = message.UserEmail,
                    HandleName = message.HandleName,
                    MessageContext = message.MessageContext,
                    CreateDate = message.CreateDate,
                    UpdateDate = message.UpdateDate,
                };

                if (threadMessage == null)
                {
                    threads.Add(threadModel);
                }
                else
                {
                    var index = threads.IndexOf(threadMessage);
                    if (index >= 0)
                    {
                        threads.RemoveAt(index);
                        threads.Insert(index, threadModel);
                    }
                }

                ThreadsChanged(this, threads.ToArray());
            });
        }

        /// <summary>
        /// サービスに登録しているすべてのユーザーを取得する
        /// </summary>
        /// <returns></returns>
        public async Task GetAllUserAsync()
            => AllUser = await _httpClient.GetFromJsonAsync<List<UserInformation>>("User");

        /// <summary>
        /// ルームに参加しているユーザーを取得する
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task GetAllRoomParticipantsAsync()
        {
            var roomDetail = await _httpClient.GetFromJsonAsync<RoomDetail>($"Room/{RoomId}");

            RoomParticipants = roomDetail.Users;
        }

        /// <summary>
        /// ルーム内のスレッド含めたすべての投稿を取得する
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task GetRoomPostsAsync()
        {
            var request = new ChatPostPostRequest()
            {
                RoomId = RoomId,
                NeedMessageTailDate = DateTime.Now
            };

            var response = await _httpClient.PostAsJsonAsync("Post", request);
            var messages = await response.Content.ReadFromJsonAsync<List<Message>>();

            var postModelTask = await Task.WhenAll(messages.Select(async message =>
            {
                var threadMessages = await _httpClient.GetFromJsonAsync<List<ThreadMessage>>($"Thread/Post/{message.Id}");

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
                            PostId = threadMessage.PostId,
                            MessageContext = threadMessage.MessageContext,
                            CreateDate = threadMessage.CreateDate,
                            UpdateDate = threadMessage.UpdateDate
                        }).ToList()
                };
            }));

            PostModels = postModelTask.ToList();
        }

        /// <summary>
        /// ルームにユーザーを追加する
        /// </summary>
        /// <param name="userEmails"></param>
        /// <returns></returns>
        public async Task AddUsersAsync(List<string> userEmails)
        {
            var response = await RoomUtility.AddUsersToRoom(RoomId, userEmails, _httpClient);
            RoomParticipants = response.Users;

            RoomParticipantsChanged?.Invoke(this, RoomParticipants.ToArray());
        }

        /// <summary>
        /// ルームからユーザーを削除する
        /// </summary>
        /// <param name="userEmails"></param>
        /// <returns></returns>
        public async Task DeleteUsersAsync(List<string> userEmails)
        {
            var response = await RoomUtility.DeleteUserFromRoom(RoomId, userEmails, _httpClient);
            RoomParticipants = response.Users;

            RoomParticipantsChanged?.Invoke(this, RoomParticipants.ToArray());
        }

        /// <summary>
        /// メッセージを送信する
        /// 送信後のメッセージの読み込みは双方向通信でメッセージの更新が来た時に行う
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(Message message)
            => await _hubConnection.SendAsync(SignalRMehod.SendMessage, message);

        /// <summary>
        /// メッセージを削除する
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public async Task DeleteMessageAsync(Guid postId)
        {
            await _httpClient.DeleteAsync($"Post/{postId}");

            var deletedMessage = PostModels.FirstOrDefault(postModel => postModel.PostId == postId);

            PostModels.Remove(deletedMessage);

            PostsChanged?.Invoke(this, PostModels.ToArray());
        }

        /// <summary>
        /// 指定したメッサーシの内容を修正します
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="newMessage"></param>
        /// <returns></returns>
        public async Task ChangeMessageAsync(Guid postId, string newMessage)
        {
            var post = PostModels.FirstOrDefault(post => post.PostId == postId);

            var result = await _httpClient.PutAsJsonAsync($"Post/{postId}", new ChatPostUpdateRequest() { Text = newMessage });

            if (result.IsSuccessStatusCode)
            {
                post.MessageContext = newMessage;

                PostsChanged?.Invoke(this, new PostModel[] { post });
            }
        }

        /// <summary>
        /// スレッドにメッセージを送信します
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendThreadMessageAsync(ThreadMessage message)
        {
            var threads = PostModels.FirstOrDefault(post => post.PostId == message.PostId).ThreadModels;

            var response = await _httpClient.PostAsJsonAsync("Post", message);

            var threadMessage = await response.Content.ReadFromJsonAsync<ThreadMessage>();

            threads.Add(new ThreadModel()
            {
                UserEmail = threadMessage.UserEmail,
                HandleName = threadMessage.HandleName,
                ThreadId = threadMessage.ThreadId,
                PostId = threadMessage.PostId,
                MessageContext = threadMessage.MessageContext,
                CreateDate = threadMessage.CreateDate,
                UpdateDate = threadMessage.UpdateDate,
            });

            ThreadsChanged?.Invoke(this, threads.ToArray());
        }

        /// <summary>
        /// スレッドからメッセージを削除します
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public async Task DeleteThreadMessageAsync(Guid postId, Guid threadId)
        {
            var threads = PostModels.FirstOrDefault(post => post.PostId == postId).ThreadModels;

            await _httpClient.DeleteAsync($"Thread/{threadId}");

            var deletedThread = threads.FirstOrDefault(thread => thread.ThreadId == threadId);
            threads.Remove(deletedThread);

            ThreadsChanged?.Invoke(this, threads.ToArray());
        }

        public async Task ChangeThreadMessageAsync(ThreadMessage message)
        {
            var thread = PostModels.FirstOrDefault(post => post.PostId == message.PostId).ThreadModels
                .FirstOrDefault(thread => thread.ThreadId == message.ThreadId);

            await _httpClient.PutAsJsonAsync("Thread", message);

            thread.MessageContext = message.MessageContext;

            ThreadsChanged(this, new ThreadModel[] { thread });
        }
    }
}