using Application;
using Application.Guest.DTO;
using Application.Guest.Port;
using Application.Guest.Request;
using Application.Guest.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<GuestDTO>> Get(int id)
        {
            var guest = await _guestPort.GetById(id);

            if (guest.Success)
                return Ok(guest.Data);

            if (guest.ErrorCode == ErrorCodes.NOT_FOUND)
                return NotFound(guest);

            return StatusCode(StatusCodes.Status500InternalServerError, guest);
        }

        [HttpPost]
        public async Task<ActionResult<GuestResponse>> Create([FromBody] GuestDTO guest)
        {
            var request = new CreateGuestRequest
            {
                Data = guest
            };

            var res = await _guestPort.CreateGuest(request);

            if (res.Success)
                return CreatedAtAction(nameof(Create), new { id = res.Data.Id }, res);

            _logger.LogWarning("Failed to create guest. ErrorCode: {ErrorCode}, Message: {Message}",
                res.ErrorCode, res.Message);

            return res.ErrorCode switch
            {
                ErrorCodes.INVALID_EMAIL => BadRequest(res),
                ErrorCodes.INVALID_PERSON_DOCUMENT_ID => BadRequest(res),
                ErrorCodes.MISSING_REQUIRED_INFORMATION => BadRequest(res),
                ErrorCodes.COULD_NOT_STORE_DATA => StatusCode(StatusCodes.Status500InternalServerError, res),
                ErrorCodes.UNEXPECTED_ERROR => StatusCode(StatusCodes.Status500InternalServerError, res),
                _ => StatusCode(StatusCodes.Status500InternalServerError, res)
            };
        }
    }
}