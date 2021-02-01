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

        public IEnumerable<UserRoom> ReadRoomListOfUser(string userEmail)
        {
            var user = _context.Users
                .Where(_user => _user.Email == userEmail)
                .FirstOrDefault();

            return _context.Rooms
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
                    Users = _room.UserInfoInRooms
                        .Select(_m => new UserInformation()
                        {
                            HandleName = _m.ApplicationUser.HandleName,
                            Email = _m.ApplicationUser.Email,
                            LastAccessDate = _m.LatestAccessDate
                        })
                    .ToList()
                })
                .AsEnumerable()
                .FirstOrDefault();

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

        internal RoomDetail AddUserToRoom(List<string> userEmails, Guid roomId)
        {
            var users = _context.Users
                .Where(_user => userEmails.Contains(_user.Email))
                .ToList();

            users.ForEach(_user =>
            {
                var userInfoInRoom = new UserInfoInRoom() { ApplicationUserId = _user.Id, RoomId = roomId };
                _context.Add(userInfoInRoom);
            });

            _context.SaveChanges();

            return ReadRoomDetailFromId(roomId);
        }

        internal RoomDetail CreateRoom(CreateRoom createRoom)
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
            _context.SaveChanges();

            return ReadRoomDetailFromName(createRoom.RoomName);
        }

        internal void DeleteRoom(Guid roomId)
        {
            var room = _context.Rooms
                .Where(_room => _room.Id == roomId)
                .FirstOrDefault();

            if (room is not null)
            {
                _context.Remove(room);
                _context.SaveChanges();
            }
        }
    }
}