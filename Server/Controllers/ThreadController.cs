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
    /// スレッドに関するAPIを提供するクラス
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly ILogger<ThreadController> _logger;

        private readonly ThreadService _service;

        /// <summary>
        /// ThreadControllerクラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ログ出力用のインスタンス</param>
        /// <param name="service">スレッドに関するメインの処理を提供するクラス</param>
        public ThreadController(ILogger<ThreadController> logger, ThreadService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// スレッドにメッセージを登録します。
        /// </summary>
        /// <param name="message">投稿を行うメッセージ</param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ThreadMessage> PostThread(ThreadMessage message)
            => await _service.CreateThread(message);
    }
}