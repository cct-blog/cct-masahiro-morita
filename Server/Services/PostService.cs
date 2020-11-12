using blazorTest.Server.Data;
using blazorTest.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Post> ReadPostWhenWindowOpened(Guid roomId, DateTime needMessageTailDate, int MessageCount)
        {
            var posts = _context.Posts
                .Where(_post => _post.RoomId.Equals(roomId))
                .Where(_post => _post.CreateDate < needMessageTailDate)
                .TakeLast(MessageCount)
                .ToList();

            return posts;
        }
    }
}