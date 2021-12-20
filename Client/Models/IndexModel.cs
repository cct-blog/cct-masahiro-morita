using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Client.Services;
using ChatApp.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using static ChatApp.Client.Pages.Chat;

namespace ChatApp.Client.Models
{
    public class IndexModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private UserInformation[] _allUsers = Array.Empty<UserInformation>();
        // すべてのユーザー
        public UserInformation[] AllUsers
        { 
            get => _allUsers;
            private set 
            {
                _allUsers = value;
                AllUserChanged?.Invoke(this, value);
            }
        }

        private ChatModel _chatModel;

        public List<RoomModel> RoomModels { get; set; } = new List<RoomModel>();

        public event EventHandler<RoomModel[]> RoomListChanged;

        public event EventHandler<UserInformation[]> AllUserChanged;


        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task Initialize(AuthenticationStateProvider authenticationStateProvider)
        {
            return GetUserBelongedRoomsAsync(authenticationStateProvider);
        }

        public async Task<UserInfo> GetUserAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            Console.WriteLine("***GetUserAsync");
            var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            return new() { Id = user.Identity.Name, Name = user.Claims.FirstOrDefault(each => each.Type == "HandleName")?.Value };
        }

        /// <summary>
        /// サービスに登録しているすべてのユーザーを取得する
        /// </summary>
        /// <returns></returns>
        public async Task GetAllUserAsync()
        {
            var client = _httpClientFactory.CreateClient("ChatApp.ServerAPI");
            AllUsers = (await client.GetFromJsonAsync<List<UserInformation>>("User")).ToArray();
        }


        public async Task GetUserBelongedRoomsAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            var httpClient = _httpClientFactory.CreateClient("ChatApp.ServerAPI");

            var rooms = await httpClient.GetFromJsonAsync<List<UserRoom>>("Room");

            foreach (var room in rooms)
            {
                var matchedRoom = RoomModels.FirstOrDefault(each => each.RoomId == room.Id);

                if (matchedRoom is null)
                {
                    RoomModels.Add(new RoomModel()
                    {
                        RoomId = room.Id,
                        RoomName = room.Name,
                        LastAccessDate = room.LastAccessDate
                    });
                }
                else
                {
                    matchedRoom = new RoomModel()
                    {
                        RoomId = room.Id,
                        RoomName = room.Name,
                        LastAccessDate = room.LastAccessDate
                    };
                }
            }

            List<RoomModel> toBeDeletedRoomModels = new();
            foreach (var roomModel in RoomModels)
            {
                var matchedRoom = rooms.FirstOrDefault(each => each.Id == roomModel.RoomId);

                if (matchedRoom is null) toBeDeletedRoomModels.Add(roomModel);
            }

            if (toBeDeletedRoomModels.Any())
            {
                foreach (var toBeDeletedRoomModel in toBeDeletedRoomModels)
                {
                    RoomModels.Remove(toBeDeletedRoomModel);
                }
            }

            RoomListChanged?.Invoke(this, RoomModels.ToArray());
        }

        public async Task CreateRoomAsync(string roomName, List<string> userEmails, AuthenticationStateProvider authenticationStateProvider)
        {
            var httpClient = _httpClientFactory.CreateClient("ChatApp.ServerAPI");

            await httpClient.PostAsJsonAsync(
                "Room",
                new CreateRoom() { RoomName = roomName, UserIds = userEmails });

            await GetUserBelongedRoomsAsync(authenticationStateProvider);
        }

        public ChatModel ChatModelFactory(HubUtility hubUtility)
        {
            _chatModel ??= new ChatModel(_httpClientFactory, hubUtility);
            return _chatModel;
        }
    }
}