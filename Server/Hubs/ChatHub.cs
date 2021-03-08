using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == message.UserEmail);

            if (user is null) throw new HubException("User is not Exsisted");

            if (!await _context.Rooms
                .AnyAsync(room => room.Id == message.RoomId))
            {
                throw new HubException("Specified room is not exsisted");
            }

            var post = new Post()
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user.Id,
                Text = message.MessageContext,
                RoomId = message.RoomId
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

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