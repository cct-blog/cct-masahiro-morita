using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace blazorTest.Server.Services
{
    public class RoomService : IService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<UserRoom> ReadRoomListOfUser(string userId)
        {
            return _context.Rooms
                .Include(room => room.UserInfoInRooms
                    .Where(userInfoInRoom => userInfoInRoom.ApplicationUserId == userId))
                .Select(_room => new UserRoom() { Id = _room.Id, Name = _room.Name })
                .AsEnumerable();
        }

        private RoomDetail ReadRoomDetail(IQueryable<Room> rooms) =>
                rooms
                .Include(_room => _room.UserInfoInRooms)
                    .ThenInclude(_userInfoInRooms => _userInfoInRooms.ApplicationUser)
                .Select(_room => new RoomDetail()
                {
                    RoomId = _room.Id,
                    RoomName = _room.Name,
                    CreateDate = _room.CreateDate,
                    UpdateDate = _room.UpdateDate,
                    UserName = _room.UserInfoInRooms.Select(_m => _m.ApplicationUser.HandleName).ToList()
                })
                .AsEnumerable()
                .First();

        internal RoomDetail ReadRoomDetailFromId(Guid id)
        {
            var roomQuery = _context.Rooms.Where(_room => _room.Id == id);
            return ReadRoomDetail(roomQuery);
        }

        internal RoomDetail ReadRoomDetailFromName(string name)
        {
            var roomQuery = _context.Rooms.Where(_room => _room.Name == name);
            return ReadRoomDetail(roomQuery);
        }

        internal RoomDetail CreateRoom(CreateRoom createRoom)
        {
            var userInfoInRooms = createRoom.UserIds
                .Select(_m => new UserInfoInRoom()
                {
                    ApplicationUserId = _m,
                    LatestAccessDate = DateTime.Now
                })
                .ToList();

            _context.Rooms
                .Add(new Room()
                {
                    Name = createRoom.RoomName,
                    UserInfoInRooms = userInfoInRooms
                });
            _context.SaveChanges();

            return ReadRoomDetailFromName(createRoom.RoomName);
        }
    }
}