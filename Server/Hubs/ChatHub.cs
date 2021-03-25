using blazorTest.Server.Data;
using blazorTest.Server.Models;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        public async Task SendMessage(Message message)
        {
            var messageLength = message.MessageContext.Length;
            if (message.MessageContext.Length > 200)
            {
                throw new HubException($"Message length is over 200, yours {messageLength}");
            }

            await SendMention(message.MessageContext, message.RoomId);

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

            var updatedPost = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == post.Id);

            await Clients.All.SendAsync(SignalRMehod.ReceiveMessage,
                new Message
                {
                    Id = updatedPost.Id,
                    HandleName = user.HandleName,
                    MessageContext = updatedPost.Text,
                    RoomId = updatedPost.RoomId,
                    UserEmail = user.Email,
                    CreateDate = updatedPost.CreateDate
                });
        }

        private async Task SendMention(string message, Guid roomId)
        {
            var mentions = MessageAnalyzer.CheckMention(message);
            await Task.WhenAll(mentions.Select(async mention =>
            {
                var mentionedUser = mention.Value.Substring(1, mention.Length - 2);

                var userData = await _context.UserInfoInRooms
                    .Include(userInfoInRoom => userInfoInRoom.ApplicationUser)
                    .FirstOrDefaultAsync(userInfoInRoom => userInfoInRoom.ApplicationUser.HandleName == mentionedUser &&
                        userInfoInRoom.RoomId == roomId);

                if (userData is not null) await Clients.User(userData.ApplicationUser.Email)
                    .SendAsync(SignalRMehod.SendMention, message);
            }));
        }
    }
}