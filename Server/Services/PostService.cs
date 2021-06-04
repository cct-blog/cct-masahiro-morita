using blazorTest.Server.Data;
using blazorTest.Server.Hubs;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.SignalR;
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

        private readonly UserService _userService;

        private readonly RoomService _roomService;

        private readonly IHubContext<ChatHub> _hubContext;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userService"></param>
        /// <param name="roomService"></param>
        /// <param name="hubContext"></param>
        public PostService(ApplicationDbContext context, UserService userService, RoomService roomService, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userService = userService;
            _roomService = roomService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// ルーム内の投稿を取得します。
        /// </summary>
        /// <param name="roomId">ルームId</param>
        /// <param name="tailDate">取得する投稿の末日時</param>
        /// <param name="MessageCount">取得するメッセージの数</param>
        /// <returns></returns>
        public async Task<IEnumerable<Message>> ReadPosts(
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

        /// <summary>
        /// 1件の投稿を取得します。
        /// </summary>
        /// <param name="postId">投稿ID</param>
        /// <returns></returns>
        public async Task<Models.Post> ReadPost(Guid postId) => await GetPostAsync(postId);

        /// <summary>
        /// ルーム内の投稿を削除します。
        /// </summary>
        /// <param name="post">削除する投稿</param>
        /// <returns>成功したらtrue、存在しないPostの場合false</returns>
        public async Task<bool> DeletePost(Models.Post post)
        {
            if (post == null)
                return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// ルーム内の投稿を更新します。
        /// </summary>
        /// <param name="post">投稿</param>
        /// <param name="text">更新するテキスト</param>
        /// <param name="updated">更新する時刻</param>
        /// <returns>成功したらtrue、存在しないPostの場合false</returns>
        public async Task<bool> UpdatePost(Models.Post post, string text, DateTime updated)
        {
            if (post == null)
                return false;

            post.UpdateDate = updated;
            post.Text = text;

            _context.Update(post);

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// メッセージからメンションを検索し、含まれる場合はメンションを送信します。
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="roomId">メッセージが投稿されたルーム</param>
        /// <returns></returns>
        public async Task SendMention(string message, Guid roomId)
        {
            var mentions = MessageAnalyzer.GetMentionedUser(message);

            if (mentions.Contains("here"))
            {
                var usersBelongedToRoom = await _roomService.ReadUsersBelongedToRoom(roomId);

                await Task.WhenAll(usersBelongedToRoom.Select(async user =>
                    await _hubContext.Clients.User(user.ApplicationUser.Email)
                        .SendAsync(SignalRMehod.SendMention, message)));

                return;
            }

            await Task.WhenAll(mentions.Select(async mention =>
            {
                var userData = await _userService.ReadUser(mention, roomId);

                if (userData is not null) await _hubContext.Clients.User(userData.ApplicationUser.Email)
                    .SendAsync(SignalRMehod.SendMention, message);
            }));
        }

        /// <summary>
        /// メッセージが更新されたことを更新を送信します
        /// </summary>
        /// <typeparam name="T">メッセージクラス</typeparam>
        /// <param name="signalRMethod">SignalRメソッド名</param>
        /// <param name="content">送信するメッセージ</param>
        /// <returns></returns>
        public async Task SendMessage<T>(string signalRMethod, T content)
            => await _hubContext.Clients.All.SendAsync(signalRMethod, content);

        /// <summary>
        /// 投稿とユーザー情報を取得します。
        /// </summary>
        /// <param name="postId">投稿ID</param>
        /// <returns></returns>
        private async Task<Models.Post> GetPostAsync(Guid postId)
        {
            return await _context.Posts
                .Include(post => post.ApplicationUser)
                .FirstOrDefaultAsync(post => post.Id == postId);

        }
    }
}