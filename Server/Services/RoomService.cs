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
    public class RoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ユーザーの所属するルームの一覧を取得します。
        /// </summary>
        /// <param name="userId">ユーザーId</param>
        /// <returns></returns>
        public async Task<IEnumerable<UserRoom>> ReadRooms(string userId)
            => await _context.UserInfoInRooms
                .Include(userInfoInRooms => userInfoInRooms.Room)
                .Where(userInfoInRoom => userInfoInRoom.ApplicationUserId == userId)
                .Select(userInfoInRoom => new UserRoom()
                {
                    Id = userInfoInRoom.Room.Id,
                    Name = userInfoInRoom.Room.Name,
                    LastAccessDate = userInfoInRoom.LatestAccessDate
                })
                .ToArrayAsync();

        /// <summary>
        /// IDからRoom情報を取得します。
        /// </summary>
        /// <param name="id">ルームId</param>
        /// <returns></returns>
        internal async Task<Room> ReadRoom(Guid id)
            => await _context.Rooms
                .FirstOrDefaultAsync(room => room.Id == id);

        /// <summary>
        /// ルーム名からRoomの詳細情報を取得します。
        /// </summary>
        /// <param name="name">ルームの名前</param>
        /// <returns></returns>
        internal async Task<RoomDetail> ReadRoomDetail(string name)
        {
            var room = await _context.Rooms
                .Include(room => room.UserInfoInRooms)
                .ThenInclude(userInfoInRooms => userInfoInRooms.ApplicationUser)
                .FirstOrDefaultAsync(room => room.Name == name);

            return CreateRoomDetail(room);
        }

        /// <summary>
        /// ルームIDからRoomの詳細情報を取得します。
        /// </summary>
        /// <param name="id">rルームId</param>
        /// <returns></returns>
        internal async Task<RoomDetail> ReadRoomDetail(Guid id)
        {
            var room = await _context.Rooms
                .Include(room => room.UserInfoInRooms)
                .ThenInclude(userInfoInRooms => userInfoInRooms.ApplicationUser)
                .FirstOrDefaultAsync(room => room.Id == id);

            return CreateRoomDetail(room);
        }

        /// <summary>
        /// DBから取得したRoom情報をRoomDetailの形に加工します。
        /// </summary>
        /// <param name="room">Room</param>
        /// <returns></returns>
        private static RoomDetail CreateRoomDetail(Room room)
        {
            if (room is null) return null;

            var userInfoInRooms = room.UserInfoInRooms
                .Select(userInfoInRoom => new UserInformation()
                {
                    HandleName = userInfoInRoom.ApplicationUser.HandleName,
                    Email = userInfoInRoom.ApplicationUser.Email,
                    LastAccessDate = userInfoInRoom.LatestAccessDate
                }).ToList();

            return new RoomDetail()
            {
                RoomId = room.Id,
                RoomName = room.Name,
                CreateDate = room.CreateDate,
                UpdateDate = room.UpdateDate,
                Users = userInfoInRooms
            };
        }

        /// <summary>
        /// ルームにユーザーを追加します。
        /// </summary>
        /// <param name="users">追加するApplicationUserの一覧</param>
        /// <param name="roomId">ルームId</param>
        /// <returns></returns>
        internal async Task AddUser(IEnumerable<ApplicationUser> users, Guid roomId)
        {
            foreach (var user in users)
            {
                var userInfoInRoom = new UserInfoInRoom() { ApplicationUserId = user.Id, RoomId = roomId };
                _context.Add(userInfoInRoom);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// ルームを新たに作成します。
        /// </summary>
        /// <param name="userData">追加するApplicationUserの一覧</param>
        /// <param name="roomName">作成するルームの名前</param>
        /// <returns></returns>
        internal async Task CreateRoom(IEnumerable<ApplicationUser> userData, string roomName)
        {
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
                    Name = roomName,
                    UserInfoInRooms = userInfoInRooms
                });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// ルームを削除します。
        /// </summary>
        /// <param name="room">Room</param>
        /// <returns></returns>
        internal async Task DeleteRoom(Room room)
        {
            _context.Remove(room);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// ルームからユーザーを削除します。
        /// </summary>
        /// <param name="userInfoInRoom">UserInfoInRoom</param>
        /// <returns></returns>
        internal async Task DeleteUserInfoInRoom(UserInfoInRoom userInfoInRoom)
        {
            _context.Remove(userInfoInRoom);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// ユーザーがルームにアクセスした最終アクセス日時を更新します。
        /// </summary>
        /// <param name="userInfoInRoom">UserInfoInRoom</param>
        /// <returns></returns>
        internal async Task PutRoomLastAccessDate(UserInfoInRoom userInfoInRoom)
        {
            userInfoInRoom.LatestAccessDate = DateTime.Now;

            _context.Update(userInfoInRoom);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// RoomとApplicationUserの中間テーブルを取得します。
        /// </summary>
        /// <param name="userId">ユーザーId</param>
        /// <param name="roomId">ルームId</param>
        /// <returns></returns>
        internal async Task<UserInfoInRoom> ReadUserInfoInRoom(string userId, Guid roomId)
            => await _context.UserInfoInRooms
                .FirstOrDefaultAsync(userInfoInRoom => userInfoInRoom.RoomId == roomId
                    && userId == userInfoInRoom.ApplicationUserId);

        /// <summary>
        /// Roomに所属するすべてのユーザーを取得します。
        /// </summary>
        /// <param name="roomId">ルームId</param>
        /// <returns>UserInfoInRoomの配列</returns>
        internal async Task<IEnumerable<UserInfoInRoom>> ReadUsersBelongedToRoom(Guid roomId)
            => await _context.UserInfoInRooms
                    .Include(userInfoInRoom => userInfoInRoom.ApplicationUser)
                    .Where(userInfoInRoom => userInfoInRoom.RoomId == roomId)
                    .ToArrayAsync();
    }
}