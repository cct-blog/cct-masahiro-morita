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
                .Where(post => post.RoomId.Equals(roomId)
                    && post.CreateDate < tailDate)
                .Include(post => post.ApplicationUser)
                .ToArrayAsync();

            return roomPost
                .OrderBy(post => post.CreateDate)
                .TakeLast(MessageCount)
                .Select(post => new Message()
                {
                    Id = post.Id,
                    RoomId = roomId,
                    MessageContext = post.Text,
                    HandleName = post.ApplicationUser.HandleName,
                    CreateDate = post.CreateDate
                });
        }

        public async Task<IEnumerable<UserBelongedRoomPost>> ReadUserBelongedRoomPost(string userEmail, DateTime tailDate)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == userEmail);
            var userId = user.Id;

            return await _context.UserInfoInRooms
                .Where(userRoom => userRoom.ApplicationUserId == userId)
                .Include(userRoom => userRoom.Room)
                .ThenInclude(room => room.Posts
                    .Where(post => post.CreateDate > tailDate))
                .Select(userRoom => new UserBelongedRoomPost()
                {
                    RoomId = userRoom.RoomId,
                    Texts = userRoom.Room.Posts.Select(post => post.Text).AsEnumerable()
                })
                .ToArrayAsync();
        }

        public async Task UpdateLastAccessDate(string userEmail, Guid roomId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == userEmail);
            var userId = user.Id;

            await ReadUserBelongedRoomPost(userEmail, DateTime.Now);

            var userInfoInRoom = await _context.UserInfoInRooms
                .FirstOrDefaultAsync(userInfo => userInfo.ApplicationUserId == userId && userInfo.RoomId == roomId);

            userInfoInRoom.LatestAccessDate = DateTime.Now;

            _context.Update(userInfoInRoom);
            await _context.SaveChangesAsync();
        }
    }
}