using blazorTest.Server.Data;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blazorTest.Server.Services
{
    public class ThreadService
    {
        private readonly ILogger<ThreadService> _logger;

        private readonly ApplicationDbContext _context;

        public ThreadService(ILogger<ThreadService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// スレッドIdからスレッド情報を取得して、ThreadMessageに変換して返します。
        /// </summary>
        /// <param name="id">スレッドId</param>
        /// <returns>ThreadMessage</returns>
        internal async Task<ThreadMessage> ReadThreadMessage(Guid id)
        {
            var thread = await ReadThread(id);

            return (thread is null)
                ? null
                : new ThreadMessage()
                {
                    ThreadId = thread.Id,
                    PostId = thread.PostId,
                    HandleName = thread.ApplicationUser.HandleName,
                    UserEmail = thread.ApplicationUser.Email,
                    RoomId = thread.Post.RoomId,
                    CreateDate = thread.CreateDate,
                    MessageContext = thread.Text
                };
        }

        /// <summary>
        /// スレッドIdからスレッド情報を取得します。
        /// </summary>
        /// <param name="id">スレッドId</param>
        /// <returns>ThreadMessage</returns>
        internal async Task<Models.Thread> ReadThread(Guid id)
            => await _context.Threads
                .Include(t => t.Post)
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(thread => thread.Id == id);

        /// <summary>
        /// 投稿に紐づくすべてのスレッドを取得します。
        /// </summary>
        /// <param name="postId">投稿Id</param>
        /// <returns></returns>
        internal async Task<IEnumerable<ThreadMessage>> ReadAllThread(Guid postId)
            => await _context.Threads
                .Include(t => t.Post)
                .Include(t => t.ApplicationUser)
                .Where(t => t.PostId == postId)
                .Select(t => new ThreadMessage()
                {
                    ThreadId = t.Id,
                    PostId = t.PostId,
                    RoomId = t.Post.RoomId,
                    MessageContext = t.Text,
                    HandleName = t.ApplicationUser.HandleName,
                    UserEmail = t.ApplicationUser.Email,
                    CreateDate = t.CreateDate
                })
                .ToArrayAsync();

        //TODO: メンションの処理を入れる。
        internal async Task<Guid> CreateThread(ThreadMessage message, string userId)
        {
            var threadId = Guid.NewGuid();

            var thread = new Models.Thread()
            {
                Id = threadId,
                ApplicationUserId = userId,
                PostId = message.PostId,
                Text = message.MessageContext
            };

            _context.Add(thread);
            await _context.SaveChangesAsync();

            return threadId;
        }

        /// <summary>
        /// 対象のスレッドのメッセージを更新します。
        /// </summary>
        /// <param name="thread">更新対象のThreadインスタンス</param>
        /// <param name="message">更新するメッセージ</param>
        /// <returns></returns>
        internal async Task UpdateThread(Models.Thread thread, string message)
        {
            thread.Text = message;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 指定したスレッドを削除します。
        /// </summary>
        /// <param name="thread">Thread</param>
        /// <returns></returns>
        internal async Task DeleteThread(Models.Thread thread)
        {
            _context.Remove(thread);
            await _context.SaveChangesAsync();
        }
    }
}