using blazorTest.Server.Models;
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
        public List<UserRoom> Get()
        {
            var userEmail = User.Identity.Name;
            return _roomService.ReadRoomListOfUser(userEmail);
        }
    }
}