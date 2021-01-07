using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context) => _context = context;

        public async Task SendMessage(Message message)
        {
            var user = _context.Users
                .Where(user => user.Email == message.UserEmail)
                .Single();

            var post = new Post()
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user.Id,
                Text = message.MessageContext,
                RoomId = message.RoomId
            };
            _context.Posts.Add(post);
            _context.SaveChanges();

            await Clients.All.SendAsync(SignalRMehod.ReceiveMessage,
                new Message
                {
                    Id = post.Id,
                    HandleName = message.HandleName,
                    MessageContext = message.MessageContext,
                    RoomId = message.RoomId,
                    UserEmail = message.UserEmail
                });
        }
    }
}