using blazorTest.Server.Helper;
using blazorTest.Server.Hubs;
using blazorTest.Server.Services;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Controllers
{
    /// <summary>
    /// スレッドに関するAPIを提供するクラス
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly ILogger<ThreadController> _logger;

        private readonly ThreadService _service;

        private readonly UserService _userService;

        private readonly RoomService _roomService;

        private readonly IHubContext<ChatHub> _hubContext;

        /// <summary>
        /// ThreadControllerクラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ログ出力用のインスタンス</param>
        /// <param name="service">スレッドに関するメインの処理を提供するクラス</param>
        /// <param name="userService">ユーザーに関するメインの処理を提供するクラス</param>
        /// <param name="roomService">ルームに関するメインの処理を提供するクラス</param>
        /// <param name="hubContext">HubContext</param>
        public ThreadController(ILogger<ThreadController> logger, ThreadService service, UserService userService, IHubContext<ChatHub> hubContext, RoomService roomService)
        {
            _logger = logger;
            _service = service;
            _userService = userService;
            _hubContext = hubContext;
            _roomService = roomService;
        }

        /// <summary>
        /// スレッドに紐づくすべてのスレッドを取得します。
        /// </summary>
        /// <param name="postId">投稿のId</param>
        /// <returns></returns>
        [HttpGet("Post/{postId:guid}")]
        public async Task<ActionResult<IEnumerable<ThreadMessage>>> GetAllThread(Guid postId)
            => await this.ExecuteAsync(() => _service.ReadAllThread(postId))
            .AsResultAsync();

        /// <summary>
        /// スレッドIdに該当するスレッドを取得します。
        /// </summary>
        /// <param name="threadId">スレッドId</param>
        /// <returns></returns>
        [HttpGet("{threadId:guid}")]
        public async Task<ActionResult<ThreadMessage>> GetThread(Guid threadId)
            => await this.ExecuteAsync(() => _service.ReadThreadMessage(threadId))
            .AsResultAsync();

        /// <summary>
        /// スレッドにメッセージを登録します。
        /// </summary>
        /// <param name="message">投稿を行うメッセージ</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ThreadMessage>> PostThread(ThreadMessage message)
        {
            var user = await _userService.ReadUser(message.UserEmail);

            if (user is null) return BadRequest("This user is deleted");

            return await this.ExecuteAsync(async () =>
            {
                var threadId = await _service.CreateThread(message, user.Id);

                var thread = await _service.ReadThreadMessage(threadId);
                await SendMention(message.MessageContext, thread.RoomId);

                await _hubContext.Clients.All.SendAsync(SignalRMehod.SendThreadMessage, thread);
                return thread;
            }).AsResultAsync();
        }

        [HttpPut]
        public async Task<ActionResult<ThreadMessage>> UpdateThread(ThreadMessage message)
        {
            (var thread, _) = await this.ExecuteAsync(() => _service.ReadThread(message.ThreadId));

            if (thread is null) return BadRequest($"Thread {message.ThreadId} is deleted");

            return await this.ExecuteAsync(async () =>
            {
                await _service.UpdateThread(thread, message.MessageContext);

                return await _service.ReadThreadMessage(message.ThreadId);
            }).AsResultAsync();
        }

        [HttpDelete("{threadId:guid}")]
        public async Task<ActionResult> DeleteThread(Guid threadId)
            => await this.ExecuteAsync(async () =>
            {
                var thread = await _service.ReadThread(threadId);
                await _service.DeleteThread(thread);
            });

        private async Task SendMention(string message, Guid roomId)
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
    }
}