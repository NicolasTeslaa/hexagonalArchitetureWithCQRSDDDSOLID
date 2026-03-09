using Application.Guest.DTO;
using Application.Guest.Port;
using Application.Guest.Request;
using Application.Guest.Responses;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/guests")]
    public class GuestController : ControllerBase
    {
        private readonly ILogger<GuestController> _logger;
        private readonly IGuestManager _guestPort;

        public GuestController(ILogger<GuestController> logger, IGuestManager guestPort)
        {
            _logger = logger;
            _guestPort = guestPort;
        }

        [HttpPost]
        public async Task<ActionResult<GuestResponse>> Create(GuestDTO guest)
        {
            var request = new CreateGuestRequest
            {
                Data = guest
            };

            var res = await _guestPort.CreateGuest(request);

            if (res.Success) return Created("", res.Data);

            _logger.LogError("Failed to create guest: {ErrorMessage}", res);
            return BadRequest(500);
        }
    }
}
