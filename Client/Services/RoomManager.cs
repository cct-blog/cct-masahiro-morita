using blazorTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace blazorTest.Client.Services
{
    public interface IRoomManager
    {
        event EventHandler<List<UserRoom>> RoomChanged;

        void RaiseRoomChanged(object sender, List<UserRoom> rooms);

    }

    public class RoomManager : IRoomManager
    {
        public event EventHandler<List<UserRoom>> RoomChanged;

        public HttpClient _httpClient;

        public void RaiseRoomChanged(object sender, List<UserRoom> rooms)
        {
            RoomChanged?.Invoke(sender, rooms);
        }
    }
}
