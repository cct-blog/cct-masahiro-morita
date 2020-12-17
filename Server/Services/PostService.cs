using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared.Models;
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

        public IEnumerable<Message> ReadPostWhenWindowOpened(Guid roomId, DateTime needMessageTailDate, int MessageCount = 50)
        {
            var posts = _context.Posts
                .Where(_post => _post.RoomId.Equals(roomId))
                .Where(_post => _post.CreateDate < needMessageTailDate)
                .ToList()
                .TakeLast(MessageCount)
                .Select(_post => new Message() { RoomId = roomId, MessageContext = _post.Text })
                .ToArray();

            return posts;
        }
    }
}