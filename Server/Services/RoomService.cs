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
                .Where(_user => _user.Email == userEmail)
                .FirstOrDefaultAsync();

            return await _context.Rooms
                .Include(room => room.UserInfoInRooms)
                .Where(_room => _room.UserInfoInRooms
                    .Where(_userInfoInRooms => _userInfoInRooms.ApplicationUserId == user.Id)
                    .Any())
                .Select(_room => new UserRoom()
                {
                    Id = _room.Id,
                    Name = _room.Name,
                    LastAccessDate = _room.UserInfoInRooms.FirstOrDefault().LatestAccessDate
                })
                .ToArrayAsync();
        }

        private async Task<RoomDetail> ReadRoomDetail(IQueryable<Room> rooms)
        {
            var roomDetail = await rooms
            .Include(_room => _room.UserInfoInRooms)
                .ThenInclude(_userInfoInRooms => _userInfoInRooms.ApplicationUser)
            .Select(_room => new RoomDetail()
            {
                RoomId = _room.Id,
                RoomName = _room.Name,
                CreateDate = _room.CreateDate,
                UpdateDate = _room.UpdateDate,
                Users = _room.UserInfoInRooms
                    .Select(_m => new UserInformation()
                    {
                        HandleName = _m.ApplicationUser.HandleName,
                        Email = _m.ApplicationUser.Email,
                        LastAccessDate = _m.LatestAccessDate
                    })
                .ToList()
            })
            .ToArrayAsync();

            return roomDetail.FirstOrDefault();
        }

        internal async Task<RoomDetail> ReadRoomDetailFromId(Guid id)
        {
            var roomQuery = _context.Rooms.Where(_room => _room.Id == id);
            return await ReadRoomDetail(roomQuery);
        }

        internal async Task<RoomDetail> ReadRoomDetailFromName(string name)
        {
            var roomQuery = _context.Rooms.Where(_room => _room.Name == name);
            return await ReadRoomDetail(roomQuery);
        }

        internal async Task<RoomDetail> AddUserToRoom(List<string> userEmails, Guid roomId)
        {
            var users = await _context.Users
                .Where(_user => userEmails.Contains(_user.Email))
                .ToListAsync();

            users.ForEach(_user =>
            {
                var userInfoInRoom = new UserInfoInRoom() { ApplicationUserId = _user.Id, RoomId = roomId };
                _context.Add(userInfoInRoom);
            });

            await _context.SaveChangesAsync();

            return await ReadRoomDetailFromId(roomId);
        }

        internal async Task<RoomDetail> CreateRoom(CreateRoom createRoom)
        {
            var userData = createRoom.UserIds
                .Select(_userEmail => _context.Users
                    .Where(_user => _user.Email == _userEmail)
                    .AsEnumerable()
                    .FirstOrDefault())
                .ToList();

            var userInfoInRooms = userData
                .Select(_m => new UserInfoInRoom()
                {
                    ApplicationUserId = _m.Id,
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
                .Where(_room => _room.Id == roomId)
                .FirstOrDefaultAsync();

            if (room is not null)
            {
                _context.Remove(room);
                await _context.SaveChangesAsync();
            }
        }
    }
}