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
        private readonly HttpClient _httpClient;

        public List<RoomModel> RoomModels { get; set; } = new List<RoomModel>();

        public event EventHandler<List<RoomModel>> RoomListChanged;

        public IndexModel(IHttpClientFactory _httpClientFactory)
        {
            _httpClient = _httpClientFactory.CreateClient("ChatApp.ServerAPI");
        }

        public async Task GetUserBelongedRoomsAsync()
        {
            var rooms = await _httpClient.GetFromJsonAsync<List<UserRoom>>("Room");

            RoomModels = rooms
                .Select((room) =>
                    new RoomModel()
                    {
                        RoomId = room.Id,
                        RoomName = room.Name,
                        LastAccessDate = room.LastAccessDate
                    })
                .ToList();

            RoomListChanged?.Invoke(this, RoomModels);
        }

        public async Task CreateRoomAsync(string roomName, List<string> userEmails)
        {
            await _httpClient.PostAsJsonAsync(
                "Room",
                new CreateRoom() { RoomName = roomName, UserIds = userEmails });

            await GetUserBelongedRoomsAsync();
        }
    }
}