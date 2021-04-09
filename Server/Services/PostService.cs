using blazorTest.Server.Data;
using blazorTest.Server.Exceptions;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// ルーム内の投稿を取得します。
        /// </summary>
        /// <param name="roomId">リームId</param>
        /// <param name="tailDate">取得する投稿の末日時</param>
        /// <param name="MessageCount">取得するメッセージの数</param>
        /// <returns></returns>
        public async Task<IEnumerable<Message>> ReadPost(
            Guid roomId, DateTime tailDate, int MessageCount = 50)
        {
            var roomPost = await _context.Posts
                .Include(post => post.ApplicationUser)
                .Where(post => post.RoomId == roomId
                    && post.CreateDate < tailDate)
                .OrderBy(post => post.CreateDate)
                .ToArrayAsync();

            roomPost = roomPost
                .TakeLast(MessageCount)
                .ToArray();

            return roomPost
                .Select(post => new Message()
                {
                    Id = post.Id,
                    RoomId = roomId,
                    MessageContext = post.Text,
                    HandleName = post.ApplicationUser.HandleName,
                    CreateDate = post.CreateDate
                });
        }
    }
}