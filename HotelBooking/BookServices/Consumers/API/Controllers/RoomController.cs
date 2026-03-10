using Microsoft.AspNetCore.Mvc;
using Application.Room.Port;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;
        private readonly IRoomManager _service;

        public RoomController(ILogger<RoomController> logger, IRoomManager service)
        {
            _logger = logger;
            _service = service;
        }


    }
}
