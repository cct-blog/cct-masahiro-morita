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
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly ILogger<PostController> _logger;

        private readonly PostService _postService;

        public PostController(ILogger<PostController> logger, PostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        /// <summary>
        /// Get the post message in user specified room.
        /// Under 50 post posting after specified datetime will be returned
        /// </summary>
        /// <param name="requestBody">
        /// The room id wanted to get, and can specify data count and tail date
        /// </param>
        /// <returns>The message with user specified condition</returns>
        [HttpGet]
        public async Task<IEnumerable<Message>> Get(ChatPostPostRequest requestBody)
        {
            var messages = await _postService.ReadRoomPost(
                requestBody.RoomId, requestBody.NeedMessageTailDate);

            return messages;
        }

        /// <summary>
        /// The message user specified will be returned
        /// This method update Room access date, therefore only call in initializing room
        /// </summary>
        /// <param name="requestBody">
        /// The room id wanted to get, and can specify data count and tail date
        /// </param>
        /// <returns>The message with user specified condition</returns>
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