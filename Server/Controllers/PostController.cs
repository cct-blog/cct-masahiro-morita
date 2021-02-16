using blazorTest.Server.Services;
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

        /// <summary>
        /// PostControllerクラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ログ出力用のインスタンス</param>
        /// <param name="postService">投稿に関するメインの処理を提供するクラス</param>
        public PostController(ILogger<PostController> logger, PostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        /// <summary>
        /// 指定したルーム内の投稿を取得します。
        /// 指定した日時から最大で50個の投稿を取得します。
        /// </summary>
        /// <param name="requestBody">
        /// 取得する投稿の条件
        /// </param>
        /// <returns>指定した条件に合致する投稿一覧</returns>
        [HttpGet]
        public async Task<IEnumerable<Message>> Get(ChatPostPostRequest requestBody)
        {
            var messages = await _postService.ReadRoomPost(
                requestBody.RoomId, requestBody.NeedMessageTailDate);

            return messages;
        }

        /// <summary>
        /// 指定したルーム内の投稿を取得します。
        /// 指定した日時から最大で50個の投稿を取得します。
        /// また、ルームにアクセスした最新の日時を更新します。
        /// </summary>
        /// <param name="requestBody">
        /// 取得する投稿の条件
        /// </param>
        /// <returns>指定した条件に合致する投稿一覧</returns>
        [HttpPost]
        public async Task<IEnumerable<Message>> Post(ChatPostPostRequest requestBody)
        {
            var messages = await _postService.ReadRoomPost(
                requestBody.RoomId, requestBody.NeedMessageTailDate);

            await _postService.UpdateLastAccessDate(User.Identity.Name, requestBody.RoomId);
            return messages;
        }
    }
}