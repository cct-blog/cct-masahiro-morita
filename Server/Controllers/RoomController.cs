using blazorTest.Server.Services;
using blazorTest.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace blazorTest.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> logger;

        private readonly RoomService _roomService;

        public RoomController(ILogger<RoomController> logger, RoomService roomService)
        {
            this.logger = logger;
            _roomService = roomService;
        }

        [HttpGet]
        public IEnumerable<UserRoom> Get()
        {
            var userEmail = User.Identity.Name;
            return _roomService.ReadRoomListOfUser(userEmail);
        }

        [HttpGet("{roomId:guid}")]
        public RoomDetail GetRoomDetail(Guid roomId) => _roomService.ReadRoomDetailFromId(roomId);

        [HttpPost]
        public RoomDetail CreateRoom(CreateRoom createRoom) => _roomService.CreateRoom(createRoom);

        [HttpPost("{roomId:guid}/User")]
        public RoomDetail AddUserToRoom(Guid roomId, List<string> userEmail)
            => _roomService.AddUserToRoom(userEmail, roomId);

        [HttpDelete("{roomId:guid}")]
        public IActionResult DeleteRoom(Guid roomId)
        {
            _roomService.DeleteRoom(roomId);
            return NoContent();
        }
    }
}