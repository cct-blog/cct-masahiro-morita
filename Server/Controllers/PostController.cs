using ChatApp.Server.Services;
using ChatApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Server.Controllers
{
    /// <summary>
    /// 投稿に関するAPIを提供するクラス
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly ILogger<PostController> _logger;

        private readonly PostService _postService;

        private readonly UserService _userService;

        private readonly RoomService _roomService;

        /// <summary>
        /// PostControllerクラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ログ出力用のインスタンス</param>
        /// <param name="postService">投稿に関するメインの処理を提供するクラス</param>
        /// <param name="userService">ユーザーに関するメインの処理を提供するクラス</param>
        /// <param name="roomService">ルームに関するメインの処理を提供するクラス</param>
        public PostController(ILogger<PostController> logger, PostService postService, UserService userService, RoomService roomService)
        {
            _logger = logger;
            _postService = postService;
            _userService = userService;
            _roomService = roomService;
        }

        /// <summary>
        /// 指定したルーム内の投稿を取得します。
        /// 指定した日時から最大で50個の投稿を取得します。
        /// </summary>
        /// <param name="request">
        /// 取得する投稿の条件
        /// </param>
        /// <returns>指定した条件に合致する投稿一覧</returns>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Message>>> Post(ChatPostPostRequest request)
        {
            var validationResult = await ValidateParameter(request.RoomId);

            if (validationResult != null)
                return validationResult;

            var messages = await _postService.ReadPosts(
                request.RoomId, request.NeedMessageTailDate);

            return Ok(messages);
        }

        /// <summary>
        /// 指定した投稿を削除します。
        /// </summary>
        /// <param name="postId">投稿ID</param>
        /// <returns></returns>
        [HttpDelete("{postId}")]
        public async Task<ActionResult> Delete(Guid postId)
        {
            var post = await _postService.ReadPost(postId);
            if (post == null)
                return NotFound();

            var validationResult = await ValidateParameter(post.RoomId);
            if (validationResult != null)
                return validationResult;

            return (await _postService.DeletePost(post)) ? Ok() : NotFound();
        }

        /// <summary>
        /// 指定した投稿を修正します。
        /// </summary>
        /// <param name="postId">投稿ID</param>
        /// <param name="request">更新内容</param>
        /// <returns></returns>
        [HttpPut("{postId}")]
        public async Task<ActionResult> Put(Guid postId, ChatPostUpdateRequest request)
        {
            var post = await _postService.ReadPost(postId);
            if (post == null)
                return NotFound();

            var validationResult = await ValidateParameter(post.RoomId);
            if (validationResult != null)
                return validationResult;

            return (await _postService.UpdatePost(post, request.Text, DateTime.Now)) ? Ok() : NotFound();
        }

        /// <summary>
        /// 指定されたルームが存在し、ユーザーがルームに所属しているかを判定します。
        /// </summary>
        private async Task<ActionResult> ValidateParameter(Guid roomId)
        {
            var userEmail = User.Identity.Name;

            var user = await _userService.ReadUser(userEmail);

            if (user is null) return BadRequest("This user is deleted");

            if (!await _userService.IsUserBelongedRoom(user.Id, roomId))
            {
                return BadRequest("User is not belonged to room");
            }

            if (await _roomService.ReadRoom(roomId) is null)
            {
                return BadRequest($"Room Id {roomId} is not exsisted");
            }
            return null;
        }
    }
}