using Application.Payment.DTO;
using Application.Payment.Ports;
using Application.Payment.Responses;
using Application.Utils;

namespace Application.MercadoPago.Adapter;

public class MercadoPagoAdapter : IPaymentProcessor
{
    public async Task<PaymentResponse> CapturePayment(string paymentIntention)
    {
        ValidationPaymentIntention.Validate(paymentIntention);

        paymentIntention += "/success";

        var dto = new PaymentStateDTO()
        {
            CreatedAt = DateTime.UtcNow,
            Message = "Payment successful",
            PaymentId = new Random().Next(1, 1000),
            State = PaymentState.Success
        };

        PaymentResponse response = new PaymentResponse()
        {
            Data = dto,
            Success = true,
        };

        return response;
    }
}
