using Application.Book.Port;
using Application.Payment.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IBookManager _bookManager;

    public PaymentController(ILogger<PaymentController> logger,
                IBookManager bookManager)
    {
        _bookManager = bookManager;
        _logger = logger;
    }

    [HttpPost]
    [Route("{bookingId}/pay")]
    public async Task<IActionResult> Pay(int bookingId, [FromBody] PaymentRequest request)
    {
        try
        {
            request.BookingId = bookingId;

            var result = await _bookManager.PayForAbooking(request);

            if (result.Success)
                return Ok(result.Data);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for booking {BookingId}", bookingId);
            return StatusCode(500, "An error occurred while processing the payment.");
        }
    }
}
