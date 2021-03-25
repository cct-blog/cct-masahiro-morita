using blazorTest.Server.Data;
using blazorTest.Shared.Models;
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

        //TODO: メンションの処理を入れる。
        internal async Task<ThreadMessage> CreateThread(ThreadMessage message)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == message.UserEmail);
            var threadId = Guid.NewGuid();

            var thread = new Models.Thread()
            {
                Id = threadId,
                ApplicationUserId = user.Id,
                PostId = message.PostId,
                Text = message.MessageContext
            };

            _context.Add(thread);
            await _context.SaveChangesAsync();

            var postedThread = await _context.Threads
                .FirstOrDefaultAsync(thread => thread.Id == threadId);

            return new ThreadMessage()
            {
                ThreadId = postedThread.Id,
                PostId = postedThread.PostId,
                HandleName = message.HandleName,
                UserEmail = message.UserEmail,
                RoomId = message.RoomId,
                CreateDate = postedThread.CreateDate,
                MessageContext = postedThread.Text
            };
        }
    }
}