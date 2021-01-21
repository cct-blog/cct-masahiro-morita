using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace blazorTest.Server.Services
{
    public class PostService : IService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Message> ReadRoomPost(
            Guid roomId, DateTime tailDate, int MessageCount = 50)
        => _context.Posts
                .Where(_post => _post.RoomId.Equals(roomId)
                    && _post.CreateDate < tailDate)
                .Include(_post => _post.ApplicationUser)
                .AsEnumerable()
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

        public IEnumerable<UserBelongedRoomPost> ReadUserBelongedRoomPost(string userEmail, DateTime tailDate)
        {
            var userId = _context.Users
                .Where(_user => _user.Email == userEmail)
                .FirstOrDefault()
                .Id;

            return _context.UserInfoInRooms
                .Where(_userRoom => _userRoom.ApplicationUserId == userId)
                .Include(_userRoom => _userRoom.Room)
                .ThenInclude(_room => _room.Posts
                    .Where(_post => _post.CreateDate > tailDate))
                .Select(_userRoom => new UserBelongedRoomPost()
                {
                    RoomId = _userRoom.RoomId,
                    Texts = _userRoom.Room.Posts.Select(_post => _post.Text).AsEnumerable()
                })
                .AsEnumerable();
        }

        public void UpdateLastAccessDate(string userEmail, Guid roomId)
        {
            var userId = _context.Users
                .Where(_user => _user.Email == userEmail)
                .FirstOrDefault()
                .Id;

            ReadUserBelongedRoomPost(userEmail, DateTime.Now);

            var userInfoInRoom = _context.UserInfoInRooms
                .Where(_userInfo => _userInfo.ApplicationUserId == userId && _userInfo.RoomId == roomId)
                .FirstOrDefault();

            userInfoInRoom.LatestAccessDate = DateTime.Now;

            _context.Update(userInfoInRoom);
            _context.SaveChanges();
        }
    }
}