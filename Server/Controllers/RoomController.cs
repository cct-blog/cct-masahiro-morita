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
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;

        private readonly RoomService _roomService;

        public RoomController(ILogger<RoomController> logger, RoomService roomService)
        {
            _logger = logger;
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IEnumerable<UserRoom>> Get()
        {
            var userEmail = User.Identity.Name;
            return await _roomService.ReadRoomListOfUser(userEmail);
        }

        [HttpGet("{roomId:guid}")]
        public async Task<RoomDetail> GetRoomDetail(Guid roomId)
            => await _roomService.ReadRoomDetailFromId(roomId);

        [HttpPost]
        public async Task<RoomDetail> CreateRoom(CreateRoom createRoom)
            => await _roomService.CreateRoom(createRoom);

        [HttpPost("{roomId:guid}/User")]
        public async Task<RoomDetail> AddUserToRoom(Guid roomId, List<string> userEmail)
            => await _roomService.AddUserToRoom(userEmail, roomId);

        [HttpDelete("{roomId:guid}")]
        public async Task<IActionResult> DeleteRoom(Guid roomId)
        {
            await _roomService.DeleteRoom(roomId);
            return NoContent();
        }
    }
}