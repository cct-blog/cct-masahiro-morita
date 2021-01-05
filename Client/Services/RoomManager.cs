using blazorTest.Shared.Models;
using System;
using System.Collections.Generic;

namespace blazorTest.Server
{
    public interface IRoomManager
    {
        event EventHandler<List<RoomDetail>> RoomChanged;

        void RaiseRoomChanged(List<RoomDetail> rooms);

    }

    public class RoomManager : IRoomManager
    {
        public event EventHandler<List<RoomDetail>> RoomChanged;

        public void RaiseRoomChanged(List<RoomDetail> rooms)
        {
            RoomChanged?.Invoke(this, rooms);
        }
    }
}
