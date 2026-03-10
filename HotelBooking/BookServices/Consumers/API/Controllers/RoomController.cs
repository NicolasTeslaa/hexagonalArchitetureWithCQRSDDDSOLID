using Application;
using Application.Room.DTO;
using Application.Room.Port;
using Application.Room.Request;
using Application.Room.Responses;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IRoomManager _service;

    public RoomController(ILogger<RoomController> logger, IRoomManager service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDTO>>> GetAll()
    {
        try
        {
            var rooms = await _service.GetAllRoomsAsync();

            return Ok(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching all rooms.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomResponse>> GetById(int id)
    {
        try
        {
            var response = await _service.GetRoomByIdAsync(id);

            if (response.Success)
                return Ok(response.Data);

            if(response.ErrorCode == ErrorCodes.NOT_FOUND)
                return NotFound(response);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching room with id {RoomId}.", id);

            return StatusCode(StatusCodes.Status500InternalServerError, new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<RoomResponse>> Create([FromBody] RoomDTO room)
    {
        try
        {
            var request = new CreateRoomRequest
            {
                Data = room
            };

            var res = await _service.AddRoomAsync(request);

            if (res.Success)
                return CreatedAtAction(nameof(GetById), new { id = res.Data.Id }, res);

            _logger.LogWarning("Failed to create room. ErrorCode: {ErrorCode}, Message: {Message}",
                res.ErrorCode, res.Message);

            return res.ErrorCode switch
            {
                ErrorCodes.MISSING_REQUIRED_INFORMATION => BadRequest(res),
                ErrorCodes.COULD_NOT_STORE_DATA => StatusCode(StatusCodes.Status500InternalServerError, res),
                ErrorCodes.UNEXPECTED_ERROR => StatusCode(StatusCodes.Status500InternalServerError, res),
                _ => StatusCode(StatusCodes.Status500InternalServerError, res)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating room.");

            return StatusCode(StatusCodes.Status500InternalServerError, new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] RoomDTO room)
    {
        try
        {
            if (id != room.Id)
            {
                return BadRequest(new RoomResponse
                {
                    Success = false,
                    ErrorCode = ErrorCodes.MISSING_REQUIRED_INFORMATION,
                    Message = "Route id is different from room id."
                });
            }

            var updated = await _service.UpdateRoomAsync(room);

            if (!updated)
            {
                _logger.LogWarning("Failed to update room with id {RoomId}.", id);

                return BadRequest(new RoomResponse
                {
                    Success = false,
                    ErrorCode = ErrorCodes.COULD_NOT_STORE_DATA,
                    Message = "Could not update room."
                });
            }

            return Ok(new RoomResponse
            {
                Success = true,
                Data = room
            });
        }
        catch (MissingRequiredInformationException ex)
        {
            _logger.LogWarning(ex, "Missing required information while updating room {RoomId}.", id);

            return BadRequest(new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.MISSING_REQUIRED_INFORMATION,
                Message = "Missing required information."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating room {RoomId}.", id);

            return StatusCode(StatusCodes.Status500InternalServerError, new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteRoomAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Room with id {RoomId} was not found for deletion.", id);

                return NotFound(new RoomResponse
                {
                    Success = false,
                    ErrorCode = ErrorCodes.NOT_FOUND,
                    Message = "Room not found."
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting room {RoomId}.", id);

            return StatusCode(StatusCodes.Status500InternalServerError, new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });
        }
    }
}