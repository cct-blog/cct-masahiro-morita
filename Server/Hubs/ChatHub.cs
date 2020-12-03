using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.SignalR;
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
                .Where(user => user.Email == message.userEmail)
                .Single();

            var post = new Post()
            {
                ApplicationUserId = user.Id,
                Text = message.messageContext,
                RoomId = message.roomId
            };
            _context.Posts.Add(post);
            _context.SaveChanges();

            message.handleName ??= user.HandleName;

            await Clients.All.SendAsync(SignalRMehod.receiveMessage, message);
        }
    }
}