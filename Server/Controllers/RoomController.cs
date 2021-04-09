using blazorTest.Server.Helper;
using blazorTest.Server.Services;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly UserService _userService;

        /// <summary>
        /// RoomControllerクラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ログ出力用のインスタンス</param>
        /// <param name="roomService">ルームに関するメインの処理を提供するクラス</param>
        /// <param name="userService">ユーザーに関するメインの処理を提供するクラス</param>
        public RoomController(ILogger<RoomController> logger, RoomService roomService, UserService userService)
        {
            _logger = logger;
            _roomService = roomService;
            _userService = userService;
        }

        /// <summary>
        /// 認証したユーザーがアクセスできるルームの一覧を取得します。
        /// </summary>
        /// <returns>アクセス可能なルームの一覧</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRoom>>> Get()
        {
            var userEmail = User.Identity.Name;
            var user = await _userService.ReadUser(userEmail);

            if (user is null) return BadRequest("This user is deleted");

            return Ok(await _roomService.ReadRooms(user.Id));
        }

        /// <summary>
        /// 指定したルームの詳細な情報を取得します。
        /// </summary>
        /// <param name="roomId">詳細情報を取得したいルームのID</param>
        /// <returns>指定したルームの詳細情報</returns>
        [HttpGet("{roomId:guid}")]
        public async Task<ActionResult<RoomDetail>> GetRoomDetail(Guid roomId)
        {
            var roomDetail = await _roomService.ReadRoomDetail(roomId);

            if (roomDetail is null) return BadRequest($"Room Id {roomId} is not exsisted");

            return Ok(roomDetail);
        }

        /// <summary>
        /// 指定した内容のルームを作成します。
        /// </summary>
        /// <param name="createRoom">作成するルームの詳細</param>
        /// <returns>作成されたルームの詳細情報</returns>
        [HttpPost]
        public async Task<ActionResult<RoomDetail>> CreateRoom(CreateRoom createRoom)
        {
            var users = await _userService.ReadUsers(createRoom.UserIds);

            if (!users.Any()) return BadRequest("All users are invalid");

            await _roomService.CreateRoom(users, createRoom.RoomName);

            return Ok(await _roomService.ReadRoomDetail(createRoom.RoomName));
        }

        /// <summary>
        /// ルームにユーザーを追加(参加)します。
        /// </summary>
        /// <param name="roomId">ユーザーを追加するルームのID</param>
        /// <param name="userEmail">追加したいユーザーのEmail一覧</param>
        /// <returns>ユーザーを追加したルームの詳細情報</returns>
        [HttpPost("{roomId:guid}/User")]
        public async Task<ActionResult<RoomDetail>> AddUserToRoom(Guid roomId, List<string> userEmail)
        {
            var users = await _userService.ReadUsers(userEmail);

            if (!users.Any()) return BadRequest("All users are invalid");

            return await this.Execute(async () =>
            {
                await _roomService.AddUser(users, roomId);
                return await _roomService.ReadRoomDetail(roomId);
            }).AsResult();
        }

        /// <summary>
        /// 指定したルームを削除します。
        /// </summary>
        /// <param name="roomId">削除したいルームのID</param>
        /// <returns>No content</returns>
        [HttpDelete("{roomId:guid}")]
        public async Task<ActionResult> DeleteRoom(Guid roomId)
        {
            var room = await _roomService
                .ReadRoom(roomId);

            if (room is null) return BadRequest();

            return await this.Execute(() => _roomService.DeleteRoom(room));
        }

        /// <summary>
        /// 指定したルームからユーザーを削除します。
        /// </summary>
        /// <param name="roomId">ユーザーを削除するルームのID</param>
        /// <param name="userEmail">削除したいユーザーのEmail</param>
        /// <returns></returns>
        [HttpDelete("{roomId:guid}/User/{userEmail}")]
        public async Task<ActionResult<RoomDetail>> DeleteUserFromRoom(Guid roomId, string userEmail)
        {
            var user = await _userService.ReadUser(userEmail);
            if (user is null) return BadRequest("Specified User is not exsisted");

            var userInfoInRoom = await _roomService.ReadUserInfoInRoom(user.Id, roomId);
            if (userInfoInRoom is null) return BadRequest("User is not belonged to room");

            await _roomService.DeleteUserInfoInRoom(userInfoInRoom);

            return Ok(await _roomService.ReadRoomDetail(roomId));
        }

        /// <summary>
        /// 指定したルームの最終アクセス日時を更新します。
        /// </summary>
        /// <param name="roomId">最終アクセス日時を更新するルームのID</param>
        /// <returns>更新後のルームの詳細情報</returns>
        [HttpPut("{roomId:guid}")]
        public async Task<ActionResult<RoomDetail>> PutRoomLastAccessDate(Guid roomId)
        {
            var userEmail = User.Identity.Name;
            var user = await _userService.ReadUser(userEmail);
            if (user is null) return BadRequest("Specified User is not exsisted");

            var userInfoInRoom = await _roomService.ReadUserInfoInRoom(user.Id, roomId);
            if (userInfoInRoom is null) return BadRequest("User is not belonged to room");

            await _roomService.PutRoomLastAccessDate(userInfoInRoom);

            return Ok(await _roomService.ReadRoomDetail(roomId));
        }
    }
}