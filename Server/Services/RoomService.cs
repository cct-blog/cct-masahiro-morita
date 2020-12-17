using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .ToArray();
        }
    }
}