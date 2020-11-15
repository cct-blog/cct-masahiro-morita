using blazorTest.Server.Data;
using blazorTest.Server.Models;
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

        public async Task SendMessage(string email, string message, Guid roomId)
        {
            var user = _context.Users
                .Where(user => user.Email == email)
                .Single();

            var post = new Post() { ApplicationUserId = user.Id, Text = message, RoomId = roomId };
            _context.Posts.Add(post);
            _context.SaveChanges();

            await Clients.All.SendAsync("ReceiveMessage", user.HandleName, email, message);
        }
    }
}