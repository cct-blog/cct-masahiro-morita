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
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// すべてのユーザーを取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UserInformation>> ReadAllUser()
            => await _context.Users
            .Select(user => new UserInformation()
            {
                HandleName = user.HandleName,
                Email = user.Email
            })
            .ToArrayAsync();

        /// <summary>
        /// ユーザーがルームに所属しているかを確認します。
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<bool> IsUserBelongedRoom(string userId, Guid roomId)
            => await _context.UserInfoInRooms
                .AnyAsync(userInfoInRoom => userInfoInRoom.ApplicationUserId == userId &&
                    userInfoInRoom.RoomId == roomId);

        /// <summary>
        /// ユーザーEmail一覧からユーザー情報を取得します。
        /// </summary>
        /// <param name="emails">ユーザーEmail一覧</param>
        /// <returns></returns>
        public async Task<IEnumerable<ApplicationUser>> ReadUsers(IEnumerable<string> emails)
            => await _context.Users
                .Where(user => emails.Contains(user.Email))
                .ToArrayAsync();

        /// <summary>
        /// ユーザーEmailからユーザー情報を取得します。
        /// </summary>
        /// <param name="email">ユーザーEmail</param>
        /// <returns></returns>
        public async Task<ApplicationUser> ReadUser(string email)
            => await _context.Users
                .FirstOrDefaultAsync(user => email.Contains(user.Email));

        /// <summary>
        /// ルームIdとハンドルネームからユーザー情報を取得します。
        /// </summary>
        /// <param name="handleName">ハンドルネーム</param>
        /// <param name="roomId">ルームId</param>
        /// <returns>UserInfoInRoom</returns>
        internal async Task<UserInfoInRoom> ReadUser(string handleName, Guid roomId)
            => await _context.UserInfoInRooms
                    .Include(userInfoInRoom => userInfoInRoom.ApplicationUser)
                    .FirstOrDefaultAsync(userInfoInRoom => userInfoInRoom.ApplicationUser.HandleName == handleName &&
                        userInfoInRoom.RoomId == roomId);
    }
}