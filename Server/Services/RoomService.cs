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

        public async Task<IEnumerable<UserRoom>> ReadRoomListOfUser(string userEmail)
        {
            var user = await _context.Users
                .Where(user => user.Email == userEmail)
                .FirstOrDefaultAsync();

            return await _context.UserInfoInRooms
                .Include(userInfoInRooms => userInfoInRooms.Room)
                .Where(userInfoInRoom => userInfoInRoom.ApplicationUserId == user.Id)
                .Select(userInfoInRoom => new UserRoom()
                {
                    Id = userInfoInRoom.Room.Id,
                    Name = userInfoInRoom.Room.Name,
                    LastAccessDate = userInfoInRoom.LatestAccessDate
                })
                .ToArrayAsync();
        }

        private async Task<RoomDetail> ReadRoomDetail(IQueryable<Room> rooms)
        {
            var roomDetail = await rooms
            .Include(room => room.UserInfoInRooms)
                .ThenInclude(userInfoInRooms => userInfoInRooms.ApplicationUser)
            .Select(room => new RoomDetail()
            {
                RoomId = room.Id,
                RoomName = room.Name,
                CreateDate = room.CreateDate,
                UpdateDate = room.UpdateDate,
                Users = room.UserInfoInRooms
                    .Select(userInfoInRoom => new UserInformation()
                    {
                        HandleName = userInfoInRoom.ApplicationUser.HandleName,
                        Email = userInfoInRoom.ApplicationUser.Email,
                        LastAccessDate = userInfoInRoom.LatestAccessDate
                    })
                .ToList()
            })
            .ToArrayAsync();

            return roomDetail.FirstOrDefault();
        }

        internal async Task<RoomDetail> ReadRoomDetailFromId(Guid id)
        {
            var roomQuery = _context.Rooms.Where(room => room.Id == id);
            return await ReadRoomDetail(roomQuery);
        }

        internal async Task<RoomDetail> ReadRoomDetailFromName(string name)
        {
            var roomQuery = _context.Rooms.Where(room => room.Name == name);
            return await ReadRoomDetail(roomQuery);
        }

        internal async Task<RoomDetail> AddUserToRoom(List<string> userEmails, Guid roomId)
        {
            var users = await _context.Users
                .Where(user => userEmails.Contains(user.Email))
                .ToListAsync();

            users.ForEach(user =>
            {
                var userInfoInRoom = new UserInfoInRoom() { ApplicationUserId = user.Id, RoomId = roomId };
                _context.Add(userInfoInRoom);
            });

            await _context.SaveChangesAsync();

            return await ReadRoomDetailFromId(roomId);
        }

        internal async Task<RoomDetail> CreateRoom(CreateRoom createRoom)
        {
            var userData = createRoom.UserIds
                .Select(userEmail => _context.Users
                    .Where(user => user.Email == userEmail)
                    .AsEnumerable()
                    .FirstOrDefault())
                .ToList();

            var userInfoInRooms = userData
                .Select(userInfoInRoom => new UserInfoInRoom()
                {
                    ApplicationUserId = userInfoInRoom.Id,
                    LatestAccessDate = DateTime.Now
                })
                .ToList();

            _context.Rooms
                .Add(new Room()
                {
                    Name = createRoom.RoomName,
                    UserInfoInRooms = userInfoInRooms
                });
            await _context.SaveChangesAsync();

            return await ReadRoomDetailFromName(createRoom.RoomName);
        }

        internal async Task DeleteRoom(Guid roomId)
        {
            var room = await _context.Rooms
                .Where(room => room.Id == roomId)
                .FirstOrDefaultAsync();

            if (room is not null)
            {
                _context.Remove(room);
                await _context.SaveChangesAsync();
            }
        }
    }
}