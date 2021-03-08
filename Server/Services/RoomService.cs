using blazorTest.Server.Data;
using blazorTest.Server.Exceptions;
using blazorTest.Server.Models;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Services
{
    public class RoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRoom>> ReadRoomListOfUser(string userEmail)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == userEmail);

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

            if (!await roomQuery.AnyAsync())
            {
                throw new HttpResponseException
                {
                    ErrorType = ErrorType.ROOM_IS_NOT_EXSISTED,
                    Value = "No User registered"
                };
            }

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

            if (!users.Any())
            {
                throw new HttpResponseException()
                {
                    ErrorType = ErrorType.INVALID_USERS,
                    Value = "No User registered"
                };
            }

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
            var userData = await _context.Users
                .Where(user => createRoom.UserIds.Contains(user.Email))
                .ToArrayAsync();

            if (!userData.Any())
            {
                throw new HttpResponseException
                {
                    ErrorType = ErrorType.INVALID_USERS,
                    Value = "No User exsisted"
                };
            }

            var lastAccessDate = DateTime.Now;

            var userInfoInRooms = userData
                .Select(user => new UserInfoInRoom()
                {
                    ApplicationUserId = user.Id,
                    LatestAccessDate = lastAccessDate
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
                .FirstOrDefaultAsync(room => room.Id == roomId);

            if (room is not null)
            {
                _context.Remove(room);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new HttpResponseException()
                {
                    ErrorType = ErrorType.ROOM_IS_NOT_EXSISTED,
                    Value = "Room Id {1} is already not exsisted"
                };
            }
        }

        internal async Task<RoomDetail> DeleteUserFromRoom(string userEmail, Guid roomId)
        {
            var userInfoInRoom = await ReadUserFromEmailAndRoomId(userEmail, roomId);

            _context.Remove(userInfoInRoom);
            await _context.SaveChangesAsync();

            return await ReadRoomDetailFromId(roomId);
        }

        internal async Task<RoomDetail> PutRoomLastAccessDate(Guid roomId, string userEmail)
        {
            var userInfoInRoom = await ReadUserFromEmailAndRoomId(userEmail, roomId);

            userInfoInRoom.LatestAccessDate = DateTime.Now;

            _context.Update(userInfoInRoom);
            await _context.SaveChangesAsync();

            return await ReadRoomDetailFromId(roomId);
        }

        private async Task<UserInfoInRoom> ReadUserFromEmailAndRoomId(string userEmail, Guid roomId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == userEmail);

            if (user is null)
            {
                throw new HttpResponseException()
                {
                    ErrorType = ErrorType.INVALID_USERS,
                    Value = "User {1} is not exsisted"
                };
            }

            var userInfoInRoom = await _context.UserInfoInRooms
                .FirstOrDefaultAsync(userInfoInRoom => userInfoInRoom.RoomId == roomId
                    && user.Id == userInfoInRoom.ApplicationUserId);

            if (userInfoInRoom is null)
            {
                throw new HttpResponseException()
                {
                    ErrorType = ErrorType.USER_NOT_BELONGED_AT_ROOM,
                    Value = "User is not belonged to room"
                };
            }

            return userInfoInRoom;
        }
    }
}