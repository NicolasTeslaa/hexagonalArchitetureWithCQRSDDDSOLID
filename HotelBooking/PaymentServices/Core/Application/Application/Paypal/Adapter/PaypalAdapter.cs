
using Application.Payment.DTO;
using Application.Payment.Ports;
using Application.Payment.Responses;
using Application.Utils;

namespace Application.Paypal.Adapter;

public class PaypalAdapter : IPaymentProcessor
{
    public Task<PaymentResponse> CapturePayment(string paymentIntention)
    {
        throw new NotImplementedException();
    }
}
