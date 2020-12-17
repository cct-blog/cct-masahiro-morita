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
    public class PostController
    {
        private readonly ILogger<PostController> _logger;

        private readonly PostService _postService;

        public PostController(ILogger<PostController> logger, PostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        [HttpPost]
        public IEnumerable<Message> Post(
            [FromBody] ChatPostPostRequest requestBody)
        {
            var messages = _postService.ReadPostWhenWindowOpened(
                requestBody.RoomId, requestBody.NeedMessageTailDate);
            return messages;
        }
    }
}