using blazorTest.Server.Exceptions;
using blazorTest.Server.Services;
using blazorTest.Shared;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Controllers
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
            var userEmail = User.Identity.Name;

            var user = await _userService.ReadUser(userEmail);

            if (user is null) return BadRequest("This user is deleted");

            if (!await _userService.IsUserBelongedRoom(user.Id, request.RoomId))
            {
                return BadRequest("User is not belonged to room");
            }

            if (await _roomService.ReadRoom(request.RoomId) is null)
            {
                return BadRequest($"Room Id {request.RoomId} is not exsisted");
            }

            var messages = await _postService.ReadPost(
                request.RoomId, request.NeedMessageTailDate);

            return Ok(messages);
        }
    }
}