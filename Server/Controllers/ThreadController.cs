using ChatApp.Server.Helper;
using ChatApp.Server.Hubs;
using ChatApp.Server.Services;
using ChatApp.Shared;
using ChatApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Server.Controllers
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

        private readonly PostService _postService;

        /// <summary>
        /// ThreadControllerクラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ログ出力用のインスタンス</param>
        /// <param name="service">スレッドに関するメインの処理を提供するクラス</param>
        /// <param name="userService">ユーザーに関するメインの処理を提供するクラス</param>
        /// <param name="postService">投稿に関するメインの処理を提供するクラス</param>
        public ThreadController(ILogger<ThreadController> logger, ThreadService service, UserService userService, PostService postService)
        {
            _logger = logger;
            _service = service;
            _userService = userService;
            _postService = postService;
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
                await _postService.SendMention(message.MessageContext, thread.RoomId);

                await _postService.SendMessage(SignalRMehod.SendThreadMessage, thread);
                return thread;
            }).AsResultAsync();
        }

        /// <summary>
        /// スレッドの投稿を更新します。
        /// </summary>
        /// <param name="message">更新したいメッセージの内容</param>
        /// <returns></returns>
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

        /// <summary>
        /// 投稿したスレッドを削除します。
        /// </summary>
        /// <param name="threadId">削除するスレッドのId</param>
        /// <returns></returns>
        [HttpDelete("{threadId:guid}")]
        public async Task<ActionResult> DeleteThread(Guid threadId)
            => await this.ExecuteAsync(async () =>
            {
                var thread = await _service.ReadThread(threadId);
                await _service.DeleteThread(thread);
            });
    }
}