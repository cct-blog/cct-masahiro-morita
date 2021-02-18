using blazorTest.Server.Services;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace blazorTest.Server.Controllers
{
    /// <summary>
    /// ルームに関するAPIを提供するクラス
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;

        private readonly RoomService _roomService;

        /// <summary>
        /// RoomControllerクラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ログ出力用のインスタンス</param>
        /// <param name="roomService">ルームに関するメインの処理を提供するクラス</param>
        public RoomController(ILogger<RoomController> logger, RoomService roomService)
        {
            _logger = logger;
            _roomService = roomService;
        }

        /// <summary>
        /// 認証したユーザーがアクセスできるルームの一覧を取得します。
        /// </summary>
        /// <returns>アクセス可能なルームの一覧</returns>
        [HttpGet]
        public async Task<IEnumerable<UserRoom>> Get()
        {
            var userEmail = User.Identity.Name;
            return await _roomService.ReadRoomListOfUser(userEmail);
        }

        /// <summary>
        /// 指定したルームの詳細な情報を取得します。
        /// </summary>
        /// <param name="roomId">詳細情報を取得したいルームのID</param>
        /// <returns>指定したルームの詳細情報</returns>
        [HttpGet("{roomId:guid}")]
        public async Task<RoomDetail> GetRoomDetail(Guid roomId)
            => await _roomService.ReadRoomDetailFromId(roomId);

        /// <summary>
        /// 指定した内容のルームを作成します。
        /// </summary>
        /// <param name="createRoom">作成するルームの詳細</param>
        /// <returns>作成されたルームの詳細情報</returns>
        [HttpPost]
        public async Task<RoomDetail> CreateRoom(CreateRoom createRoom)
            => await _roomService.CreateRoom(createRoom);

        /// <summary>
        /// ルームにユーザーを追加(参加)します。
        /// </summary>
        /// <param name="roomId">ユーザーを追加するルームのID</param>
        /// <param name="userEmail">追加したいユーザーのEmail一覧</param>
        /// <returns>ユーザーを追加したルームの詳細情報</returns>
        [HttpPost("{roomId:guid}/User")]
        public async Task<RoomDetail> AddUserToRoom(Guid roomId, List<string> userEmail)
            => await _roomService.AddUserToRoom(userEmail, roomId);

        /// <summary>
        /// 指定したルームを削除します。
        /// </summary>
        /// <param name="roomId">削除したいルームのID</param>
        /// <returns>No content</returns>
        [HttpDelete("{roomId:guid}")]
        public async Task<IActionResult> DeleteRoom(Guid roomId)
        {
            await _roomService.DeleteRoom(roomId);
            return NoContent();
        }

        /// <summary>
        /// 指定したルームからユーザーを削除します。
        /// </summary>
        /// <param name="roomId">ユーザーを削除するルームのID</param>
        /// <param name="userEmail">削除したいユーザーのEmail一覧</param>
        /// <returns></returns>
        [HttpDelete("{roomId:guid}/User")]
        public async Task<RoomDetail> DeleteUserFromRoom(Guid roomId, List<string> userEmail)
            => await _roomService.DeleteUserFromRoom(userEmail, roomId);
    }
}