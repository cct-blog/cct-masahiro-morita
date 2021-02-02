using blazorTest.Server.Data;
using blazorTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Services
{
    public class PostService : IService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Message>> ReadRoomPost(
            Guid roomId, DateTime tailDate, int MessageCount = 50)
        {
            var roomPost = await _context.Posts
                .Where(_post => _post.RoomId.Equals(roomId)
                    && _post.CreateDate < tailDate)
                .Include(_post => _post.ApplicationUser)
                .ToArrayAsync();

            return roomPost
                .OrderBy(post => post.CreateDate)
                .TakeLast(MessageCount)
                .Select(_post => new Message()
                {
                    Id = _post.Id,
                    RoomId = roomId,
                    MessageContext = _post.Text,
                    HandleName = _post.ApplicationUser.HandleName,
                    CreateDate = _post.CreateDate
                });
        }

        public async Task<IEnumerable<UserBelongedRoomPost>> ReadUserBelongedRoomPost(string userEmail, DateTime tailDate)
        {
            var user = await _context.Users
                .Where(_user => _user.Email == userEmail)
                .FirstOrDefaultAsync();
            var userId = user.Id;

            return await _context.UserInfoInRooms
                .Where(_userRoom => _userRoom.ApplicationUserId == userId)
                .Include(_userRoom => _userRoom.Room)
                .ThenInclude(_room => _room.Posts
                    .Where(_post => _post.CreateDate > tailDate))
                .Select(_userRoom => new UserBelongedRoomPost()
                {
                    RoomId = _userRoom.RoomId,
                    Texts = _userRoom.Room.Posts.Select(_post => _post.Text).AsEnumerable()
                })
                .ToArrayAsync();
        }

        public async Task UpdateLastAccessDate(string userEmail, Guid roomId)
        {
            var user = await _context.Users
                .Where(_user => _user.Email == userEmail)
                .FirstOrDefaultAsync();
            var userId = user.Id;

            await ReadUserBelongedRoomPost(userEmail, DateTime.Now);

            var userInfoInRoom = await _context.UserInfoInRooms
                .Where(_userInfo => _userInfo.ApplicationUserId == userId && _userInfo.RoomId == roomId)
                .FirstOrDefaultAsync();

            userInfoInRoom.LatestAccessDate = DateTime.Now;

            _context.Update(userInfoInRoom);
            await _context.SaveChangesAsync();
        }
    }
}