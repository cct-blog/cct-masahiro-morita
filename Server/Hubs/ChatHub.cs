using ChatApp.Server.Data;
using ChatApp.Server.Models;
using ChatApp.Server.Services;
using ChatApp.Shared;
using ChatApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        private readonly PostService _postService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="postService"></param>
        public ChatHub(ApplicationDbContext context, PostService postService)
        {
            _context = context;
            _postService = postService;
        }

        [Authorize]
        public async Task SendMessage(Message message)
        {
            if (message.MessageContext == null)
            {
                throw new HubException($"Message is null,");
            }
            var messageLength = message.MessageContext.Length;
            if (messageLength > 200)
            {
                throw new HubException($"Message length is over 200, yours {messageLength}");
            }

            await _postService.SendMention(message.MessageContext, message.RoomId);

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
                    CreateDate = updatedPost.CreateDate,
                    UpdateDate = updatedPost.UpdateDate,
                });
        }
    }
}