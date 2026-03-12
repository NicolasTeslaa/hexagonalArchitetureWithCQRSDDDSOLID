using Application.Payment.Ports;
using Application.Payment.Responses;

namespace Application.Stripe.Adapter;

public class StripeAdapter : IPaymentProcessor
{
    public Task<PaymentResponse> CapturePayment(string paymentIntention)
    {
        throw new NotImplementedException();
    }
}
