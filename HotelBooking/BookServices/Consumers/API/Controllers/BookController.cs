using Application;
using Application.Book.DTO;
using Application.Book.Port;
using Application.Book.Request;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/books")]
public class BookController : ControllerBase
{
    private readonly IBookManager _service;
    private readonly ILogger<BookController> _logger;

    public BookController(IBookManager service, ILogger<BookController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetBookById(int id)
    {
        try
        {
            var response = await _service.GetById(id);

            if (response.Success)
                return Ok(response.Data);

            return response.ErrorCode switch
            {
                ErrorCodes.NOT_FOUND => NotFound(response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving a book by ID.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateBook([FromBody] BookDTO request)
    {
        try
        {
            var response = await _service.CreateBookAsync(request);

            if (response.Success)
                return Ok(response.Data);

            return response.ErrorCode switch
            {
                ErrorCodes.ROOM_REQUIRED => BadRequest(response),
                ErrorCodes.GUEST_REQUIRED => BadRequest(response),
                ErrorCodes.INVALID_BOOKING_DATE_RANGE => BadRequest(response),
                ErrorCodes.BOOKING_IN_PAST => BadRequest(response),
                ErrorCodes.ROOM_UNAVAILABLE => Conflict(response),
                ErrorCodes.BOOKING_CONFLICT => Conflict(response),
                ErrorCodes.NOT_FOUND => NotFound(response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating a book.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });
        }
    }
}