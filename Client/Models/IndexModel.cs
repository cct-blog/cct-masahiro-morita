using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Shared.Models;

namespace ChatApp.Client.Models
{
    public class IndexModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<RoomModel> RoomModels { get; set; } = new List<RoomModel>();

        public event EventHandler<RoomModel[]> RoomListChanged;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task Initialize()
        {
            return GetUserBelongedRoomsAsync();
        }

        public async Task GetUserBelongedRoomsAsync()
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

        public async Task CreateRoomAsync(string roomName, List<string> userEmails)
        {
            var httpClient = _httpClientFactory.CreateClient("ChatApp.ServerAPI");

            await httpClient.PostAsJsonAsync(
                "Room",
                new CreateRoom() { RoomName = roomName, UserIds = userEmails });

            await GetUserBelongedRoomsAsync();
        }
    }
}