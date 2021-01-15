using blazorTest.Server.Data;
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

        public IEnumerable<Message> ReadPostWhenWindowOpened(
            Guid roomId, DateTime needMessageTailDate, int MessageCount = 50)
        => _context.Posts
                .Where(_post => _post.RoomId.Equals(roomId)
                    && _post.CreateDate < needMessageTailDate)
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
    }
}